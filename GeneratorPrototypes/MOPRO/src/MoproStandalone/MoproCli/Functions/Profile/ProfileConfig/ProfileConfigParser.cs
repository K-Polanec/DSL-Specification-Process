using EA;
using Mopro.Utils.Logging;
using System.Text.RegularExpressions;

namespace Mopro.Functions.Profile.ProfileConfig
{
    class ProfileConfigParser
    {
        public string ProfileLogoRelPath { get; set; }
        public string ProfileIconRelPath { get; set; }
        public string TechnologyName { get; set; }
        public double Version { get; set; }
        public string Url { get; set; }
        public string Support { get; set; }
        public string ProfileDescription { get; set; }

        private bool validConfigFile = false;
        private Logger logger = Static.logger;

        const string configFileName = "profile.config";
        private string configSearchPath = "";

        public ProfileConfigParser(Repository repository) 
        {
            string metaModelPath = repository.ConnectionString;
            configSearchPath = Path.GetDirectoryName(metaModelPath) + "\\" + configFileName;
            configSearchPath = configSearchPath.Replace("/", "\\");

            ParseConfigFile();
        }

        public bool IsConfigFileValid()
        {
            return validConfigFile;
        }

        private void ParseConfigFile()
        {
            if (configSearchPath == "" || !System.IO.File.Exists(configSearchPath))
            {
                validConfigFile = false;
            }

            try
            {
                string[] lines = System.IO.File.ReadAllLines(configSearchPath);

                foreach (string line in lines)
                {
                    ParseLine(line);
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning($"Error reading the config file: {ex.Message}");
                validConfigFile = false;
            }

            validConfigFile = true;
        }

        private void ParseLine(string line)
        {
            // Using a regular expression to match key-value pairs
            var match = Regex.Match(line, @"(\w+)=(.*)");

            if (match.Success)
            {
                string key = match.Groups[1].Value;
                string value = match.Groups[2].Value;

                // Assign values based on the key
                switch (key)
                {
                    case "profileLogoRelPath":
                        ProfileLogoRelPath = value;
                        break;
                    case "profileIconRelPath":
                        ProfileIconRelPath = value;
                        break;
                    case "technologyName":
                        TechnologyName = value;
                        break;
                    case "version":
                        double version;
                        if (double.TryParse(value, out version))
                        {
                            Version = version;
                        }
                        else
                        {
                            logger.LogError($"Invalid version format: {value}");
                        }
                        break;
                    case "url":
                        Url = value;
                        break;
                    case "support":
                        Support = value;
                        break;
                    case "profileDescription":
                        ProfileDescription = value;
                        break;
                    default:
                        logger.LogWarning($"Unknown key: {key}");
                        break;
                }
            }
            else
            {
                Console.WriteLine($"Invalid line format: {line}");
            }
        }

        public void PrintConfigLines()
        {
            logger.LogInfo($"Configuration for creating the profile:\n" +
                $"\tProfile Logo Relative Path: {ProfileLogoRelPath}\n" +
                $"\tProfile Icon Relative Path: {ProfileIconRelPath}\n" +
                $"\tTechnology Name: {TechnologyName}\n" +
                $"\tVersion: {Version}\n" +
                $"\tURL: {Url}\n" +
                $"\tSupport: {Support}\n" +
                $"\tProfile Description: {ProfileDescription}");
        }
    }
}

