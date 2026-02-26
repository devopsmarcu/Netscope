using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using NetScope.Models;
using NetScope.Services;

namespace NetScope
{
    public partial class SettingsWindow : Window
    {
        private readonly DatabaseService _dbService;
        private ObservableCollection<DhcpServer> _servers;
        private ObservableCollection<MacFilterPolicy> _policies;
        private ObservableCollection<DhcpScope> _scopes;

        public SettingsWindow()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
            
            _servers = new ObservableCollection<DhcpServer>(_dbService.GetServers());
            _policies = new ObservableCollection<MacFilterPolicy>(_dbService.GetPolicies());
            _scopes = new ObservableCollection<DhcpScope>(_dbService.GetScopes());

            dgServers.ItemsSource = _servers;
            dgPolicies.ItemsSource = _policies;
            dgScopes.ItemsSource = _scopes;
        }

        private void BtnAddServer_Click(object sender, RoutedEventArgs e)
        {
            string address = txtServerAddress.Text.Trim();
            if (string.IsNullOrWhiteSpace(address)) return;

            if (_servers.Any(s => s.Address.Equals(address, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Este servidor já está na lista.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var server = new DhcpServer { Address = address, Name = address, IsActive = true };
            _servers.Add(server);
            txtServerAddress.Clear();
        }

        private void BtnDeleteServer_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is DhcpServer server)
            {
                _servers.Remove(server);
            }
        }

        private void BtnAddPolicy_Click(object sender, RoutedEventArgs e)
        {
            string name = txtPolicyName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name)) return;

            if (_policies.Any(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Esta política já está na lista.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var policy = new MacFilterPolicy { Name = name, IsActive = true };
            _policies.Add(policy);
            txtPolicyName.Clear();
        }

        private void BtnDeletePolicy_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is MacFilterPolicy policy)
            {
                _policies.Remove(policy);
            }
        }

        private void BtnAddScope_Click(object sender, RoutedEventArgs e)
        {
            string id = txtScopeId.Text.Trim();
            string name = txtScopeName.Text.Trim();
            if (string.IsNullOrWhiteSpace(id)) return;

            if (_scopes.Any(s => s.ScopeId.Equals(id, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Este escopo já está na lista.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var scope = new DhcpScope { ScopeId = id, Name = name };
            _scopes.Add(scope);
            txtScopeId.Clear();
            txtScopeName.Clear();
        }

        private void BtnDeleteScope_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn && btn.Tag is DhcpScope scope)
            {
                _scopes.Remove(scope);
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _dbService.SyncData(_servers.ToList(), _policies.ToList(), _scopes.ToList());
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao salvar configurações: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
