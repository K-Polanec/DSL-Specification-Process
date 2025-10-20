using System;
using System.Text.RegularExpressions;
using System.Xml;
using EA;

namespace EA
{
    public static class EaRepositoryExtensions
    {
        // EA GUID pattern: {XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX}
        private static readonly Regex EaGuidRegex = new Regex(
            @"^\{[0-9A-Fa-f]{8}-([0-9A-Fa-f]{4}-){3}[0-9A-Fa-f]{12}\}$",
            RegexOptions.Compiled);

        public static Diagram? GetDiagramByGuidExtensionMethod(this Repository repository, string diagramGuid)
        {
            if (string.IsNullOrWhiteSpace(diagramGuid))
                return null;

            // Add braces if missing
            if (!diagramGuid.StartsWith('{'))
                diagramGuid = "{" + diagramGuid + "}";

            // Validate GUID format using regex
            if (!EaGuidRegex.IsMatch(diagramGuid))
                return null;

            // Query t_diagram for Diagram_ID
            string sql = $@"
            SELECT Diagram_ID 
            FROM t_diagram 
            WHERE ea_guid = '{diagramGuid}'";

            string xmlResult = repository.SQLQuery(sql);
            if (string.IsNullOrWhiteSpace(xmlResult))
                return null;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlResult);

                var node = xmlDoc.SelectSingleNode("//Diagram_ID");
                if (node != null && int.TryParse(node.InnerText, out int diagramID))
                {
                    return repository.GetDiagramByID(diagramID);
                }
            }
            catch (Exception ex)
            {
                // Optional: log or handle error
                Console.WriteLine($"Error retrieving diagram: {ex.Message}");
            }

            return null;
        }
    }
}