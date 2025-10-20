using EA;
using Mopro.Model;
using System.Xml;

namespace Mopro.Functions.Profile.Shapescript
{
    class ShapescriptBuilderImage : ShapescriptBuilderEntity
    {

        private string imageName = "";
        private string PNGpath = @"";
        private string fullImagePath = "";

        public ShapescriptBuilderImage(Repository repository, Dictionary<string, string> csmProperties) : base(repository, csmProperties)
        {
            string metaModelPath = repository.ConnectionString;

            if (csmProperties.ContainsKey(MetamodelConstants.CSMPropImgRelPath) && csmProperties["imgRelPath"] != "")
            {
                PNGpath = csmProperties[MetamodelConstants.CSMPropImgRelPath];
                imageName = Path.GetFileNameWithoutExtension(PNGpath);

                fullImagePath = Path.GetDirectoryName(metaModelPath) + "\\" + PNGpath;
                fullImagePath = fullImagePath.Replace("/", "\\");
            }
        }

        public override string getShapescript()
        {
            if (customShapescript != null && customShapescript != "!none") return customShapescript;

            if (imageName == "") return "";

            string nameCompartment = getNameCompartmentInfo(MetamodelConstants.BorderLayoutPositions.S);

            string stereotypeInfo = getStereotypeInfo();

            string shapescript = string.Format(
                "shape main" +
                "{{" +
                    "layouttype= \"border\";" +
                    "defSize({0},{1});" +
                    "image(\"{2}\",0,0,100,100);" + 

                     "{3}" +
                     "{4}" +
                "}}",
                absolutWidth.ToString(),
                absolutHeight.ToString(),
                imageName,
                stereotypeInfo,
                nameCompartment);

            return shapescript;
        }


        public override string getBase64ImageRepresentation()
        {
            string base64ImageRepresentation = "";

            if (fullImagePath == "" || !System.IO.File.Exists(fullImagePath)) return "";


            byte[] imageArray = System.IO.File.ReadAllBytes(fullImagePath);
            base64ImageRepresentation = Convert.ToBase64String(imageArray);

            return base64ImageRepresentation;
        }

        public override void setDataSetImageXMLElement(ref XmlDocument doc, ref XmlNode image, int id)
        {
            string base64ImageRepresentation = getBase64ImageRepresentation();
            if (base64ImageRepresentation != "")
            {
                XmlElement elem = doc.CreateElement(MetamodelConstants.DataRow);
                XmlNode set = image.AppendChild(elem);

                XmlElement col1 = doc.CreateElement(MetamodelConstants.Column);
                col1.SetAttribute("name", "ImageID");
                col1.SetAttribute("value", id.ToString());

                XmlElement col2 = doc.CreateElement(MetamodelConstants.Column);
                col2.SetAttribute("name", "Name");
                col2.SetAttribute("value", imageName);

                XmlElement col3 = doc.CreateElement(MetamodelConstants.Column);
                col3.SetAttribute("name", "Type");
                col3.SetAttribute("value", "Bitmap");

                XmlElement col4 = doc.CreateElement(MetamodelConstants.Column);
                col4.SetAttribute("name", "Image");
                col4.SetAttribute("xmlns:dt", "urn:schemas-microsoft-com:datatypes");
                col4.SetAttribute("dt:dt", "bin.base64");
                col4.InnerText = base64ImageRepresentation;

                set.AppendChild(col1);
                set.AppendChild(col2);
                set.AppendChild(col3);
                set.AppendChild(col4);
            }

        }

        private void getLabelInfo(out string labelInfo)
        {
            labelInfo = "";
            if (showName)
            {
                labelInfo = string.Format(
                    "shape label" +
                    "{{" +
                        "setOrigin(\"S\",0,-20);" +
                        "setfontcolor(100,100,255);" +
                    "}}");
            }
        }


    }
}

