using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Threading;
using LogAnalyzer.Models;
using LogAnalyzer.Properties;
using LogAnalyzer.Services;
using Forms = System.Windows.Forms;
using MediaBrush = System.Windows.Media.Brush;
using MediaColor = System.Windows.Media.Color;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;

namespace LogAnalyzer.Views
{
    public sealed class ColorChoice : INotifyPropertyChanged
    {
        private bool _isSelected;

        public string Key { get; }
        public MediaBrush Brush { get; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                {
                    return;
                }

                _isSelected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ColorChoice(string key, MediaBrush brush)
        {
            Key = key;
            Brush = brush;
        }
    }

    public sealed class PaletteSlot : INotifyPropertyChanged
    {
        private string _brushKey;
        private MediaBrush _brush;
        private bool _isPopupOpen;

        public string PaletteType { get; }
        public int Index { get; }

        public string BrushKey
        {
            get => _brushKey;
            set
            {
                if (string.Equals(_brushKey, value, StringComparison.Ordinal))
                {
                    return;
                }

                _brushKey = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BrushKey)));
            }
        }

        public MediaBrush Brush
        {
            get => _brush;
            set
            {
                if (ReferenceEquals(_brush, value))
                {
                    return;
                }

                _brush = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Brush)));
            }
        }

        public bool IsPopupOpen
        {
            get => _isPopupOpen;
            set
            {
                if (_isPopupOpen == value)
                {
                    return;
                }

                _isPopupOpen = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPopupOpen)));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public PaletteSlot(string paletteType, int index, string brushKey, MediaBrush brush)
        {
            PaletteType = paletteType;
            Index = index;
            _brushKey = brushKey;
            _brush = brush;
        }
    }

    public sealed class PaletteChoice
    {
        public string Id { get; }
        public string Name { get; }

        public PaletteChoice(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    public partial class SettingsWindow : Window, INotifyPropertyChanged
    {
        private const string ThemePrefix = "theme:";
        private const string ArgbPrefix = "argb:";
        private static readonly string[] DefaultPiePaletteA =
        {
            $"{ThemePrefix}AnalysisProviderColor1", $"{ThemePrefix}AnalysisProviderColor2", $"{ThemePrefix}AnalysisProviderColor3", $"{ThemePrefix}AnalysisProviderColor4", $"{ThemePrefix}AnalysisProviderColor5",
            $"{ThemePrefix}AnalysisProviderColor6", $"{ThemePrefix}AnalysisProviderColor7", $"{ThemePrefix}AnalysisProviderColor8", $"{ThemePrefix}AnalysisProviderColor1", $"{ThemePrefix}AnalysisProviderColor2"
        };

        private static readonly string[] DefaultPiePaletteB =
        {
            $"{ThemePrefix}AnalysisProviderColor8", $"{ThemePrefix}AnalysisProviderColor7", $"{ThemePrefix}AnalysisProviderColor6", $"{ThemePrefix}AnalysisProviderColor5", $"{ThemePrefix}AnalysisProviderColor4",
            $"{ThemePrefix}AnalysisProviderColor3", $"{ThemePrefix}AnalysisProviderColor2", $"{ThemePrefix}AnalysisProviderColor1", $"{ThemePrefix}AnalysisProviderColor8", $"{ThemePrefix}AnalysisProviderColor7"
        };

        private static readonly string[] DefaultBarPaletteA =
        {
            $"{ThemePrefix}GridHeaderBackgroundBrush", $"{ThemePrefix}GridSelectionBackgroundBrush", $"{ThemePrefix}PanelBackgroundBrush", $"{ThemePrefix}BorderBrush"
        };

        private static readonly string[] DefaultBarPaletteB =
        {
            $"{ThemePrefix}GridSelectionBackgroundBrush", $"{ThemePrefix}GridHeaderBackgroundBrush", $"{ThemePrefix}InputBackgroundBrush", $"{ThemePrefix}BorderBrush"
        };

        private static readonly string[] GraphColorChoiceKeys =
        {
            "GridHeaderBackgroundBrush", "GridSelectionBackgroundBrush", "PanelBackgroundBrush", "BorderBrush",
            "InputBackgroundBrush", "InputForegroundBrush", "ForegroundBrush",
            "AnalysisProviderColor1", "AnalysisProviderColor2", "AnalysisProviderColor3", "AnalysisProviderColor4",
            "AnalysisProviderColor5", "AnalysisProviderColor6", "AnalysisProviderColor7", "AnalysisProviderColor8",
            "AnalysisProviderColorOther"
        };

        private readonly MainWindow _ownerWindow;
        private readonly DispatcherTimer _graphSaveDebounceTimer;
        private bool _isApplying;
        private bool _isReady;
        private bool _isGraphBindingUpdating;
        private string _selectedPiePaletteId = "A";
        private string _selectedBarPaletteId = "A";
        private string _selectedHourLabelTextKey = $"{ThemePrefix}InputForegroundBrush";
        private UserConfig _userConfig = new();
        private string _configSavePath = string.Empty;
        private MediaBrush _barNormalPreviewBrush = new SolidColorBrush(System.Windows.Media.Colors.Transparent);
        private MediaBrush _barPeakPreviewBrush = new SolidColorBrush(System.Windows.Media.Colors.Transparent);
        private MediaBrush _barLabelPreviewBrush = new SolidColorBrush(System.Windows.Media.Colors.Transparent);

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<MediaBrush> PiePalettePreviewSwatches { get; } = new();
        public ObservableCollection<PaletteSlot> PiePaletteSlots { get; } = new();
        public ObservableCollection<PaletteSlot> BarPaletteSlots { get; } = new();
        public ObservableCollection<PaletteChoice> PiePaletteChoices { get; } = new();
        public ObservableCollection<PaletteChoice> BarPaletteChoices { get; } = new();
        public ObservableCollection<ColorChoice> GraphColorChoices { get; } = new();
        public ObservableCollection<ColorChoice> HourLabelColorChoices { get; } = new();

        public MediaBrush BarNormalPreviewBrush
        {
            get => _barNormalPreviewBrush;
            private set
            {
                if (!ReferenceEquals(_barNormalPreviewBrush, value))
                {
                    _barNormalPreviewBrush = value;
                    OnPropertyChanged();
                }
            }
        }

        public MediaBrush BarPeakPreviewBrush
        {
            get => _barPeakPreviewBrush;
            private set
            {
                if (!ReferenceEquals(_barPeakPreviewBrush, value))
                {
                    _barPeakPreviewBrush = value;
                    OnPropertyChanged();
                }
            }
        }

        public MediaBrush BarLabelPreviewBrush
        {
            get => _barLabelPreviewBrush;
            private set
            {
                if (!ReferenceEquals(_barLabelPreviewBrush, value))
                {
                    _barLabelPreviewBrush = value;
                    OnPropertyChanged();
                }
            }
        }

        public SettingsWindow(MainWindow ownerWindow)
        {
            _ownerWindow = ownerWindow;
            _graphSaveDebounceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _graphSaveDebounceTimer.Tick += GraphSaveDebounceTimer_Tick;
            InitializeComponent();
            LoadFromSettings();
            Loaded += SettingsWindow_Loaded;
        }

        private void SettingsWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _isReady = true;
            if (LstCategories != null && LstCategories.SelectedIndex < 0)
            {
                LstCategories.SelectedIndex = 0;
            }

            UpdateCategoryPanels(LstCategories?.SelectedIndex ?? 0);
        }

        private void LoadFromSettings()
        {
            _isApplying = true;
            try
            {
                LoadUserGraphConfig();

                TxtOutputDirSetting.Text = Settings.Default.OutputDirectory ?? string.Empty;
                ChkWriteJsonlSetting.IsChecked = Settings.Default.WriteJsonl;
                ChkWriteCsvSetting.IsChecked = Settings.Default.WriteCsv;
                ChkWriteSummarySetting.IsChecked = Settings.Default.WriteSummary;

                ChkDedupMessageSetting.IsChecked = Settings.Default.DedupMessage;
                ChkIncludeXmlSetting.IsChecked = Settings.Default.IncludeXml;
                ChkShowOnlyNewSetting.IsChecked = Settings.Default.ShowOnlyNew;

                bool is12h = string.Equals(Settings.Default.TimeFormat, "12h", StringComparison.OrdinalIgnoreCase);
                RbTime12.IsChecked = is12h;
                RbTime24.IsChecked = !is12h;

                bool isDark = string.Equals(Settings.Default.UiTheme, "Dark", StringComparison.OrdinalIgnoreCase);
                RbThemeDark.IsChecked = isDark;
                RbThemeLight.IsChecked = !isDark;

                bool isJa = string.Equals(Settings.Default.UiLanguage, "ja", StringComparison.OrdinalIgnoreCase);
                RbLanguageJa.IsChecked = isJa;
                RbLanguageEn.IsChecked = !isJa;

                _selectedPiePaletteId = NormalizePaletteId(_userConfig.SelectedPiePaletteId);
                _selectedBarPaletteId = NormalizePaletteId(_userConfig.SelectedBarPaletteId);
                _selectedHourLabelTextKey = NormalizeSlotValue(_userConfig.BarLabelTextSlot, $"{ThemePrefix}InputForegroundBrush");

                BuildGraphColorChoices();
                BuildPaletteChoices();
                CmbPiePalette.SelectedValue = _selectedPiePaletteId;
                CmbBarPalette.SelectedValue = _selectedBarPaletteId;
                LoadPiePaletteSlots();
                LoadBarPaletteSlots();
                BuildHourLabelColorChoices();
                UpdateGraphColorPreviews();
                UpdateGraphSaveStatusSaved();
            }
            finally
            {
                _isApplying = false;
            }
        }

        private void LoadUserGraphConfig()
        {
            (UserConfig config, string loadedFrom) = UserConfigService.LoadOrDefault(CreateDefaultGraphConfig);
            _userConfig = config;
            _configSavePath = loadedFrom;
            EnsureGraphConfigIntegrity();
        }

        private UserConfig CreateDefaultGraphConfig()
        {
            string[] pieA = BuildGradientSlots("PanelBackgroundBrush", 10);
            string[] pieB = BuildGradientSlots("GridSelectionBackgroundBrush", 10);
            string[] barA = BuildGradientSlots("GridHeaderBackgroundBrush", 4);
            string[] barB = BuildGradientSlots("GridSelectionBackgroundBrush", 4);

            return new UserConfig
            {
                SelectedPiePaletteId = "A",
                SelectedBarPaletteId = "A",
                BarLabelTextSlot = $"{ThemePrefix}InputForegroundBrush",
                PiePalettes = new ObservableCollection<GraphPalette>
                {
                    new() { Id = "A", Name = string.Format(GetText("SettingsPalettePieNameFormat"), "A"), Colors = pieA.ToList() },
                    new() { Id = "B", Name = string.Format(GetText("SettingsPalettePieNameFormat"), "B"), Colors = pieB.ToList() }
                }.ToList(),
                BarPalettes = new ObservableCollection<GraphPalette>
                {
                    new() { Id = "A", Name = string.Format(GetText("SettingsPaletteBarNameFormat"), "A"), Colors = barA.ToList() },
                    new() { Id = "B", Name = string.Format(GetText("SettingsPaletteBarNameFormat"), "B"), Colors = barB.ToList() }
                }.ToList()
            };
        }

        private void EnsureGraphConfigIntegrity()
        {
            if (_userConfig.PiePalettes.Count == 0 || _userConfig.BarPalettes.Count == 0)
            {
                _userConfig = CreateDefaultGraphConfig();
                return;
            }

            foreach (GraphPalette palette in _userConfig.PiePalettes)
            {
                EnsurePaletteColorCount(palette, 10, BuildGradientSlots("PanelBackgroundBrush", 10));
            }

            foreach (GraphPalette palette in _userConfig.BarPalettes)
            {
                EnsurePaletteColorCount(palette, 4, BuildGradientSlots("GridHeaderBackgroundBrush", 4));
            }

            _userConfig.SelectedPiePaletteId = NormalizePaletteId(_userConfig.SelectedPiePaletteId);
            _userConfig.SelectedBarPaletteId = NormalizePaletteId(_userConfig.SelectedBarPaletteId);
            _userConfig.BarLabelTextSlot = NormalizeSlotValue(_userConfig.BarLabelTextSlot, $"{ThemePrefix}InputForegroundBrush");
        }

        private static void EnsurePaletteColorCount(GraphPalette palette, int expectedCount, string[] defaults)
        {
            if (palette.Colors == null)
            {
                palette.Colors = new System.Collections.Generic.List<string>();
            }

            while (palette.Colors.Count < expectedCount)
            {
                palette.Colors.Add(defaults[Math.Min(palette.Colors.Count, defaults.Length - 1)]);
            }

            if (palette.Colors.Count > expectedCount)
            {
                palette.Colors = palette.Colors.Take(expectedCount).ToList();
            }
        }

        private bool EnsurePaletteDefaults()
        {
            bool changed = false;

            SetIfEmpty("PiePaletteSelectedId", "A", ref changed);
            SetIfEmpty("BarPaletteSelectedId", "A", ref changed);
            SetIfEmpty(nameof(Settings.PiePaletteId), "PaletteA", ref changed);
            SetIfEmpty(nameof(Settings.BarPaletteSelectedId), "PaletteA", ref changed);
            SetIfEmpty(nameof(Settings.HourBarLabelTextKey), $"{ThemePrefix}InputForegroundBrush", ref changed);

            string[] pieA = BuildGradientSlots("PanelBackgroundBrush", 10);
            string[] pieB = BuildGradientSlots("GridSelectionBackgroundBrush", 10);
            string[] barA = BuildGradientSlots("GridHeaderBackgroundBrush", 4);
            string[] barB = BuildGradientSlots("GridSelectionBackgroundBrush", 4);

            for (int i = 0; i < 10; i++)
            {
                SetIfEmpty($"PiePaletteA_ColorKey{i + 1}", pieA[i], ref changed);
                SetIfEmpty($"PiePaletteB_ColorKey{i + 1}", pieB[i], ref changed);
            }

            for (int i = 0; i < 4; i++)
            {
                SetIfEmpty($"BarPaletteA_ColorKey{i + 1}", barA[i], ref changed);
                SetIfEmpty($"BarPaletteB_ColorKey{i + 1}", barB[i], ref changed);
            }

            if (string.IsNullOrWhiteSpace(Settings.Default.PiePaletteAColorKeys))
            {
                Settings.Default.PiePaletteAColorKeys = string.Join("|", pieA);
                changed = true;
            }

            if (string.IsNullOrWhiteSpace(Settings.Default.PiePaletteBColorKeys))
            {
                Settings.Default.PiePaletteBColorKeys = string.Join("|", pieB);
                changed = true;
            }

            if (string.IsNullOrWhiteSpace(Settings.Default.BarPaletteAColorKeys))
            {
                Settings.Default.BarPaletteAColorKeys = string.Join("|", barA);
                changed = true;
            }

            if (string.IsNullOrWhiteSpace(Settings.Default.BarPaletteBColorKeys))
            {
                Settings.Default.BarPaletteBColorKeys = string.Join("|", barB);
                changed = true;
            }

            return changed;
        }

        private string[] BuildGradientSlots(string baseBrushKey, int steps)
        {
            MediaColor baseColor = ResolveColor(baseBrushKey, System.Windows.Media.Colors.Black);
            MediaColor targetColor = System.Windows.Media.Colors.White;
            string[] result = new string[steps];
            if (steps <= 1)
            {
                result[0] = $"{ArgbPrefix}{baseColor}";
                return result;
            }

            for (int i = 0; i < steps; i++)
            {
                double t = (double)i / (steps - 1);
                byte a = (byte)Math.Round(baseColor.A + (targetColor.A - baseColor.A) * t);
                byte r = (byte)Math.Round(baseColor.R + (targetColor.R - baseColor.R) * t);
                byte g = (byte)Math.Round(baseColor.G + (targetColor.G - baseColor.G) * t);
                byte b = (byte)Math.Round(baseColor.B + (targetColor.B - baseColor.B) * t);
                result[i] = $"{ArgbPrefix}{MediaColor.FromArgb(a, r, g, b)}";
            }

            return result;
        }

        private MediaColor ResolveColor(string key, MediaColor fallback)
        {
            if (TryFindResource(key) is SolidColorBrush brush)
            {
                return brush.Color;
            }

            return fallback;
        }

        private void SetIfEmpty(string key, string value, ref bool changed)
        {
            string? current = TryGetRawSettingValue(key);
            if (string.IsNullOrWhiteSpace(current))
            {
                SetSettingValue(key, value);
                changed = true;
            }
        }

        private void SaveSettingsSafe()
        {
            if (_isApplying)
            {
                return;
            }

            try
            {
                Settings.Default.Save();
                _ownerWindow.NotifySettingsChanged();
            }
            catch (Exception ex)
            {
                _ownerWindow.AppendLogFromSettingsWindow($"ERROR: Failed to save settings: {ex.Message}");
            }
        }

        private void QueueGraphConfigSave()
        {
            TxtGraphConfigSaveState.Text = GetText("SettingsGraphSavePending");
            _graphSaveDebounceTimer.Stop();
            _graphSaveDebounceTimer.Start();
        }

        private void GraphSaveDebounceTimer_Tick(object? sender, EventArgs e)
        {
            _graphSaveDebounceTimer.Stop();
            SaveGraphConfigNow();
        }

        private void SaveGraphConfigNow()
        {
            UserConfigService.SaveResult result = UserConfigService.Save(_userConfig);
            if (result.Success)
            {
                _configSavePath = result.SavedPath;
                UpdateGraphSaveStatusSaved();
            }
            else
            {
                TxtGraphConfigSaveState.Text = string.Format(GetText("SettingsGraphSaveFailedFormat"), result.ErrorMessage ?? string.Empty);
                TxtGraphConfigPath.Text = string.Format(GetText("SettingsGraphSavedPathFormat"), _configSavePath);
            }
        }

        private void UpdateGraphSaveStatusSaved()
        {
            TxtGraphConfigSaveState.Text = string.Format(
                GetText("SettingsGraphSavedAtFormat"),
                DateTime.Now.ToString("HH:mm:ss"));
            TxtGraphConfigPath.Text = string.Format(GetText("SettingsGraphSavedPathFormat"), _configSavePath);
        }

        private void LstCategories_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!_isReady)
            {
                return;
            }

            int index = LstCategories?.SelectedIndex ?? -1;
            if (index < 0)
            {
                return;
            }

            if (PanelOutput == null || PanelTimeFormat == null || PanelTheme == null || PanelLanguage == null || PanelAnalysis == null)
            {
                return;
            }

            UpdateCategoryPanels(index);
        }

        private void UpdateCategoryPanels(int index)
        {
            PanelOutput.Visibility = index == 0 ? Visibility.Visible : Visibility.Collapsed;
            PanelTimeFormat.Visibility = index == 1 ? Visibility.Visible : Visibility.Collapsed;
            PanelTheme.Visibility = index == 2 ? Visibility.Visible : Visibility.Collapsed;
            PanelLanguage.Visibility = index == 3 ? Visibility.Visible : Visibility.Collapsed;
            PanelAnalysis.Visibility = index == 4 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void TxtOutputDirSetting_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_isApplying)
            {
                return;
            }

            Settings.Default.OutputDirectory = TxtOutputDirSetting.Text ?? string.Empty;
            SaveSettingsSafe();
        }

        private void BtnBrowseOutputDirSetting_Click(object sender, RoutedEventArgs e)
        {
            using Forms.FolderBrowserDialog dialog = new();
            dialog.ShowNewFolderButton = true;

            string current = TxtOutputDirSetting.Text?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(current) && Directory.Exists(current))
            {
                dialog.InitialDirectory = current;
            }

            if (dialog.ShowDialog() == Forms.DialogResult.OK)
            {
                TxtOutputDirSetting.Text = dialog.SelectedPath;
            }
        }

        private void OutputFlag_Changed(object sender, RoutedEventArgs e)
        {
            if (_isApplying)
            {
                return;
            }

            Settings.Default.WriteJsonl = ChkWriteJsonlSetting.IsChecked == true;
            Settings.Default.WriteCsv = ChkWriteCsvSetting.IsChecked == true;
            Settings.Default.WriteSummary = ChkWriteSummarySetting.IsChecked == true;
            SaveSettingsSafe();
        }

        private void AdvancedFlag_Changed(object sender, RoutedEventArgs e)
        {
            if (_isApplying)
            {
                return;
            }

            Settings.Default.DedupMessage = ChkDedupMessageSetting.IsChecked == true;
            Settings.Default.IncludeXml = ChkIncludeXmlSetting.IsChecked == true;
            Settings.Default.ShowOnlyNew = ChkShowOnlyNewSetting.IsChecked == true;
            SaveSettingsSafe();
        }

        private void RbTimeFormat_Checked(object sender, RoutedEventArgs e)
        {
            if (_isApplying)
            {
                return;
            }

            Settings.Default.TimeFormat = RbTime12.IsChecked == true ? "12h" : "24h";
            SaveSettingsSafe();
        }

        private void RbTheme_Checked(object sender, RoutedEventArgs e)
        {
            if (_isApplying)
            {
                return;
            }

            string theme = RbThemeDark.IsChecked == true ? "Dark" : "Light";
            Settings.Default.UiTheme = theme;
            SaveSettingsSafe();

            if (System.Windows.Application.Current is App app)
            {
                app.ApplyTheme(theme);
            }
        }

        private void RbLanguage_Checked(object sender, RoutedEventArgs e)
        {
            if (_isApplying)
            {
                return;
            }

            string language = RbLanguageJa.IsChecked == true ? "ja" : "en";
            Settings.Default.UiLanguage = language;
            SaveSettingsSafe();

            if (System.Windows.Application.Current is App app)
            {
                app.ApplyLanguage(language);
            }
        }

        private void BtnClearOutputFolderSetting_Click(object sender, RoutedEventArgs e)
        {
            if (!_ownerWindow.ConfirmFromSettingsWindow(
                    "ConfirmClearOutputFolderTitle",
                    "ConfirmClearOutputFolderMessage",
                    "Confirm Clear Output Folder",
                    "Clear the output folder path from settings?"))
            {
                return;
            }

            TxtOutputDirSetting.Text = string.Empty;
        }

        private void BtnResetSettingsSetting_Click(object sender, RoutedEventArgs e)
        {
            if (!_ownerWindow.ConfirmFromSettingsWindow(
                    "ConfirmResetSettingsTitle",
                    "ConfirmResetSettingsMessage",
                    "Confirm Reset Settings",
                    "Reset all user settings to defaults?"))
            {
                return;
            }

            try
            {
                Settings.Default.Reset();
                Settings.Default.Save();
            }
            catch (Exception ex)
            {
                _ownerWindow.AppendLogFromSettingsWindow($"ERROR: Failed to save settings: {ex.Message}");
                return;
            }

            LoadFromSettings();
            _ownerWindow.NotifySettingsChanged();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            if (_graphSaveDebounceTimer.IsEnabled)
            {
                _graphSaveDebounceTimer.Stop();
                SaveGraphConfigNow();
            }
            Close();
        }

        private void BtnAddPiePalette_Click(object sender, RoutedEventArgs e)
        {
            string id = Guid.NewGuid().ToString("N");
            GraphPalette palette = new()
            {
                Id = id,
                Name = string.Format(GetText("SettingsPalettePieNameFormat"), _userConfig.PiePalettes.Count + 1),
                Colors = BuildGradientSlots("PanelBackgroundBrush", 10).ToList()
            };
            _userConfig.PiePalettes.Add(palette);
            BuildPaletteChoices();
            _selectedPiePaletteId = id;
            CmbPiePalette.SelectedValue = id;
            LoadPiePaletteSlots();
            UpdateGraphColorPreviews();
            QueueGraphConfigSave();
        }

        private void BtnClonePiePalette_Click(object sender, RoutedEventArgs e)
        {
            GraphPalette? source = _userConfig.PiePalettes.FirstOrDefault(p => p.Id == _selectedPiePaletteId);
            if (source == null)
            {
                return;
            }

            string id = Guid.NewGuid().ToString("N");
            GraphPalette cloned = new()
            {
                Id = id,
                Name = $"{source.Name} {GetText("SettingsPaletteCloneSuffix")}",
                Colors = source.Colors.ToList()
            };
            _userConfig.PiePalettes.Add(cloned);
            BuildPaletteChoices();
            _selectedPiePaletteId = id;
            CmbPiePalette.SelectedValue = id;
            LoadPiePaletteSlots();
            UpdateGraphColorPreviews();
            QueueGraphConfigSave();
        }

        private void BtnDeletePiePalette_Click(object sender, RoutedEventArgs e)
        {
            if (_userConfig.PiePalettes.Count <= 1)
            {
                return;
            }

            GraphPalette? source = _userConfig.PiePalettes.FirstOrDefault(p => p.Id == _selectedPiePaletteId);
            if (source == null)
            {
                return;
            }

            _userConfig.PiePalettes.Remove(source);
            _selectedPiePaletteId = _userConfig.PiePalettes.First().Id;
            _userConfig.SelectedPiePaletteId = _selectedPiePaletteId;
            BuildPaletteChoices();
            CmbPiePalette.SelectedValue = _selectedPiePaletteId;
            LoadPiePaletteSlots();
            UpdateGraphColorPreviews();
            QueueGraphConfigSave();
        }

        private void BtnAddBarPalette_Click(object sender, RoutedEventArgs e)
        {
            string id = Guid.NewGuid().ToString("N");
            GraphPalette palette = new()
            {
                Id = id,
                Name = string.Format(GetText("SettingsPaletteBarNameFormat"), _userConfig.BarPalettes.Count + 1),
                Colors = BuildGradientSlots("GridHeaderBackgroundBrush", 4).ToList()
            };
            _userConfig.BarPalettes.Add(palette);
            BuildPaletteChoices();
            _selectedBarPaletteId = id;
            CmbBarPalette.SelectedValue = id;
            LoadBarPaletteSlots();
            UpdateGraphColorPreviews();
            QueueGraphConfigSave();
        }

        private void BtnCloneBarPalette_Click(object sender, RoutedEventArgs e)
        {
            GraphPalette? source = _userConfig.BarPalettes.FirstOrDefault(p => p.Id == _selectedBarPaletteId);
            if (source == null)
            {
                return;
            }

            string id = Guid.NewGuid().ToString("N");
            GraphPalette cloned = new()
            {
                Id = id,
                Name = $"{source.Name} {GetText("SettingsPaletteCloneSuffix")}",
                Colors = source.Colors.ToList()
            };
            _userConfig.BarPalettes.Add(cloned);
            BuildPaletteChoices();
            _selectedBarPaletteId = id;
            CmbBarPalette.SelectedValue = id;
            LoadBarPaletteSlots();
            UpdateGraphColorPreviews();
            QueueGraphConfigSave();
        }

        private void BtnDeleteBarPalette_Click(object sender, RoutedEventArgs e)
        {
            if (_userConfig.BarPalettes.Count <= 1)
            {
                return;
            }

            GraphPalette? source = _userConfig.BarPalettes.FirstOrDefault(p => p.Id == _selectedBarPaletteId);
            if (source == null)
            {
                return;
            }

            _userConfig.BarPalettes.Remove(source);
            _selectedBarPaletteId = _userConfig.BarPalettes.First().Id;
            _userConfig.SelectedBarPaletteId = _selectedBarPaletteId;
            BuildPaletteChoices();
            CmbBarPalette.SelectedValue = _selectedBarPaletteId;
            LoadBarPaletteSlots();
            UpdateGraphColorPreviews();
            QueueGraphConfigSave();
        }

        private void UpdateGraphColorPreviews()
        {
            PiePalettePreviewSwatches.Clear();
            foreach (PaletteSlot slot in PiePaletteSlots)
            {
                PiePalettePreviewSwatches.Add(slot.Brush);
            }

            BarNormalPreviewBrush = BarPaletteSlots.Count > 0
                ? BarPaletteSlots[0].Brush
                : ResolveBrush("GridHeaderBackgroundBrush", "GridHeaderBackgroundBrush");
            BarPeakPreviewBrush = BarPaletteSlots.Count > 1
                ? BarPaletteSlots[1].Brush
                : BarNormalPreviewBrush;
            BarLabelPreviewBrush = ResolveBrushFromSlot(_selectedHourLabelTextKey, "InputForegroundBrush");

            ApplyChoiceSelection(HourLabelColorChoices, ExtractThemeKeyOrFallback(_selectedHourLabelTextKey, "InputForegroundBrush"));
        }

        private void BuildGraphColorChoices()
        {
            BuildChoiceList(GraphColorChoices, GraphColorChoiceKeys);
        }

        private void BuildPaletteChoices()
        {
            _isGraphBindingUpdating = true;
            try
            {
                PiePaletteChoices.Clear();
                foreach (GraphPalette palette in _userConfig.PiePalettes)
                {
                    PiePaletteChoices.Add(new PaletteChoice(palette.Id, palette.Name));
                }

                BarPaletteChoices.Clear();
                foreach (GraphPalette palette in _userConfig.BarPalettes)
                {
                    BarPaletteChoices.Add(new PaletteChoice(palette.Id, palette.Name));
                }

                CmbPiePalette.ItemsSource = PiePaletteChoices;
                CmbBarPalette.ItemsSource = BarPaletteChoices;
            }
            finally
            {
                _isGraphBindingUpdating = false;
            }
        }

        private void BuildHourLabelColorChoices()
        {
            BuildChoiceList(HourLabelColorChoices, new[]
            {
                "InputForegroundBrush",
                "ForegroundBrush",
                "PanelBackgroundBrush",
                "GridHeaderBackgroundBrush"
            });
        }

        private void BuildChoiceList(ObservableCollection<ColorChoice> collection, string[] keys)
        {
            collection.Clear();
            foreach (string key in keys)
            {
                collection.Add(new ColorChoice(key, ResolveBrush(key, "BorderBrush")));
            }
        }

        private static void ApplyChoiceSelection(ObservableCollection<ColorChoice> choices, string selectedKey)
        {
            foreach (ColorChoice choice in choices)
            {
                choice.IsSelected = string.Equals(choice.Key, selectedKey, StringComparison.Ordinal);
            }
        }

        private void LoadPiePaletteSlots()
        {
            string[] keys = GetSelectedPaletteColors(_userConfig.PiePalettes, _selectedPiePaletteId, 10, BuildGradientSlots("PanelBackgroundBrush", 10));
            ReplaceSlots(PiePaletteSlots, "Pie", keys);
        }

        private void LoadBarPaletteSlots()
        {
            string[] keys = GetSelectedPaletteColors(_userConfig.BarPalettes, _selectedBarPaletteId, 4, BuildGradientSlots("GridHeaderBackgroundBrush", 4));
            ReplaceSlots(BarPaletteSlots, "Bar", keys);

            if (BarPaletteSlots.Count > 0)
            {
                Settings.Default.HourBarFillKey = ExtractThemeKeyOrFallback(BarPaletteSlots[0].BrushKey, "GridHeaderBackgroundBrush");
            }

            if (BarPaletteSlots.Count > 1)
            {
                Settings.Default.HourPeakFillKey = ExtractThemeKeyOrFallback(BarPaletteSlots[1].BrushKey, "GridSelectionBackgroundBrush");
            }
        }

        private static string[] GetSelectedPaletteColors(
            System.Collections.Generic.List<GraphPalette> palettes,
            string selectedId,
            int expectedCount,
            string[] defaults)
        {
            GraphPalette? palette = palettes.FirstOrDefault(p => string.Equals(p.Id, selectedId, StringComparison.OrdinalIgnoreCase))
                ?? palettes.FirstOrDefault();
            if (palette == null)
            {
                return defaults;
            }

            string[] result = new string[expectedCount];
            for (int i = 0; i < expectedCount; i++)
            {
                string fallback = defaults[Math.Min(i, defaults.Length - 1)];
                string value = i < palette.Colors.Count ? palette.Colors[i] : fallback;
                result[i] = NormalizeSlotValue(value, fallback);
            }

            return result;
        }

        private void ReplaceSlots(ObservableCollection<PaletteSlot> slots, string paletteType, string[] keys)
        {
            slots.Clear();
            for (int i = 0; i < keys.Length; i++)
            {
                string slotValue = keys[i];
                slots.Add(new PaletteSlot(paletteType, i, slotValue, ResolveBrushFromSlot(slotValue, "BorderBrush")));
            }
        }

        private string[] GetPiePaletteKeys(string paletteId)
        {
            string normalized = NormalizePaletteId(paletteId);
            string[] defaults = normalized == "B" ? DefaultPiePaletteB : DefaultPiePaletteA;
            string prefix = normalized == "B" ? "PiePaletteB_ColorKey" : "PiePaletteA_ColorKey";
            string legacyRaw = normalized == "B" ? Settings.Default.PiePaletteBColorKeys : Settings.Default.PiePaletteAColorKeys;
            string[] legacy = ParsePaletteKeys(legacyRaw, 10, defaults);
            string[] result = new string[10];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = NormalizeSlotValue(GetSettingValue($"{prefix}{i + 1}", legacy[i]), defaults[i]);
            }

            return result;
        }

        private string[] GetBarPaletteKeys(string paletteId)
        {
            string normalized = NormalizePaletteId(paletteId);
            string raw = normalized == "B"
                ? Settings.Default.BarPaletteBColorKeys
                : Settings.Default.BarPaletteAColorKeys;
            string[] defaults = normalized == "B" ? DefaultBarPaletteB : DefaultBarPaletteA;
            string[] keys = ParsePaletteKeys(raw, 4, defaults);
            string prefix = normalized == "B" ? "BarPaletteB_ColorKey" : "BarPaletteA_ColorKey";
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i] = NormalizeSlotValue(GetSettingValue($"{prefix}{i + 1}", keys[i]), defaults[i]);
            }

            if (string.IsNullOrWhiteSpace(raw))
            {
                string legacyBar = string.IsNullOrWhiteSpace(Settings.Default.HourBarFillKey)
                    ? defaults[0]
                    : Settings.Default.HourBarFillKey;
                string legacyPeak = string.IsNullOrWhiteSpace(Settings.Default.HourPeakFillKey)
                    ? defaults[1]
                    : Settings.Default.HourPeakFillKey;
                keys[0] = NormalizeSlotValue(legacyBar, defaults[0]);
                keys[1] = NormalizeSlotValue(legacyPeak, defaults[1]);
            }

            return keys;
        }

        private static string[] ParsePaletteKeys(string? raw, int count, string[] defaults)
        {
            string[] parsed = string.IsNullOrWhiteSpace(raw)
                ? Array.Empty<string>()
                : raw.Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            string[] result = new string[count];
            for (int i = 0; i < count; i++)
            {
                if (i < parsed.Length && !string.IsNullOrWhiteSpace(parsed[i]))
                {
                    result[i] = parsed[i];
                }
                else if (i < defaults.Length)
                {
                    result[i] = defaults[i];
                }
                else
                {
                    result[i] = defaults[0];
                }
            }

            return result;
        }

        private void SavePiePaletteSlots()
        {
            GraphPalette? palette = _userConfig.PiePalettes
                .FirstOrDefault(p => string.Equals(p.Id, _selectedPiePaletteId, StringComparison.OrdinalIgnoreCase));
            if (palette == null)
            {
                return;
            }

            palette.Colors = PiePaletteSlots.Select(slot => NormalizeSlotValue(slot.BrushKey, $"{ThemePrefix}BorderBrush")).ToList();
            _userConfig.SelectedPiePaletteId = _selectedPiePaletteId;
            QueueGraphConfigSave();
        }

        private void SaveBarPaletteSlots()
        {
            GraphPalette? palette = _userConfig.BarPalettes
                .FirstOrDefault(p => string.Equals(p.Id, _selectedBarPaletteId, StringComparison.OrdinalIgnoreCase));
            if (palette == null)
            {
                return;
            }

            palette.Colors = BarPaletteSlots.Select(slot => NormalizeSlotValue(slot.BrushKey, $"{ThemePrefix}BorderBrush")).ToList();
            _userConfig.SelectedBarPaletteId = _selectedBarPaletteId;
            QueueGraphConfigSave();

            if (BarPaletteSlots.Count > 0)
            {
                Settings.Default.HourBarFillKey = ExtractThemeKeyOrFallback(BarPaletteSlots[0].BrushKey, "GridHeaderBackgroundBrush");
            }

            if (BarPaletteSlots.Count > 1)
            {
                Settings.Default.HourPeakFillKey = ExtractThemeKeyOrFallback(BarPaletteSlots[1].BrushKey, "GridSelectionBackgroundBrush");
            }
        }

        private MediaBrush ResolveBrush(string key, string fallbackKey)
        {
            if (TryFindResource(key) is SolidColorBrush selectedBrush)
            {
                return selectedBrush;
            }

            if (TryFindResource(fallbackKey) is SolidColorBrush fallbackBrush)
            {
                return fallbackBrush;
            }

            return new SolidColorBrush(System.Windows.Media.Colors.Transparent);
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void PaletteSlotChoice_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button button ||
                button.DataContext is not ColorChoice selected ||
                button.Tag is not PaletteSlot slot)
            {
                return;
            }

            slot.BrushKey = $"{ThemePrefix}{selected.Key}";
            slot.Brush = selected.Brush;
            slot.IsPopupOpen = false;

            if (string.Equals(slot.PaletteType, "Pie", StringComparison.Ordinal))
            {
                SavePiePaletteSlots();
            }
            else
            {
                SaveBarPaletteSlots();
            }

            UpdateGraphColorPreviews();
            QueueGraphConfigSave();
        }

        private void PaletteSlotCustomColor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not System.Windows.Controls.Button button || button.Tag is not PaletteSlot slot)
            {
                return;
            }
            OpenCustomColorDialogForSlot(slot);
        }

        private void PiePalette_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_isApplying || _isGraphBindingUpdating)
            {
                return;
            }

            _selectedPiePaletteId = NormalizePaletteId(CmbPiePalette.SelectedValue as string);
            _userConfig.SelectedPiePaletteId = _selectedPiePaletteId;
            LoadPiePaletteSlots();
            UpdateGraphColorPreviews();
            QueueGraphConfigSave();
        }

        private void BarPalette_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_isApplying || _isGraphBindingUpdating)
            {
                return;
            }

            _selectedBarPaletteId = NormalizePaletteId(CmbBarPalette.SelectedValue as string);
            _userConfig.SelectedBarPaletteId = _selectedBarPaletteId;
            LoadBarPaletteSlots();
            UpdateGraphColorPreviews();
            QueueGraphConfigSave();
        }

        private void HourLabelColorSwatch_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.Tag is string key && !string.IsNullOrWhiteSpace(key))
            {
                _selectedHourLabelTextKey = NormalizeSlotValue($"{ThemePrefix}{key}", $"{ThemePrefix}InputForegroundBrush");
                Settings.Default.HourBarLabelTextKey = _selectedHourLabelTextKey;
                _userConfig.BarLabelTextSlot = _selectedHourLabelTextKey;
                UpdateGraphColorPreviews();
                QueueGraphConfigSave();
            }

            TglHourLabelColorPicker.IsChecked = false;
        }

        private void HourLabelCustomColor_Click(object sender, RoutedEventArgs e)
        {
            using Forms.ColorDialog dialog = new();
            dialog.FullOpen = true;
            dialog.AnyColor = true;
            dialog.Color = ToFormsColor(BarLabelPreviewBrush);

            if (dialog.ShowDialog() != Forms.DialogResult.OK)
            {
                return;
            }

            MediaColor selectedColor = MediaColor.FromArgb(dialog.Color.A, dialog.Color.R, dialog.Color.G, dialog.Color.B);
            _selectedHourLabelTextKey = $"{ArgbPrefix}{selectedColor}";
            Settings.Default.HourBarLabelTextKey = _selectedHourLabelTextKey;
            _userConfig.BarLabelTextSlot = _selectedHourLabelTextKey;
            UpdateGraphColorPreviews();
            QueueGraphConfigSave();
            TglHourLabelColorPicker.IsChecked = false;
        }

        private void BtnChangePieColor_Click(object sender, RoutedEventArgs e)
        {
            PaletteSlot? target = PiePaletteSlots.FirstOrDefault();
            if (target == null)
            {
                return;
            }

            OpenCustomColorDialogForSlot(target);
        }

        private void BtnChangeBarColor_Click(object sender, RoutedEventArgs e)
        {
            PaletteSlot? target = BarPaletteSlots.FirstOrDefault();
            if (target == null)
            {
                return;
            }

            OpenCustomColorDialogForSlot(target);
        }

        private static string NormalizePaletteId(string? paletteId)
        {
            if (string.IsNullOrWhiteSpace(paletteId))
            {
                return "A";
            }

            if (string.Equals(paletteId, "B", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(paletteId, "PaletteB", StringComparison.OrdinalIgnoreCase))
            {
                return "B";
            }

            return "A";
        }

        private static string ToPaletteTag(string paletteId)
        {
            return string.Equals(paletteId, "B", StringComparison.OrdinalIgnoreCase) ? "PaletteB" : "PaletteA";
        }

        private static string GetSettingValue(string key, string fallback)
        {
            try
            {
                object? value = Settings.Default[key];
                if (value is string s && !string.IsNullOrWhiteSpace(s))
                {
                    return s;
                }
            }
            catch (SettingsPropertyNotFoundException)
            {
                System.Diagnostics.Debug.WriteLine($"Missing settings key: {key}");
            }

            return fallback;
        }

        private static string? TryGetRawSettingValue(string key)
        {
            try
            {
                return Settings.Default[key] as string;
            }
            catch (SettingsPropertyNotFoundException)
            {
                System.Diagnostics.Debug.WriteLine($"Missing settings key: {key}");
                return null;
            }
        }

        private static void SetSettingValue(string key, string value)
        {
            Settings.Default[key] = value;
        }

        private MediaBrush ResolveBrushFromSlot(string slotValue, string fallbackKey)
        {
            string normalized = NormalizeSlotValue(slotValue, $"{ThemePrefix}{fallbackKey}");
            if (normalized.StartsWith(ThemePrefix, StringComparison.OrdinalIgnoreCase))
            {
                string key = normalized[ThemePrefix.Length..];
                return ResolveBrush(key, fallbackKey);
            }

            if (normalized.StartsWith(ArgbPrefix, StringComparison.OrdinalIgnoreCase))
            {
                string argb = normalized[ArgbPrefix.Length..];
                try
                {
                    object? colorObj = System.Windows.Media.ColorConverter.ConvertFromString(argb);
                    if (colorObj is MediaColor color)
                    {
                        return new SolidColorBrush(color);
                    }
                }
                catch
                {
                    // fall through to fallback
                }
            }

            return ResolveBrush(fallbackKey, fallbackKey);
        }

        private static string NormalizeSlotValue(string? slotValue, string fallbackSlotValue)
        {
            string fallback = string.IsNullOrWhiteSpace(fallbackSlotValue)
                ? $"{ThemePrefix}BorderBrush"
                : fallbackSlotValue;

            if (string.IsNullOrWhiteSpace(slotValue))
            {
                return fallback;
            }

            if (slotValue.StartsWith(ThemePrefix, StringComparison.OrdinalIgnoreCase) ||
                slotValue.StartsWith(ArgbPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return slotValue;
            }

            return $"{ThemePrefix}{slotValue}";
        }

        private static System.Drawing.Color ToFormsColor(MediaBrush brush)
        {
            if (brush is SolidColorBrush solid)
            {
                return System.Drawing.Color.FromArgb(solid.Color.A, solid.Color.R, solid.Color.G, solid.Color.B);
            }

            return System.Drawing.Color.White;
        }

        private static string ExtractThemeKeyOrFallback(string slotValue, string fallbackKey)
        {
            if (string.IsNullOrWhiteSpace(slotValue))
            {
                return fallbackKey;
            }

            if (slotValue.StartsWith(ThemePrefix, StringComparison.OrdinalIgnoreCase))
            {
                string key = slotValue[ThemePrefix.Length..];
                return string.IsNullOrWhiteSpace(key) ? fallbackKey : key;
            }

            return fallbackKey;
        }

        private void OpenCustomColorDialogForSlot(PaletteSlot slot)
        {
            using Forms.ColorDialog dialog = new();
            dialog.FullOpen = true;
            dialog.AnyColor = true;
            dialog.Color = ToFormsColor(slot.Brush);

            if (dialog.ShowDialog() != Forms.DialogResult.OK)
            {
                return;
            }

            MediaColor selectedColor = MediaColor.FromArgb(dialog.Color.A, dialog.Color.R, dialog.Color.G, dialog.Color.B);
            slot.BrushKey = $"{ArgbPrefix}{selectedColor}";
            slot.Brush = new SolidColorBrush(selectedColor);
            slot.IsPopupOpen = false;

            if (string.Equals(slot.PaletteType, "Pie", StringComparison.Ordinal))
            {
                SavePiePaletteSlots();
            }
            else
            {
                SaveBarPaletteSlots();
            }

            UpdateGraphColorPreviews();
            QueueGraphConfigSave();
        }

        private string GetText(string key)
        {
            return TryFindResource(key) as string ?? key;
        }
    }
}
