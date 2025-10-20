using EA;
using Mopro.Model;

namespace Mopro.Functions.Profile.Shapescript
{
    class ShapescriptBuilderRelationship : ShapescriptBuilder
    {
        private string arrowStyleSource = MetamodelConstants.ArrowStyle.none.ToString();
        private string arrowStyleTarget = MetamodelConstants.ArrowStyle.none.ToString();
        private string lineStyle = MetamodelConstants.LineStyle.solid.ToString();
        private string lineThickness = "1.0";
        
        // unused property: private string relationDirection;

        public ShapescriptBuilderRelationship(Repository repository, Dictionary<string, string> csmProperties) : base(repository, csmProperties) 
        {
            foreach (KeyValuePair<string, string> prop in csmProperties)
            {
                switch (prop.Key)
                {
                    case MetamodelConstants.CSMPropArrowStyleSource:
                        setArrowStyleSourceFromPropValue(prop.Value);
                        break;
                    case MetamodelConstants.CSMPropArrowStyleTarget:
                        setArrowStyleTargetFromPropValue(prop.Value);
                        break;
                    case MetamodelConstants.CSMPropLineStyle:
                        setLineStyleFromPropValue(prop.Value);
                        break;
                    case MetamodelConstants.CSMPropLineThickness:
                        lineThickness = prop.Value;
                        break;
                }
            }
        }

        public override string getShapescript()
        {
            if (customShapescript != null && customShapescript != "!none") return customShapescript;

            string penColorInfo = getPenColorInfo();
            string stereotypeInfo = getStereotypeInfo();

            string sourceInfo = getShapeSourceInfo();
            string targetInfo = getShapeTargetInfo();
            string shapescript = string.Format(
                "shape main" +
                "{{" +
                    "layouttype= \"border\";" +
                    "setlinestyle(\"{0}\");" +
                    "setpenwidth(\"{1}\");" +
                    "{2}" +
                    "moveto(0,0);" +
                    "lineto(100,0);" + 
                    "{3}" +
                "}}" +
                "{4}" +
                "{5}" +
                // necessary to suppress default stereotype label (no better solution found)
                "label middlebottomlabel" +
                "{{" +
                    "println(\"\");" +
                "}}",
                lineStyle,
                lineThickness,
                penColorInfo,
                stereotypeInfo,
                sourceInfo,
                targetInfo);

            return shapescript;
        }

        public override string getBase64ImageRepresentation()
        {
            return "";
        }

        private string getPenColorInfo()
        {
            string penColorInfo = "";
            if (fillColor != null && fillColor != "")
            {
                penColorInfo = string.Format("setPenColor({0});", fillColor);
            }
            return penColorInfo;
        }

        private string getShapeSourceInfo()
        {
            string sourceArrowInfo = getArrowStyleShapeInfo(arrowStyleSource);
            string targetArrowInfo = getArrowStyleShapeInfo(arrowStyleTarget);
            string shapeSourceInfo = string.Format(
                "shape source" +
                "{{" +
                    // draw source arrow shape at the source
                    "if(HasProperty(\"Direction\", \"Source -> Destination\")) {{" +
                        "{0}" +
                    // draw target arrow shape at the source
                    "}} else if(HasProperty(\"Direction\", \"Destination -> Source\")) {{" +
                        "{1}" +
                    "}} else if(HasProperty(\"Direction\", \"Bi-Directional\")) {{" +
                        "{1}" +
                    "}}" +
                "}}",
                sourceArrowInfo,
                targetArrowInfo);
            return shapeSourceInfo;
        }

        private string getShapeTargetInfo()
        {
            string sourceArrowInfo = getArrowStyleShapeInfo(arrowStyleSource);
            string targetArrowInfo = getArrowStyleShapeInfo(arrowStyleTarget);
            string shapeTargetInfo = string.Format(
                "shape target" +
                "{{" +
                    // draw source arrow shape at the target
                    "if(HasProperty(\"Direction\", \"Destination -> Source\")) {{" +
                        "{0}" +
                    // draw target arrow shape at the target
                    "}} else if(HasProperty(\"Direction\", \"Source -> Destination\")) {{" +
                        "{1}" +
                    "}} else if(HasProperty(\"Direction\", \"Bi-Directional\")) {{" +
                        "{1}" +
                    "}}" +
                "}}",
                sourceArrowInfo,
                targetArrowInfo);
            return shapeTargetInfo;
        }

        private string getArrowStyleShapeInfo(string arrowStyle)
        {
            string arrowStyleShapeInfo = string.Format(
                "{0}" +
                "setpenwidth(\"{1}\");" +
                "startpath();", 
                getPenColorInfo(),
                lineThickness);

            switch (arrowStyle)
            {
                case nameof(MetamodelConstants.ArrowStyle.arrow):
                    arrowStyleShapeInfo = arrowStyleShapeInfo +
                        getArrowShape();
                    break;
                case nameof(MetamodelConstants.ArrowStyle.circleCross):
                    arrowStyleShapeInfo = arrowStyleShapeInfo +
                        getCircleCrossShape();
                    break;
                case nameof(MetamodelConstants.ArrowStyle.diamond):
                    arrowStyleShapeInfo = arrowStyleShapeInfo +
                        getDiamondShape();
                    break;
                case nameof(MetamodelConstants.ArrowStyle.diamondFilled):
                    arrowStyleShapeInfo = arrowStyleShapeInfo +
                        getDiamondShape() +
                        getFillColorInfo();
                    break;
                case nameof(MetamodelConstants.ArrowStyle.none):
                    break;
                case nameof(MetamodelConstants.ArrowStyle.triangle):
                    arrowStyleShapeInfo = arrowStyleShapeInfo +
                        getTriangleShape();
                    break;
                case nameof(MetamodelConstants.ArrowStyle.triangleFilled):
                    arrowStyleShapeInfo = arrowStyleShapeInfo +
                        getTriangleShape() +
                        getFillColorInfo();
                    break;
            }

            arrowStyleShapeInfo = arrowStyleShapeInfo + 
                "endpath();" +
                "fillandstrokepath();";

            return arrowStyleShapeInfo;
        }

        private string getTriangleShape()
        {
            return "moveto(0,0);" +
                    "lineto(16,6);" +
                    "lineto(16,-6);";
        }

        private string getArrowShape()
        {
            return "moveto(0, 0);" +
                    "lineto(16, 6);" +
                    "moveto(0, 0);" +
                    "lineto(16, -6);";
        }

        private string getDiamondShape()
        {
            return "moveto(0, 0);" +
                    "lineto(12, 6);" +
                    "lineto(24, 0);" +
                    "lineto(12, -6);" +
                    "lineto(0, 0);";
        }

        private string getCircleCrossShape()
        {
            return "ellipse(0,6,12,-6);" +
                    "moveto(6,6);" +
                    "lineto(6,-6);" +
                    "moveto(0,0);" +
                    "lineto(12,0);";
        }

        private void setArrowStyleSourceFromPropValue(string propValue)
        {
            if (Enum.IsDefined(typeof(MetamodelConstants.ArrowStyle), propValue))
            {
                arrowStyleSource = propValue;
            }
        }

        private void setArrowStyleTargetFromPropValue(string propValue)
        {
            if (Enum.IsDefined(typeof(MetamodelConstants.ArrowStyle), propValue))
            {
                arrowStyleTarget = propValue;
            }
        }

        private void setLineStyleFromPropValue(string propValue)
        {
            if (Enum.IsDefined(typeof(MetamodelConstants.LineStyle), propValue))
            {
                lineStyle = propValue;
            }
        }
    }
}
