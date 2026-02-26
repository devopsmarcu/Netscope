using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using NetScope.Helpers;
using NetScope.Models;
using NetScope.Services;

namespace NetScope.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly DatabaseService _dbService;
        private ObservableCollection<DhcpServer> _servers;
        private ObservableCollection<MacFilterPolicy> _policies;
        private ObservableCollection<DhcpScope> _scopes;
        private string _newServerAddress = string.Empty;
        private string _newPolicyName = string.Empty;
        private string _newScopeId = string.Empty;
        private string _newScopeName = string.Empty;
        private string _selectedLanguage = "auto";

        public SettingsViewModel()
        {
            _dbService = new DatabaseService();
            
            _servers = new ObservableCollection<DhcpServer>(_dbService.GetServers());
            _policies = new ObservableCollection<MacFilterPolicy>(_dbService.GetPolicies());
            _scopes = new ObservableCollection<DhcpScope>(_dbService.GetScopes());
            _selectedLanguage = _dbService.GetSettings().Language;

            AddServerCommand = new RelayCommand(_ => AddServer(), _ => !string.IsNullOrWhiteSpace(NewServerAddress));
            AddPolicyCommand = new RelayCommand(_ => AddPolicy(), _ => !string.IsNullOrWhiteSpace(NewPolicyName));
            AddScopeCommand = new RelayCommand(_ => AddScope(), _ => !string.IsNullOrWhiteSpace(NewScopeId));
            DeleteServerCommand = new RelayCommand<DhcpServer>(s => { if (s != null) _servers.Remove(s); });
            DeletePolicyCommand = new RelayCommand<MacFilterPolicy>(p => { if (p != null) _policies.Remove(p); });
            DeleteScopeCommand = new RelayCommand<DhcpScope>(s => { if (s != null) _scopes.Remove(s); });
        }

        public ObservableCollection<DhcpServer> Servers => _servers;
        public ObservableCollection<MacFilterPolicy> Policies => _policies;
        public ObservableCollection<DhcpScope> Scopes => _scopes;

        public string NewServerAddress { get => _newServerAddress; set => SetProperty(ref _newServerAddress, value); }
        public string NewPolicyName { get => _newPolicyName; set => SetProperty(ref _newPolicyName, value); }
        public string NewScopeId { get => _newScopeId; set => SetProperty(ref _newScopeId, value); }
        public string NewScopeName { get => _newScopeName; set => SetProperty(ref _newScopeName, value); }
        public string SelectedLanguage { get => _selectedLanguage; set => SetProperty(ref _selectedLanguage, value); }

        public ICommand AddServerCommand { get; }
        public ICommand AddPolicyCommand { get; }
        public ICommand AddScopeCommand { get; }
        public ICommand DeleteServerCommand { get; }
        public ICommand DeletePolicyCommand { get; }
        public ICommand DeleteScopeCommand { get; }

        private void AddServer()
        {
            if (_servers.Any(s => s.Address.Equals(NewServerAddress, StringComparison.OrdinalIgnoreCase)))
            {
                ShowWarning("MsgServerExists");
                return;
            }
            _servers.Add(new DhcpServer { Address = NewServerAddress, Name = NewServerAddress, IsActive = true });
            NewServerAddress = string.Empty;
        }

        private void AddPolicy()
        {
            if (_policies.Any(p => p.Name.Equals(NewPolicyName, StringComparison.OrdinalIgnoreCase)))
            {
                ShowWarning("MsgPolicyExists");
                return;
            }
            _policies.Add(new MacFilterPolicy { Name = NewPolicyName, IsActive = true });
            NewPolicyName = string.Empty;
        }

        private void AddScope()
        {
            if (_scopes.Any(s => s.ScopeId.Equals(NewScopeId, StringComparison.OrdinalIgnoreCase)))
            {
                ShowWarning("MsgScopeExists");
                return;
            }
            _scopes.Add(new DhcpScope { ScopeId = NewScopeId, Name = NewScopeName });
            NewScopeId = string.Empty;
            NewScopeName = string.Empty;
        }

        public bool Save()
        {
            try
            {
                _dbService.SyncData(_servers.ToList(), _policies.ToList(), _scopes.ToList());
                
                var settings = _dbService.GetSettings();
                if (settings.Language != SelectedLanguage)
                {
                    settings.Language = SelectedLanguage;
                    _dbService.SaveSettings(settings);
                    App.ChangeLanguage(SelectedLanguage);
                }
                return true;
            }
            catch (Exception ex)
            {
                string template = (string)Application.Current.TryFindResource("MsgErrorSave") ?? "Error: {0}";
                MessageBox.Show(string.Format(template, ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private void ShowWarning(string resourceKey)
        {
            string msg = (string)Application.Current.TryFindResource(resourceKey) ?? "Warning";
            MessageBox.Show(msg, "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}
