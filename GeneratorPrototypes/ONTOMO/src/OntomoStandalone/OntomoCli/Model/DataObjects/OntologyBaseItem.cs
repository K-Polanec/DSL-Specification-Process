namespace Ontomo.Model.DataObjects
{
    /// <summary>
    /// Represents a base item in an ontology, which includes properties not specific to the type of the onotology item such as name, IRI, parents, and childs.
    /// </summary>
    public class OntologyBaseItem
    {
        /// <summary>
        /// The name of the ontology item.
        /// <br/>
        /// Corresponds to the value of the "rdfs:Preflabel = MyName"
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Internationalized Resource Identifier (IRI) used to uniquely identify the ontology item.
        /// <br/>
        /// Corresponds to the value of the "owl:*** rdf:about = MyIRI".
        /// <br/>
        /// Typically, this is a URL or a URI that points to the ontology item for documentation reasons.
        /// </summary>
        public string IRI { get; set; } = "";

        /// <summary>
        /// A description of the ontology items semantics (Meaning, purpose, typical usage, related concepts, etc.).
        /// <br/>
        /// Corresponds to the value of the "skos:definition = MyDescription".
        /// </summary>
        public string Definition { get; set; } = "";

        /// <summary>
        /// A list of parents of the ontology item. Only the direct parents inluded in the specific language are listed here, not the entire hierarchy.
        /// <br/>
        /// Correspond to the values of "rdfs:subClassOf rdf:resource = MyIRI".
        /// </summary>
        public List<OntologyBaseItem> Parents { get; set; } = new List<OntologyBaseItem>();

        /// <summary>
        /// A list of childs of the ontology item. Only the direct childs inluded in the specific language are listed here, not the entire hierarchy.
        /// <br/>
        /// Does not correspond to a specific RDF property, but is inferred from the "rdfs:subClassOf rdf:resource = MyIRI" in reverse direction traversal.
        /// </summary>
        public List<OntologyBaseItem> Childs { get; set; } = new List<OntologyBaseItem>();

        /// <summary>
        /// A unique identifier for the ontology item, derived from the IRI. Structured to fit the Enterprise Architect GUID format.
        /// </summary>
        public string EAGUID
        {
            get
            {
                return Utils.GUID.DeriveEnterpriseArchitectGUID(IRI);
            }
        }




    }
}
