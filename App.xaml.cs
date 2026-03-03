using System;
using System.Diagnostics;
using System.Windows;
using LogAnalyzer.Properties;
using LogAnalyzer.Views;
using ModernWpf;

namespace LogAnalyzer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        public void ApplyTheme(string? theme)
        {
            string normalized = string.Equals(theme, "Dark", StringComparison.OrdinalIgnoreCase) ? "Dark" : "Light";

            try
            {
                ThemeManager.Current.ApplicationTheme =
                    string.Equals(normalized, "Dark", StringComparison.OrdinalIgnoreCase)
                        ? ApplicationTheme.Dark
                        : ApplicationTheme.Light;
            }
            catch (Exception ex)
            {
                string message = $"ERROR: ApplyTheme failed ({normalized}): {ex.Message}";
                string inner = ex.InnerException?.Message ?? string.Empty;
                Debug.WriteLine(message);
                if (!string.IsNullOrWhiteSpace(inner))
                {
                    Debug.WriteLine($"ERROR: ApplyTheme inner: {inner}");
                }

                if (Current?.MainWindow is MainWindow mainWindow)
                {
                    mainWindow.AppendLogFromSettingsWindow(message);
                    if (!string.IsNullOrWhiteSpace(inner))
                    {
                        mainWindow.AppendLogFromSettingsWindow($"ERROR: ApplyTheme inner: {inner}");
                    }
                }
            }
        }

        public void ApplyLanguage(string? language)
        {
            string normalized = string.Equals(language, "ja", StringComparison.OrdinalIgnoreCase) ? "ja" : "en";
            Uri dictionaryUri = new($"Resources/Strings.{normalized}.xaml", UriKind.Relative);

            var dictionaries = Current.Resources.MergedDictionaries;
            for (int i = dictionaries.Count - 1; i >= 0; i--)
            {
                Uri? source = dictionaries[i].Source;
                if (source != null && source.OriginalString.Contains("Resources/Strings.", StringComparison.OrdinalIgnoreCase))
                {
                    dictionaries.RemoveAt(i);
                }
            }

            dictionaries.Add(new ResourceDictionary { Source = dictionaryUri });
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            string uiLanguage = string.Equals(Settings.Default.UiLanguage, "ja", StringComparison.OrdinalIgnoreCase) ? "ja" : "en";
            if (!string.Equals(Settings.Default.UiLanguage, uiLanguage, StringComparison.OrdinalIgnoreCase))
            {
                Settings.Default.UiLanguage = uiLanguage;
                Settings.Default.Save();
            }

            string uiTheme = string.Equals(Settings.Default.UiTheme, "Dark", StringComparison.OrdinalIgnoreCase) ? "Dark" : "Light";
            if (!string.Equals(Settings.Default.UiTheme, uiTheme, StringComparison.OrdinalIgnoreCase))
            {
                Settings.Default.UiTheme = uiTheme;
                Settings.Default.Save();
            }

            ApplyTheme(uiTheme);
            ApplyLanguage(uiLanguage);

            if (!Settings.Default.AgreeToAdminLogAccess)
            {
                AgreementWindow win = new();
                bool? result = win.ShowDialog();

                if (result != true)
                {
                    Shutdown();
                    return;
                }

                Settings.Default.AgreeToAdminLogAccess = true;
                Settings.Default.Save();
            }

            base.OnStartup(e);

            MainWindow mainWindow = new();
            MainWindow = mainWindow;
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            mainWindow.Show();
        }
    }
}
