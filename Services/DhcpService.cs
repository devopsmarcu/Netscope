using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NetScope.Models;
using NetScope.Helpers;

namespace NetScope.Services
{
    public class DhcpService
    {
        private readonly DatabaseService _dbService;
        private readonly ScriptRunner _scriptRunner;

        public DhcpService(DatabaseService dbService)
        {
            _dbService = dbService;
            _scriptRunner = new ScriptRunner();
        }

        public async Task<string> SearchLeaseAsync(string value, string type)
        {
            var servers = _dbService.GetServers().Where(s => s.IsActive).ToList();
            if (!servers.Any())
            {
                return "Error: No active DHCP servers configured.";
            }

            // Validation and Sanitization
            if (string.IsNullOrWhiteSpace(value)) return "Error: Search value is empty.";

            string validatedValue = value;
            switch (type)
            {
                case "MAC":
                    if (!ValidationHelper.IsValidMac(value)) return "Invalid MAC format.";
                    validatedValue = ValidationHelper.NormalizeMac(value);
                    break;
                case "IP":
                    if (!ValidationHelper.IsValidIp(value)) return "Invalid IP address.";
                    break;
                case "Description":
                    validatedValue = ValidationHelper.SanitizeDescription(value);
                    break;
                case "Hostname":
                    if (!ValidationHelper.IsValidHostname(value)) return "Invalid hostname characters.";
                    break;
            }

            var parameters = new Dictionary<string, object>
            {
                { "SearchValue", validatedValue },
                { "SearchType", type },
                { "DhcpServers", servers.Select(s => s.Address).ToArray() }
            };

            return await _scriptRunner.ExecuteScriptAsync("SearchLease.ps1", parameters);
        }

        public async Task<MacPolicyResult?> CheckMacPolicyStatusAsync(string mac)
        {
            if (!ValidationHelper.IsValidMac(mac)) return null;
            string normalizedMac = ValidationHelper.NormalizeMac(mac);

            var servers = _dbService.GetServers().Where(s => s.IsActive).ToList();
            var policies = _dbService.GetPolicies().Where(p => p.IsActive).ToList();

            if (!servers.Any() || !policies.Any()) return null;

            List<string> allowedScopes = new List<string>();
            bool isAllowed = false;

            foreach (var server in servers)
            {
                foreach (var policy in policies)
                {
                    var parameters = new Dictionary<string, object>
                    {
                        { "MacAddress", normalizedMac },
                        { "DhcpServer", server.Address },
                        { "PolicyName", policy.Name }
                    };

                    var result = await _scriptRunner.ExecuteScriptJsonAsync<MacPolicyResult>("CheckMacPolicy.ps1", parameters);

                    if (result != null && result.IsAllowed)
                    {
                        isAllowed = true;
                        allowedScopes.AddRange(result.Scopes);
                    }
                }
            }

            return new MacPolicyResult
            {
                Mac = normalizedMac,
                IsAllowed = isAllowed,
                Scopes = allowedScopes.Distinct().ToList()
            };
        }
    }
}
