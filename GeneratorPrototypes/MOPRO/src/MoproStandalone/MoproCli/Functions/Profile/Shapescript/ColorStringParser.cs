using Mopro.Model;
using System.Text.RegularExpressions;

namespace Mopro.Functions.Profile.Shapescript
{
    static class ColorStringParser
    {

        static public string getRgbColorStringForShapescript(string rawColourString)
        {
           if(rawColourString == null ||  rawColourString.Length == 0 || rawColourString == "!none") return "";

           if (rawColourString.StartsWith("(") && rawColourString.EndsWith(")") && rawColourString.Split(',').Length == 3) return rawColourString.Replace("(", "").Replace(")", "");


           if (Regex.IsMatch(rawColourString, @"^\d") && rawColourString.Split(',').Length == 3) return rawColourString;

           foreach(MetamodelConstants.ColorTypes colorType in Enum.GetValues(typeof(MetamodelConstants.ColorTypes)))
           {
                if (rawColourString.StartsWith(colorType.ToString()))
                {
                    int startIndex = rawColourString.IndexOf("(") + 1;
                    int length = rawColourString.IndexOf(")") - startIndex;
                    return rawColourString.Substring(startIndex, length);
                }
                //TODO: parse other colorTypes
           }
 
           return "";
        }
    }
}
