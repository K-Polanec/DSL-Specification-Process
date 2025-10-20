namespace Ontomo.Model
{
    /// <summary>
    /// Represents the model for labelling in the Ontomo application using special annotations and object properties.
    /// </summary>
    public static class OntomoLabellingModel
    {
        #region Ontomo annotations
        /// <summary>
        /// Prefix for Ontomo-related annotations used in the labelling model.
        /// </summary>
        public static readonly string OntomoAnnotationPrefix = "ontomo";

        /// <summary>
        /// Separator between annotation prefix and ontology name.
        /// </summary>
        public static readonly string NamespaceSeparator = ":";

        /// <summary>
        /// Annotation for Ontomo indicating the type "view" of an entity.
        /// This means the annotated element repesents a view of the metamodel to be generated, such as a diagram or a specific representation of the model.
        /// <br/>
        /// E.g.: MyEntity -[has annotation]-> ontomo:view = MyLanguage
        /// <br/>
        /// !!! This annotation is replaced by "webprotege:partialItemIRI" in the WebProtege export, where partialItemIRI is the last segment of the IRI of the newly created owl:AnnotationProperty after a "/".
        /// </summary>
        public static readonly string OntomoViewAnnotation = OntomoAnnotationPrefix + NamespaceSeparator + "view";

        /// <summary>
        /// Annotation for Ontomo indicating the type "language" of an entity.
        /// This means the annotated element should be an instantiable language element (entity or relation) part of the metamodel to be generated.
        /// <br/>
        /// E.g.: MyEntity -[has annotation]-> ontomo:language = "MyLanguage"
        /// <br/>
        /// !!! This annotation is replaced by "webprotege:partialItemIRI" in the WebProtege export, where partialItemIRI is the last segment of the IRI of the newly created owl:AnnotationProperty after a "/".
        /// </summary>
        public static readonly string OntomoLanguageAnnotation = OntomoAnnotationPrefix + NamespaceSeparator + "language";

        /// <summary>
        /// Annotation for Ontomo indicating the type "languageAbstract" of an entity.
        /// This means the annotated element should be an abstract language element (entity or relation) part of the metamodel to be generated.
        /// <br/>
        /// E.g.: MyEntity -[has annotation]-> ontomo:languageAbstract = "MyLanguage"
        /// <br/>
        /// !!! This annotation is replaced by "webprotege:partialItemIRI" in the WebProtege export, where partialItemIRI is the last segment of the IRI of the newly created owl:AnnotationProperty after a "/".
        /// </summary>
        public static readonly string OntomoLanguageAbstractAnnotation = OntomoAnnotationPrefix + NamespaceSeparator + "languageAbstract";

        #endregion Ontomo annotations

        #region Ontomo object properties
        /// <summary>
        /// Object property for Ontomo indicating the "featured in" context of an entity.
        /// By assigning it as a relationship property to the entity, it indicates that the entity is typically appearing in the specific view that is specified in the value field of this object property.
        /// <br/>
        /// E.g.: MyEntity -[has relationship]-> featured in = MyView
        /// </summary>
        public static readonly string OntomoFeaturedInProperty = "featured in";

        /// <summary>
        /// Object property for Ontomo indicating the "introduced in" context of an entity.
        /// By assigning it as a relationship property to the entity, it indicates that the entity is typically sourced / created in the specific view that is specified in the value field of this object property.
        /// <br/>
        /// E.g.: MyEntity -[has relationship]-> introduced in = MyView
        /// </summary>
        public static readonly string OntomoIntroducedInProperty = "introduced in";

        #endregion Ontomo object properties
    }
}
