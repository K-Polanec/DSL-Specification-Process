using Ontomo.Utils.Logging;
using Ontomo.CLI;
using Ontomo.Model.DataObjects;
using EA;

namespace Ontomo
{
    /// <summary>
    /// Static class for holding static methods and properties related to the Ontomo CLI.
    /// </summary>
    public static class Static
    {
        // Logging
        public static Logger Logger { get; set; } = new Logger(); // Logger instance for logging messages. To be initialized and parametrized on program startup.

        // CliOptions
        public static CliOptions? Options { get; set; } = new CliOptions(); // CLI options for the Ontomo application. To be initialized and parametrized on program startup.

        public static string LanguageName { get => Options?.LanguageName != null ? Options.LanguageName : ""; }

        public static string InputFilePath { get => Options?.OntologyFile != null ? Path.GetFullPath(Options.OntologyFile) : ""; }

        public static string OutputFilePath { get => Options?.OutputFile != null ? Path.GetFullPath(Options.OutputFile) : ""; }

        public static string XMITemplatePath { get => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resource", "MOPROTemplate.xml"); }

        public static string EATemplatePath { get => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resource", "MOPROTemplate.eapx"); }

        // Extracted Ontology Items to handover from RDFImportHandler to the EAExportHandler
        public static List<OntologyBaseItem> ExtractedOntologyItems { get; set; } = new List<OntologyBaseItem>();

        public static string? OntomoModelGUID { get; set; }

        // Cleanup connection to EA.exe and forcefully close the repository on failure
        private static Repository? _exitRepositoryReference;

        public static Repository? ExitRepositoryReference
        {
            get => _exitRepositoryReference;
            set
            {
                if (_exitRepositoryReference != null)
                {
                    _exitRepositoryReference.CloseFile();
                    _exitRepositoryReference.Exit();
                }
                _exitRepositoryReference = value;
            }
        }

        private static bool _cleanedUp = false;

        public static bool CleanedUp
        {
            get => _cleanedUp;
            set
            {
                if (value)
                {
                    _cleanedUp = true;
                }
                else
                {
                    _cleanedUp = false;
                }
            }
        }
    }
}
