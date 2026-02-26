using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using NetScope.Services;
using NetScope.Models;

namespace NetScope
{
    public partial class MainWindow : Window
    {
        private readonly DatabaseService _dbService;
        private readonly DhcpService _dhcpService;
        private string lastSearchResult = string.Empty;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                
                _dbService = new DatabaseService();
                _dhcpService = new DhcpService(_dbService);
                
                UpdateServerCount();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao inicializar interface: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateServerCount()
        {
            try
            {
                if (lblServerCount != null)
                {
                    int count = _dbService.GetServers().Count(s => s.IsActive);
                    lblServerCount.Text = $"Servidores monitorados: {count}";
                }
            }
            catch { }
        }

        private void CmbSearchType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbSearchType?.SelectedItem is ComboBoxItem selectedItem && txtSearch != null)
            {
                string searchType = selectedItem.Tag?.ToString() ?? "";
                
                switch (searchType)
                {
                    case "MAC":
                        txtSearch.ToolTip = "Exemplo: 00-17-c8-a4-f8-4c ou 0017c8a4f84c";
                        break;
                    case "IP":
                        txtSearch.ToolTip = "Exemplo: 10.50.14.29";
                        break;
                    case "Hostname":
                        txtSearch.ToolTip = "Exemplo: PC-USUARIO-01 ou workstation (busca parcial)";
                        break;
                    case "Description":
                        txtSearch.ToolTip = "Exemplo: texto da descrição (busca parcial)";
                        break;
                }
                
                txtSearch.Text = "";
            }
        }

        private void TxtSearch_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                BtnSearch_Click(sender, new RoutedEventArgs());
            }
        }

        private async void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (txtSearch == null || cmbSearchType == null || txtResult == null) return;

            string searchValue = txtSearch.Text.Trim();
            if (string.IsNullOrWhiteSpace(searchValue))
            {
                MessageBox.Show("Por favor, digite um valor para busca.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ComboBoxItem selectedItem = (ComboBoxItem)cmbSearchType.SelectedItem;
            string searchType = selectedItem?.Tag?.ToString() ?? "";

            btnSearch.IsEnabled = false;
            txtResult.Text = "Consultando...";


            try
            {
                string result = await _dhcpService.SearchLeaseAsync(searchValue, searchType);
                txtResult.Text = result;
                lastSearchResult = result;
                
                ShowWakeOnLanButton(result);

                // Verificação de Políticas
                var leaseInfo = ExtractLeaseInfo(result);
                string macToVerify = leaseInfo?.MacAddress ?? (searchType == "MAC" ? searchValue : "");

                if (!string.IsNullOrEmpty(macToVerify))
                {
                    txtResult.Text += "\n\nVerificando políticas de acesso...";
                    var policyResult = await _dhcpService.CheckMacPolicyStatusAsync(macToVerify);
                    
                    if (policyResult != null)
                    {
                        string status = policyResult.IsAllowed ? "Sim ✅" : "Não ❌";
                        string scopes = policyResult.IsAllowed && policyResult.Scopes.Count > 0 
                                        ? string.Join(", ", policyResult.Scopes) 
                                        : (policyResult.IsAllowed ? "Todos (Global)" : "-");
                        
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("RESULTADO DA CONSULTA DHCP");
                        sb.AppendLine(result);
                        sb.AppendLine($"MAC Liberado  : {status}");
                        sb.AppendLine($"Escopos Lib.  : {scopes}");
                        
                        // Verificar se o escopo encontrado está no cadastro de "Nomes"
                        var knownScopes = _dbService.GetScopes();
                        foreach (var scope in policyResult.Scopes)
                        {
                            var known = knownScopes.FirstOrDefault(s => s.ScopeId == scope);
                            if (known != null)
                            {
                                sb.AppendLine($"Unidade/Local : {known.Name}");
                            }
                        }
                        
                        txtResult.Text = sb.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                txtResult.Text = $"Erro durante a busca: {ex.Message}";
            }
            finally
            {
                btnSearch.IsEnabled = true;
            }
        }

        private void MenuSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var settingsWindow = new SettingsWindow();
                settingsWindow.Owner = this;
                
                if (settingsWindow.ShowDialog() == true)
                {
                    UpdateServerCount();
                    MessageBox.Show("Configurações atualizadas com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao abrir configurações: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e) => Close();

        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            string aboutMessage = "NetScope - DHCP Management Tool\n\n" +
                                "Versão: 2.1 (Genérica)\n" +
                                "Desenvolvido por Marcus Santos 💻\n" +
                                "Solução multi-servidor para consulta de leases e políticas DHCP.\n\n" +
                                $"Servidores configurados: {_dbService.GetServers().Count(s => s.IsActive)}";
            
            MessageBox.Show(aboutMessage, "Sobre", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ShowWakeOnLanButton(string result)
        {
            if (btnWakeOnLan == null) return;
            btnWakeOnLan.Visibility = (result.Contains("IP Address") && result.Contains("MAC Address")) 
                                      ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnWakeOnLan_Click(object sender, RoutedEventArgs e)
        {
            var leaseInfo = ExtractLeaseInfo(lastSearchResult);
            if (leaseInfo != null)
            {
                var wolWindow = new WakeOnLanWindow(leaseInfo.IpAddress, leaseInfo.MacAddress, leaseInfo.Hostname);
                wolWindow.Owner = this;
                wolWindow.ShowDialog();
            }
        }

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

    public class LeaseInfo
    {
        public string IpAddress { get; set; } = "";
        public string MacAddress { get; set; } = "";
        public string Hostname { get; set; } = "";
    }
}