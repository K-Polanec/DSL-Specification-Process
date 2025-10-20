using CommandLine;

namespace Ontomo.CLI
{
    /// <summary>
    /// Represents the command-line options for the Ontomo CLI application.
    /// </summary>
    public class CliOptions
    {
        [Option('f', "ontology-file", Required = true, HelpText = "Path to the ontology file in RDF/XML format.")]
        public string? OntologyFile { get; set; }

        [Option('o', "output-file", Required = false, HelpText = "Path to save the generated metamodeling project as .eapx Enterprise Architect modeling project.")]
        public string? OutputFile { get; set; } = "generatedMetamodel.eapx";

        [Option('l', "language-name", Required = true, HelpText = "Name of the language; used to extract certainly-tagged elements from ontology.")]
        public string? LanguageName { get; set; } = "";

        [Option("log-level", Required = false, HelpText = "Set the log level (e.g., Info, Warn, Error).")]
        public string? LogLevel { get; set; }

        [Option("log-file", Required = false, HelpText = "Path to the log file.")]
        public string? LogFile { get; set; }

        [Option("non-interactive", Required = false, HelpText = "Run in non-interactive mode.")]
        public bool NonInteractive { get; set; } = false;
    }
}
