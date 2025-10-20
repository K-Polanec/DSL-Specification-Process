namespace Ontomo.Model.DataObjects
{
    /// <summary>
    /// Represents a relation in an ontology, which can connect different entities.
    /// </summary>
    public class OntologyRelation : OntologyBaseItem
    {
        /// <summary>
        /// Indicates if the relation is abstract.
        /// </summary>
        public bool IsAbstract { get; set; } = false;

        /// <summary>
        /// A list of domains in the ontology to which this relation applies.
        /// This is equal to the type of the source of connectors in the metamodel.
        /// </summary>
        public List<OntologyBaseItem> Domains { get; set; } = new List<OntologyBaseItem>();

        /// <summary>
        /// A list of ranges in the ontology to which this relation applies.
        /// This is equal to the type of the target of connectors in the metamodel.
        /// </summary>
        public List<OntologyBaseItem> Ranges { get; set; } = new List<OntologyBaseItem>();
    }
}
