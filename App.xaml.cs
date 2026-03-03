using System;
using System.Diagnostics;
using System.Windows;
using MediaBrush = System.Windows.Media.Brush;
using MediaBrushes = System.Windows.Media.Brushes;
using MediaColor = System.Windows.Media.Color;
using MediaColorConverter = System.Windows.Media.ColorConverter;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;
using LogAnalyzer.Properties;
using LogAnalyzer.Services;
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

            ApplyOutputListSelectionColor();
        }

        public void ApplyOutputListSelectionColor()
        {
            MediaBrush brush = ResolveBrushFromSlot(Settings.Default.OutputListSelectionKey, "OutputListSelectionBrush");
            Current.Resources["OutputListSelectionBrush"] = brush;
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
            SettingsIniService.LoadIntoSettings();

            string uiLanguage = string.Equals(Settings.Default.UiLanguage, "ja", StringComparison.OrdinalIgnoreCase) ? "ja" : "en";
            if (!string.Equals(Settings.Default.UiLanguage, uiLanguage, StringComparison.OrdinalIgnoreCase))
            {
                Settings.Default.UiLanguage = uiLanguage;
                SettingsIniService.SaveSettings();
            }

            string uiTheme = string.Equals(Settings.Default.UiTheme, "Dark", StringComparison.OrdinalIgnoreCase) ? "Dark" : "Light";
            if (!string.Equals(Settings.Default.UiTheme, uiTheme, StringComparison.OrdinalIgnoreCase))
            {
                Settings.Default.UiTheme = uiTheme;
                SettingsIniService.SaveSettings();
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
                SettingsIniService.SaveSettings();
            }

            base.OnStartup(e);

            MainWindow mainWindow = new();
            MainWindow = mainWindow;
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            mainWindow.Show();
        }

        private MediaBrush ResolveBrushFromSlot(string? slotValue, string fallbackKey)
        {
            const string themePrefix = "theme:";
            const string argbPrefix = "argb:";

            string normalized = string.IsNullOrWhiteSpace(slotValue)
                ? $"{themePrefix}{fallbackKey}"
                : slotValue;

            if (!normalized.StartsWith(themePrefix, StringComparison.OrdinalIgnoreCase) &&
                !normalized.StartsWith(argbPrefix, StringComparison.OrdinalIgnoreCase))
            {
                normalized = $"{themePrefix}{normalized}";
            }

            if (normalized.StartsWith(themePrefix, StringComparison.OrdinalIgnoreCase))
            {
                string key = normalized[themePrefix.Length..];
                if (Current.TryFindResource(key) is MediaBrush selectedBrush)
                {
                    return selectedBrush;
                }
            }

            if (normalized.StartsWith(argbPrefix, StringComparison.OrdinalIgnoreCase))
            {
                string argb = normalized[argbPrefix.Length..];
                try
                {
                    object? colorObj = MediaColorConverter.ConvertFromString(argb);
                    if (colorObj is MediaColor color)
                    {
                        return new SolidColorBrush(color);
                    }
                }
                catch
                {
                    // fall back to theme resource
                }
            }

            return (MediaBrush)(Current.TryFindResource(fallbackKey) ?? MediaBrushes.Transparent);
        }
    }
}
