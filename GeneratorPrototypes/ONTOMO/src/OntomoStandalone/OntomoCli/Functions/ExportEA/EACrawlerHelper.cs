using EA;
using Ontomo.Utils.Logging;

namespace Ontomo.Functions.ExportEA
{
    internal class EACrawlerHelper
    {
        private readonly Logger logger = Static.Logger;
        private readonly Repository repository;
        private static string ontomoModelGUID { get => Static.OntomoModelGUID ?? "";}

        public EACrawlerHelper(Repository repository, Logger? customLogger = null)
        {
            this.repository = repository;
            logger = customLogger ?? Static.Logger;
            logger.LogInfo("EACrawlerHelper initialized.");
        }

        internal string extractPackageGuidByName(string packageName)
        {
            Package ontomoModel = repository.GetPackageByGuid(ontomoModelGUID);

            string packageGUID = findPackageGuidRecursively(ontomoModel, packageName);

            if (string.IsNullOrEmpty(packageGUID))
            {
                logger.LogError($"Package '{packageName}' not found in the model.");
            }

            logger.LogInfo($"Package GUID for '{packageName}' extracted: {packageGUID}");
            return packageGUID;
        }

        internal string findPackageGuidRecursively(Package parent, string packageName)
        {
            foreach (Package childPackage in parent.Packages)
            {
                if (childPackage.Name.Equals(packageName))
                {
                    logger.LogInfo($"Found package '{packageName}' with GUID: {childPackage.PackageGUID}");
                    return childPackage.PackageGUID;
                }

                var result = findPackageGuidRecursively(childPackage, packageName);
                if (!string.IsNullOrEmpty(result))
                {
                    return result; // Return the found GUID
                }

            }

            return String.Empty; // Return empty string if package not found
        }

        internal Element addNewClass(string targetPackageGUID, string newElementName, string newElementGUID = "")
        {
            Package targetPackage = repository.GetPackageByGuid(targetPackageGUID);
            Element newElement = (Element)targetPackage.Elements.AddNew(newElementName, "Class");

            targetPackage.Elements.Refresh();
            targetPackage.Update();

            if (!string.IsNullOrEmpty(newElementGUID))
            {
                this.setElementGUID(newElement, newElementGUID);
            }

            return newElement;
        }

        internal void setElementGUID(Element newElement, string newGUID)
        {
            Element? testElement = null;

            try
            {
                repository?.GetElementByGuid(newGUID);
            }
            catch (Exception e)
            {
                logger.LogInfo($"No element with GUID {newGUID} found: {e.Message}");
            }

            if (testElement == null)
            {
                string SQLquery = $"UPDATE t_object " +
                                  $"SET ea_guid = {newGUID} " +
                                  $"WHERE Object_ID = {newElement.ElementID}";

                repository?.Execute(SQLquery);
            }
            else
            {
                logger.LogError($"Element with GUID {newGUID} already exists in the project. Cannot set new GUID for element {newElement.Name}.");
            }
        }

        internal void setDiagramGUID(Diagram newDiagram, string newGUID)
        {
            Diagram? testDiagram = null;

            try
            {
                repository.GetDiagramByGuidExtensionMethod(newGUID);
            }
            catch (Exception e)
            {
                logger.LogInfo($"No diagram with GUID {newGUID} found: {e.Message}");
            }

            if (testDiagram == null)
            {
                string SQLquery = $"UPDATE t_diagram " +
                                  $"SET ea_guid = {newGUID} " +
                                  $"WHERE Diagram_ID = {newDiagram.DiagramID}";

                repository.Execute(SQLquery);
            }
            else
            {
                logger.LogError($"Diagram with GUID {newGUID} already exists in the project. Cannot set new GUID for diagram {newDiagram.Name}.");
            }
        }
#pragma warning disable S2325 // Methods and properties that don't access instance data should be static
        internal void connectTwoElementsViaOneConnector(Element sourceClass, Element targetClass,
#pragma warning restore S2325 // Methods and properties that don't access instance data should be static
            string connectorName = "", string connectorType = "Association", string direction = "Unspecified",
            string supplierRole = "", bool isOwnedBy = false)
        {
            Connector newConnector = (Connector)sourceClass.Connectors.AddNew(connectorName, connectorType);
            newConnector.Direction = direction;
            newConnector.SupplierEnd.Role = supplierRole;
            newConnector.SupplierEnd.OwnedByClassifier = isOwnedBy;
            newConnector.ClientID = sourceClass.ElementID;
            newConnector.SupplierID = targetClass.ElementID;

            newConnector.Update();
            sourceClass.Connectors.Refresh();
        }

        internal DiagramObject addElementToDiagram(Element element, Diagram diagram)
        {
            foreach (DiagramObject existingObject in diagram.DiagramObjects)
            {
                if (existingObject.ElementID == element.ElementID)
                {
                    logger.LogInfo($"Element {element.Name} already exists in the diagram {diagram.Name}.");
                    return existingObject; // Return existing object if it already exists
                }
            }
            DiagramObject diagramObject = (DiagramObject)diagram.DiagramObjects.AddNew("", "");
            diagramObject.ElementID = element.ElementID;
            diagramObject.Update();
            diagram.DiagramObjects.Refresh();

            return diagramObject;
        }
    }
}
