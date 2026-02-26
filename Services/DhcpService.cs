using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using NetScope.Models;

namespace NetScope.Services
{
    public class DhcpService
    {
        private readonly DatabaseService _dbService;

        public DhcpService(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        public async Task<string> SearchLeaseAsync(string value, string type)
        {
            var servers = _dbService.GetServers().Where(s => s.IsActive).ToList();
            if (!servers.Any())
            {
                return "---------------------------------------------\nErro: Nenhum servidor DHCP configurado ou ativo.\nPor favor, cadastre os servidores nas Configurações.\n---------------------------------------------";
            }

            string result = "";
            string serversArray = string.Join("','", servers.Select(s => s.Address));

            string script = "";
            switch (type)
            {
                case "MAC":
                    script = GetMacSearchScript(value, serversArray);
                    break;
                case "IP":
                    script = GetIpSearchScript(value, serversArray);
                    break;
                case "Hostname":
                    script = GetHostnameSearchScript(value, serversArray);
                    break;
                case "Description":
                    script = GetDescriptionSearchScript(value, serversArray);
                    break;
            }

            return await Task.Run(() => RunPowerShell(script));
        }

        public async Task<MacPolicyResult?> CheckMacPolicyStatusAsync(string mac)
        {
            var servers = _dbService.GetServers().Where(s => s.IsActive).ToList();
            var policies = _dbService.GetPolicies().Where(p => p.IsActive).ToList();

            if (!servers.Any() || !policies.Any()) return null;

            // Para simplicidade, vamos verificar a primeira política ativa no primeiro servidor
            // Ou podemos iterar, mas o script original recebia um servidor e uma política.
            // Vamos tornar genérico: verificar o MAC em todas as políticas ativas de todos os servidores ativos.
            
            string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CheckMacPolicy.ps1");
            if (!File.Exists(scriptPath)) scriptPath = "CheckMacPolicy.ps1";

            List<string> allowedScopes = new List<string>();
            bool isAllowed = false;

            foreach (var server in servers)
            {
                foreach (var policy in policies)
                {
                    string scriptArgs = $"-ExecutionPolicy Bypass -File \"{scriptPath}\" -MacAddress \"{mac}\" -DhcpServer \"{server.Address}\" -PolicyName \"{policy.Name}\"";
                    
                    var result = await Task.Run(() => {
                        string output = RunPowerShellRaw(scriptArgs);
                        if (!string.IsNullOrEmpty(output))
                        {
                            int jsonStart = output.IndexOf("{");
                            int jsonEnd = output.LastIndexOf("}");
                            if (jsonStart >= 0 && jsonEnd > jsonStart)
                            {
                                string json = output.Substring(jsonStart, jsonEnd - jsonStart + 1);
                                return JsonSerializer.Deserialize<MacPolicyResult>(json);
                            }
                        }
                        return null;
                    });

                    if (result != null && result.IsAllowed)
                    {
                        isAllowed = true;
                        allowedScopes.AddRange(result.Scopes);
                    }
                }
            }

            return new MacPolicyResult
            {
                Mac = mac,
                IsAllowed = isAllowed,
                Scopes = allowedScopes.Distinct().ToList()
            };
        }

        private string GetMacSearchScript(string mac, string serversArray)
        {
            string normalizedMac = System.Text.RegularExpressions.Regex.Replace(mac.ToUpper(), "[^A-F0-9]", "");
            return $@"
$DhcpServers = @('{serversArray}')
$Mac = '{normalizedMac}'
$found = $false

foreach ($DhcpServer in $DhcpServers) {{
    try {{
        $scopes = Get-DhcpServerv4Scope -ComputerName $DhcpServer -ErrorAction Stop
        foreach ($scope in $scopes) {{
            $lease = Get-DhcpServerv4Lease -ComputerName $DhcpServer -ScopeId $scope.ScopeId -ErrorAction SilentlyContinue | Where-Object {{ ($_.ClientId -replace '[:-]', '') -ieq $Mac }}
            if ($lease) {{
                $macFormatted = (($lease.ClientId -replace '[^A-Fa-f0-9]', '').ToUpper() -replace '(.{2})(?!$)', '$1-')
                $scopeIP = $scope.ScopeId.IPAddressToString
                $leaseIP = $lease.IPAddress.IPAddressToString
                $hostName = if ($lease.HostName) {{ $lease.HostName }} else {{ '(vazio)' }}
                $status = $lease.AddressState
                $description = if ($lease.Description) {{ $lease.Description }} else {{ '(vazio)' }}
                
                Write-Host ('Servidor DHCP : ' + $DhcpServer)
                Write-Host ('Escopo        : ' + $scopeIP)
                Write-Host ('MAC Address   : ' + $macFormatted)
                Write-Host ('IP Address    : ' + $leaseIP)
                Write-Host ('Host Name     : ' + $hostName)
                Write-Host ('Descrição     : ' + $description)
                Write-Host ('Status        : ' + $status)
                $found = $true
                break
            }}
        }}
        if ($found) {{ break }}
    }}
    catch {{ }}
}}

if (-not $found) {{
    $macDisplay = (($Mac -replace '(.{{2}})(?!$)', '$1-'))
    Write-Host 'Nenhum IP encontrado para o MAC: ' $macDisplay
}}
";
        }

        private string GetIpSearchScript(string ip, string serversArray)
        {
            return $@"
$DhcpServers = @('{serversArray}')
$SearchIP = '{ip}'
$found = $false

foreach ($DhcpServer in $DhcpServers) {{
    try {{
        $scopes = Get-DhcpServerv4Scope -ComputerName $DhcpServer -ErrorAction Stop
        foreach ($scope in $scopes) {{
            $lease = Get-DhcpServerv4Lease -ComputerName $DhcpServer -ScopeId $scope.ScopeId -ErrorAction SilentlyContinue | Where-Object {{ $_.IPAddress.IPAddressToString -eq $SearchIP }}
            if ($lease) {{
                $macFormatted = (($lease.ClientId -replace '[^A-Fa-f0-9]', '').ToUpper() -replace '(.{2})(?!$)', '$1-')
                $scopeIP = $scope.ScopeId.IPAddressToString
                $leaseIP = $lease.IPAddress.IPAddressToString
                $hostName = if ($lease.HostName) {{ $lease.HostName }} else {{ '(vazio)' }}
                $status = $lease.AddressState
                $description = if ($lease.Description) {{ $lease.Description }} else {{ '(vazio)' }}
                
                Write-Host ('Servidor DHCP : ' + $DhcpServer)
                Write-Host ('Escopo        : ' + $scopeIP)
                Write-Host ('MAC Address   : ' + $macFormatted)
                Write-Host ('IP Address    : ' + $leaseIP)
                Write-Host ('Host Name     : ' + $hostName)
                Write-Host ('Descrição     : ' + $description)
                Write-Host ('Status        : ' + $status)
                $found = $true
                break
            }}
        }}
        if ($found) {{ break }}
    }}
    catch {{ }}
}}

if (-not $found) {{
    Write-Host 'Nenhum MAC encontrado para o IP: ' $SearchIP
}}
";
        }

        private string GetHostnameSearchScript(string hostname, string serversArray)
        {
            return $@"
$DhcpServers = @('{serversArray}')
$SearchHostname = '{hostname}'
$found = $false

foreach ($DhcpServer in $DhcpServers) {{
    try {{
        $scopes = Get-DhcpServerv4Scope -ComputerName $DhcpServer -ErrorAction Stop
        foreach ($scope in $scopes) {{
            $leases = Get-DhcpServerv4Lease -ComputerName $DhcpServer -ScopeId $scope.ScopeId -ErrorAction SilentlyContinue | Where-Object {{ 
                $_.HostName -and (
                    $_.HostName -like ('*' + $SearchHostname + '*') -or 
                    $_.HostName -eq $SearchHostname -or
                    $_.HostName -like ($SearchHostname + '.*') -or
                    $_.HostName -like ('*.' + $SearchHostname)
                )
            }}
            if ($leases) {{
                foreach ($lease in $leases) {{
                    $macFormatted = (($lease.ClientId -replace '[^A-Fa-f0-9]', '').ToUpper() -replace '(.{2})(?!$)', '$1-')
                    $scopeIP = $scope.ScopeId.IPAddressToString
                    $leaseIP = $lease.IPAddress.IPAddressToString
                    $hostName = if ($lease.HostName) {{ $lease.HostName }} else {{ '(vazio)' }}
                    $status = $lease.AddressState
                    $description = if ($lease.Description) {{ $lease.Description }} else {{ '(vazio)' }}
                    
                    Write-Host ('Servidor DHCP : ' + $DhcpServer)
                    Write-Host ('Escopo        : ' + $scopeIP)
                    Write-Host ('MAC Address   : ' + $macFormatted)
                    Write-Host ('IP Address    : ' + $leaseIP)
                    Write-Host ('Host Name     : ' + $hostName)
                    Write-Host ('Descrição     : ' + $description)
                    Write-Host ('Status        : ' + $status)
                    $found = $true
                }}
            }}
        }}
    }}
    catch {{ }}
}}

if (-not $found) {{
    Write-Host 'Nenhum lease encontrado para o hostname: ' $SearchHostname
}}
";
        }

        private string GetDescriptionSearchScript(string description, string serversArray)
        {
            return $@"
$DhcpServers = @('{serversArray}')
$SearchDescription = '{description}'
$found = $false

foreach ($DhcpServer in $DhcpServers) {{
    try {{
        $scopes = Get-DhcpServerv4Scope -ComputerName $DhcpServer -ErrorAction Stop
        foreach ($scope in $scopes) {{
            $leases = Get-DhcpServerv4Lease -ComputerName $DhcpServer -ScopeId $scope.ScopeId -ErrorAction SilentlyContinue | Where-Object {{ $_.Description -like ('*' + $SearchDescription + '*') }}
            if ($leases) {{
                foreach ($lease in $leases) {{
                    $macFormatted = (($lease.ClientId -replace '[^A-Fa-f0-9]', '').ToUpper() -replace '(.{2})(?!$)', '$1-')
                    $scopeIP = $scope.ScopeId.IPAddressToString
                    $leaseIP = $lease.IPAddress.IPAddressToString
                    $hostName = if ($lease.HostName) {{ $lease.HostName }} else {{ '(vazio)' }}
                    $status = $lease.AddressState
                    $description = if ($lease.Description) {{ $lease.Description }} else {{ '(vazio)' }}
                    
                    Write-Host ('Servidor DHCP : ' + $DhcpServer)
                    Write-Host ('Escopo        : ' + $scopeIP)
                    Write-Host ('MAC Address   : ' + $macFormatted)
                    Write-Host ('IP Address    : ' + $leaseIP)
                    Write-Host ('Host Name     : ' + $hostName)
                    Write-Host ('Descrição     : ' + $description)
                    Write-Host ('Status        : ' + $status)
                    $found = $true
                }}
            }}
        }}
    }}
    catch {{ }}
}}

if (-not $found) {{
    Write-Host 'Nenhum lease encontrado para a descrição: ' $SearchDescription
}}
";
        }

        private string RunPowerShell(string script)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo()
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script.Replace("\"", "`\"")}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8
                };

                using (Process? ps = Process.Start(psi))
                {
                    if (ps == null) return "Erro: Falha ao iniciar PowerShell.";
                    string output = ps.StandardOutput.ReadToEnd();
                    ps.WaitForExit();
                    return output.Trim();
                }
            }
            catch (Exception ex)
            {
                return $"Erro ao executar consulta: {ex.Message}";
            }
        }

        private string RunPowerShellRaw(string args)
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo()
                {
                    FileName = "powershell.exe",
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8
                };

                using (Process? ps = Process.Start(psi))
                {
                    if (ps == null) return "";
                    string output = ps.StandardOutput.ReadToEnd();
                    ps.WaitForExit();
                    return output;
                }
            }
            catch { return ""; }
        }
    }

    public class MacPolicyResult
    {
        [JsonPropertyName("mac")]
        public string Mac { get; set; } = string.Empty;
        
        [JsonPropertyName("isAllowed")]
        public bool IsAllowed { get; set; }
        
        [JsonPropertyName("scopes")]
        public List<string> Scopes { get; set; } = new List<string>();
    }
}
