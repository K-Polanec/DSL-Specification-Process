using EA;
using Mopro.Model;

namespace Mopro.Functions.Profile.Shapescript
{
    abstract class ShapescriptBuilderEntity : ShapescriptBuilder
    {
        protected int absolutHeight = 100;
        protected int absolutWidth = 100;
        protected string v_align_name = MetamodelConstants.VerticalAlignment.top.ToString();
        protected string h_align_name = MetamodelConstants.HorizontalAlignment.center.ToString();

        public ShapescriptBuilderEntity(Repository repository, Dictionary<string, string> csmProperties) : base(repository, csmProperties)
        {
            foreach (KeyValuePair<string, string> prop in csmProperties)
            {
                switch (prop.Key)
                {
                    case MetamodelConstants.CSMPropNameAlignmentHorizontal:
                        setAlignNameHorizontalFromPropValue(prop.Value);
                        break;
                    case MetamodelConstants.CSMPropRelativeHeight:
                        setAbsolutHeightFromPropValue(prop.Value);
                        break;
                    case MetamodelConstants.CSMPropRelativeWidth:
                        setAbsolutWidthFromPropValue(prop.Value);
                        break;
                }
            }
        }

        protected string getNameCompartmentInfo(MetamodelConstants.BorderLayoutPositions pos)
        {
            string nameCompartment = "";
            if (showName)
            {
                nameCompartment = string.Format(
                    "addsubshape(\"namecompartment\", \"{0}\");" +
                    "shape namecompartment" +
                    "{{" +
                        "h_align = \"{1}\";" +
                        "v_align = \"{2}\";" +
                        "editablefield = \"name\";" +
                        "println(\"#name#\");" +
                    "}}",
                    //absolutWidth.ToString(),
                    //absolutHeight.ToString(),
                    pos.ToString(),
                    h_align_name,
                    v_align_name
                );
            }
            return nameCompartment;
        }


        private void setAbsolutHeightFromPropValue(string propValue)
        {
            absolutHeight = (int)(absolutHeight * float.Parse(propValue, System.Globalization.CultureInfo.InvariantCulture));
        }

        private void setAbsolutWidthFromPropValue(string propValue)
        {
            absolutWidth = (int)(absolutWidth * float.Parse(propValue, System.Globalization.CultureInfo.InvariantCulture));
        }

        private void setAlignNameHorizontalFromPropValue(string propValue)
        {
            if (Enum.IsDefined(typeof(MetamodelConstants.HorizontalAlignment), propValue))
            {
                h_align_name = propValue.ToString();
            }
        }

    }
}
