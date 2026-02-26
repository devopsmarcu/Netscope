using System;
using System.IO;
using System.Text.Json;
using NetScope.Models;

namespace NetScope.Services
{
    public class StorageService
    {
        private readonly string _oldDbPath;
        private readonly string _newDbPath;
        private readonly string _appDataFolder;

        public StorageService()
        {
            _oldDbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "database.json");
            _appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NetScope");
            _newDbPath = Path.Combine(_appDataFolder, "database.json");

            InitializeStorage();
        }

        private void InitializeStorage()
        {
            try
            {
                if (!Directory.Exists(_appDataFolder))
                {
                    Directory.CreateDirectory(_appDataFolder);
                    LoggerService.Log("Created AppData directory");
                }

                // Migration
                if (File.Exists(_oldDbPath) && !File.Exists(_newDbPath))
                {
                    File.Move(_oldDbPath, _newDbPath);
                    LoggerService.Log("Migrated database from local directory to AppData");
                }
            }
            catch (Exception ex)
            {
                LoggerService.LogError("Failed to initialize storage", ex);
            }
        }

        public AppData Load()
        {
            try
            {
                if (File.Exists(_newDbPath))
                {
                    string json = File.ReadAllText(_newDbPath);
                    return JsonSerializer.Deserialize<AppData>(json) ?? new AppData();
                }
            }
            catch (Exception ex)
            {
                LoggerService.LogError("Failed to load data", ex);
            }
            return new AppData();
        }

        public void Save(AppData data)
        {
            string tempFile = _newDbPath + ".tmp";
            try
            {
                string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(tempFile, json);
                
                if (File.Exists(_newDbPath)) File.Delete(_newDbPath);
                File.Move(tempFile, _newDbPath);
            }
            catch (Exception ex)
            {
                LoggerService.LogError("Failed to save data", ex);
                if (File.Exists(tempFile)) File.Delete(tempFile);
                throw;
            }
        }
    }
}
