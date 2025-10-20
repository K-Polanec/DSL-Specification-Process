using EA;
using Mopro.Model;

namespace Mopro.Functions.Profile.Shapescript
{
    class ShapescriptBuilderNativeEntitity : ShapescriptBuilderEntity
    {
        private string borderColor;
        private string borderLineStyle = MetamodelConstants.LineStyle.solid.ToString();
        private string borderLineThickness = "1.0";
        private MetamodelConstants.GeometricShape shape = MetamodelConstants.GeometricShape.native;
        public ShapescriptBuilderNativeEntitity(Repository repository, Dictionary<string, string> csmProperties) : base(repository, csmProperties) 
        {
            foreach (KeyValuePair<string, string> prop in csmProperties)
            {
                switch (prop.Key) 
                {
                    case MetamodelConstants.CSMPropBorderColor:
                        borderColor = prop.Value.Replace("(", "").Replace(")", "");
                        break;
                    case MetamodelConstants.CSMPropBorderLineStyle:
                        setBorderLineStyleFromPropValue(prop.Value);
                        break;
                    case MetamodelConstants.CSMPropBorderLineThickness:
                        borderLineThickness = prop.Value;
                        break;
                    case MetamodelConstants.CSMPropShape:
                        setShapeFromPropValue(prop.Value);
                        break;
                    case MetamodelConstants.CSMPropNameAlignmentVertical:
                        setAlignNameVerticalFromPropValue(prop.Value);
                        break;
                }
            }

        }

        public override string getShapescript()
        {
            if (customShapescript != null && customShapescript != "!none")
            {
                return customShapescript;
            }

            string fillColorInfo = getFillColorInfo();

            string nameCompartment = getNameCompartmentInfo(MetamodelConstants.BorderLayoutPositions.CENTER);

            string stereotypeInfo = getStereotypeInfo();

            string shapeInfo = "";
            if (shape == MetamodelConstants.GeometricShape.rectangle)
            {
                shapeInfo = string.Format("rectangle(0,0,100,100);",
                    absolutWidth, absolutHeight);

            }
            else if(shape == MetamodelConstants.GeometricShape.ellipsis) 
            {
                shapeInfo = string.Format("ellipse(0,0,100,100);",
                    absolutWidth, absolutHeight);
            }
            else
            {
                shapeInfo = "drawnativeshape();";
                nameCompartment = "";
                stereotypeInfo = "";
            }

            string penColorInfo = "";
            if(borderColor != "")
            {
                penColorInfo = string.Format("setPenColor({0});", borderColor);
            }


            string shapescript = string.Format(
                "shape main" +
                "{{" +
                    "layouttype= \"border\";" +
                    "v_align= \"CENTER\";" + 
                    "h_align= \"CENTER\";" + 
                    "defSize({0},{1});" +
                    "{2}" +
                    "setlinestyle(\"{3}\");" +
                    "{4}" +
                    "setPenWidth({5});" +
                    "{6}" +
                    "{7}" +
                    "{8}" +
                "}}",
                absolutWidth.ToString(),
                absolutHeight.ToString(),
                fillColorInfo,
                borderLineStyle,
                penColorInfo,
                borderLineThickness,
                shapeInfo,
                stereotypeInfo,
                nameCompartment
                ); ;
            return shapescript;
        }

        public override string getBase64ImageRepresentation()
        {
            return "";
        }

        private void setBorderLineStyleFromPropValue(string propValue)
        {
            if (Enum.IsDefined(typeof(MetamodelConstants.LineStyle), propValue))
            {
                borderLineStyle = propValue;
            }
        }

        private void setShapeFromPropValue(string propValue)
        {
            if (Enum.IsDefined(typeof(MetamodelConstants.GeometricShape), propValue))
            {
                shape = (MetamodelConstants.GeometricShape)Enum.Parse(typeof(MetamodelConstants.GeometricShape), propValue);
            }
        }

        private void setAlignNameVerticalFromPropValue(string propValue)
        {
            if (Enum.IsDefined(typeof(MetamodelConstants.VerticalAlignment), propValue))
            {
                v_align_name = propValue.ToString();
            }
        }

    }
}
