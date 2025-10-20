using System.Xml;
using EA;
using System.Diagnostics;
using Mopro.Model;
using Mopro.Functions.Profile.Shapescript;
using Mopro.Functions.Profile.XmlProfile;
using Mopro.Functions.Profile.ProfileConfig;
using System.Text;
using Mopro.Utils.Logging;

namespace Mopro.Logic
{
    class CreateProfileLogicMetamodel
    {
        Logger logger = Static.logger;
        #region Helper Methods
        /// <summary>
        /// Selects and returns the profile package from the repository. Requires user interaction if multiple profile packages are available.
        /// </summary>
        /// <param name="repository"></param>
        /// <returns>The selected EA.Package, or null if none selected or found.</returns>
        private EA.Package? selectProfilePackage(EA.Repository repository)
        {
            string sql = @"SELECT o.ea_guid FROM t_object o WHERE o.Stereotype = 'profile' AND o.Object_Type = 'Package'";

            string resultXml = repository.SQLQuery(sql);

            // Load the XML
            var doc = new XmlDocument();
            doc.LoadXml(resultXml);

            // Extract GUIDs into a list
            var result = new List<string>();
            XmlNodeList? nodes = doc.SelectNodes("//ea_guid");

            foreach (XmlNode node in nodes)
            {
                if (node != null && !string.IsNullOrWhiteSpace(node.InnerText))
                {
                    result.Add(node.InnerText.Trim());
                }
            }

            if (result.Count == 0)
            {
                logger.LogInfo("No profile packages found in the repository, so nothing to do for me.");
                return null;
            }

            List<EA.Package> profilePackages = new List<EA.Package>();

            string profilePackageInfo = $"Found {result.Count} profile package(s) in the repository:\n" +
                $"| Index | Name\t\t | Created\t | Modified\t | GUID\n" +
                new string('-', 80) + "\n";

            for (short i = 0; i < result.Count; i++)
            {
                //EA.Package pkg = (EA.Package)result.GetAt(i);
                EA.Package pkg = repository.GetPackageByGuid(result[i]);
                profilePackages.Add(pkg);

                profilePackageInfo += $"| {i,5} | {pkg.Name,-15} | {pkg.Created,-10:yyyy-MM-dd} | {pkg.Modified,-10:yyyy-MM-dd} | {pkg.PackageGUID}\n";
            }

            logger.LogInfo(profilePackageInfo);

            if (profilePackages.Count == 1)
            {
                logger.LogInfo("One profile package found, selecting it automatically.");
                return profilePackages[0];
            }
            else if (profilePackages.Count < 1)
            {
                logger.LogError("No profile package found. Aborting Execution.");
                Environment.Exit(-1);
            } else if (Static.ProfilePackage != "") {
                EA.Package? pack = profilePackages.Where<Package>
                    (p => (p.Name  == Static.ProfilePackage)).
                    FirstOrDefault<Package>();
                if (pack != null)
                    return pack;
            }

            if (Static.NonInteractive)
            {
                logger.LogError("Multiple profile packages found, but non-interactive mode is enabled. Aborting execution.");
                Environment.Exit(-1);
            }

            logger.LogInfo("\nPlease select the index of the profile package to use: ");

            if (int.TryParse(Console.ReadLine(), out int selectedIndex)
                && selectedIndex >= 0
                && selectedIndex < profilePackages.Count)
            {
                return profilePackages[selectedIndex];
            }
            else
            {

                logger.LogWarning("Invalid selection. Aborting.");
                return null;
            }
        }
        #endregion Helper Methods

        private void getMetamodelDiagramList(ref List<Diagram> diagrams, Package package, bool encapsulatedInMetamodelPackage = false)
        {
            bool metaPackageFlag = encapsulatedInMetamodelPackage;
            if (package.StereotypeEx == MetamodelConstants.StereotypeMetamodel || metaPackageFlag)
            {
                metaPackageFlag = true;
                foreach (Diagram diagram in package.Diagrams)
                {
                    diagrams.Add(diagram);
                }
            }

            foreach (Package subPackage in package.Packages)
            {
                getMetamodelDiagramList(ref diagrams, subPackage, metaPackageFlag);
            }
        }

        private void getDiagramNamesInPackageList(ref List<string> profileDiagrams, Package package)
        {

            foreach (Diagram diagram in package.Diagrams)
            {
                profileDiagrams.Add(diagram.Name);
            }

            foreach (Package subPackage in package.Packages)
            {
                getDiagramNamesInPackageList(ref profileDiagrams, subPackage);
            }
        }

        private void getElementIDsInPackageList(ref List<int> dslElementIDs, Package package)
        {
            foreach (Element elem in package.Elements)
            {
                dslElementIDs.Add(elem.ElementID);
            }
            foreach (Package subPackage in package.Packages)
            {
                getElementIDsInPackageList(ref dslElementIDs, subPackage);
            }
        }

        private Package getAbstractSyntaxModelPackage(Package profilePackage)
        {
            foreach (Package package in profilePackage.Packages)
            {
                if (package.Name == MetamodelConstants.PackageNameASM) return package;
            }
            return null;
        }

        private Package getConcreteSyntaxModelPackage(Package profilePackage)
        {
            foreach (Package package in profilePackage.Packages)
            {
                if (package.Name == MetamodelConstants.PackageNameCSM) return package;
            }
            return null;
        }

        private Package getUML4ProfilePackage(Package profilePackage)
        {
            foreach (Package package in profilePackage.Packages)
            {
                if (package.Name == MetamodelConstants.PackageNameUMLElems) return package;
            }
            return null;
        }

        /// <summary>
        /// Get all attributes from an element, excluding inherited attributes
        /// </summary>
        /// <param name="element"></param>
        /// <param name="attrDict"></param>
        /// <returns></returns>
        private void getDirectlyAttachedAttributesFromElement(EA.IDualElement element, out Dictionary<string, string> attrDict)
        {
            attrDict = new Dictionary<string, string>();

            for (short propCount = 0; propCount < element.Attributes.Count; propCount++)
            {
                var dynamicItem = element.Attributes.GetAt(propCount);
                EA.Attribute attr = (EA.Attribute)dynamicItem;
                string name = attr.Name;
                string value = attr.Default;
                attrDict.Add(name, value);
            }
            return;
        }

        /// <summary>
        /// Get all attributes from an element, including inherited attributes
        /// </summary>
        /// <param name="element"></param>
        /// <param name="attrDict"></param>
        /// <returns></returns>
        private void getAllAttributesFromElement(EA.IDualElement element, out Dictionary<string, string> attrDict)
        {
            attrDict = new Dictionary<string, string>();

            for (short propCount = 0; propCount < element.Attributes.Count; propCount++)
            {
                var dynamicItem = element.Attributes.GetAt(propCount);
                EA.Attribute attr = (EA.Attribute)dynamicItem;
                string name = attr.Name;
                string value = attr.Default;
                attrDict.Add(name, value);
            }

            for (short propCount = 0; propCount < element.BaseClasses.Count; propCount++)
            {
                var dynamicItem = element.BaseClasses.GetAt(propCount);
                EA.Element baseClass = (EA.Element)dynamicItem;
                Dictionary<string, string> baseClassAttributes;
                getAllAttributesFromElement(baseClass, out baseClassAttributes);
                foreach (var baseClassAttribute in baseClassAttributes)
                {
                    if (!attrDict.ContainsKey(baseClassAttribute.Key))
                    {
                        attrDict.Add(baseClassAttribute.Key, baseClassAttribute.Value);
                    }
                }
            }

            // overwriting default values with RunState values (Features - Override Attribute Initializers...)
            Dictionary<string, string> filledAttrDict = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> attr in attrDict)
            {
                string runState = element.RunState;
                if (runState.Contains(attr.Key))
                {
                    string searchKey = "Variable=" + attr.Key + ";Value=";
                    string value = runState.Substring(runState.IndexOf(searchKey) + searchKey.Length);
                    value = value.Substring(0, value.IndexOf(";"));

                    filledAttrDict.Add(attr.Key, value);
                }
                else
                {
                    filledAttrDict.Add(attr.Key, attr.Value);
                }
            }

            attrDict = filledAttrDict;
            return;
        }

        private bool isCSMElement(EA.IDualElement element)
        {
            for (short baseClassCount = 0; baseClassCount < element.BaseClasses.Count; baseClassCount++)
            {
                var dynamicItem = element.BaseClasses.GetAt(baseClassCount);
                EA.Element baseClass = (EA.Element)dynamicItem;

                if (baseClass.Name == MetamodelConstants.CSMElementName) return true;

                if (isCSMElement(baseClass)) return true;
            }

            return false;
        }

        private bool isRelationshipElement(EA.Element mostDistantParent)
        {
            if (mostDistantParent.Name == ModelConstants.Dependency ||
                mostDistantParent.Name == ModelConstants.Association ||
                mostDistantParent.Name == ModelConstants.Generalization) return true;
            return false;
        }

        private EA.Element getMostDistantParent(EA.Element element, List<int> umlElementIDs, List<int> dslElementIDs)
        {
            EA.Element mostDistantParent = element;
            if (dslElementIDs.Contains(element.ElementID))
            {
                var stopwatch = Stopwatch.StartNew();
                TimeSpan timeout = TimeSpan.FromSeconds(5);

                while (!umlElementIDs.Contains(mostDistantParent.ElementID))
                {
                    if (!(stopwatch.Elapsed < timeout)) return element;

                    if (mostDistantParent.BaseClasses.Count > 0)
                    {
                        mostDistantParent = (Element)mostDistantParent.BaseClasses.GetAt(0);
                    }
                }
            }
            return mostDistantParent;
        }

        public CreateProfileLogicMetamodel(Repository repository)
        {

            ProfileConfigParser configParser = new ProfileConfigParser(repository);

            if (configParser.IsConfigFileValid())
            {
                configParser.PrintConfigLines();
            }
            else
            {
                logger.LogWarning($"Invalid config file");
            }

            Package? profilePackage = selectProfilePackage(repository);
            if (profilePackage == null)
            {
                logger.LogError("No profile package found. Aborting execution.");
                Environment.Exit(-1);
            }

            logger.LogInfo($"Selected profile package: {profilePackage.Name} ({profilePackage.PackageGUID})\n" +
                $"Creating profile implementation...");

            Package abstractSyntaxModelPackage = getAbstractSyntaxModelPackage(profilePackage);
            Package concreteSyntaxModelPackage = getConcreteSyntaxModelPackage(profilePackage);

            if (abstractSyntaxModelPackage == null)
            {
                logger.LogError("Profile Package must contain Abstract Syntax Model Package");
                return;
            }

            Package uml4ProfilePackage = getUML4ProfilePackage(profilePackage);

            string profileName = profilePackage.Name;

            var xmlDoc = new XmlDocument();

            List<Diagram> diagrams = new List<Diagram>();
            getMetamodelDiagramList(ref diagrams, profilePackage);

            List<string> profileDiagrams = new List<string>();
            getDiagramNamesInPackageList(ref profileDiagrams, abstractSyntaxModelPackage);

            List<string> csmDiagrams = new List<string>();
            getDiagramNamesInPackageList(ref csmDiagrams, concreteSyntaxModelPackage);

            List<string> uml4ProfileDiagrams = new List<string>();
            getDiagramNamesInPackageList(ref uml4ProfileDiagrams, uml4ProfilePackage);

            List<int> umlElementIDs = new List<int>();
            getElementIDsInPackageList(ref umlElementIDs, uml4ProfilePackage);

            List<int> dslElementIDs = new List<int>();
            getElementIDsInPackageList(ref dslElementIDs, abstractSyntaxModelPackage);

            ProfileXmlBuilder xmlBuilder = new ProfileXmlBuilder(repository, profileName, configParser);

            foreach (Diagram profileDiagram in diagrams)
            {
                if (profileDiagrams.Contains(profileDiagram.Name))
                {
                    xmlBuilder.setDiagramProfileNodeElements(profileDiagram.Name, profileDiagram.Type);
                }
                xmlBuilder.initializeDiagramToolboxNodeElement(profileDiagram.Name);

                foreach (DiagramObject diagramElement in profileDiagram.DiagramObjects)
                {
                    Element ih = repository.GetElementByID(diagramElement.ElementID);

                    if (ih.Type != ModelConstants.Class) continue;

                    string elementName = ih.Name;

                    Element? parent = null;
                    Element? mostDistantParent = null;

                    if (csmDiagrams.Contains(profileDiagram.Name))
                    {
                        // prevents dslElements from beeing added twice
                        if (dslElementIDs.Contains(ih.ElementID)) continue;
                        // adds suffix csm to csm Elements for preventing elements with the same name
                        else if (isCSMElement(ih)) elementName += "_csm";
                    }
                    else if (uml4ProfileDiagrams.Contains(profileDiagram.Name) && dslElementIDs.Contains(ih.ElementID))
                    {
                        continue;
                    }


                    if (ih.BaseClasses.Count > 0)
                    {
                        parent = (Element?)ih.BaseClasses.GetAt(0);
                    }

                    List<EA.Attribute> properties = new List<EA.Attribute>();
                    foreach (EA.Attribute property in ih.Attributes)
                    {
                        properties.Add(property);
                    }

                    string baseStereotype = "";

                    Element? csmElement = null;
                    string csmElementName = "";

                    Dictionary<string, (int elemID, string cardinality)> metaConstraints = new Dictionary<string, (int, string)>();

                    Dictionary<string, string> csmProperties = new Dictionary<string, string>();

                    foreach (Connector connector in ih.Connectors)
                    {
                        if (connector.Type == ModelConstants.Generalization && connector.ClientID == ih.ElementID)
                        {
                            Element conn = repository.GetElementByID(connector.SupplierID);
                            baseStereotype = conn.Name;
                        }

                        if (connector.Type == ModelConstants.Association)
                        {
                            if (connector.ClientEnd.Role != "" && connector.ClientID != ih.ElementID)
                            {
                                if (!metaConstraints.ContainsKey(connector.ClientEnd.Role))
                                {
                                    metaConstraints.Add(connector.ClientEnd.Role, (connector.ClientID, connector.ClientEnd.Cardinality));
                                }
                            }
                            if (connector.SupplierEnd.Role != "" && connector.SupplierID != ih.ElementID)
                            {
                                if (!metaConstraints.ContainsKey(connector.SupplierEnd.Role))
                                {
                                    metaConstraints.Add(connector.SupplierEnd.Role, (connector.SupplierID, connector.SupplierEnd.Cardinality));
                                }
                            }

                            csmElement = (connector.ClientID == ih.ElementID) ? repository.GetElementByID(connector.SupplierID) : repository.GetElementByID(connector.ClientID);
                            if (!isCSMElement(csmElement) || csmElement == null) continue;

                            getAllAttributesFromElement(csmElement, out csmProperties);
                            csmElementName = csmElement.Name;

                        }
                    }

                    // find most distant UML parent 
                    mostDistantParent = getMostDistantParent(ih, umlElementIDs, dslElementIDs);

                    ShapescriptBuilder scBuilder = ShapescriptBuilderFactory.getShapescriptBuilder(repository, csmProperties);

                    xmlBuilder.setProfileNodeElements(ih, elementName, csmElementName, parent, mostDistantParent, baseStereotype, umlElementIDs, dslElementIDs, scBuilder, properties, csmProperties, metaConstraints);
                    xmlBuilder.setImageNodeElements(scBuilder);

                    xmlBuilder.setToolboxNodeElements(profileDiagram.Name, mostDistantParent, ih, elementName, isRelationshipElement(mostDistantParent));
                }
                xmlBuilder.appendDiagramToolboxNodeElement();
            }

            string eaMdgPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                    @"\Sparx Systems\EA\MDGTechnologies";


            if (!Directory.Exists(eaMdgPath))
            {
                logger.LogInfo($"EA User-imported MDG technology directory did not exist - created it in: {eaMdgPath}");
                Directory.CreateDirectory(eaMdgPath);
                return;
            }

            string profilePath = @eaMdgPath + profileName + " Profile.xml";
            try
            {
                xmlDoc = xmlBuilder.GetXmlDocument();

                // Check if Static.OutputFile is set to a valid path  
                if (!string.IsNullOrWhiteSpace(Static.OutputFile) && Directory.Exists(Path.GetDirectoryName(Static.OutputFile)))
                {
                    profilePath = Path.GetFullPath(Static.OutputFile);

                    // EA uses legacy encoding not by default available in .NET 8, therefore:  
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                    xmlDoc.Save(profilePath);

                    string xmltext = System.IO.File.ReadAllText(profilePath);
                    xmltext = xmltext.Replace(" dt=", " dt:dt=");
                    System.IO.File.WriteAllText(profilePath, xmltext);

                    if (System.IO.File.Exists(profilePath))
                    {
                        logger.LogInfo("Successfully saved mdg xml file to " + profilePath);
                    }
                }
                else
                {
                    SaveFileDialog saveFileDialog = new SaveFileDialog();
                    saveFileDialog.InitialDirectory = eaMdgPath;
                    saveFileDialog.Filter = "XML-File | *.xml";
                    saveFileDialog.FileName = profileName;

                    // EA uses legacy encoding not by default available in .NET 8, therefore:  
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        xmlDoc.Save(saveFileDialog.FileName);
                        string savedProfilePath = Path.GetFullPath(saveFileDialog.FileName);

                        string xmltext = System.IO.File.ReadAllText(savedProfilePath);
                        xmltext = xmltext.Replace(" dt=", " dt:dt=");
                        System.IO.File.WriteAllText(savedProfilePath, xmltext);

                        if (System.IO.File.Exists(savedProfilePath))
                        {
                            logger.LogInfo("Successfully saved mdg xml file to " + savedProfilePath);
                        }
                    }
                }
            }
            catch (System.UnauthorizedAccessException unEx)
            {
                string exceptionString = unEx.Message;
                logger.LogError("Error on saving profile.\n" +
                    $"Privileges of User insufficient to save in this directory: \"{Static.OutputFile ?? profilePath}\"\n" +
                    $"May try to start Enterprise Architect in administrative rights mode?");
            }
            catch (Exception ex)
            {
                string exceptionString = ex.Message;
                logger.LogError("Unknown error on saving profile.\n" +
                    $"Message: {exceptionString}");
            }
        }
    }
}
