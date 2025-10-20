using EA;
using Ontomo.Model.DataObjects;
using Ontomo.Utils.Logging;

namespace Ontomo.Functions.ExportEA
{
    /// <summary>
    /// Handles the export of extraced data to an Enterprise Architect model conforming to the Meta-Object Facility (MOF) standard. 
    /// Additionally, the modeling structure is adapted to be compatible with the MOPRO application.
    /// </summary>
    internal class EAExportHandler
    {
        private readonly Logger logger = Static.Logger;
        private readonly Repository repository;
        internal static string OntomoModelGUID { get => Static.OntomoModelGUID ?? ""; }
        private readonly List<OntologyBaseItem> ExtractedOntologyItems = Static.ExtractedOntologyItems;
        private readonly EACrawlerHelper crawlerHelper;

        private string modelElementsGUID = "";
        private string csmElementsGUID = "";
        private string semanticModelGUID = "";

        public EAExportHandler(Repository repository, EACrawlerHelper crawlerHelper)
        {

            this.repository = repository;
            this.crawlerHelper = crawlerHelper;
            logger.LogInfo("EAExportHandler initialized.");
        }

        public void Execute()
        {
            if (this.repository.ConnectionString != "")
            {
                logger.LogInfo("EA project is open. Starting export of extracted ontology items.");
                _ = savePackageGUIDs();

                createViewsAndDiagrams();
                importOntologyEntitiesToModelElements();
                importOntologyRelationsToModelElements();
                relateElementsToParents();
                connectUmlParents();

                foreach (var ontologyItem in ExtractedOntologyItems.OfType<OntologyEntity>().ToList())
                {
                    addOntologyEntityToAllRelevantDiagrams(ontologyItem);
                }
                addOntologyRelationsToRelevantDiagrams();


            }
            else
            {
                logger.LogWarning("No EA project is open. Export of extracted ontology items could not be performed.");
            }
        }

        private void connectUmlParents()
        {
            // Get the Elements acting as UML parent elements from UML4ProfilePackage
            Package uml4ProfilePackage = repository.GetPackageByGuid(crawlerHelper.extractPackageGuidByName("UML4Profile"));
            if (uml4ProfilePackage == null)
            {
                logger.LogError("UML4Profile package not found. Cannot connect UML parents.");
                return;
            }

            Element? UmlClassElement = uml4ProfilePackage.Elements.GetByName("Class") as Element;
            Element? UmlAssociationElement = uml4ProfilePackage.Elements.GetByName("Association") as Element;

            if (UmlClassElement == null || UmlAssociationElement == null)
            {
                logger.LogError("UML Class or Association element not found in UML4Profile package. Cannot connect UML parents.");
                return;
            }

            // Iterate through all ontology entities and connect them to the UML Class element
            foreach (var ontologyItem in ExtractedOntologyItems.OfType<OntologyEntity>().ToList())
            {
                Element? ontologyElement = repository.GetElementByGuid(ontologyItem.EAGUID);
                if (ontologyElement == null)
                {
                    logger.LogDebug($"Ontology entity '{ontologyItem.Name}' with GUID {ontologyItem.EAGUID} not found. Skipping.");
                    continue;
                }
                // Check if the element is already connected to a parent
                bool alreadyConnected = false;
                for (short i = 0; i < ontologyElement.Connectors.Count; i++)
                {
                    Connector connector = (Connector)ontologyElement.Connectors.GetAt(i);
                    if (connector.Type == "Generalization" && connector.ClientID == ontologyElement.ElementID)
                    {
                        alreadyConnected = true;
                        break;
                    }
                }
                if (!alreadyConnected)
                {
                    crawlerHelper.connectTwoElementsViaOneConnector(ontologyElement, UmlClassElement, "", "Generalization", "Source -> Destination");
                    logger.LogInfo($"Connected ontology entity '{ontologyItem.Name}' to UML Class.");
                }
            }

            // Iterate through all ontology relations and connect them to the UML Association element
            foreach (var ontologyItem in ExtractedOntologyItems.OfType<OntologyRelation>().ToList())
            {
                Element? ontologyElement = repository.GetElementByGuid(ontologyItem.EAGUID);
                if (ontologyElement == null)
                {
                    logger.LogDebug($"Ontology relation '{ontologyItem.Name}' with GUID {ontologyItem.EAGUID} not found. Skipping.");
                    continue;
                }
                // Check if the element is already connected to the UML Association
                bool alreadyConnected = false;
                for (short i = 0; i < ontologyElement.Connectors.Count; i++)
                {
                    Connector connector = (Connector)ontologyElement.Connectors.GetAt(i);
                    if (connector.Type == "Generalization" && connector.ClientID == ontologyElement.ElementID)
                    {
                        alreadyConnected = true;
                        break;
                    }
                }
                if (!alreadyConnected)
                {
                    crawlerHelper.connectTwoElementsViaOneConnector(ontologyElement, UmlAssociationElement, "", "Generalization", "Source -> Destination");
                    logger.LogInfo($"Connected ontology relation '{ontologyItem.Name}' to UML Association.");
                }
            }
        }

        private bool savePackageGUIDs()
        {
            modelElementsGUID = crawlerHelper.extractPackageGuidByName("Model Elements");
            csmElementsGUID = crawlerHelper.extractPackageGuidByName("CSM Elements");
            semanticModelGUID = crawlerHelper.extractPackageGuidByName("Semantic Model");

            if (!string.IsNullOrEmpty(modelElementsGUID) && !string.IsNullOrEmpty(csmElementsGUID) && !string.IsNullOrEmpty(semanticModelGUID))
            {
                logger.LogInfo("Necessary package GUIDs successfully extracted.");
                return true;
            }
            return false;

        }

        private void createViewsAndDiagrams()
        {
            int targetPackageID = repository.GetPackageByGuid(modelElementsGUID).ParentID;
            Package targetPackage = repository.GetPackageByID(targetPackageID);

            if (targetPackage == null)
            {
                logger.LogError("Target package not found. Cannot create views and diagrams.");
                return;
            }

            foreach (var ontologyView in ExtractedOntologyItems.OfType<OntologyView>().ToList())
            {
                Diagram? existingViewDiagram = null;
                Package? existingViewPackage = null;

                try
                {
                    existingViewDiagram = repository.GetDiagramByGuidExtensionMethod(ontologyView.EAGUID);
                    existingViewPackage = (Package)targetPackage.Packages.GetByName(ontologyView.Name);
                }
                catch (Exception e)
                {
                    logger.LogInfo($"Error retrieving existing view package or diagram: {e.Message}");
                }

                if (existingViewDiagram == null && existingViewPackage == null)
                {
                    Package viewPackage = (Package)targetPackage.Packages.AddNew(ontologyView.Name, "Package");
                    viewPackage.StereotypeEx = "view";
                    viewPackage.Update();

                    Diagram viewDiagram = (Diagram)viewPackage.Diagrams.AddNew(ontologyView.Name, "Class");
                    viewPackage.Diagrams.Refresh();
                    viewDiagram.Update();
                    crawlerHelper.setDiagramGUID(viewDiagram, ontologyView.EAGUID);


                    viewPackage.Diagrams.Refresh();

                    logger.LogInfo($"Created view package and diagram '{ontologyView.Name}' with GUID: {viewPackage.PackageGUID}");

                }
                else if (existingViewPackage != null && existingViewDiagram == null)
                {
                    Diagram viewDiagram = (Diagram)existingViewPackage.Diagrams.AddNew(ontologyView.Name, "Class");
                    crawlerHelper.setDiagramGUID(viewDiagram, ontologyView.EAGUID);
                    viewDiagram.Update();

                    existingViewPackage.Diagrams.Refresh();

                    logger.LogInfo($"Created view diagram '{ontologyView.Name}' in existing view package");

                }
                else if (existingViewDiagram != null && existingViewPackage == null)
                {
                    Package viewPackage = repository.GetPackageByID(existingViewDiagram.ParentID);
                    viewPackage.Name = existingViewDiagram.Name = ontologyView.Name;

                }
                else if (existingViewDiagram != null && existingViewPackage != null
                    && existingViewDiagram.ParentID == existingViewPackage.PackageID)
                {

                    logger.LogInfo($"View package '{ontologyView.Name}' and diagram already exist in the model with name '{existingViewDiagram.Name}'");
                    existingViewDiagram.Name = existingViewPackage.Name = ontologyView.Name;
                    logger.LogInfo($"Updated existing view package and diagram names to '{ontologyView.Name}'");

                }
            }
        }

        private void importOntologyEntitiesToModelElements()
        {
            foreach (var ontologyItem in ExtractedOntologyItems.OfType<OntologyEntity>().ToList())
            {
                Element existingElement = repository.GetElementByGuid(ontologyItem.EAGUID);
                if (existingElement == null)
                {
                    Element newElement = crawlerHelper.addNewClass(modelElementsGUID, ontologyItem.Name, ontologyItem.EAGUID);

                    addElementToSemanticDiagram(newElement);
                    addCsmElementsWithAssociations(newElement);

                    existingElement = newElement;
                    logger.LogInfo($"Imported ontology entity '{ontologyItem.Name}' with GUID: {ontologyItem.EAGUID}");
                }
                else
                {
                    logger.LogInfo($"Ontology entity '{ontologyItem.Name}' already exists. Updating existing element.");
                }

                existingElement.Name = ontologyItem.Name;
                existingElement.Notes = ontologyItem.Definition;
                existingElement.Abstract = ontologyItem.IsAbstract ? "true" : "false";

                existingElement.Update();
                existingElement.Refresh();

            }
        }

        private void importOntologyRelationsToModelElements()
        {
            foreach (var ontologyRelation in ExtractedOntologyItems.OfType<OntologyRelation>().ToList())
            {
                Element existingRelation = repository.GetElementByGuid(ontologyRelation.EAGUID);
                if (existingRelation == null)
                {
                    Element newRelationElement = crawlerHelper.addNewClass(modelElementsGUID, ontologyRelation.Name,
                        ontologyRelation.EAGUID);

                    addElementToSemanticDiagram(newRelationElement);
                    addCsmElementsWithAssociations(newRelationElement);

                    existingRelation = newRelationElement;

                    logger.LogInfo($"Imported ontology relation '{ontologyRelation.Name}' with GUID: {ontologyRelation.EAGUID}");
                }
                else
                {
                    logger.LogInfo($"Ontology relation '{ontologyRelation.Name}' already exists. Updating existing relation.");
                }

                existingRelation.Name = ontologyRelation.Name;
                existingRelation.Notes = ontologyRelation.Definition;
                existingRelation.Abstract = ontologyRelation.IsAbstract ? "true" : "false";

                existingRelation.Update();
                existingRelation.Refresh();

                removeAllDirectedAssociations(existingRelation);

                ontologyRelation.Domains.ForEach(domain =>
                    {
                        Element domainElement = repository.GetElementByGuid(domain.EAGUID);
                        if (domainElement != null)
                        {
                            crawlerHelper.connectTwoElementsViaOneConnector(existingRelation,
                                domainElement, "", "Association",
                                "Source -> Destination", "source", true);
                        }
                    });

                ontologyRelation.Ranges.ForEach(range =>
                {
                    Element rangeElement = repository.GetElementByGuid(range.EAGUID);
                    if (rangeElement != null)
                    {
                        crawlerHelper.connectTwoElementsViaOneConnector(existingRelation,
                            rangeElement, "", "Association",
                            "Source -> Destination", "target", true);
                    }
                });

                logger.LogInfo("Connectors between classes are established");
            }
        }

        private void addCsmElementsWithAssociations(Element asmElement)
        {
            Element newCsmElement = crawlerHelper.addNewClass(csmElementsGUID, asmElement.Name);
            crawlerHelper.connectTwoElementsViaOneConnector(asmElement, newCsmElement);

            logger.LogInfo($"Added CSM element '{newCsmElement.Name}'");

        }

        private void addElementToSemanticDiagram(Element element)
        {
            Diagram semanticDiagram = (Diagram)repository.GetPackageByGuid(semanticModelGUID).Diagrams.GetByName("Semantics");
            DiagramObject newDiagramObject = crawlerHelper.addElementToDiagram(element, semanticDiagram);
            newDiagramObject.ShowNotes = true;

            logger.LogInfo($"Added element '{element.Name}' to semantic diagram with GUID: {semanticDiagram.DiagramGUID}");
        }

        private void removeAllDirectedAssociations(Element sourceElement)
        {
            for (short i = 0; i < sourceElement.Connectors.Count; i++)
            {
                Connector connector = (Connector)sourceElement.Connectors.GetAt(i);

                bool isAssociation = connector.Type == "Association";
                bool isSource = connector.ClientID == sourceElement.ElementID;
                bool isDirectionCorrect = connector.Direction == "Source -> Destination";

                if (isAssociation && isSource && isDirectionCorrect)
                {
                    sourceElement.Connectors.DeleteAt(i, true);
#pragma warning disable S127 // "for" loop stop conditions should be invariant
                    i--; // Adjust index after deletion
#pragma warning restore S127 // "for" loop stop conditions should be invariant
                }
            }
            logger.LogInfo($"Removed all directed associations from element '{sourceElement.Name}'");
        }

        private void relateElementsToParents()
        {
            foreach (var ontologyItem in ExtractedOntologyItems)
            {
                ontologyItem.Parents.ForEach(parent =>
                {
                    Element parentElement = repository.GetElementByGuid(parent.EAGUID);
                    Element childElement = repository.GetElementByGuid(ontologyItem.EAGUID);

                    bool alreadyConnected = false;

                    if (childElement != null)
                    {
                        for (short i = 0; i < childElement.Connectors.Count; i++)
                        {
                            Connector connector = (Connector)childElement.Connectors.GetAt(i);

                            bool isGeneralization = connector.Type == "Generalization";
                            bool isToCorrectParent = connector.SupplierID == parentElement.ElementID;
                            bool isFromCorrectChild = connector.ClientID == childElement.ElementID;

                            if (isGeneralization && isToCorrectParent && isFromCorrectChild)
                            {
                                alreadyConnected = true;
                                break;
                            }
                        }
                    }

                    if (!alreadyConnected && childElement != null)
                    {
                        crawlerHelper.connectTwoElementsViaOneConnector(childElement,
                                parentElement, "", "Generalization",
                                "Source -> Destination");

                        logger.LogInfo("Relating elements to their parents completed.");
                    }
                });
            }
        }

        private void addOntologyEntityToAllRelevantDiagrams(OntologyEntity entity)
        {
            Element ontologyElement = repository.GetElementByGuid(entity.EAGUID);
#pragma warning disable S3267 // Loops should be simplified with "LINQ" expressions
            foreach (OntologyView view in entity.IntroducedInList)
            {
                Diagram? viewDiagram = repository.GetDiagramByGuidExtensionMethod(view.EAGUID) as Diagram;
                if (viewDiagram == null)
                {
                    logger.LogWarning($"View diagram with GUID {view.EAGUID} not found for ontology entity '{ontologyElement.Name}'. Skipping.");
                    continue;
                }
                crawlerHelper.addElementToDiagram(ontologyElement, viewDiagram);

                logger.LogInfo($"Added ontology entity '{ontologyElement.Name}' to 'introduced in' view diagram '{viewDiagram.Name}'");
            }

            foreach (OntologyView view in entity.FeaturedInList)
            {
                Diagram? viewDiagram = repository.GetDiagramByGuidExtensionMethod(view.EAGUID) as Diagram;
                if (viewDiagram == null)
                {
                    logger.LogWarning($"View diagram with GUID {view.EAGUID} not found for ontology entity '{ontologyElement.Name}'. Skipping.");
                    continue;
                }
                crawlerHelper.addElementToDiagram(ontologyElement, viewDiagram);

                logger.LogInfo($"Added ontology entity '{ontologyElement.Name}' to 'featured in' view diagram '{viewDiagram.Name}'");
            }
#pragma warning restore S3267 // Loops should be simplified with "LINQ" expressions

        }

        private void addOntologyRelationsToRelevantDiagrams()
        {
            if (repository == null)
                return;

            foreach (var relationItem in ExtractedOntologyItems.OfType<OntologyRelation>().ToList())
            {
                var connectedEntities = relationItem.Domains.Concat(relationItem.Ranges).OfType<OntologyEntity>().ToList();

                for (int i = 0; i < connectedEntities.Count - 1; i++)
                {
                    for (int j = 0; j < connectedEntities.Count; j++)
                    {
                        var entity1 = connectedEntities[i];
                        var entity2 = connectedEntities[j];

                        var viewsOfEntity1 = entity1.IntroducedInList.Concat(entity1.FeaturedInList).OfType<OntologyView>().ToList();
                        var viewsOfEntity2 = entity2.IntroducedInList.Concat(entity2.FeaturedInList).OfType<OntologyView>().ToList();

                        var commonViews = viewsOfEntity1
                            .Where(view1 => viewsOfEntity2.Any(view2 => view2.EAGUID == view1.EAGUID)).ToList();

                        foreach (var commonView in commonViews)
                        {
                            Element? relationElement = repository.GetElementByGuid(relationItem.EAGUID);
                            Diagram? viewDiagram = repository.GetDiagramByGuidExtensionMethod(commonView.EAGUID) as EA.Diagram;

                            if (relationElement != null && viewDiagram != null)
                            {
                                crawlerHelper.addElementToDiagram(relationElement, viewDiagram);
                                logger.LogInfo($"Added ontology relation '{relationElement.Name}' to view diagram '{viewDiagram.Name}'");
                            }
                        }
                    }
                }
            }
        }


        internal void testMethod(Repository repository)
        {
            Package targetPackage = repository.GetPackageByGuid(modelElementsGUID);

            for (int i = 0; i < 5; i++)
            {
                targetPackage.Elements.AddNew("Class" + i.ToString(), "Class");
                targetPackage.Elements.Refresh();
            }

            Element class1 = (Element)targetPackage.Elements.GetByName("Class1");
            Element class2 = (Element)targetPackage.Elements.GetByName("Class2");

            Connector newConnector = (Connector)class1.Connectors.AddNew("", "Association");

            newConnector.ClientID = class1.ElementID;
            newConnector.SupplierID = class2.ElementID;

            newConnector.Update();
            class1.Connectors.Refresh();


            targetPackage.Elements.Refresh();

            Diagram newDiagram = (Diagram)targetPackage.Diagrams.AddNew("TestDiagram", "Class");
            newDiagram.Update();
            targetPackage.Diagrams.Refresh();

            DiagramObject newDiagramObject1 = (DiagramObject)newDiagram.DiagramObjects.AddNew("", "");
            newDiagramObject1.ElementID = class1.ElementID;

            newDiagramObject1.Update();
            newDiagram.DiagramObjects.Refresh();

        }
    }
}
