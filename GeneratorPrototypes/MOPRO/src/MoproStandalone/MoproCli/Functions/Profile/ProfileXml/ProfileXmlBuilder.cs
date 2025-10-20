using EA;
using Mopro.Functions.Profile.ProfileConfig;
using Mopro.Model;
using System.Xml;

namespace Mopro.Functions.Profile.XmlProfile
{
    class ProfileXmlBuilder
    {
        private Repository repository;
        private string profileName;
        private string metaModelPath;
        private XmlDocument xmlDoc = new XmlDocument();
        private XmlDocument xmlToolboxDoc = new XmlDocument();

        private XmlElement root;
        private XmlNode profile;
        private XmlNode image;
        private XmlNode diagramProfile;
        private XmlNode toolbox;

        private XmlElement currentProfileToolboxElem;

        public ProfileXmlBuilder(Repository repository, string profileName, ProfileConfigParser configParser) 
        {
            this.repository = repository;
            this.profileName = profileName;
            this.metaModelPath = repository.ConnectionString;

            string xmlStructure = getXmlStructureWithProfileDetails(configParser);

            xmlDoc.LoadXml(xmlStructure);

            setBasicXmlNodeElements();
        }

        public ref XmlDocument GetXmlDocument()
        {
            return ref xmlDoc;
        }

        private string getXmlStructureWithProfileDetails(ProfileConfigParser configParser)
        {
            double myProfileVersion = 1.0;
            string myProfileDescription = "My created MDG Technology.";
            string myInfoUrl = "";
            string mySupportUrl = "";
            string myLogoString = "";
            string myIconString = "";

            if (configParser.IsConfigFileValid())
            {
                if(configParser.TechnologyName != null && configParser.TechnologyName.Length > 0)
                {
                    this.profileName = configParser.TechnologyName;
                }
                if (configParser.Version != 0)
                {
                    myProfileVersion = configParser.Version;
                }
                if (configParser.ProfileDescription != null && configParser.ProfileDescription.Length > 0)
                {
                    myProfileDescription = configParser.ProfileDescription;
                }
                if (configParser.Url != null && configParser.Url.Length > 0)
                {
                    myInfoUrl = configParser.Url;
                }
                if (configParser.Support != null && configParser.Support.Length > 0)
                {
                    mySupportUrl = configParser.Support;
                }
                if(configParser.ProfileIconRelPath != null && configParser.ProfileIconRelPath.Length > 0)
                {
                    string fullIconPath = Path.GetDirectoryName(metaModelPath) + "\\" + configParser.ProfileIconRelPath;
                    fullIconPath = fullIconPath.Replace("/", "\\");

                    myIconString = getProfileIconString(fullIconPath);
                }
                if(configParser.ProfileLogoRelPath != null && configParser.ProfileLogoRelPath.Length > 0) 
                {
                    string fullLogoPath = Path.GetDirectoryName(metaModelPath) + "\\" + configParser.ProfileLogoRelPath;
                    fullLogoPath = fullLogoPath.Replace("/", "\\");

                    myLogoString = getProfileLogoString(fullLogoPath);
                }
            }

            string guid = Guid.NewGuid().ToString().Substring(0, 12);
            string profileNameId = String.Concat(this.profileName.Where(c => !Char.IsWhiteSpace(c)));
            if(profileNameId.Length > 12) profileNameId = profileNameId.Substring(0, 12);
            string xmlStructure = xmlProfileStructure.Replace("MyProfileID", profileNameId);
            xmlStructure = xmlStructure.Replace("MyProfileGUID", guid);

            xmlStructure = xmlStructure.Replace("MyProfileName", this.profileName);
            xmlStructure = xmlStructure.Replace("MyProfileVersion", myProfileVersion.ToString());
            xmlStructure = xmlStructure.Replace("MyProfileNotes", myProfileDescription);
            xmlStructure = xmlStructure.Replace("MyInfoUrl", myInfoUrl);
            xmlStructure = xmlStructure.Replace("MySupportUrl", mySupportUrl);
            xmlStructure = xmlStructure.Replace("MyLogo", myLogoString);
            xmlStructure = xmlStructure.Replace("MyIcon", myIconString);

             

            return xmlStructure;
        }

        public void setProfileNodeElements(Element ih, string elementName, string csmElementName, Element parent, Element mostDistantParent, string baseStereotype, List<int> umlElementIDs, List<int> dslElementIDs,
            ShapescriptBuilder scBuilder, List<EA.Attribute> properties, Dictionary<string, string> csmProperties, Dictionary<string, (int elemID, string cardinality)> metaConstraints)
        {
            foreach (XmlNode childNode in profile.ChildNodes)
            {
                if (childNode.Name == MetamodelConstants.Documentation && childNode.Attributes["name"].Value == profileName)
                {
                    XmlNode stereotypes = childNode.NextSibling.FirstChild;
                    XmlElement elem = getBasicXmlElem(ih, elementName, csmElementName, parent, baseStereotype, umlElementIDs);
                    XmlNode stereotype = stereotypes.AppendChild(elem);
                    
                    if (scBuilder != null)
                    {
                        scBuilder.setStereotypeImageXMLElement(ref xmlDoc, ref stereotype);
                        scBuilder.setStereotypeIconXMLElement(ref xmlDoc, ref stereotype);
                    }

                    addMetaConstraintsToStereotypeElement(ref stereotype, metaConstraints);
                    addParentInfoToStereotype(ref stereotype, mostDistantParent, ih, dslElementIDs, umlElementIDs, properties, csmProperties);
                }
            }
        }

        public void setImageNodeElements(ShapescriptBuilder scBuilder)
        {
            if (image.Name == "DataSet" && scBuilder != null)
            {
                Random rnd = new Random();
                scBuilder.setDataSetImageXMLElement(ref xmlDoc, ref image, rnd.Next(100000000, 999999999));
            }
        }

        public void setToolboxNodeElements(string diagramName, Element mostDistantParent, Element ih, string elementName, bool isRelationship)
        {
            foreach (XmlNode xmlNode in currentProfileToolboxElem.ChildNodes)
            {
                if (xmlNode.Name == MetamodelConstants.Documentation && xmlNode.Attributes["name"].Value == diagramName + " Toolbox")
                {
                    XmlNode stereotypesNode = xmlNode.NextSibling.FirstChild;
                    XmlNode elementsTVNode = stereotypesNode.FirstChild.FirstChild.NextSibling;
                    XmlNode connectorsTVNode = stereotypesNode.FirstChild.NextSibling.FirstChild.NextSibling;

                    string parentName = mostDistantParent.Name;
                    if (mostDistantParent == ih) parentName = ModelConstants.Class;

                    XmlElement tag = xmlToolboxDoc.CreateElement(MetamodelConstants.Tag);
                    tag.SetAttribute("name", profileName + "::" + elementName + "(UML::" + parentName + ")");
                    tag.SetAttribute("type", "");
                    tag.SetAttribute("description", "");
                    tag.SetAttribute("unit", "");
                    tag.SetAttribute("values", "");
                    tag.SetAttribute("default", elementName);

                    if (isRelationship && connectorsTVNode.Name == MetamodelConstants.TaggedValues && mostDistantParent != null)
                    {
                        connectorsTVNode.AppendChild(tag);  
                    }
                    else if (elementsTVNode.Name == MetamodelConstants.TaggedValues && mostDistantParent != null)
                    {
                        elementsTVNode.AppendChild(tag);
                    }  
                }
            }

        }

        public void initializeDiagramToolboxNodeElement(string diagramName)
        {
            string xmlToolboxStructure = xmlProfileToolboxStructure.Replace("DiagramName", diagramName);
            xmlToolboxStructure = xmlToolboxStructure.Replace("Elements", diagramName + " Elements");
            xmlToolboxStructure = xmlToolboxStructure.Replace("Relationships", diagramName + " Relationships");

            xmlToolboxDoc.LoadXml(xmlToolboxStructure);
            currentProfileToolboxElem = xmlToolboxDoc.DocumentElement;
            
        }

        public void appendDiagramToolboxNodeElement()
        {
            if (toolbox.Name == MetamodelConstants.UIToolboxes)
            {
                toolbox.AppendChild(toolbox.OwnerDocument.ImportNode(
                    xmlToolboxDoc.DocumentElement, true));
            }
        }

        public void setDiagramProfileNodeElements(string diagramName, string diagramType)
        {
            if (diagramProfile.Name == MetamodelConstants.DiagramProfile)
            {
                foreach (XmlNode xmlNode in diagramProfile.ChildNodes)
                {
                    if (xmlNode.FirstChild.Name == MetamodelConstants.Documentation && xmlNode.FirstChild.Attributes["name"].Value == profileName + " Diagrams")
                    {
                        XmlNode stereotypesNode = xmlNode.FirstChild.NextSibling.FirstChild;

                        if (stereotypesNode.Name == MetamodelConstants.Stereotypes)
                        {
                            XmlElement stereotype = xmlDoc.CreateElement(MetamodelConstants.Stereotype);
                            stereotype.SetAttribute("name", diagramName + " Diagram");
                            stereotype.SetAttribute("notes", "");
                            stereotype.SetAttribute("cx", "0");
                            stereotype.SetAttribute("cy", "0");

                            XmlElement baseelem = xmlDoc.CreateElement(MetamodelConstants.AppliesTo);
                            XmlElement applyelem = xmlDoc.CreateElement(MetamodelConstants.Apply);
                            //Diagram_Custom
                            applyelem.SetAttribute("type", diagramType);

                            XmlElement propAlias = xmlDoc.CreateElement(MetamodelConstants.Property);
                            propAlias.SetAttribute("name", "alias");
                            propAlias.SetAttribute("value", diagramName + " Diagram");

                            XmlElement propToolbox = xmlDoc.CreateElement(MetamodelConstants.Property);
                            propToolbox.SetAttribute("name", "toolbox");
                            propToolbox.SetAttribute("value", diagramName + " Toolbox");

                            applyelem.AppendChild(propAlias);
                            applyelem.AppendChild(propToolbox);

                            baseelem.AppendChild(applyelem);
                            stereotype.AppendChild(baseelem);
                            stereotypesNode.AppendChild(stereotype);
                        }
                    }
                }
            }
        }

        private void setBasicXmlNodeElements()
        {
            root = xmlDoc.DocumentElement;
            profile = root;
            image = root;
            diagramProfile = root;
            toolbox = root;

            foreach (XmlNode childNode in root.ChildNodes)
            {
                if (childNode.Name == MetamodelConstants.UMLProfiles)
                {
                    profile = childNode.FirstChild;
                }

                if (childNode.Name == MetamodelConstants.Images)
                {
                    image = childNode.FirstChild.FirstChild;
                }

                if (childNode.Name == MetamodelConstants.DiagramProfile)
                {
                    diagramProfile = childNode;
                }

                if (childNode.Name == MetamodelConstants.UIToolboxes)
                {
                    toolbox = childNode;
                }
            }

        }

        private XmlElement getBasicXmlElem(Element ih, string elementName, string csmElementName, Element parent, string baseStereotype, List<int> umlElementIDs)
        {
            XmlElement elem = xmlDoc.CreateElement(MetamodelConstants.Stereotype);
            elem.SetAttribute("name", elementName);
            elem.SetAttribute("notes", ih.Notes);
            elem.SetAttribute("cx", "0");
            elem.SetAttribute("cy", "0");
     
            if (csmElementName != "")
            {
                elem.SetAttribute("csmElement", csmElementName + "_csm");
            }

            if (baseStereotype != "" && parent != null)
            {
                if (umlElementIDs.Contains(ih.ElementID))
                {
                    string baseClass = ("UML::" + baseStereotype).Replace(" ", "%20");
                    elem.SetAttribute(MetamodelConstants.Generalizes, baseClass);
                    elem.SetAttribute(MetamodelConstants.BaseStereotypes, baseClass);
                }
                else if (!umlElementIDs.Contains(parent.ElementID))
                {
                    string baseClass = (@profileName + "::" + baseStereotype).Replace(" ", "%20");
                    elem.SetAttribute(MetamodelConstants.Generalizes, baseClass);
                    elem.SetAttribute(MetamodelConstants.BaseStereotypes, baseClass);
                }
            }
            return elem;
        }

        private void addMetaConstraintsToStereotypeElement(ref XmlNode stereotype, Dictionary<string, (int elemID, string cardinality)> metaConstraints)
        {
            if (metaConstraints.Count > 0)
            {
                if ((metaConstraints.ContainsKey(MetamodelConstants.UmlRoles.source.ToString()) &&
                    metaConstraints.ContainsKey(MetamodelConstants.UmlRoles.target.ToString())) ||
                    metaConstraints.ContainsKey(MetamodelConstants.UmlRoles.type.ToString()))
                {
                    XmlElement metaConstraintsElem = xmlDoc.CreateElement(MetamodelConstants.Metaconstraints);

                    foreach (KeyValuePair<string, (int elemID, string cardinality)> metaConstraint in metaConstraints)
                    {
                        if (MetamodelConstants.umlRoles.Contains(metaConstraint.Key))
                        {
                            string constraintElemName = repository.GetElementByID(metaConstraint.Value.elemID).Name;
                            XmlElement metaConstraintElem = xmlDoc.CreateElement(MetamodelConstants.Metaconstraint);
                            metaConstraintElem.SetAttribute("umlRole", metaConstraint.Key);
                            metaConstraintElem.SetAttribute("constraint", profileName + "::" + constraintElemName);
                            if (metaConstraint.Value.cardinality != "")
                            {
                                metaConstraintElem.SetAttribute("cardinality", metaConstraint.Value.cardinality);
                            }
                            metaConstraintsElem.AppendChild(metaConstraintElem);
                        }
                    }
                    stereotype.AppendChild(metaConstraintsElem);
                }
            }
        }

        private void addParentInfoToStereotype(ref XmlNode stereotype,  Element mostDistantParent, Element ih, List<int> dslElementIDs, List<int> umlElementIDs,
            List<EA.Attribute> properties, Dictionary<string, string> csmProperties)
        {
            XmlElement baseelem;

            if (!dslElementIDs.Contains(ih.ElementID))
            {
                baseelem = getUMLParentElement();
                stereotype.AppendChild(baseelem);
            }
            else
            {
                if (mostDistantParent != ih)
                {
                    baseelem = getParentElement(mostDistantParent, csmProperties);
                    stereotype.AppendChild(baseelem);
                }
            }

            if (properties.Count > 0)
            {
                baseelem = getPropertyElement(properties);
                stereotype.AppendChild(baseelem);
            }
        }

        private XmlElement getUMLParentElement()
        {
            XmlElement baseelem = xmlDoc.CreateElement(MetamodelConstants.AppliesTo);
            XmlElement applyelem = xmlDoc.CreateElement(MetamodelConstants.Apply);
            applyelem.SetAttribute("type", ModelConstants.Class);

            baseelem.AppendChild(applyelem);
            return baseelem;
        }

        private XmlElement getParentElement(Element parent, Dictionary<string, string> csmProperties)
        {
            XmlElement baseelem = xmlDoc.CreateElement(MetamodelConstants.AppliesTo);
            XmlElement applyelem = xmlDoc.CreateElement(MetamodelConstants.Apply);
            applyelem.SetAttribute("type", parent.Name);

            if (csmProperties.ContainsKey(MetamodelConstants.CSMPropRelationDirection) &&
            csmProperties[MetamodelConstants.CSMPropRelationDirection] != "")
            {
                string relationDirectionValue = csmProperties[MetamodelConstants.CSMPropRelationDirection];
                string relationDirectionStrValue = MetamodelConstants.RelationDirectionStringDict[relationDirectionValue];

                XmlElement propelem = xmlDoc.CreateElement(MetamodelConstants.Property);
                propelem.SetAttribute("name", "direction");
                propelem.SetAttribute("value", relationDirectionStrValue);
                applyelem.AppendChild(propelem);
            }

            baseelem.AppendChild(applyelem);
            return baseelem;
        }

        private XmlElement getPropertyElement(List<EA.Attribute> properties)
        {
            XmlElement baseelem = xmlDoc.CreateElement(MetamodelConstants.TaggedValues);

            foreach (EA.Attribute property in properties)
            {
                XmlElement applyelem = xmlDoc.CreateElement(MetamodelConstants.Tag);
                applyelem.SetAttribute("name", property.Name);
                applyelem.SetAttribute("type", property.Type);
                applyelem.SetAttribute("description", "");
                applyelem.SetAttribute("unit", "");
                applyelem.SetAttribute("values", "");
                applyelem.SetAttribute("default", property.Default);

                baseelem.AppendChild(applyelem);
            }
            return baseelem;
        }

        private string getProfileIconString(string fullIconPath)
        {
            string iconString = "<Icon><Image type=\"bitmap\" xmlns:dt=\"urn:schemas-microsoft-com:datatypes\" dt:dt=\"bin.base64\">";

            if (fullIconPath == "" || !System.IO.File.Exists(fullIconPath)) return "";

            byte[] imageArray = System.IO.File.ReadAllBytes(fullIconPath);
            string base64IconRepresentation = Convert.ToBase64String(imageArray);


            return iconString + base64IconRepresentation + "</Image></Icon>";

        }

        private string getProfileLogoString(string fullLogoPath)
        {
            string logoPath = "<Logo><Image type=\"bitmap\" xmlns:dt=\"urn:schemas-microsoft-com:datatypes\" dt:dt=\"bin.base64\">";

            if (fullLogoPath == "" || !System.IO.File.Exists(fullLogoPath)) return "";

            byte[] imageArray = System.IO.File.ReadAllBytes(fullLogoPath);
            string base64LogoRepresentation = Convert.ToBase64String(imageArray);

            return logoPath + base64LogoRepresentation + "</Image></Logo>";
        }

        private const string xmlProfileToolboxStructure =
            @"<UMLProfile profiletype=""uml2"">
              <Documentation id=""5C1D493F-F"" name=""DiagramName Toolbox"" version=""1.0"" notes=""My DiagramName Profile Toolbox.""/>
              <Content>
                <Stereotypes>
                  <Stereotype name=""Elements"" notes="""" bgcolor=""-1"" fontcolor=""-1"" bordercolor=""-1"" borderwidth=""-1"" hideicon=""0"">
                    <AppliesTo>
                      <Apply type=""ToolboxPage""/>
                    </AppliesTo>
                    <TaggedValues>
                    </TaggedValues>
                  </Stereotype>
                  <Stereotype name=""Relationships"" notes="""" bgcolor=""-1"" fontcolor=""-1"" bordercolor=""-1"" borderwidth=""-1"" hideicon=""0"">
                    <AppliesTo>
                      <Apply type=""ToolboxPage""/>
                    </AppliesTo>
                    <TaggedValues>
                    </TaggedValues>
                  </Stereotype>
                </Stereotypes>
                <TaggedValueTypes/>
              </Content>
            </UMLProfile>";

        private const string xmlProfileStructure =
        @"<?xml version=""1.0"" encoding=""windows-1252""?>
        <MDG.Technology version=""1.0"">
          <Documentation id=""MyProfileID"" name=""MyProfileName"" version=""MyProfileVersion"" notes=""MyProfileNotes"" infoURI=""MyInfoUrl"" supportURI=""MySupportUrl""/>MyLogoMyIcon
          <UMLProfiles>
            <UMLProfile profiletype=""uml2"">
              <Documentation id=""MyProfileGUID"" name=""MyProfileName"" version=""1.0"" notes=""My created MyProfileName Profile.""/>
              <Content>
                <Stereotypes>
                </Stereotypes>
                <TaggedValueTypes/>
              </Content>
            </UMLProfile>
          </UMLProfiles>
          <Images><RefData version=""1.0"" exporter=""EA.25""><DataSet name = ""Model Images"" table=""t_image"" filter=""Name='#Name#'"" stoplist="";ImageID;"">
          </DataSet></RefData></Images>
          <DiagramProfile>
            <UMLProfile profiletype=""uml2"">
              <Documentation id=""B4F563A7-8"" name=""MyProfileName Diagrams"" version=""1.0"" notes=""My MyProfileName Profile Diagrams.""/>
              <Content>
                <Stereotypes>
                </Stereotypes>
                <TaggedValueTypes/>
              </Content>
            </UMLProfile>
          </DiagramProfile>
          <UIToolboxes>
          </UIToolboxes>
        </MDG.Technology>
        ";

    }
}
