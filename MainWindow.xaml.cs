using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using LogAnalyzer.Models;
using LogAnalyzer.Properties;
using LogAnalyzer.Services;
using LogAnalyzer.Views;
using Forms = System.Windows.Forms;

namespace LogAnalyzer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const int MaxPreviewEvents = 100000;
        private List<EventRecordInfo> _lastCollectedNormalized = new();
        private bool _isApplyingSettings;
        private string _timeFormat = "24h";
        private AnalysisResult _currentAnalysis = new();
        private AnalysisWindow? _analysisWindow;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;

            BtnCollect.Click += BtnCollect_Click;
            BtnAnalyze.Click += (_, _) =>
            {
                OnSimpleButtonClicked("Analyze");
                RefreshAnalysis(openWindow: true);
            };
            BtnRunAll.Click += (_, _) => OnSimpleButtonClicked("Run All");
            BtnExport.Click += BtnExport_Click;
            BtnCancel.Click += BtnCancel_Click;
            BtnRefreshAnalysis.Click += (_, _) => RefreshAnalysis(openWindow: true);
            BtnOpenAnalysisWindow.Click += (_, _) => OpenOrUpdateAnalysisWindow();
            BtnCopyEventJson.Click += BtnCopyEventJson_Click;
            BtnCopyMessage.Click += BtnCopyMessage_Click;
            GridPreview.SelectionChanged += GridPreview_SelectionChanged;
            CmbSort.SelectionChanged += CmbSort_SelectionChanged;
            TabRight.SelectionChanged += TabRight_SelectionChanged;

            BtnQuick1h.Click += (_, _) => ApplyQuickRange(TimeSpan.FromHours(1));
            BtnQuick24h.Click += (_, _) => ApplyQuickRange(TimeSpan.FromHours(24));
            BtnQuick7d.Click += (_, _) => ApplyQuickRange(TimeSpan.FromDays(7));
            BtnQuick30d.Click += (_, _) => ApplyQuickRange(TimeSpan.FromDays(30));

            BtnOpenOutputFolder.Click += BtnOpenOutputFolder_Click;
            BtnOpenSelectedOutput.Click += BtnOpenSelectedOutput_Click;

            MenuOpenSettings.Click += MenuOpenSettings_Click;
            MenuExit.Click += (_, _) => Close();
            MenuAbout.Click += MenuAbout_Click;

            MenuLangEnglish.Click += (_, _) => ChangeLanguage("en");
            MenuLangJapanese.Click += (_, _) => ChangeLanguage("ja");
            MenuTimeFormat24.Click += (_, _) => ChangeTimeFormat("24h");
            MenuTimeFormat12.Click += (_, _) => ChangeTimeFormat("12h");

            MenuThemeLight.Click += (_, _) => ChangeTheme("Light");
            MenuThemeDark.Click += (_, _) => ChangeTheme("Dark");

            ChkSystem.Checked += FilterControl_Changed;
            ChkSystem.Unchecked += FilterControl_Changed;
            ChkApplication.Checked += FilterControl_Changed;
            ChkApplication.Unchecked += FilterControl_Changed;
            ChkSetup.Checked += FilterControl_Changed;
            ChkSetup.Unchecked += FilterControl_Changed;
            ChkSecurity.Checked += FilterControl_Changed;
            ChkSecurity.Unchecked += FilterControl_Changed;
            ChkForwardedEvents.Checked += FilterControl_Changed;
            ChkForwardedEvents.Unchecked += FilterControl_Changed;
            ChkCritical.Checked += FilterControl_Changed;
            ChkCritical.Unchecked += FilterControl_Changed;
            ChkError.Checked += FilterControl_Changed;
            ChkError.Unchecked += FilterControl_Changed;
            ChkWarning.Checked += FilterControl_Changed;
            ChkWarning.Unchecked += FilterControl_Changed;
            ChkInformation.Checked += FilterControl_Changed;
            ChkInformation.Unchecked += FilterControl_Changed;

            TxtTopN.TextChanged += TxtTopN_TextChanged;
            DpStart.SelectedDateChanged += TimeRange_Changed;
            DpEnd.SelectedDateChanged += TimeRange_Changed;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ApplySettingsToUi();
            RefreshAnalysis();
            UpdateSummaryText();
        }

        private void ApplySettingsToUi()
        {
            _isApplyingSettings = true;
            try
            {
                ChkSystem.IsChecked = Settings.Default.LastLogsSystem;
                ChkApplication.IsChecked = Settings.Default.LastLogsApplication;
                ChkCritical.IsChecked = Settings.Default.LastLevelCritical;
                ChkError.IsChecked = Settings.Default.LastLevelError;
                ChkWarning.IsChecked = Settings.Default.LastLevelWarning;
                ChkInformation.IsChecked = Settings.Default.LastLevelInformation;

                TxtTopN.Text = Settings.Default.LastTopN > 0 ? Settings.Default.LastTopN.ToString() : "500";

                if (Settings.Default.LastUseTimeRange)
                {
                    if (Settings.Default.LastStartTime != DateTime.MinValue)
                    {
                        DpStart.SelectedDate = Settings.Default.LastStartTime;
                    }

                    if (Settings.Default.LastEndTime != DateTime.MinValue)
                    {
                        DpEnd.SelectedDate = Settings.Default.LastEndTime;
                    }
                }
                else
                {
                    DpStart.SelectedDate = null;
                    DpEnd.SelectedDate = null;
                }

                if (System.Windows.Application.Current is App app)
                {
                    app.ApplyTheme(Settings.Default.UiTheme);
                    app.ApplyLanguage(Settings.Default.UiLanguage);
                }

                _timeFormat = NormalizeTimeFormat(Settings.Default.TimeFormat);
                RefreshThemeMenuChecks();
                RefreshLanguageMenuChecks();
                RefreshTimeFormatMenuChecks();
                RebuildPreviewRows();
                RefreshAnalysis();
            }
            finally
            {
                _isApplyingSettings = false;
            }
        }

        private void ChangeLanguage(string language)
        {
            string normalized = string.Equals(language, "ja", StringComparison.OrdinalIgnoreCase) ? "ja" : "en";
            Settings.Default.UiLanguage = normalized;
            SaveSettingsSafe();

            if (System.Windows.Application.Current is App app)
            {
                app.ApplyLanguage(normalized);
            }

            RefreshLanguageMenuChecks();
        }

        private void RefreshLanguageMenuChecks()
        {
            bool isJapanese = string.Equals(Settings.Default.UiLanguage, "ja", StringComparison.OrdinalIgnoreCase);
            MenuLangJapanese.IsChecked = isJapanese;
            MenuLangEnglish.IsChecked = !isJapanese;
        }

        private void ChangeTimeFormat(string format)
        {
            string normalized = NormalizeTimeFormat(format);
            _timeFormat = normalized;
            Settings.Default.TimeFormat = normalized;
            SaveSettingsSafe();
            RefreshTimeFormatMenuChecks();
            RebuildPreviewRows();
        }

        private static string NormalizeTimeFormat(string? format)
        {
            return string.Equals(format, "12h", StringComparison.OrdinalIgnoreCase) ? "12h" : "24h";
        }

        private void RefreshTimeFormatMenuChecks()
        {
            bool is12h = string.Equals(_timeFormat, "12h", StringComparison.OrdinalIgnoreCase);
            MenuTimeFormat12.IsChecked = is12h;
            MenuTimeFormat24.IsChecked = !is12h;
        }

        private void ChangeTheme(string theme)
        {
            string normalized = string.Equals(theme, "Dark", StringComparison.OrdinalIgnoreCase) ? "Dark" : "Light";
            Settings.Default.UiTheme = normalized;
            SaveSettingsSafe();

            if (System.Windows.Application.Current is App app)
            {
                app.ApplyTheme(normalized);
            }

            RefreshThemeMenuChecks();
        }

        private void RefreshThemeMenuChecks()
        {
            bool isDark = string.Equals(Settings.Default.UiTheme, "Dark", StringComparison.OrdinalIgnoreCase);
            MenuThemeDark.IsChecked = isDark;
            MenuThemeLight.IsChecked = !isDark;
        }

        private void SetStatus(string text)
        {
            try
            {
                StatusText.Text = text ?? string.Empty;
            }
            catch
            {
                // Intentionally swallow to keep UI interactions safe.
            }
        }

        private void AppendRunLog(string message)
        {
            try
            {
                string line = $"[{DateTime.Now:HH:mm:ss}] {message ?? string.Empty}";
                if (!string.IsNullOrEmpty(TxtRunLog.Text))
                {
                    TxtRunLog.AppendText(Environment.NewLine);
                }

                TxtRunLog.AppendText(line);
                TxtRunLog.ScrollToEnd();
            }
            catch
            {
                // Intentionally swallow to avoid throwing from UI logging.
            }
        }

        private void OnSimpleButtonClicked(string buttonLabel)
        {
            string message = $"{buttonLabel} clicked";
            SetStatus(message);
            AppendRunLog(message);
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            SetStatus("Cancel clicked");
            AppendRunLog("Cancel clicked (MVP-1: cancellation is not implemented yet)");
        }

        private async void BtnCollect_Click(object sender, RoutedEventArgs e)
        {
            if (!TryBuildCollectRequest(out CollectRequest request))
            {
                return;
            }

            SetStatus("Collecting...");
            AppendRunLog(
                $"Collecting events: Logs={string.Join(",", request.LogNames)}; Levels={string.Join(",", request.LevelNames)}; Range={request.StartTime:yyyy-MM-dd HH:mm:ss}..{request.EndTime:yyyy-MM-dd HH:mm:ss}; Max={request.MaxEvents}");
            SetCollectingUiState(true);

            try
            {
                EventLogCollector collector = new();
                (List<EventRecordInfo> normalizedEvents, List<string> skippedLogs) = await Task.Run(() =>
                {
                    List<EventRecordInfo> combined = new();
                    List<string> skipped = new();
                    foreach (string logName in request.LogNames)
                    {
                        try
                        {
                            combined.AddRange(collector.Collect(logName, request.StartTime, request.EndTime, request.LevelValues, request.MaxEvents));
                        }
                        catch (Exception ex)
                        {
                            skipped.Add($"{logName}::{ex.Message}");
                        }
                    }

                    List<EventRecordInfo> result = combined
                        .OrderByDescending(static row => row.TimeCreated)
                        .Take(request.MaxEvents)
                        .ToList();
                    return (result, skipped);
                });

                foreach (string skipped in skippedLogs)
                {
                    string[] parts = skipped.Split(new[] { "::" }, 2, StringSplitOptions.None);
                    string logName = parts[0];
                    string reason = parts.Length > 1 ? parts[1] : string.Empty;
                    string format = GetResourceString("CollectSkippedLogFormat", "Skipped log '{0}': {1}");
                    AppendRunLog(string.Format(System.Globalization.CultureInfo.CurrentUICulture, format, logName, reason));
                }

                _lastCollectedNormalized = normalizedEvents;
                List<EventRow> rows = normalizedEvents.Select(ToDisplayRow).ToList();
                GridPreview.ItemsSource = rows;
                ApplyCurrentSort();
                StatusEventCount.Text = rows.Count.ToString();

                SetStatus($"Collected: {rows.Count}");
                AppendRunLog($"Collected {rows.Count} events");
                RefreshAnalysis();
                UpdateSummaryText();
            }
            catch (Exception ex)
            {
                AppendRunLog($"ERROR: {ex.Message}");
                SetStatus("Error");
            }
            finally
            {
                SetCollectingUiState(false);
            }
        }

        private void SetCollectingUiState(bool isCollecting)
        {
            BtnCollect.IsEnabled = !isCollecting;
            BtnAnalyze.IsEnabled = !isCollecting;
            BtnRunAll.IsEnabled = !isCollecting;
            BtnCancel.IsEnabled = isCollecting;
        }

        private void GridPreview_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (GridPreview.SelectedItem is not EventRow row)
            {
                TxtEventDetail.Text = string.Empty;
                return;
            }

            StringBuilder sb = new();
            sb.AppendLine($"TimeCreated: {row.TimeCreated:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Level: {row.Level}");
            sb.AppendLine($"ProviderName: {row.ProviderName}");
            sb.AppendLine($"Id: {row.Id}");
            sb.AppendLine($"MessageShort: {row.MessageShort}");
            sb.AppendLine("Message:");
            sb.AppendLine(row.Message);

            TxtEventDetail.Text = sb.ToString();
            if (TabRight.SelectedIndex == 1)
            {
                UpdateSummaryText();
            }
        }

        private void CmbSort_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ApplyCurrentSort();
        }

        private void ApplyCurrentSort()
        {
            if (GridPreview.ItemsSource == null)
            {
                return;
            }

            ICollectionView view = CollectionViewSource.GetDefaultView(GridPreview.ItemsSource);
            if (view == null)
            {
                return;
            }

            using (view.DeferRefresh())
            {
                view.SortDescriptions.Clear();
                switch (CmbSort.SelectedIndex)
                {
                    case 1:
                        view.SortDescriptions.Add(new SortDescription(nameof(EventRow.TimeCreated), ListSortDirection.Ascending));
                        break;
                    case 2:
                        view.SortDescriptions.Add(new SortDescription(nameof(EventRow.Level), ListSortDirection.Ascending));
                        view.SortDescriptions.Add(new SortDescription(nameof(EventRow.TimeCreated), ListSortDirection.Descending));
                        break;
                    case 3:
                        view.SortDescriptions.Add(new SortDescription(nameof(EventRow.ProviderName), ListSortDirection.Ascending));
                        view.SortDescriptions.Add(new SortDescription(nameof(EventRow.TimeCreated), ListSortDirection.Descending));
                        break;
                    case 4:
                        view.SortDescriptions.Add(new SortDescription(nameof(EventRow.Id), ListSortDirection.Ascending));
                        view.SortDescriptions.Add(new SortDescription(nameof(EventRow.TimeCreated), ListSortDirection.Descending));
                        break;
                    default:
                        view.SortDescriptions.Add(new SortDescription(nameof(EventRow.TimeCreated), ListSortDirection.Descending));
                        break;
                }
            }
        }

        private static EventRow ToEventRow(EventRecordInfo item)
        {
            string oneLine = (item.Message ?? string.Empty).Replace('\n', ' ');
            string shortMessage = oneLine.Length > 200 ? $"{oneLine[..200]}..." : oneLine;

            return new EventRow
            {
                TimeCreated = item.TimeCreated.LocalDateTime,
                TimeDisplay = item.TimeCreated.LocalDateTime.ToString("yyyy/MM/dd HH:mm:ss"),
                Level = item.Level ?? string.Empty,
                ProviderName = item.ProviderName ?? string.Empty,
                Id = item.Id,
                Message = item.Message ?? string.Empty,
                MessageShort = shortMessage
            };
        }

        private EventRow ToDisplayRow(EventRecordInfo item)
        {
            EventRow row = ToEventRow(item);
            row.TimeDisplay = FormatTime(row.TimeCreated);
            return row;
        }

        private string FormatTime(DateTime time)
        {
            return string.Equals(_timeFormat, "12h", StringComparison.OrdinalIgnoreCase)
                ? time.ToString("M/d/yyyy h:mm:ss tt")
                : time.ToString("yyyy/MM/dd HH:mm:ss");
        }

        private void RebuildPreviewRows()
        {
            if (_lastCollectedNormalized.Count == 0)
            {
                return;
            }

            GridPreview.ItemsSource = _lastCollectedNormalized.Select(ToDisplayRow).ToList();
            ApplyCurrentSort();
        }

        private async Task WriteJsonlAsync(List<EventRecordInfo> normalizedEvents, string batchId)
        {
            string outputDirectory = ResolveOutputDirectory();
            Directory.CreateDirectory(outputDirectory);

            string fileName = BuildUniqueFileName(outputDirectory, "events", batchId, ".jsonl");
            string fullPath = Path.Combine(outputDirectory, fileName);
            AppendRunLog($"Writing {fileName}: {fullPath}");

            try
            {
                await Task.Run(() => EventExporter.WriteJsonl(normalizedEvents, fullPath));
                AppendRunLog($"Wrote {fileName}: {normalizedEvents.Count} lines");
                AppendOutputEntry($"{fileName}: {fullPath}");
            }
            catch (Exception ex)
            {
                AppendRunLog($"ERROR: Failed to write {fileName}: {ex.Message}");
            }
        }

        private string ResolveOutputDirectory()
        {
            string fromSettings = Settings.Default.OutputDirectory?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(fromSettings))
            {
                return fromSettings;
            }

            string documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string defaultDir = Path.Combine(documents, "LogAnalyzer", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            Settings.Default.OutputDirectory = defaultDir;
            SaveSettingsSafe();
            return defaultDir;
        }

        private async void BtnExport_Click(object? sender, RoutedEventArgs e)
        {
            if (_lastCollectedNormalized.Count == 0)
            {
                AppendRunLog("ERROR: No collected events to export. Run Collect first.");
                return;
            }

            string batchId = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            if (Settings.Default.WriteJsonl)
            {
                await WriteJsonlAsync(_lastCollectedNormalized, batchId);
            }
        }

        private static string BuildUniqueFileName(string directory, string baseName, string batchId, string extension)
        {
            string stem = $"{baseName}_{batchId}";
            string candidate = $"{stem}{extension}";
            int suffix = 2;

            while (File.Exists(Path.Combine(directory, candidate)))
            {
                candidate = $"{stem}_{suffix}{extension}";
                suffix++;
            }

            return candidate;
        }

        private void AppendOutputEntry(string line)
        {
            ListOutputs.Items.Add(line);
            ListOutputs.SelectedIndex = ListOutputs.Items.Count - 1;
            ListOutputs.ScrollIntoView(ListOutputs.SelectedItem);
        }

        private void BtnBrowseOutputDir_Click(object sender, RoutedEventArgs e)
        {
            SelectOutputFolder();
        }

        private void MenuOpenSettings_Click(object sender, RoutedEventArgs e)
        {
            OpenSettingsWindow();
        }

        private void OpenSettingsWindow()
        {
            SettingsWindow window = new(this)
            {
                Owner = this
            };

            window.ShowDialog();
            ApplySettingsToUi();
        }

        public void NotifySettingsChanged()
        {
            ApplySettingsToUi();
        }

        public void AppendLogFromSettingsWindow(string message)
        {
            AppendRunLog(message);
        }

        public bool ConfirmFromSettingsWindow(string titleKey, string messageKey, string fallbackTitle, string fallbackMessage)
        {
            return ConfirmAction(titleKey, messageKey, fallbackTitle, fallbackMessage);
        }

        private void SelectOutputFolder()
        {
            using Forms.FolderBrowserDialog dialog = new();
            dialog.ShowNewFolderButton = true;

            string current = Settings.Default.OutputDirectory?.Trim() ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(current) && Directory.Exists(current))
            {
                dialog.InitialDirectory = current;
            }

            if (dialog.ShowDialog() == Forms.DialogResult.OK)
            {
                Settings.Default.OutputDirectory = dialog.SelectedPath;
                SaveSettingsSafe();
            }
        }

        private void BtnOpenOutputFolder_Click(object sender, RoutedEventArgs e)
        {
            OpenOutputFolder();
        }

        private void OpenOutputFolder()
        {
            string directory = Settings.Default.OutputDirectory?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(directory))
            {
                AppendRunLog("ERROR: Output folder is empty.");
                return;
            }

            try
            {
                Directory.CreateDirectory(directory);
                OpenWithShell(directory);
            }
            catch (Exception ex)
            {
                AppendRunLog($"ERROR: Failed to open output folder: {ex.Message}");
            }
        }

        private void BtnOpenSelectedOutput_Click(object sender, RoutedEventArgs e)
        {
            string? selectedText = ListOutputs.SelectedItem?.ToString();
            string? filePath = ExtractPathFromOutputItem(selectedText);

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                AppendRunLog("ERROR: Selected output file was not found.");
                return;
            }

            try
            {
                OpenWithShell(filePath);
            }
            catch (Exception ex)
            {
                AppendRunLog($"ERROR: Failed to open file: {ex.Message}");
            }
        }

        private static string? ExtractPathFromOutputItem(string? itemText)
        {
            if (string.IsNullOrWhiteSpace(itemText))
            {
                return null;
            }

            int separator = itemText.IndexOf(':');
            if (separator < 0 || separator >= itemText.Length - 1)
            {
                return itemText.Trim();
            }

            return itemText[(separator + 1)..].Trim();
        }

        private static void OpenWithShell(string path)
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = path,
                UseShellExecute = true
            };

            Process.Start(startInfo);
        }

        private void ApplyQuickRange(TimeSpan range)
        {
            DateTime end = DateTime.Now;
            DateTime start = end.Subtract(range);
            DpStart.SelectedDate = start;
            DpEnd.SelectedDate = end;
            PersistTimeRangeSettings();
        }

        private bool TryBuildCollectRequest(out CollectRequest request)
        {
            request = null!;

            List<string> logNames = GetSelectedLogNames();

            if (logNames.Count == 0)
            {
                AppendRunLog(GetResourceString("SelectAtLeastOneLogError", "ERROR: Select at least one log."));
                SetStatus("Error");
                SetCollectingUiState(false);
                return false;
            }

            List<int> levelValues = new();
            List<string> levelNames = new();
            if (ChkCritical.IsChecked == true)
            {
                levelValues.Add(1);
                levelNames.Add("Critical");
            }

            if (ChkError.IsChecked == true)
            {
                levelValues.Add(2);
                levelNames.Add("Error");
            }

            if (ChkWarning.IsChecked == true)
            {
                levelValues.Add(3);
                levelNames.Add("Warning");
            }

            if (ChkInformation.IsChecked == true)
            {
                levelValues.Add(4);
                levelNames.Add("Information");
            }

            if (levelValues.Count == 0)
            {
                AppendRunLog("ERROR: Select at least one level.");
                SetStatus("Error");
                SetCollectingUiState(false);
                return false;
            }

            DateTime now = DateTime.Now;
            DateTime startTime;
            DateTime endTime;
            if (DpStart.SelectedDate.HasValue && DpEnd.SelectedDate.HasValue)
            {
                startTime = DpStart.SelectedDate.Value;
                endTime = DpEnd.SelectedDate.Value;
            }
            else
            {
                endTime = now;
                startTime = now.AddHours(-24);
            }

            if (startTime > endTime)
            {
                AppendRunLog("ERROR: Start time must be earlier than or equal to End time.");
                SetStatus("Error");
                SetCollectingUiState(false);
                return false;
            }

            int maxEvents = 500;
            if (int.TryParse(TxtTopN.Text, out int parsedTopN) && parsedTopN > 0)
            {
                maxEvents = parsedTopN;
            }

            if (maxEvents > MaxPreviewEvents)
            {
                maxEvents = MaxPreviewEvents;
            }

            request = new CollectRequest(logNames, levelValues, levelNames, startTime, endTime, maxEvents);
            return true;
        }

        private void FilterControl_Changed(object sender, RoutedEventArgs e)
        {
            PersistFilterSettings();
        }

        private void TxtTopN_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_isApplyingSettings)
            {
                return;
            }

            if (!int.TryParse(TxtTopN.Text, out int topN) || topN <= 0)
            {
                topN = 500;
            }

            if (topN > MaxPreviewEvents)
            {
                topN = MaxPreviewEvents;
            }

            Settings.Default.LastTopN = topN;
            SaveSettingsSafe();
        }

        private void TimeRange_Changed(object? sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            PersistTimeRangeSettings();
        }

        private void PersistFilterSettings()
        {
            if (_isApplyingSettings)
            {
                return;
            }

            Settings.Default.LastLogsSystem = ChkSystem.IsChecked == true;
            Settings.Default.LastLogsApplication = ChkApplication.IsChecked == true;
            Settings.Default.LastLevelCritical = ChkCritical.IsChecked == true;
            Settings.Default.LastLevelError = ChkError.IsChecked == true;
            Settings.Default.LastLevelWarning = ChkWarning.IsChecked == true;
            Settings.Default.LastLevelInformation = ChkInformation.IsChecked == true;
            SaveSettingsSafe();
        }

        private void PersistTimeRangeSettings()
        {
            if (_isApplyingSettings)
            {
                return;
            }

            if (DpStart.SelectedDate.HasValue && DpEnd.SelectedDate.HasValue)
            {
                Settings.Default.LastUseTimeRange = true;
                Settings.Default.LastStartTime = DpStart.SelectedDate.Value;
                Settings.Default.LastEndTime = DpEnd.SelectedDate.Value;
            }
            else
            {
                Settings.Default.LastUseTimeRange = false;
            }

            SaveSettingsSafe();
        }

        private void RefreshAnalysis(bool openWindow = false)
        {
            try
            {
                string logsLabel = BuildLogsLabel();
                DateTime? rangeStart = DpStart.SelectedDate;
                DateTime? rangeEnd = DpEnd.SelectedDate;

                _currentAnalysis = AnalysisService.Analyze(_lastCollectedNormalized, logsLabel, rangeStart, rangeEnd);
                TxtAnalysisStatus.Text =
                    $"Logs: {_currentAnalysis.LogsLabel}  Range: {FormatRange(_currentAnalysis.RangeStart)} - {FormatRange(_currentAnalysis.RangeEnd)}  Total: {_currentAnalysis.Total}";
                UpdateSummaryText();

                if (openWindow || (_analysisWindow != null && _analysisWindow.IsLoaded))
                {
                    OpenOrUpdateAnalysisWindow(openWindow);
                }
            }
            catch (Exception ex)
            {
                AppendRunLog($"ERROR: Analysis failed: {ex.Message}");
                TxtAnalysisStatus.Text = "解析に失敗しました。Run Log を確認してください。";
            }
        }

        private void OpenOrUpdateAnalysisWindow(bool bringToFront = true)
        {
            string machine = TxtMachineName.Text;
            string levels = BuildLevelsLabel();
            int maxEvents = ParseTopN();

            if (_analysisWindow == null || !_analysisWindow.IsLoaded)
            {
                _analysisWindow = new AnalysisWindow(_currentAnalysis, machine, levels, maxEvents)
                {
                    Owner = this
                };
                _analysisWindow.Closed += (_, _) => _analysisWindow = null;
                _analysisWindow.Show();
                return;
            }

            _analysisWindow.SetResult(_currentAnalysis, machine, levels, maxEvents);
            if (bringToFront && _analysisWindow.WindowState == WindowState.Minimized)
            {
                _analysisWindow.WindowState = WindowState.Normal;
            }

            if (bringToFront)
            {
                _analysisWindow.Activate();
            }
        }

        private string BuildLogsLabel()
        {
            List<string> labels = new();
            if (ChkSystem.IsChecked == true)
            {
                labels.Add(GetResourceString("LogSystem", "System"));
            }

            if (ChkApplication.IsChecked == true)
            {
                labels.Add(GetResourceString("LogApplication", "Application"));
            }

            if (ChkSetup.IsChecked == true)
            {
                labels.Add(GetResourceString("LogSetup", "Setup"));
            }

            if (ChkSecurity.IsChecked == true)
            {
                labels.Add(GetResourceString("LogSecurity", "Security"));
            }

            if (ChkForwardedEvents.IsChecked == true)
            {
                labels.Add(GetResourceString("LogForwardedEvents", "Forwarded Events"));
            }

            return labels.Count == 0 ? "(none)" : string.Join(",", labels);
        }

        private List<string> GetSelectedLogNames()
        {
            List<string> logNames = new();
            if (ChkSystem.IsChecked == true)
            {
                logNames.Add("System");
            }

            if (ChkApplication.IsChecked == true)
            {
                logNames.Add("Application");
            }

            if (ChkSetup.IsChecked == true)
            {
                logNames.Add("Setup");
            }

            if (ChkSecurity.IsChecked == true)
            {
                logNames.Add("Security");
            }

            if (ChkForwardedEvents.IsChecked == true)
            {
                logNames.Add("ForwardedEvents");
            }

            return logNames;
        }

        private string BuildLevelsLabel()
        {
            List<string> levels = new();
            if (ChkCritical.IsChecked == true)
            {
                levels.Add("Critical");
            }

            if (ChkError.IsChecked == true)
            {
                levels.Add("Error");
            }

            if (ChkWarning.IsChecked == true)
            {
                levels.Add("Warning");
            }

            if (ChkInformation.IsChecked == true)
            {
                levels.Add("Information");
            }

            return levels.Count == 0 ? "(none)" : string.Join(",", levels);
        }

        private int ParseTopN()
        {
            int maxEvents = 500;
            if (int.TryParse(TxtTopN.Text, out int parsedTopN) && parsedTopN > 0)
            {
                maxEvents = parsedTopN;
            }

            return maxEvents > MaxPreviewEvents ? MaxPreviewEvents : maxEvents;
        }

        private void BtnCopyEventJson_Click(object sender, RoutedEventArgs e)
        {
            EventRecordInfo? selected = GetSelectedRecordInfo();
            if (selected == null)
            {
                SetStatus(GetResourceString("StatusNoEventSelected", "No event selected."));
                return;
            }

            string json = JsonSerializer.Serialize(selected, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            CopyTextToClipboard(json, "StatusCopiedJson", "JSON copied.");
        }

        private void BtnCopyMessage_Click(object sender, RoutedEventArgs e)
        {
            EventRecordInfo? selected = GetSelectedRecordInfo();
            CopyTextToClipboard(selected?.Message ?? string.Empty, "StatusCopiedMessage", "Message copied.");
        }

        private void TabRight_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (TabRight.SelectedIndex == 1)
            {
                UpdateSummaryText();
            }
        }

        private void UpdateSummaryText()
        {
            int total = _currentAnalysis.Total;
            StringBuilder sb = new();
            sb.AppendLine("# " + GetResourceString("TabSummary", "Summary (MD)"));
            sb.AppendLine();
            sb.AppendLine($"{GetResourceString("Analysis.Target", "Target")}: {BuildLogsLabel()}");
            sb.AppendLine($"{GetResourceString("Analysis.Period", "Period")}: {FormatRange(_currentAnalysis.RangeStart)} - {FormatRange(_currentAnalysis.RangeEnd)}");
            sb.AppendLine($"{GetResourceString("Analysis.Filter", "Filter")}: {BuildLevelsLabel()} / Max={ParseTopN()}");
            sb.AppendLine();
            sb.AppendLine($"- {GetResourceString("Analysis.Total", "Total")}: {total}");
            sb.AppendLine($"- {GetResourceString("Analysis.Critical", "Critical")}: {_currentAnalysis.CriticalCount}");
            sb.AppendLine($"- {GetResourceString("Analysis.Error", "Error")}: {_currentAnalysis.ErrorCount}");
            sb.AppendLine($"- {GetResourceString("Analysis.Warning", "Warning")}: {_currentAnalysis.WarningCount}");
            sb.AppendLine($"- {GetResourceString("Analysis.Info", "Info")}: {_currentAnalysis.InfoCount}");
            sb.AppendLine($"- {GetResourceString("Analysis.Other", "Other")}: {_currentAnalysis.OtherLevelCount}");
            sb.AppendLine();
            sb.AppendLine(GetResourceString("Analysis.Description", "Summarizes frequent providers, event IDs, and hourly concentration in this period."));

            if (_currentAnalysis.TopProviders.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("## " + GetResourceString("Analysis.TopProviders", "Top Providers"));
                foreach (KeyValuePair<string, int> item in _currentAnalysis.TopProviders.Take(10))
                {
                    sb.AppendLine($"- {item.Key}: {item.Value}");
                }
            }

            if (_currentAnalysis.TopEventIds.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("## " + GetResourceString("Analysis.TopEventIds", "Top Event IDs"));
                foreach (KeyValuePair<int, int> item in _currentAnalysis.TopEventIds.Take(10))
                {
                    sb.AppendLine($"- {item.Key}: {item.Value}");
                }
            }

            TxtSummary.Text = sb.ToString();
        }


        private EventRecordInfo? GetSelectedRecordInfo()
        {
            if (GridPreview.SelectedItem is not EventRow row)
            {
                return null;
            }

            EventRecordInfo? matched = _lastCollectedNormalized.FirstOrDefault(item =>
                item.Id == row.Id &&
                string.Equals(item.ProviderName ?? string.Empty, row.ProviderName ?? string.Empty, StringComparison.Ordinal) &&
                item.TimeCreated.LocalDateTime == row.TimeCreated &&
                string.Equals(item.Message ?? string.Empty, row.Message ?? string.Empty, StringComparison.Ordinal));

            if (matched != null)
            {
                return matched;
            }

            return new EventRecordInfo
            {
                TimeCreated = new DateTimeOffset(row.TimeCreated),
                LogName = string.Empty,
                Level = row.Level ?? string.Empty,
                ProviderName = row.ProviderName ?? string.Empty,
                Id = row.Id,
                MachineName = TxtMachineName.Text ?? string.Empty,
                Message = row.Message ?? string.Empty
            };
        }

        private void CopyTextToClipboard(string text, string successStatusKey, string successFallback)
        {
            try
            {
                System.Windows.Clipboard.SetText(text ?? string.Empty);
                string status = GetResourceString(successStatusKey, successFallback);
                SetStatus(status);
                AppendRunLog(status);
            }
            catch (Exception ex)
            {
                string failed = GetResourceString("StatusCopyFailed", "Copy failed.");
                SetStatus(failed);
                AppendRunLog($"ERROR: {failed} {ex.Message}");
            }
        }

        private static string FormatRange(DateTime? value)
        {
            return value.HasValue ? value.Value.ToString("yyyy-MM-dd HH:mm:ss") : "(n/a)";
        }

        private void SaveSettingsSafe()
        {
            if (_isApplyingSettings)
            {
                return;
            }

            try
            {
                Settings.Default.Save();
            }
            catch (Exception ex)
            {
                AppendRunLog($"ERROR: Failed to save settings: {ex.Message}");
            }
        }

        private bool ConfirmAction(string titleKey, string messageKey, string fallbackTitle, string fallbackMessage)
        {
            string title = GetResourceString(titleKey, fallbackTitle);
            string message = GetResourceString(messageKey, fallbackMessage);
            MessageBoxResult result = System.Windows.MessageBox.Show(this, message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            return result == MessageBoxResult.Yes;
        }

        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string title = GetResourceString("AboutDialogTitle", "About");
                string appName = GetResourceString("AppTitle", "LogAnalyzer");
                string version = GetResourceString("AppVersion", "1.0");
                string format = GetResourceString("AboutDialogMessageFormat", "{0}{1}Version: {2}");
                string newline = Environment.NewLine;
                string message = string.Format(
                    System.Globalization.CultureInfo.CurrentUICulture,
                    format,
                    appName,
                    newline,
                    version);

                System.Windows.MessageBox.Show(this, message, title, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                AppendRunLog($"ERROR: Failed to open About dialog: {ex.Message}");
            }
        }

        private string GetResourceString(string key, string fallback)
        {
            return TryFindResource(key) as string ?? fallback;
        }

        private sealed class CollectRequest
        {
            public CollectRequest(List<string> logNames, List<int> levelValues, List<string> levelNames, DateTime startTime, DateTime endTime, int maxEvents)
            {
                LogNames = logNames;
                LevelValues = levelValues;
                LevelNames = levelNames;
                StartTime = startTime;
                EndTime = endTime;
                MaxEvents = maxEvents;
            }

            public List<string> LogNames { get; }
            public List<int> LevelValues { get; }
            public List<string> LevelNames { get; }
            public DateTime StartTime { get; }
            public DateTime EndTime { get; }
            public int MaxEvents { get; }
        }

    }
}
