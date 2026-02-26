using System;
using System.Windows;
using NetScope.ViewModels;
using NetScope.Services;

namespace NetScope.Views
{
    public partial class MainWindow : Window
    {
        private MainViewModel ViewModel => (MainViewModel)DataContext;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void TxtSearch_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (ViewModel.SearchCommand.CanExecute(null))
                {
                    ViewModel.SearchCommand.Execute(null);
                }
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
                    ViewModel.UpdateServerCount();
                }
            }
            catch (Exception ex)
            {
                string template = (string)Application.Current.TryFindResource("MsgErrorInterface") ?? "Error: {0}";
                MessageBox.Show(string.Format(template, ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                LoggerService.LogError("Failed to open settings", ex);
            }
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e) => Close();

        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            string aboutMessage = "NetScope - DHCP Management Tool\n\n" +
                                "Version: 2.2 (Professional)\n" +
                                "Developed by Marcus Santos 💻\n" +
                                "Multi-server solution for DHCP leases and policies.\n\n" +
                                "Architecture: MVVM + Secure PowerShell Service";
            
            MessageBox.Show(aboutMessage, "About", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnWakeOnLan_Click(object sender, RoutedEventArgs e)
        {
            var leaseInfo = ViewModel.GetLastLease();
            if (leaseInfo != null)
            {
                var wolWindow = new WakeOnLanWindow(leaseInfo.IpAddress, leaseInfo.MacAddress, leaseInfo.Hostname);
                wolWindow.Owner = this;
                wolWindow.ShowDialog();
            }
        }
    }
}