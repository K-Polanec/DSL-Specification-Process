using EA;
using Mopro.Utils.Logging;

namespace Mopro
{
    public static class Static
    {
        public static Repository? ExitRepositoryReference;
        public static bool CleanedUp = false;
        public static bool NonInteractive = false;
        public static Logger? logger;
        public static string OutputFile = "";
        public static string ProfilePackage = "";
    }
}
