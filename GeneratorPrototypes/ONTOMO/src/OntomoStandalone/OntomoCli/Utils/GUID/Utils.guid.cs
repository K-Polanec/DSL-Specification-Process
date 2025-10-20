using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Ontomo.Utils
{
    public partial class GUID
    {
        /// <summary>
        /// Derives a GUID in the format used by Enterprise Architect from a given input string.
        /// <br/>
        /// E.g. Use an IRI from the ontology as input to derive a GUID that can be used in Enterprise Architect:
        /// https://example.com/myOntology/abcdItem123 -> {XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX}
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string DeriveEnterpriseArchitectGUID(string input)
        {
            using (var md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                Guid guid = new Guid(hash);
                return "{" + guid.ToString().ToUpperInvariant() + "}";
            }
        }
    }
}
