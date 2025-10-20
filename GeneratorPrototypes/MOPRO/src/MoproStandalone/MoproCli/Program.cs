using CommandLine;
using EA;
using Mopro.Logic;
using Mopro.Utils.Logging;

namespace Mopro
{
    internal class Program
    {

        public class CliOptions
        {
            [Option('f', "metamodel-file", Required = true, HelpText = "Path to the .eap/.eapx/.qea/.qeax metamodel file.")]
            public string? MetamodelFile { get; set; }

            [Option('o', "output-file", Required = false, HelpText = "Path to save the generated mdg profile xml-file.")]
            public string? OutputFile { get; set; } = "";

            [Option('p', "profile-package", Required = false, HelpText = "Name of the profile package from which the implemented profile should be built.")]
            public string? ProfilePackage { get; set; } = "";

            [Option('l', "log-level", Required = false, HelpText = "Set the log level (e.g., Info, Warn, Error).")]
            public string? LogLevel { get; set; }

            [Option("log-file", Required = false, HelpText = "Path to the log file.")]
            public string? LogFile { get; set; }

            [Option('s', "skip-errors", Required = false, HelpText = "Flag to skip all occurring errors without user interaction.")]
            public bool SkipErrors { get; set; }

            [Option('n', "non-interactive", Required = false, HelpText = "Run in non-interactive mode.")]
            public bool NonInteractive { get; set; } = false;
        }

        [STAThread]
        static void Main(string[] args)
        {
            Static.logger = new Logger();
            // Set up the exit handler to clean up the repository on exit
#pragma warning disable CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).
            AppDomain.CurrentDomain.ProcessExit += OnExit;
            Console.CancelKeyPress += OnExit;
#pragma warning restore CS8622 // Nullability of reference types in type of parameter doesn't match the target delegate (possibly because of nullability attributes).

            Parser.Default.ParseArguments<CliOptions>(args)
                .WithParsed(RunWithOptions)
                .WithNotParsed(HandleParseErrors);
        }

        static void RunWithOptions(CliOptions options)
        {
            Static.NonInteractive = options.NonInteractive;
            Static.OutputFile = options.OutputFile ?? "";
            Static.ProfilePackage = options.ProfilePackage ?? "";

            Logger logger = Static.logger;
            logger.LogFilePath = options.LogFile ?? "";
            logger.Level = options.LogLevel switch
            {
                "Debug" => LogLevel.Debug,
                "Info" => LogLevel.Info,
                "Warn" => LogLevel.Warning,
                "Error" => LogLevel.Error,
                _ => LogLevel.Info // Default to Info if no valid level is provided
            };

            string filePath = Path.GetFullPath(options.MetamodelFile);
            #region Main Logic
            #region Mopro Hello
            Console.WriteLine("" +
                @"
 __   __  _______  _______  ______    _______                       
|  |_|  ||       ||       ||    _ |  |       |                      
|       ||   _   ||    _  ||   | ||  |   _   |   ____               
|       ||  | |  ||   |_| ||   |_||_ |  | |  |  |____|              
|       ||  |_|  ||    ___||    __  ||  |_|  |                      
| ||_|| ||       ||   |    |   |  | ||       |                      
|_|   |_||_______||___|    |___|  |_||_______|                      
 __   __  _______  ___      ___      _______  __                    
|  | |  ||       ||   |    |   |    |       ||  |                   
|  |_|  ||    ___||   |    |   |    |   _   ||  |                   
|       ||   |___ |   |    |   |    |  | |  ||  |                   
|       ||    ___||   |___ |   |___ |  |_|  ||__|                   
|   _   ||   |___ |       ||       ||       | __                    
|__| |__||_______||_______||_______||_______||__|
");
            #endregion Mopro Hello
            Console.WriteLine("" +
                @"
---------------------------------------------------------
Welcome to Mopro CLI!
This is a command line interface for MOPRO, the
>> MOF-based UML Profile Generator <<
You can use this to create and manage your profiles for Enterprise Architect MDG Technolgies.
---------------------------------------------------------");
            logger.LogInfo($"Starting Enterprise Architect and attempting to open repository at: {filePath} ...");

            Repository repository = new Repository();
            Static.ExitRepositoryReference = repository;
            bool opened = false;

            try
            {
                if (!System.IO.File.Exists(filePath))
                {
                    logger.LogError($"The specified file does not exist: {filePath}");
                    Console.WriteLine("The specified file does not exist. Please check the path and try again.");
                    Environment.Exit(-1);
                }
                opened = repository.OpenFile(filePath);
            }
            catch (Exception ex)
            {
                logger.LogError($"Exception on opening repository: \n{ex.Message}");

                if (options.SkipErrors)
                {
                    logger.LogWarning("Skipping errors as per user request.");
                }
                else
                {

                    Console.WriteLine($"Is repository open anyway? : {(repository.Models.Count > 0 ? "YES" : "NO")}");
                    if (options.NonInteractive)
                    {
                        logger.LogError("Repository could not be opened. Exiting application.");
                        Environment.Exit(-1);
                    }
                    Console.Write("Continue anyway? (y/n): ");
                    char key = Console.ReadKey().KeyChar;
                    Console.WriteLine();

                    if (key != 'y' && key != 'Y')
                    {
                        logger.LogInfo("Exiting application.");
                        return;
                    }
                }

                logger.LogWarning("### OPERATING IN INSECURE TERRITORY ###");
                opened = true;
            }

            if (opened)
            {
                logger.LogInfo("Repository opened successfully.");
                CreateProfileLogicMetamodel logic = new CreateProfileLogicMetamodel(repository);
            }
            else
            {
                logger.LogError("Failed to open repository.");
            }
            #endregion Main Logic
        }

        static void HandleParseErrors(IEnumerable<Error> errors)
        {
            if (errors.IsHelp() || errors.IsVersion())
                return;

            Static.logger.LogError("\n### Failed to parse command-line arguments:###");
            foreach (var error in errors)
            {
                Static.logger.LogError($"{error.Tag}");
            }
            Environment.Exit(1);
        }

        static void OnExit(object sender, EventArgs e)
        {
            var cleanupTask = System.Threading.Tasks.Task.Run(() =>
            {
                if (Static.CleanedUp || Static.ExitRepositoryReference == null)
                    return;

                try
                {
                    Static.logger.LogInfo("\nCleaning up and closing repository connection...");
                    if (Static.ExitRepositoryReference != null && Static.ExitRepositoryReference.Models.Count > 0)
                    {
                        Static.ExitRepositoryReference.CloseFile();
                        Static.ExitRepositoryReference.Exit();
                    }
                }
                catch (Exception ex)
                {
                    Static.logger.LogWarning($"Exception during repository cleanup: {ex.Message}");
                }
                finally
                {
                    Static.CleanedUp = true;
                }
            });
            int maxMsWait = 2000;
            bool completedInTime = cleanupTask.Wait(millisecondsTimeout: maxMsWait);

            if (!completedInTime)
            {
                Static.logger.LogError($"Repository cleanup exceeded the maximum allowed time of {((float)maxMsWait) / 1000} seconds. Terminating.");
                Environment.Exit(1); // Force termination
            }

            Console.WriteLine("" +
                @"                                                 
 __   __  _______  _______  ______    _______                       
|  |_|  ||       ||       ||    _ |  |       |                      
|       ||   _   ||    _  ||   | ||  |   _   |   ____               
|       ||  | |  ||   |_| ||   |_||_ |  | |  |  |____|              
|       ||  |_|  ||    ___||    __  ||  |_|  |                      
| ||_|| ||       ||   |    |   |  | ||       |                      
|_|   |_||_______||___|    |___|  |_||_______|                      
 _______  __   __  _______          _______  __   __  _______       
|  _    ||  | |  ||       |        |  _    ||  | |  ||       |      
| |_|   ||  |_|  ||    ___|        | |_|   ||  |_|  ||    ___|      
|       ||       ||   |___         |       ||       ||   |___       
|  _   | |_     _||    ___| ___    |  _   | |_     _||    ___| ___  
| |_|   |  |   |  |   |___ |_  |   | |_|   |  |   |  |   |___ |   | 
|_______|  |___|  |_______|  |_|   |_______|  |___|  |_______||___| ");
        }
    }
}
