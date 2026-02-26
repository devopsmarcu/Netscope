using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using NetScope.Services;

namespace NetScope
{
    public partial class App : Application
    {
        public static void ChangeLanguage(string cultureCode)
        {
            try
            {
                // If "auto", detect from system
                if (string.IsNullOrEmpty(cultureCode) || cultureCode.ToLower() == "auto")
                {
                    cultureCode = CultureInfo.CurrentUICulture.Name;
                }

                // Supported languages
                string[] supported = { "pt-BR", "en-US", "es-ES" };
                
                // Find best match or fallback to en-US
                string? target = supported.FirstOrDefault(c => c.Equals(cultureCode, StringComparison.OrdinalIgnoreCase));
                
                if (target == null)
                {
                    // Try to match only the language part (e.g., "pt" instead of "pt-BR")
                    string lang = cultureCode.Split('-')[0];
                    target = supported.FirstOrDefault(c => c.StartsWith(lang, StringComparison.OrdinalIgnoreCase)) ?? "en-US";
                }

                ResourceDictionary dict = new ResourceDictionary();
                dict.Source = new Uri($"Resources/lang.{target}.xaml", UriKind.Relative);

                // Find existing lang dictionary and replace it
                var oldDict = Application.Current.Resources.MergedDictionaries
                    .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Resources/lang."));

                if (oldDict != null)
                {
                    int index = Application.Current.Resources.MergedDictionaries.IndexOf(oldDict);
                    Application.Current.Resources.MergedDictionaries[index] = dict;
                }
                else
                {
                    Application.Current.Resources.MergedDictionaries.Add(dict);
                }

                // Set thread cultures for formatting consistency
                var culture = new CultureInfo(target);
                CultureInfo.DefaultThreadCurrentCulture = culture;
                CultureInfo.DefaultThreadCurrentUICulture = culture;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error changing language: {ex.Message}");
            }
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                // Initialize Database and Load Language
                var db = new DatabaseService();
                string prefLang = db.GetSettings().Language;
                
                ChangeLanguage(prefLang);

                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing application: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1);
            }
        }
    }
}
