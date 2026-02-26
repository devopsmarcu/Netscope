using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using NetScope.Helpers;
using NetScope.Models;
using NetScope.Services;

namespace NetScope.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly DatabaseService _dbService;
        private readonly DhcpService _dhcpService;

        private string _searchText = string.Empty;
        private string _searchType = "MAC";
        private string _results = string.Empty;
        private string _serverCountText = string.Empty;
        private bool _isBusy;
        private bool _canWakeOnLan;
        private LeaseInfo? _lastLease;

        public MainViewModel()
        {
            _dbService = new DatabaseService();
            _dhcpService = new DhcpService(_dbService);
            
            SearchCommand = new RelayCommand(async _ => await ExecuteSearchAsync(), _ => !IsBusy && !string.IsNullOrWhiteSpace(SearchText));
            UpdateServerCount();
            
            // Initialization result
            Results = (string)Application.Current.TryFindResource("ResultsWaiting") ?? "Waiting for search...";
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public string SearchType
        {
            get => _searchType;
            set => SetProperty(ref _searchType, value);
        }

        public string Results
        {
            get => _results;
            set => SetProperty(ref _results, value);
        }

        public string ServerCountText
        {
            get => _serverCountText;
            set => SetProperty(ref _serverCountText, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public bool CanWakeOnLan
        {
            get => _canWakeOnLan;
            set => SetProperty(ref _canWakeOnLan, value);
        }

        public ICommand SearchCommand { get; }

        public void UpdateServerCount()
        {
            int count = _dbService.GetServers().Count(s => s.IsActive);
            string template = (string)Application.Current.TryFindResource("FooterMonitoredServers") ?? "Monitored servers: {0}";
            ServerCountText = string.Format(template, count);
        }

        private async Task ExecuteSearchAsync()
        {
            IsBusy = true;
            CanWakeOnLan = false;
            Results = (string)Application.Current.TryFindResource("ResultsSearching") ?? "Searching...";

            try
            {
                string result = await _dhcpService.SearchLeaseAsync(SearchText, SearchType);
                Results = result;
                
                _lastLease = ExtractLeaseInfo(result);
                CanWakeOnLan = _lastLease != null;

                // Policy Check
                await RunPolicyCheckAsync(result);
            }
            catch (Exception ex)
            {
                Results = $"Search error: {ex.Message}";
                LoggerService.LogError("Search failed", ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task RunPolicyCheckAsync(string baseResult)
        {
            string macToVerify = _lastLease?.MacAddress ?? (SearchType == "MAC" ? SearchText : "");
            if (string.IsNullOrEmpty(macToVerify) || !ValidationHelper.IsValidMac(macToVerify)) return;

            Results += "\n\nChecking access policies...";
            var policyResult = await _dhcpService.CheckMacPolicyStatusAsync(macToVerify);
            
            if (policyResult != null)
            {
                string status = policyResult.IsAllowed ? "Yes ✅" : "No ❌";
                string scopes = policyResult.IsAllowed && policyResult.Scopes.Count > 0 
                                ? string.Join(", ", policyResult.Scopes) 
                                : (policyResult.IsAllowed ? "Global" : "-");
                
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("DHCP QUERY RESULT");
                sb.AppendLine(baseResult);
                sb.AppendLine($"MAC Allowed   : {status}");
                sb.AppendLine($"Allowed Scopes: {scopes}");
                
                var knownScopes = _dbService.GetScopes();
                foreach (var scope in policyResult.Scopes)
                {
                    var known = knownScopes.FirstOrDefault(s => s.ScopeId == scope);
                    if (known != null) sb.AppendLine($"Location      : {known.Name}");
                }
                
                Results = sb.ToString();
            }
        }

        public LeaseInfo? GetLastLease() => _lastLease;

        private LeaseInfo? ExtractLeaseInfo(string result)
        {
            try
            {
                var lines = result.Split('\n');
                string ip = "", mac = "", host = "";

                foreach (var line in lines)
                {
                    if (line.Contains("IP Address")) ip = line.Split(':').Last().Trim();
                    else if (line.Contains("MAC Address")) mac = line.Split(':').Last().Trim();
                    else if (line.Contains("Host Name")) host = line.Split(':').Last().Trim();
                }

                if (!string.IsNullOrEmpty(ip) && !string.IsNullOrEmpty(mac))
                    return new LeaseInfo { IpAddress = ip, MacAddress = mac, Hostname = host };
            }
            catch { }
            return null;
        }
    }
}
