namespace Mopro.Utils.Logging
{
    /// <summary>  
    /// Enumeration for different log levels. Item names correspond to the log level names printed in uppercase.  
    /// </summary>  
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }


    public class Logger
    {
        /// <summary>  
        /// The current log level. Messages with a lower level than this will not be logged.  
        /// </summary>  
        public LogLevel Level { get; set; } = LogLevel.Info;

        /// <summary>  
        /// Path to the log file where logs will be written. If empty, logs will not be written to any file.  
        /// Supports both absolute and relative paths based on the program's execution location.  
        /// When set to a valid file path, the output stream to the specified file is added to _outputStreams.  
        /// If set to an empty string, the output stream to the file is removed if it exists.  
        /// </summary>  
        private string _logFilePath = "";
        public string LogFilePath
        {
            get => _logFilePath;
            set
            {
                if (_logFilePath != value)
                {
                    // Remove existing file stream if it exists  
                    var existingFileStream = _outputStreams.FirstOrDefault(s => s is StreamWriter writer && writer.BaseStream is FileStream fs && fs.Name == Path.GetFullPath(_logFilePath));
                    if (existingFileStream != null)
                    {
                        _outputStreams.Remove(existingFileStream);
                        existingFileStream.Dispose();
                    }

                    _logFilePath = value;

                    // Add new file stream if the path is valid  
                    if (!string.IsNullOrWhiteSpace(_logFilePath))
                    {
                        try
                        {
                            var fullPath = Path.GetFullPath(_logFilePath);
                            var fileStream = new StreamWriter(fullPath, append: true) { AutoFlush = true };
                            _outputStreams.Add(fileStream);
                        }
                        catch (Exception ex)
                        {
                            Console.Error.WriteLine($"Failed to set log file path: {ex.Message}");
                        }
                    }
                }
            }
        }

        /// <summary>  
        /// Indicates whether log messages should be printed to the console.  
        /// When set to true, Console.Out is added to _outputStreams.  
        /// When set to false, Console.Out is removed from _outputStreams.  
        /// </summary>  
        private bool _logToConsole = true;
        public bool LogToConsole
        {
            get => _logToConsole;
            set
            {
                _logToConsole = value;
                if (value)
                {
                    if (!_outputStreams.Contains(Console.Out))
                    {
                        _outputStreams.Add(Console.Out);
                    }
                }
                else
                {
                    _outputStreams.Remove(Console.Out);
                }
            }
        }

        /// <summary>  
        /// Indicates whether log messages should be preceded by a timestamp.  
        /// </summary>  
        private bool _includeTimestamp = false;
        public bool IncludeTimestamp
        {
            get => _includeTimestamp;
            set => _includeTimestamp = value;
        }

        /// <summary>  
        /// Prints a log message to all output streams defined in _outputStreams.  
        /// </summary>  
        /// <param name="message"></param>  
        private void printLog(string message)
        {
            foreach (var stream in _outputStreams)
            {
                stream.WriteLine(message);
            }
        }

        /// <summary>  
        /// Contains all the streams to which the logs will be written. Default is only Console.Out.  
        /// </summary>  
        private List<TextWriter> _outputStreams { get; set; } = new List<TextWriter> { Console.Out };

        /// <summary>  
        /// Logs a message to the console with a specified log level.  
        /// </summary>  
        /// <param name="level">Conditionally print log if "level" is at least Logger.Settings.Level or higher</param>  
        /// <param name="message"></param>  
        public void Log(LogLevel level = LogLevel.Info, string message = "<no information provided>")
        {
            if (level < Level)
                return;

            string prefix = $"[ {level.ToString().ToUpper()} ]: ";
            string timestamp = IncludeTimestamp ? $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] " : "";

            printLog(timestamp + prefix + message);
        }

        /// <summary>  
        /// Logs an error message with the Error log level if configured Logger.Level is at least LogLevel.Error or lower.  
        /// </summary>  
        /// <param name="message"></param>  
        public void LogError(string message)
        {
            Log(LogLevel.Error, message);
        }

        /// <summary>  
        /// Logs a warning message with the Warning log level if configured Logger.Level is at least LogLevel.Warning or lower.  
        /// </summary>  
        /// <param name="message"></param>  
        public void LogWarning(string message)
        {
            Log(LogLevel.Warning, message);
        }

        /// <summary>  
        /// Logs an informational message with the Info log level if configured Logger.Level is at least LogLevel.Info or lower.  
        /// </summary>  
        /// <param name="message"></param>  
        public void LogInfo(string message)
        {
            Log(LogLevel.Info, message);
        }

        /// <summary>  
        /// Logs a debug message with the Debug log level if configured Logger.Level is at least LogLevel.Debug or lower.  
        /// </summary>  
        /// <param name="message"></param>  
        public void LogDebug(string message)
        {
            Log(LogLevel.Debug, message);
        }

        /// <summary>  
        /// Constructor to initialize Logger with optional parameters.  
        /// </summary>  
        /// <param name="level">Initial log level.</param>  
        /// <param name="logToConsole">Whether to log to console.</param>  
        /// <param name="logFilePath">Path to the log file.</param>  
        /// <param name="includeTimestamp">Whether to include timestamps in log messages.</param>  
        public Logger(LogLevel level = LogLevel.Info, bool logToConsole = true, string logFilePath = "", bool includeTimestamp = false)
        {
            Level = level;
            LogToConsole = logToConsole;
            LogFilePath = logFilePath;
            IncludeTimestamp = includeTimestamp;
        }
    }
}
