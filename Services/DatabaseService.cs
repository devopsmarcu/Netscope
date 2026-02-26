using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using NetScope.Models;

namespace NetScope.Services
{
    public class DatabaseService
    {
        private readonly string _dbPath;
        private AppData _data;

        public DatabaseService()
        {
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _dbPath = Path.Combine(appDirectory, "database.json");
            _data = new AppData();
            Load();
        }

        public List<DhcpServer> GetServers() => _data.Servers;
        public List<MacFilterPolicy> GetPolicies() => _data.Policies;
        public List<DhcpScope> GetScopes() => _data.Scopes;

        public void SaveServer(DhcpServer server)
        {
            if (server.Id == 0)
            {
                server.Id = _data.Servers.Any() ? _data.Servers.Max(s => s.Id) + 1 : 1;
                _data.Servers.Add(server);
            }
            else
            {
                var existing = _data.Servers.FirstOrDefault(s => s.Id == server.Id);
                if (existing != null)
                {
                    existing.Name = server.Name;
                    existing.Address = server.Address;
                    existing.Description = server.Description;
                    existing.IsActive = server.IsActive;
                }
            }
            Save();
        }

        public void DeleteServer(int id)
        {
            _data.Servers.RemoveAll(s => s.Id == id);
            Save();
        }

        public void SavePolicy(MacFilterPolicy policy)
        {
            if (policy.Id == 0)
            {
                policy.Id = _data.Policies.Any() ? _data.Policies.Max(p => p.Id) + 1 : 1;
                _data.Policies.Add(policy);
            }
            else
            {
                var existing = _data.Policies.FirstOrDefault(p => p.Id == policy.Id);
                if (existing != null)
                {
                    existing.Name = policy.Name;
                    existing.Description = policy.Description;
                    existing.IsActive = policy.IsActive;
                }
            }
            Save();
        }

        public void DeletePolicy(int id)
        {
            _data.Policies.RemoveAll(p => p.Id == id);
            Save();
        }

        public void SaveScope(DhcpScope scope)
        {
            if (scope.Id == 0)
            {
                scope.Id = _data.Scopes.Any() ? _data.Scopes.Max(s => s.Id) + 1 : 1;
                _data.Scopes.Add(scope);
            }
            else
            {
                var existing = _data.Scopes.FirstOrDefault(s => s.Id == scope.Id);
                if (existing != null)
                {
                    existing.Name = scope.Name;
                    existing.ScopeId = scope.ScopeId;
                    existing.ServerAddress = scope.ServerAddress;
                    existing.Description = scope.Description;
                }
            }
            Save();
        }

        public void DeleteScope(int id)
        {
            _data.Scopes.RemoveAll(s => s.Id == id);
            Save();
        }

        public void SyncData(List<DhcpServer> servers, List<MacFilterPolicy> policies, List<DhcpScope> scopes)
        {
            _data.Servers = servers;
            _data.Policies = policies;
            _data.Scopes = scopes;
            Save();
        }

        private void Load()
        {
            try
            {
                if (File.Exists(_dbPath))
                {
                    string json = File.ReadAllText(_dbPath);
                    _data = JsonSerializer.Deserialize<AppData>(json) ?? new AppData();
                }
            }
            catch
            {
                _data = new AppData();
            }
        }

        private void Save()
        {
            try
            {
                string json = JsonSerializer.Serialize(_data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_dbPath, json);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao salvar banco de dados: {ex.Message}");
            }
        }

        private class AppData
        {
            public List<DhcpServer> Servers { get; set; } = new List<DhcpServer>();
            public List<MacFilterPolicy> Policies { get; set; } = new List<MacFilterPolicy>();
            public List<DhcpScope> Scopes { get; set; } = new List<DhcpScope>();
        }
    }
}
