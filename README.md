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

---
## Required Tools
The following tools are required or recommended to reproduce the transformations:

- Sparx Systems Enterprise Architect
- .NET / C# runtime
- OWL/RDF-compatible ontology editor, e.g., WebProtégé or Protégé
- SysML v2-compatible environment, e.g., JupyterLab with SysML v2 support

## Executing the Transformation Process
The transformations can be reproduced as follows:

1. Use the provided ontology artifact as input for ONTOMO.
2. Run ONTOMO to generate the corresponding MOF/UML metamodel structure.
3. Manually complete information that cannot be represented in the ontology, especially detailed ASM and CSM information.
4. Use the completed metamodel as input for MOPRO to generate the UML profile / MDG Technology implementation.
5. Alternatively, use the completed metamodel as input for MOSys to generate a SysML v2 library.
6. Compare the generated outputs with the provided reference artifacts.

## Running the Prototypes

The prototype executables are not included in this repository. Instead, the prototypes have to be built and executed in a .NET environment from the provided source code.

Each prototype is implemented as a command-line application. The available parameters can be displayed with ```--help```. ONTOMO for instance can be executed with a command like this:

```powershell
PS> ./OntomoCli.exe --ontology-file myOntology.ttl --language-name "FRTI" --output-file ./generatedMetamodel.eapx
```
Be aware that you need to have Enterprise Architect installed and licensed on the machine where you run ontomo, as ontomo uses the Enterprise Architect automation interface to create and populate the metamodel.

For MOSys for instance, those would be the available command line options: <img width="711" height="271" alt="image" src="https://github.com/user-attachments/assets/b4f06dc3-8ccc-455d-90e9-5de65460ee16" />

MOSys then creates an output folder in the working directory and saves the generated file in this folder. 


