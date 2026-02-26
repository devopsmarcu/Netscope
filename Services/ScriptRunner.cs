using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace NetScope.Services
{
    public class ScriptRunner
    {
        private readonly string _scriptsPath;

        public ScriptRunner()
        {
            _scriptsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Scripts");
        }

        public async Task<string> ExecuteScriptAsync(string fileName, Dictionary<string, object> parameters)
        {
            return await Task.Run(() =>
            {
                string scriptPath = Path.Combine(_scriptsPath, fileName);
                if (!File.Exists(scriptPath))
                {
                    LoggerService.Log($"Script not found: {scriptPath}", "ERROR");
                    return $"Error: Script {fileName} not found.";
                }

                try
                {
                    StringBuilder argsBuilder = new StringBuilder();
                    argsBuilder.Append("-NoProfile -ExecutionPolicy Bypass -File ");
                    argsBuilder.Append($"\"{scriptPath}\"");

                    foreach (var param in parameters)
                    {
                        argsBuilder.Append($" -{param.Key} ");
                        
                        // Securely escape values for the command line
                        string value = param.Value?.ToString() ?? "";
                        if (param.Value is Array array)
                        {
                            // Join arrays for PS: "a","b","c"
                            var elements = new List<string>();
                            foreach (var element in array) elements.Add($"'{element?.ToString()?.Replace("'", "''")}'");
                            value = string.Join(",", elements);
                            argsBuilder.Append(value);
                        }
                        else
                        {
                            argsBuilder.Append($"\"{value.Replace("\"", "`\"")}\"");
                        }
                    }

                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = argsBuilder.ToString(),
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        StandardOutputEncoding = Encoding.UTF8
                    };

                    using (Process? ps = Process.Start(psi))
                    {
                        if (ps == null) return "Error: Could not start PowerShell.";
                        
                        string output = ps.StandardOutput.ReadToEnd();
                        string error = ps.StandardError.ReadToEnd();
                        ps.WaitForExit();

                        if (!string.IsNullOrEmpty(error))
                        {
                            LoggerService.LogError($"PS Script Error ({fileName}): {error}");
                            return output + "\nError: " + error;
                        }

                        return output.Trim();
                    }
                }
                catch (Exception ex)
                {
                    LoggerService.LogError($"Failed to run script {fileName}", ex);
                    return $"Error: {ex.Message}";
                }
            });
        }

        public async Task<T?> ExecuteScriptJsonAsync<T>(string fileName, Dictionary<string, object> parameters) where T : class
        {
            string rawOutput = await ExecuteScriptAsync(fileName, parameters);
            if (string.IsNullOrEmpty(rawOutput)) return null;

            try
            {
                int start = rawOutput.IndexOf('{');
                int end = rawOutput.LastIndexOf('}');
                if (start >= 0 && end > start)
                {
                    string json = rawOutput.Substring(start, end - start + 1);
                    return System.Text.Json.JsonSerializer.Deserialize<T>(json);
                }
            }
            catch (Exception ex)
            {
                LoggerService.LogError($"Failed to parse JSON from {fileName}", ex);
            }
            return null;
        }
    }
}
