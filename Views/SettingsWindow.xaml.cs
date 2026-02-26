using System.Windows;
using NetScope.ViewModels;

namespace NetScope.Views
{
    public partial class SettingsWindow : Window
    {
        private SettingsViewModel ViewModel => (SettingsViewModel)DataContext;

        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.Save())
            {
                DialogResult = true;
                Close();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
