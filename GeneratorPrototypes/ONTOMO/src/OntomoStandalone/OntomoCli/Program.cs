using CommandLine;
using Ontomo.CLI;
using Ontomo.Utils.Logging;

namespace Ontomo
{
    internal class Program
    {
        protected Program() { }

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += OnExit;
            Console.CancelKeyPress += OnExit; 
            Static.Logger = new Logger();


            Parser.Default.ParseArguments<CliOptions>(args)
                .WithParsed(RunWithOptions)
                .WithNotParsed(HandleParseErrors);
        }

        static void RunWithOptions(CliOptions options)
        {
            Static.Options = options;
            Logger logger = Static.Logger;
            logger.LogFilePath = options.LogFile ?? "";
            logger.Level = options.LogLevel switch
            {
                "Debug" => LogLevel.Debug,
                "Info" => LogLevel.Info,
                "Warn" => LogLevel.Warning,
                "Error" => LogLevel.Error,
                _ => LogLevel.Info
            };

            Console.WriteLine(CliHandlerLogic.WelcomeMessage);

            CliHandlerLogic.HandleCliLogic();

        }

        static void HandleParseErrors(IEnumerable<Error> errors)
        {
            if (errors.IsHelp() || errors.IsVersion())
                return;

            Static.Logger.LogError("\n### Failed to parse command-line arguments:###");
            foreach (var error in errors)
            {
                Static.Logger.LogError($"{error.Tag}");
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
                    Static.Logger.LogInfo("\nCleaning up and closing repository connection...");
                    if (Static.ExitRepositoryReference != null && Static.ExitRepositoryReference.Models.Count > 0)
                    {
                        Static.ExitRepositoryReference.CloseFile();
                        Static.ExitRepositoryReference.Exit();
                    }
                }
                catch (Exception ex)
                {
                    Static.Logger.LogWarning($"Exception during repository cleanup: {ex.Message}");
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
                Static.Logger.LogError($"Repository cleanup exceeded the maximum allowed time of {((float)maxMsWait) / 1000} seconds. Terminating.");
                Environment.Exit(1); // Force termination
            }

            Console.WriteLine(CliHandlerLogic.ClosingMessage);
        }
    }
}