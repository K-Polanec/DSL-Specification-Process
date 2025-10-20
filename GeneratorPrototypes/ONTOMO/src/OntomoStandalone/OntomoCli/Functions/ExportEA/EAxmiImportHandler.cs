using Ontomo.Utils.Logging;
using EA;

namespace Ontomo.Functions.ExportEA
{
    internal class EAxmiImportHandler
    {
        private readonly Logger logger = Static.Logger;
        internal string templateXmiFilePath = Static.XMITemplatePath;
        internal string languageName = Static.LanguageName;

        private readonly Repository repository;
        private readonly EACrawlerHelper crawlerHelper;

        private readonly bool newFileCreated = false;

        public EAxmiImportHandler(Repository repository, bool newFileCreated, Logger? customLogger = null)
        {
            logger = customLogger ?? Static.Logger;
            this.newFileCreated = newFileCreated;
            this.repository = repository;
            this.crawlerHelper = new EACrawlerHelper(repository);

            eaXmiImportHandlerMainLogic();
            EAExportHandler exportHandler = new EAExportHandler(repository, crawlerHelper);
            exportHandler.Execute();

        }

        private void eaXmiImportHandlerMainLogic()
        {
            if (repository.ConnectionString != "")
            {
                logger.LogInfo("EA project is open. Initializing template xmi import.");

                if (!newFileCreated)
                {
                    logger.LogInfo("Opened existing file, checking project content...");

                    if (countRootModels() == 0)
                    {
                        createNewModel();
                        logger.LogInfo("Existing model is empty, created new root model... Now triggering XMI import.");
                        triggerImport();

                    }
                    else
                    {
                        logger.LogInfo("Existing model is not empty, checking if profile package exists...");
                        if (!findProfilePackage())
                        {
                            createNewModel();
                            logger.LogError("Profile package not found. Created new root model... Now triggering XMI import.");
                            triggerImport();
                        }
                        else
                        {
                            logger.LogInfo("Profile package found. Skipping XMI import and continuing directly with ontology import.");

                        }
                    }
                }
                else
                {
                    logger.LogInfo("New EA file created, skipping XMI import...");
                    Package ontomoModel = (Package) repository.Models.GetAt(0);
                    Static.OntomoModelGUID = ontomoModel.PackageGUID;
                }
            }
            else
            {
                logger.LogWarning("No EA project is open. Template xmi could not be imported.");
            }
        }

        private int countRootModels()
        {
            return repository.Models.Count;
        }

        internal bool createNewModel()
        {
            string modelName = "ONTOMO Model ";
            string timestamp = DateTime.Now.ToString("dd.MM HH:mm");
            modelName += timestamp;

            int modelCount = countRootModels();

            Package ontomoModel = (Package) repository.Models.AddNew(modelName, "5");
            ontomoModel.Update();
            repository.Models.Refresh();
            Static.OntomoModelGUID = ontomoModel.PackageGUID;

            if (countRootModels() > modelCount)
            {
                logger.LogInfo("New model created successfully.");
                return true;
            }
            
            logger.LogError("Failed to create new model in EA project.");
            return false;

        }

        private bool findProfilePackage()
        {
            string profilePackageGUID = "";
            
            for (short doNumber = 0; doNumber < countRootModels(); doNumber++)
            {
                Package rootModel = (Package) repository.Models.GetAt(doNumber);
                profilePackageGUID = crawlerHelper.findPackageGuidRecursively(rootModel, Static.LanguageName);
                if (!string.IsNullOrEmpty(profilePackageGUID))
                {
                    if(!repository.GetPackageByGuid(profilePackageGUID).StereotypeEx.Contains("profile"))
                    {
                        continue;
                    }

                    logger.LogInfo("Found profile package with name " + Static.LanguageName);
                    Static.OntomoModelGUID = rootModel.PackageGUID;
                    return true;
                }
            }

            logger.LogInfo("No profile package found with the name " + Static.LanguageName + ".");
            return false;
        }

        internal void triggerImport()
        {
            repository.GetProjectInterface().ImportPackageXMI(Static.OntomoModelGUID, templateXmiFilePath, 1, 1);
            logger.LogInfo("Template xmi import completed successfully.");
            setProfileName();
            logger.LogInfo("Profile name set to " + Static.LanguageName);
        }

        internal void setProfileName()
        {
            Package ontomoModel = repository.GetPackageByGuid(Static.OntomoModelGUID);
            Package profilePackage = (Package) ontomoModel.Packages.GetByName("Cybersecurity"); //change name in template xmi
            profilePackage.Name = Static.LanguageName;
        }
    }
}
