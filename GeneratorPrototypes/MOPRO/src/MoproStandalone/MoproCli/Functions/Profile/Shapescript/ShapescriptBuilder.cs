using EA;
using Mopro.Functions.Profile.Shapescript;
using System.IO.Compression;
using System.Text;
using System.Xml;

namespace Mopro.Model
{
    abstract class ShapescriptBuilder
    {

        protected string fillColor { get; set; }
        protected bool showName { get; set; }
        protected bool stereotypeVisible { get; set; }
        protected string fullToolboxIconPath { get; set; }
        protected string customShapescript {  get; set; }


        public ShapescriptBuilder(Repository repository, Dictionary<string, string> csmProperties) 
        { 
            foreach(KeyValuePair<string, string> prop in csmProperties)
            {
                switch (prop.Key)
                {
                    case MetamodelConstants.CSMPropFillColor:
                        fillColor = ColorStringParser.getRgbColorStringForShapescript(prop.Value);
                        break;
                    case MetamodelConstants.CSMPropShowName:
                        showName = prop.Value == "false" ? false : true;
                        break;
                    case MetamodelConstants.CSMPropStereotypeVisible:
                        stereotypeVisible = prop.Value == "false" ? false : true;
                        break;
                    case MetamodelConstants.CSMPropToolboxIconRelPath:
                        fullToolboxIconPath = getFullIconPath(repository, prop.Value);
                        break;
                    case MetamodelConstants.CSMPropCustomShapescript:
                        customShapescript = getCustomShapescript(prop.Value);
                        break;
                }
            }
        
        }

        private string getCustomShapescript(string shapeScript)
        {
            if (shapeScript != null && shapeScript != "!none")
            {
                if (shapeScript.StartsWith("shape main")) return shapeScript;
                //else if (shapeScript.Equals("<memo>" && )
            }
            return null;
        }

        private string getFullIconPath(Repository repository, string relPath)
        {
            string metaModelPath = repository.ConnectionString;
            string fullIconPath = "";

            if (relPath != "")
            {
                string imageName = Path.GetFileNameWithoutExtension(relPath);

                fullIconPath = Path.GetDirectoryName(metaModelPath) + "\\" + relPath;
                fullIconPath = fullIconPath.Replace("/", "\\");
            }

            return fullIconPath;
        }

        public abstract string getShapescript();

        public abstract string getBase64ImageRepresentation();

        public string getBase64TextRepresentation()
        {
            string base64TextRepresentation = "";
            string shapescript = getShapescript();

            string tempPath = Path.GetTempPath();
            System.IO.File.WriteAllBytes(tempPath + "str.dat", Encoding.Unicode.GetBytes(shapescript));

            using (ZipArchive zip = ZipFile.Open(tempPath + "application.zip", ZipArchiveMode.Create))
            {
                zip.CreateEntryFromFile(tempPath + "str.dat", "str.dat");
            }

            byte[] text = System.IO.File.ReadAllBytes(tempPath + "application.zip");
            base64TextRepresentation = Convert.ToBase64String(text);

            System.IO.File.Delete(tempPath + "str.dat");
            System.IO.File.Delete(tempPath + "application.zip");

            return base64TextRepresentation;
        }

        public string getBase64IconRepresentation()
        {
            string base64IconRepresentation = "";

            if (fullToolboxIconPath == "" || !System.IO.File.Exists(fullToolboxIconPath)) return "";


            byte[] imageArray = System.IO.File.ReadAllBytes(fullToolboxIconPath);
            base64IconRepresentation = Convert.ToBase64String(imageArray);

            return base64IconRepresentation;
        }

        public virtual void setDataSetImageXMLElement(ref XmlDocument doc, ref XmlNode image, int id) { }

        public void setStereotypeImageXMLElement(ref XmlDocument doc, ref XmlNode stereotype)
        {
            string base64TextRepresentation = getBase64TextRepresentation();
            if(base64TextRepresentation != "")
            {
                XmlElement imageelem = doc.CreateElement(MetamodelConstants.Image);
                imageelem.SetAttribute("type", "EAShapeScript 1.0");
                imageelem.SetAttribute("xmlns:dt", "urn:schemas-microsoft-com:datatypes");
                imageelem.SetAttribute("dt:dt", "bin.base64");
                imageelem.InnerText = base64TextRepresentation;

                stereotype.AppendChild(imageelem);
            }
        }

        public void setStereotypeIconXMLElement(ref XmlDocument doc, ref XmlNode stereotype)
        {
            string base64IconRepresentation = getBase64IconRepresentation();
            if (base64IconRepresentation != "")
            {
                XmlElement iconeelem = doc.CreateElement(MetamodelConstants.Icon);
                iconeelem.SetAttribute("type", "bitmap");
                iconeelem.SetAttribute("xmlns:dt", "urn:schemas-microsoft-com:datatypes");
                iconeelem.SetAttribute("dt:dt", "bin.base64");
                iconeelem.InnerText = base64IconRepresentation;

                stereotype.AppendChild(iconeelem);
            }
        }

        protected string getFillColorInfo()
        {
            string fillColorInfo = "";
            if (fillColor != null && fillColor != "")
            {
                fillColorInfo = string.Format("setFillColor({0});", fillColor);
            }
            return fillColorInfo;
        }

        protected string getStereotypeInfo()
        {
            string stereotypeInfo = "";
            if(stereotypeVisible)
            {
                stereotypeInfo = string.Format(
                    "addsubshape(\"stereotypecompartment\", \"N\");" +
                    "shape stereotypecompartment" +
                    "{{" +
                        "h_align = \"center\";" +
                        "v_align = \"top\";" +
                        "preferredheight=30;" + 
                        "editablefield = \"stereotype\";" +
                        "println(\"<<#stereotype#>>\");" +
                    "}}"
                    );
            }

            return stereotypeInfo;
        }


    }
}
