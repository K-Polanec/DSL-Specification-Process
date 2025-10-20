namespace Ontomo.Model.DataObjects
{
    public class OntologyDataAttribute : OntologyBaseItem
    {
        /// <summary>
        /// A list of domains (IRIs) to which this data attribute applies.
        /// This means the types of entities that can have this data attribute.
        /// </summary>
        public List<OntologyBaseItem> Domains { get; set; } = new List<OntologyBaseItem>();

        /// <summary>
        /// The range of the data attribute, which defines the type of value it can hold.
        /// <br/>
        /// E.g. "xsd:string", "xsd:integer", "xsd:boolean", or a specific ontology item IRI.
        /// <br/>As multiple ranges for one data attribute are not supported by this implementation, this is a single string.
        /// </summary>
        public string Range { get; set; } = "";
    }
}
