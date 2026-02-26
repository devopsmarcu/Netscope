using System;
using System.IO;

namespace NetScope.Services
{
    public static class LoggerService
    {
        private static readonly string LogFilePath;

        static LoggerService()
        {
            try
            {
                string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NetScope");
                if (!Directory.Exists(appDataFolder)) Directory.CreateDirectory(appDataFolder);
                LogFilePath = Path.Combine(appDataFolder, "app.log");
            }
            catch
            {
                LogFilePath = "app.log"; // Fallback to local
            }
        }

        public static void Log(string message, string level = "INFO")
        {
            try
            {
                string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}{Environment.NewLine}";
                File.AppendAllText(LogFilePath, logEntry);
            }
            catch
            {
                // Cannot log to file
            }
        }

        public static void LogError(string message, Exception? ex = null)
        {
            string detail = ex != null ? $"\nException: {ex.Message}\nStack: {ex.StackTrace}" : "";
            Log($"{message}{detail}", "ERROR");
        }
    }
}
