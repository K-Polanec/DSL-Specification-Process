using Ontomo.Utils.Logging;
using VDS.RDF;
using Ontomo.Model.DataObjects;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace Ontomo.Functions.ImportRDF
{
    public class RdfImporterSingleClass
    {
        private readonly Logger _logger;
        private readonly IGraph _graph;
        private readonly string _languageName;

        private readonly List<OntologyBaseItem> _extractedItems = new List<OntologyBaseItem>();

        public RdfImporterSingleClass(string rdfInputFile, string languageName = "", Logger? customLogger = null)
        {
            _logger = customLogger ?? Static.Logger;
            _languageName = languageName != "" ? languageName : Static.LanguageName;

            _graph = new VDS.RDF.Graph();
            FileLoader.Load(_graph, rdfInputFile);
        }

        public List<OntologyBaseItem> ExtractOntologyItems()
        {
            _logger.LogInfo("Extracting ontology items from RDF graph...");

            #region Extract ontomo:view elements  
            string sparqlViewExtraction = @"  
        PREFIX ontomo: <http://dsse.at/ontologies/ontomo#>  
        PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>  
        PREFIX owl: <http://www.w3.org/2002/07/owl#>  
        PREFIX skos: <http://www.w3.org/2004/02/skos/core#>  

        SELECT ?class ?value ?definition ?name  
        WHERE {  
           ?class a owl:Class ;  
               ontomo:view ?value .  
           OPTIONAL { ?class skos:definition ?definition . }  
           OPTIONAL { ?class skos:prefLabel ?name . }  
           FILTER(REGEX(STR(?value), ""(^|,\\s*)$$languageName$$(\\s*,|$)"", ""i""))  
        }"
               .Replace("$$languageName$$", _languageName);

            if (_graph.ExecuteQuery(sparqlViewExtraction) is not SparqlResultSet results)
            {
                _logger.LogWarning("No results found for ontomo:view extraction query. Quitting further extraction process.");
                return _extractedItems;
            }

            foreach (var result in results)
            {
                if (result["class"] is not IUriNode classNode)
                    continue;

                string classIri = classNode.Uri.ToString();
                string definition = result.TryGetValue("definition", out var defNode) && defNode is ILiteralNode defLiteral ? defLiteral.Value : "";
                string name = result.TryGetValue("name", out var nameNode) && nameNode is ILiteralNode nameLiteral ? nameLiteral.Value : "";

                OntologyView view = new OntologyView
                {
                    IRI = classIri,
                    Name = name,
                    Definition = definition
                };

                _extractedItems.Add(view);
            }
            #endregion Extract ontomo:view elements  
            _logger.LogInfo($"Extracted {_extractedItems.OfType<OntologyView>().Count()} ontomo:view items from RDF graph.");

            #region Extract ontomo:language data attributes  
            string sparql = @"  
        PREFIX ontomo: <http://dsse.at/ontologies/ontomo#>  
        PREFIX webprotege: <http://webprotege.stanford.edu/>  
        PREFIX owl: <http://www.w3.org/2002/07/owl#>  
        PREFIX skos: <http://www.w3.org/2004/02/skos/core#>  
        PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>  

        SELECT ?property ?value ?definition ?name ?domain ?range  
        WHERE {  
           ?property a owl:DatatypeProperty ;  
                     ontomo:language ?value .  
           OPTIONAL { ?property skos:definition ?definition . }  
           OPTIONAL { ?property skos:prefLabel ?name . }  
           OPTIONAL { ?property rdfs:domain ?domain . }  
           OPTIONAL { ?property rdfs:range ?range . }  

           FILTER(REGEX(STR(?value), ""(^|,\\s*)$$languageName$$(\\s*,|$)"", ""i""))  
        }"
            .Replace("$$languageName$$", _languageName);

            if (_graph.ExecuteQuery(sparql) is SparqlResultSet resultsDataAttr)
            {
                var grouped = resultsDataAttr.GroupBy(r => ((IUriNode)r["property"]).Uri.ToString());

                foreach (var group in grouped)
                {
                    var first = group.First();
                    string iri = group.Key;

                    string name = first.TryGetValue("name", out var nameNode) && nameNode is ILiteralNode nameLiteral
                        ? nameLiteral.Value
                        : "";

                    string definition = first.TryGetValue("definition", out var defNode) && defNode is ILiteralNode defLiteral
                        ? defLiteral.Value
                        : "";

                    string range = first.TryGetValue("range", out var rangeNode) && rangeNode is IUriNode rangeUri
                        ? rangeUri.Uri.ToString()
                        : "";

                    // Collect all domains (may be multiple)  
                    var domains = group
                        .Where(r => r.TryGetValue("domain", out var d) && d is IUriNode)
                        .Select(r =>
                        {
                            var uri = ((IUriNode)r["domain"]).Uri;
                            return new OntologyBaseItem
                            {
                                IRI = uri.ToString(),
                                Name = GetLabel(_graph, uri) // Optional: fetch prefLabel from graph  
                            };
                        })
                        .DistinctBy(d => d.IRI)
                        .ToList();

                    var attr = new OntologyDataAttribute
                    {
                        IRI = iri,
                        Name = name,
                        Definition = definition,
                        Range = range,
                        Domains = domains
                    };

                    _extractedItems.Add(attr);
                }
            }
            #endregion Extract ontomo:language data attributes  
            _logger.LogInfo($"Extracted {_extractedItems.OfType<OntologyDataAttribute>().Count()} ontomo:language data attributes from RDF graph.");

            #region Extract ontomo:language / ontomo:languageAbstract elements  

            string sparqlElems = @"  
        PREFIX ontomo: <http://dsse.at/ontologies/ontomo#>  
        PREFIX webprotege: <http://webprotege.stanford.edu/>  
        PREFIX owl: <http://www.w3.org/2002/07/owl#>  
        PREFIX skos: <http://www.w3.org/2004/02/skos/core#>  

        SELECT DISTINCT ?class ?langVal ?abstractVal ?definition ?name  
        WHERE {  
           ?class a owl:Class .  

           OPTIONAL { ?class ontomo:language ?langVal . }  
           OPTIONAL { ?class ontomo:languageAbstract ?abstractVal . }  

           OPTIONAL { ?class skos:definition ?definition . }  
           OPTIONAL { ?class skos:prefLabel ?name . }  

           FILTER(  
               (BOUND(?langVal) && REGEX(STR(?langVal), ""(^|,\\s*)$$languageName$$(\\s*,|$)"", ""i"")) ||  
               (BOUND(?abstractVal) && REGEX(STR(?abstractVal), ""(^|,\\s*)$$languageName$$(\\s*,|$)"", ""i""))  
           )  
        }"
            .Replace("$$languageName$$", _languageName);

            if (_graph.ExecuteQuery(sparqlElems) is SparqlResultSet resultsElem)
            {
                HashSet<string> seen = new();

                foreach (var result in resultsElem)
                {
                    if (result["class"] is not IUriNode classNode)
                        continue;

                    string iri = classNode.Uri.ToString();
                    if (seen.Contains(iri))
                        continue;
                    seen.Add(iri);

                    string definition = result.TryGetValue("definition", out var defNode) && defNode is ILiteralNode defLiteral
                        ? defLiteral.Value
                        : "";

                    string name = result.TryGetValue("name", out var nameNode) && nameNode is ILiteralNode nameLiteral
                        ? nameLiteral.Value
                        : "";

                    bool hasLanguage = result.TryGetValue("langVal", out var langNode) && langNode is ILiteralNode;
                    bool hasAbstract = result.TryGetValue("abstractVal", out var absNode) && absNode is ILiteralNode;

                    bool isAbstract = hasAbstract && !hasLanguage;

                    _extractedItems.Add(new OntologyEntity
                    {
                        IRI = iri,
                        Name = name,
                        Definition = definition,
                        IsAbstract = isAbstract
                    });
                }
            }
            #endregion Extract ontomo:language / ontomo:languageAbstract elements
            _logger.LogInfo($"Extracted {_extractedItems.OfType<OntologyEntity>().Count()} ontomo:language / ontomo:languageAbstract elements from RDF graph.");

            #region Extract ontomo:language / ontomo:languageAbstract relations
            string sparqlRelations = @"
PREFIX ontomo: <http://dsse.at/ontologies/ontomo#>
PREFIX owl: <http://www.w3.org/2002/07/owl#>
PREFIX skos: <http://www.w3.org/2004/02/skos/core#>

SELECT DISTINCT ?property ?value ?definition ?name ?hasLang ?hasLangAbstract
WHERE {
    ?property a owl:ObjectProperty .

    OPTIONAL { ?property ontomo:language ?hasLang . }
    OPTIONAL { ?property ontomo:languageAbstract ?hasLangAbstract . }

    OPTIONAL { ?property skos:definition ?definition . }
    OPTIONAL { ?property skos:prefLabel ?name . }

    FILTER (
        (BOUND(?hasLang) && REGEX(STR(?hasLang), '(^|,\\s*)" + _languageName + @"(\\s*,|$)', 'i')) ||
        (BOUND(?hasLangAbstract) && REGEX(STR(?hasLangAbstract), '(^|,\\s*)" + _languageName + @"(\\s*,|$)', 'i'))
    )
}";

            if (_graph.ExecuteQuery(sparqlRelations) is SparqlResultSet resultsRelations)
            {

                HashSet<string> seenRel = new();

                foreach (var result in resultsRelations)
                {
                    if (result["property"] is not IUriNode propertyNode)
                        continue;

                    string iri = propertyNode.Uri.ToString();

                    if (seenRel.Contains(iri))
                        continue;

                    seenRel.Add(iri);

                    string definition = result.TryGetValue("definition", out var defNode) && defNode is ILiteralNode defLiteral ? defLiteral.Value : "";
                    string name = result.TryGetValue("name", out var nameNode) && nameNode is ILiteralNode nameLiteral ? nameLiteral.Value : "";

                    bool hasLanguage = result.TryGetValue("hasLang", out var _) && !string.IsNullOrWhiteSpace(result["hasLang"].ToString());
                    bool hasAbstract = result.TryGetValue("hasLangAbstract", out var _) && !string.IsNullOrWhiteSpace(result["hasLangAbstract"].ToString());

                    bool isAbstract = hasAbstract && !hasLanguage;

                    _extractedItems.Add(new OntologyRelation
                    {
                        IRI = iri,
                        Name = name,
                        Definition = definition,
                        IsAbstract = isAbstract
                    });
                }
            }
            #endregion Extract ontomo:language / ontomo:languageAbstract relations
            _logger.LogInfo($"Extracted {_extractedItems.OfType<OntologyRelation>().Count()} ontomo:language / ontomo:languageAbstract elements from RDF graph.");

            _logger.LogInfo($"Extracted in sum {_extractedItems.Count} ontology items from RDF graph.");

            #region Add featuredIn / introducedIn views to entities
            foreach (var ontoEntity in _extractedItems.OfType<OntologyEntity>())
            {
                string entityIri = ontoEntity.IRI;
                string ontomoFeaturedInIRI = "http://dsse.at/ontologies/ontomo#featuredIn";
                string ontomoIntroducedInIRI = "http://dsse.at/ontologies/ontomo#introducedIn";

                string sparqlEntityViews = $@"
PREFIX owl: <http://www.w3.org/2002/07/owl#>
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT DISTINCT ?property ?range
WHERE {{
    <{entityIri}> rdfs:subClassOf ?restriction .

    ?restriction owl:onProperty ?property ;
                 owl:someValuesFrom ?range .

    FILTER (?property IN (<{ontomoFeaturedInIRI}>, <{ontomoIntroducedInIRI}>))
}}";


                if (_graph.ExecuteQuery(sparqlEntityViews) is SparqlResultSet resultsEV)
                {
                    foreach (var r in resultsEV)
                    {
                        if (r["property"] is IUriNode propNode && r["range"] is IUriNode rangeNode)
                        {
                            var propIri = propNode.Uri.ToString();
                            var rangeIri = rangeNode.Uri.ToString();


                            if (propIri == ontomoFeaturedInIRI)
                            {
                                // Add to FeaturedInList
                                OntologyView? view = _extractedItems.OfType<OntologyView>().FirstOrDefault(v => v.IRI == rangeIri);
                                if (view != null)
                                {
                                    ontoEntity.FeaturedInList.Add(view);
                                }
                            }
                            else if (propIri == ontomoIntroducedInIRI)
                            {
                                // Add to IntroducedInList
                                OntologyView? view = _extractedItems.OfType<OntologyView>().FirstOrDefault(v => v.IRI == rangeIri);
                                if (view != null)
                                {
                                    ontoEntity.IntroducedInList.Add(view);
                                }

                            }
                        }
                    }
                }
            }

            #endregion Add featuredIn / introducedIn views to entities

            #region Add Inheritances in Parents / Childs lists to entities
            foreach (var ontoItem in _extractedItems.Where(i => i is OntologyEntity || i is OntologyRelation))
            {
                string iriEntityRelation = ontoItem.IRI;

                string sparqlParentClasses = $@"
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
PREFIX owl: <http://www.w3.org/2002/07/owl#>

SELECT DISTINCT ?parent
WHERE {{
    <{iriEntityRelation}> rdfs:subClassOf ?parent .
    FILTER(isIRI(?parent))
}}";
                if (_graph.ExecuteQuery(sparqlParentClasses) is SparqlResultSet resultsParents)
                {
                    foreach (var result in resultsParents)
                    {
                        if (result["parent"] is IUriNode parentNode)
                        {
                            string parentIri = parentNode.Uri.ToString();

                            var parentItem = _extractedItems
                                .Where(i => i is OntologyEntity || i is OntologyRelation)
                                .FirstOrDefault(i => i.IRI == parentIri);

                            if (parentItem != null)
                            {
                                // Assuming you have a Parents or SuperClasses list
                                if (ontoItem is OntologyEntity oe)
                                {
                                    oe.Parents.Add(parentItem);
                                }
                                else if (ontoItem is OntologyRelation orl)
                                {
                                    orl.Parents.Add(parentItem);
                                }
                            }
                        }
                    }
                }
            }

            // Now populate Childs based on Parents
            foreach (var ontoItem in _extractedItems.Where(i => i is OntologyEntity || i is OntologyRelation))
            {
                var parents = (ontoItem as dynamic).Parents;

                foreach (var parent in parents)
                {
                    if (parent is OntologyEntity parentEntity)
                    {
                        if (!parentEntity.Childs.Contains(ontoItem))
                            parentEntity.Childs.Add(ontoItem);
                    }
                    else if (parent is OntologyRelation parentRelation && !parentRelation.Childs.Contains(ontoItem))
                    {
                        parentRelation.Childs.Add(ontoItem);
                    }
                }
            }
            #endregion Add Inheritances in Parents / Childs lists to entities

            #region Add dataAttributes to entities
            foreach (var dataAttr in _extractedItems.OfType<OntologyDataAttribute>())
            {
                string attrIri = dataAttr.IRI;

                string sparqlDomainQuery = $@"
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT DISTINCT ?domain
WHERE {{
    <{attrIri}> rdfs:domain ?domain .
}}";

                if (_graph.ExecuteQuery(sparqlDomainQuery) is SparqlResultSet resultsDataAtt)
                {
                    foreach (var result in resultsDataAtt)
                    {
                        if (result["domain"] is IUriNode domainNode)
                        {
                            string domainIri = domainNode.Uri.ToString();

                            var domainEntity = _extractedItems
                                .OfType<OntologyEntity>()
                                .FirstOrDefault(e => e.IRI == domainIri);

                            if (domainEntity != null && !domainEntity.DataAttributes.Contains(dataAttr))
                            {
                                domainEntity.DataAttributes.Add(dataAttr);
                            }
                        }
                    }
                }
            }
            #endregion Add dataAttributes to entities

            #region Add Domains / Ranges to Relations
            foreach (var relation in _extractedItems.OfType<OntologyRelation>())
            {
                string relationIri = relation.IRI;

                string sparqlRelationQuery = $@"
PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>

SELECT DISTINCT ?domain ?range
WHERE {{
    OPTIONAL {{ <{relationIri}> rdfs:domain ?domain . }}
    OPTIONAL {{ <{relationIri}> rdfs:range ?range . }}
}}";

                if (_graph.ExecuteQuery(sparqlRelationQuery) is SparqlResultSet resultsRelDomRange)
                {
                    foreach (var result in resultsRelDomRange)
                    {
                        // Handle domain
                        if (result.TryGetValue("domain", out var domainNode) && domainNode is IUriNode domainUriNode)
                        {
                            string domainIri = domainUriNode.Uri.ToString();

                            var domainEntity = _extractedItems
                                .OfType<OntologyEntity>()
                                .FirstOrDefault(e => e.IRI == domainIri);

                            if (domainEntity != null && !relation.Domains.Contains(domainEntity))
                            {
                                relation.Domains.Add(domainEntity);
                            }
                        }

                        // Handle range
                        if (result.TryGetValue("range", out var rangeNode) && rangeNode is IUriNode rangeUriNode)
                        {
                            string rangeIri = rangeUriNode.Uri.ToString();

                            var rangeEntity = _extractedItems
                                .OfType<OntologyEntity>()
                                .FirstOrDefault(e => e.IRI == rangeIri);

                            if (rangeEntity != null && !relation.Ranges.Contains(rangeEntity))
                            {
                                relation.Ranges.Add(rangeEntity);
                            }
                        }
                    }
                }
            }
            #endregion Add Domains / Ranges to Relations

            #region Correct tab-issue in definitions
            foreach (var item in _extractedItems)
            {
                var defProp = item.GetType().GetProperty("Definition");
                if (defProp != null && defProp.PropertyType == typeof(string))
                {
                    string? definition = (string?)defProp.GetValue(item);
                    if (!string.IsNullOrEmpty(definition) && definition.Contains("\t"))
                    {
                        // Replace three tabs with linefeed, then remove remaining tabs
                        definition = definition.Replace("\t\t\t", "\n")
                                               .Replace("\t", "");

                        defProp.SetValue(item, definition);
                    }
                }
            }
            #endregion Correct tab-issue in definitions

            return _extractedItems;
        }

        private static string GetLabel(IGraph g, Uri uri)
        {
            var node = g.CreateUriNode(uri);
            var labelTriple = g.GetTriplesWithSubjectPredicate(node, g.CreateUriNode("skos:prefLabel")).FirstOrDefault();

            if (labelTriple?.Object is ILiteralNode labelNode)
                return labelNode.Value;

            return uri.Fragment.TrimStart('#');
        }
    }
}