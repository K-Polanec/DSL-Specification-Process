namespace Mopro.Model
{
    class MetamodelConstants
    {
        // Stereotypes
        public const string StereotypeMetamodel = "Metamodel";

        // Package Names
        public const string PackageNameASM = "Abstract Syntax Model";
        public const string PackageNameCSM = "Concrete Syntax Model";
        public const string PackageNameUMLElems = "UML4Profile";

        // CSM Elements
        public const string CSMElementName = "CSM Element";

        // CSM Properties
        public const string CSMPropFillColor = "fillColor";
        public const string CSMPropShowName = "showName";
        public const string CSMPropStereotypeVisible = "stereotypeVisible";
        public const string CSMPropToolboxIconRelPath = "toolboxIconRelPath";
        public const string CSMPropNameAlignmentHorizontal = "nameAlignmentHorizontal";
        public const string CSMPropNameAlignmentVertical = "nameAlignmentVertical";
        public const string CSMPropRelativeHeight = "relativeHeight";
        public const string CSMPropRelativeWidth = "relativeWidth";
        public const string CSMPropArrowStyleSource = "arrowStyleSource";
        public const string CSMPropArrowStyleTarget = "arrowStyleTarget";
        public const string CSMPropLineStyle = "lineStyle";
        public const string CSMPropLineThickness = "lineThickness";
        public const string CSMPropRelationDirection = "relationDirection";
        public const string CSMPropImgRelPath = "imgRelPath";
        public const string CSMPropBorderColor = "borderColor";
        public const string CSMPropBorderLineStyle = "borderLineStyle";
        public const string CSMPropBorderLineThickness = "borderLineThickness";
        public const string CSMPropShape = "shape";
        public const string CSMPropCustomShapescript = "customShapescript";

        // CSM Property Enums
        public enum ArrowStyle
        {
            arrow,
            circleCross,
            diamond,
            diamondFilled,
            none,
            triangle,
            triangleFilled
        }

        public enum ColorTypes
        {
            cymk,
            hex,
            hext,
            hsl,
            hsla,
            rgb,
            rgba
        }

        public enum GeometricShape
        {
            ellipsis,
            rectangle,
            native
        }

        public enum ImageFileType
        {
            bmp,
            jpeg,
            jpg,
            png,
            svg
        }

        public enum LineStyle
        {
            dash,
            dot,
            solid,
            dashdot,
            dashdotdot,
            dashdotdotdot,
            dashgap,
            dotgap,
            dashdotgap,
            dashdotdotgap,
            dashdotdotdotgap
        }

        public enum MeasuringUnit
        {
            mm,
            pt,
            px,
            rem
        }

        public enum RelationDirection
        {
            bidirectional,
            unidirectional,
            unspecified
        }

        public static readonly Dictionary<string, string> RelationDirectionStringDict = new Dictionary<string, string>()
        {
            {nameof(RelationDirection.bidirectional),  "Bi-Directional"},
            {nameof(RelationDirection.unidirectional),  "Source -> Destination"},
            {nameof(RelationDirection.unspecified), "Unspecified"}
        };

        public enum HorizontalAlignment
        {
            center,
            left,
            right
        }

        public enum VerticalAlignment
        {
            center,
            top,
            bottom
        }

        public enum BorderLayoutPositions
        {
            N,
            S,
            E,
            W,
            CENTER
        }

        // MDG Structure Names
        public const string UMLProfiles = "UMLProfiles";
        public const string Images = "Images";
        public const string UIToolboxes = "UIToolboxes";
        public const string DiagramProfile = "DiagramProfile";
        public const string Documentation = "Documentation";
        public const string Stereotypes = "Stereotypes";
        public const string Stereotype = "Stereotype";
        public const string Image = "Image";
        public const string Icon = "Icon";
        public const string AppliesTo = "AppliesTo";
        public const string Apply = "Apply";
        public const string Property = "Property";
        public const string Metaconstraints = "metaconstraints";
        public const string Metaconstraint = "metaconstraint";
        public const string TaggedValues = "TaggedValues";
        public const string Tag = "Tag";
        public const string DataRow = "DataRow";
        public const string Column = "Column";

        public const string Generalizes = "generalizes";
        public const string BaseStereotypes = "baseStereotypes";

        public enum UmlRoles
        {
            source,
            target,
            type
        };

        public static List<string> umlRoles = new List<string>
        {
            "source",
            "target",
            "type"
        };
    }
}
