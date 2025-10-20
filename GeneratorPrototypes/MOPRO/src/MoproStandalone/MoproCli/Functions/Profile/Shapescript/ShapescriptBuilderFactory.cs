using EA;
using Mopro.Model;

namespace Mopro.Functions.Profile.Shapescript
{
    abstract class ShapescriptBuilderFactory
    {
        public static ShapescriptBuilder getShapescriptBuilder(Repository repository, Dictionary<string, string> csmProperties)
        {
            ShapescriptBuilder scBuilder = null;
            if (csmProperties.ContainsKey(MetamodelConstants.CSMPropImgRelPath))
            {
                scBuilder = new ShapescriptBuilderImage(repository, csmProperties);
            }
            else if (csmProperties.ContainsKey(MetamodelConstants.CSMPropShape))
            {
                scBuilder = new ShapescriptBuilderNativeEntitity(repository, csmProperties);
            }
            else if (csmProperties.ContainsKey(MetamodelConstants.CSMPropLineStyle))
            {
                scBuilder = new ShapescriptBuilderRelationship(repository, csmProperties);
            }

            return scBuilder;
        }

    }
}
