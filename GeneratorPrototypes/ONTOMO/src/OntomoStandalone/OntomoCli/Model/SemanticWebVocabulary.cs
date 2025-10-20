namespace Ontomo.Model
{
    /// <summary>
    /// Semantic Web Vocabulary (SWV) constants.
    /// </summary>
    internal static class SemanticWebVocabulary
    {
        public static readonly string NamespaceSeparator = ":";

        #region RDF
        public static readonly string RdfNamespace = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
        public static readonly string RdfPrefix = "rdfs";
        public static readonly string RdfPrefixWithSeparator = RdfPrefix + NamespaceSeparator;

        public static readonly string RdfAbout = RdfPrefixWithSeparator + "about";
        public static readonly string RdfResource = RdfPrefixWithSeparator + "resource";
        public static readonly string RdfDatatype = RdfPrefixWithSeparator + "datatype";
        #endregion RDF

        #region RDFS
        public static readonly string RdfsNamespace = "http://www.w3.org/2000/01/rdf-schema#";
        public static readonly string RdfsPrefix = "rdfs";
        public static readonly string RdfsPrefixWithSeparator = RdfsPrefix + NamespaceSeparator;

        public static readonly string RdfsLabel = RdfsPrefixWithSeparator + "label";
        public static readonly string RdfsDomain = RdfsPrefixWithSeparator + "domain";
        public static readonly string RdfsRange = RdfsPrefixWithSeparator + "range";
        public static readonly string RdfsSubClassOf = RdfsPrefixWithSeparator + "subClassOf";
        public static readonly string RdfsSubPropertyOf = RdfsPrefixWithSeparator + "subPropertyOf";
        #endregion RDFS

        #region OWL
        public static readonly string OwlNamespace = "http://www.w3.org/2002/07/owl#";
        public static readonly string OwlPrefix = "rdfs";
        public static readonly string OwlPrefixWithSeparator = OwlPrefix + NamespaceSeparator;

        public static readonly string OwlOntology = OwlPrefixWithSeparator + "Ontology";
        public static readonly string OwlAnnotationProperty = OwlPrefixWithSeparator + "AnnotationProperty";
        public static readonly string OwlObjectProperty = OwlPrefixWithSeparator + "ObjectProperty";
        public static readonly string OwlDatatypeProperty = OwlPrefixWithSeparator + "DatatypeProperty";
        public static readonly string OwlClass = OwlPrefixWithSeparator + "Class";
        public static readonly string OwlRestriction = OwlPrefixWithSeparator + "Restriction";
        public static readonly string OwlOnProperty = OwlPrefixWithSeparator + "onProperty";
        public static readonly string OwlSomeValuesFrom = OwlPrefixWithSeparator + "someValuesFrom";
        #endregion OWL

        #region SKOS
        public static readonly string SkosNamespace = "http://www.w3.org/2004/02/skos/core#";
        public static readonly string SkosPrefix = "skos";
        public static readonly string SkosPrefixWithSeparator = SkosPrefix + NamespaceSeparator;

        public static readonly string SkosDefinition = SkosPrefixWithSeparator + "definition";
        public static readonly string SkosPrefLabel = SkosPrefixWithSeparator + "prefLabel";
        #endregion SKOS

        #region WebProtege
        public static readonly string WebProtegeNamespace = "http://webprotege.stanford.edu/";
        public static readonly string WebProtegePrefix = "webprotege";

        #endregion WebProtege
    }
}