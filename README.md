# DSL-Specification-Process
A unified, standards-based process for specifying, transforming, and implementing domain-specific languages — from ontology to metamodel to modeling tool.

This repository contains the concrete artifacts and prototype source code developed in the context of a standardized language specification approach for graphical domain-specific languages (DSLs) in model-based systems engineering (MBSE). It includes the ARAM development artifacts (ontology, metamodel, and implementation) as well as corresponding application artifacts from the cybersecurity domain, alongside the source code of supporting prototypes used to create and transform these artifacts.

## Application Artefacts
This folder contains the ontology and metamodel of the cybersecurity DSL. Both artefacts were developed by following the finalized *DSL Specification Process*. 
The ontology is represented by a turtle file which was created using *Web Protégé*. The metamodel is a MOF-conformant metamodel which only uses UML elements. The metamodel artifact itself was created using *Sparx Systems Enterprise Architect*. The folder contains the metamodel as an XMI file as well as an HTML export of the Enterprise Architect model. Since the implementation of the DSL is automatically generated only as an XMI file, no ther is no Enterprise Architect model and consequently no HTML export of the UML profile itself. Information about the cybersecurity DSL and toolbox can be found [here](https://dsse.at/syseng/cybersecurity-toolbox/).

## Development Artefacts
This folder contains the ontology, metamodel, and implementation of the ARAM DSL. These artefacts were developed during the development of the *DSL Specification Process*. 
The ontology is represented by a turtle file which was created using *Web Protégé*. The metamodel is a MOF-conformant metamodel which only uses UML elements. The metamodel artifact itself was created using *Sparx Systems Enterprise Architect*. The folder contains the metamodel as an XMI file as well as an HTML export of the Enterprise Architect model. The implementation was realized as a UML profile and implemented through the MDG Technology of Enterprise Architect. The folder contains an XMI export of the MDG Technology as well as an HTML export of the Enterprise Architect model in which the UML profile is specified. Information about the ARAM DSL and toolbox can be found [here](https://dsse.at/aram/).

## Generator Prototypes
This folder includes the source code of the two generator prototypes (1) the MOF-based Profile Generator (MOPRO) and (2) the Ontology-to-MOF Transformer (ONTOMO).
