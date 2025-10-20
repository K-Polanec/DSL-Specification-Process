using Ontomo.Utils.Logging;
using EA;
using File = System.IO.File;

namespace Ontomo.Functions.ExportEA
{
    internal class EARepositoryHandler
    {
        private readonly Logger logger = Static.Logger;
        internal string outputFilePath = Static.OutputFilePath;
        internal string EATemplatePath = Static.EATemplatePath;
        private readonly Repository repository;
        private bool opened = false;

        private bool newFileCreated = false;

        public EARepositoryHandler(string outputFilePath = "", string EATemplatePath = "")
        {
            logger.LogInfo("EARepositoryHandler initialized.");
            this.outputFilePath = outputFilePath != "" ? outputFilePath : Static.OutputFilePath;
            this.EATemplatePath = EATemplatePath != "" ? EATemplatePath : Static.EATemplatePath;
            repository = new Repository();
        }

        public void Execute()
        {

            logger.LogInfo("Checking if an EA project is already open.");
            if (openRepository())
            {
                Static.ExitRepositoryReference = repository;
                EAxmiImportHandler xmiImportHandler = new EAxmiImportHandler(repository, newFileCreated);
            }

            repository.CloseFile();
            repository.Exit();
            logger.LogInfo("EARepositoryHandler completed and repository closed.");
        }

        internal bool openRepository()
        {
            try
            {
                if (!File.Exists(outputFilePath))
                {
                    logger.LogInfo($"Creating new EA project at {outputFilePath}");
                    Console.WriteLine($"Creating new EA project at {outputFilePath}");
                    File.Copy(EATemplatePath, outputFilePath);
                    newFileCreated = true;
                    logger.LogInfo("EA project created successfully.");
                }
                else
                {
                    logger.LogInfo($"EA project already exists at {outputFilePath}");
                }

                if (repository != null && !opened)
                {
                    logger.LogInfo("Opening EA project.");
                    Console.WriteLine("Opening EA project.");
                    repository.OpenFile(outputFilePath);
                    opened = true;
                    logger.LogInfo("EA project opened successfully.");
                    return opened;
                }
                else
                {
                    logger.LogWarning("EA project is already open or repository is null.");
                    Console.WriteLine("EA project is already open or repository is null.");
                    return opened;
                }

            }
            catch (Exception e)
            {
                logger.LogError($"An error occurred while opening the EA project: \n{e.Message}");
                Console.WriteLine(e);
                return false;
            }

        }
    }
}
