namespace Ontomo.Model.DataObjects
{
    /// <summary>
    /// Represents an ontology entity-like item.
    /// </summary>
    public class OntologyEntity : OntologyBaseItem
    {
        /// <summary>
        /// Indicates if the relation is abstract.
        /// </summary>
        public bool IsAbstract { get; set; } = false;

        /// <summary>
        /// A list of data attributes that are associated with this ontology entity.
        /// </summary>
        public List<OntologyDataAttribute> DataAttributes { get; set; } = new List<OntologyDataAttribute>();

        /// <summary>
        /// A list of ontology views that this entity is featured in.
        /// This is used to determine which views are relevant for this entity.
        /// </summary>
        public List<OntologyView> FeaturedInList { get; set; } = new List<OntologyView>();

        /// <summary>
        /// A list of ontology views that introduced this entity.
        /// This is used to determine the origin of this entity, where it is usually created or defined.
        /// </summary>
        public List<OntologyView> IntroducedInList { get; set; } = new List<OntologyView>();
    }
}
