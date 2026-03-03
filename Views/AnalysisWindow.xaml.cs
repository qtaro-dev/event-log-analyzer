using System;
using System.Configuration;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using LogAnalyzer.Models;
using LogAnalyzer.Properties;
using LogAnalyzer.Services;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using MediaBrush = System.Windows.Media.Brush;

namespace LogAnalyzer.Views
{
    public partial class AnalysisWindow : Window
    {
        private const int LegendWrapMaxLength = 26;
        private const string ThemePrefix = "theme:";
        private const string ArgbPrefix = "argb:";
        private static readonly string[] DefaultPiePaletteA =
        {
            "AnalysisProviderColor1", "AnalysisProviderColor2", "AnalysisProviderColor3", "AnalysisProviderColor4", "AnalysisProviderColor5",
            "AnalysisProviderColor6", "AnalysisProviderColor7", "AnalysisProviderColor8", "AnalysisProviderColor1", "AnalysisProviderColor2"
        };

        private static readonly string[] DefaultPiePaletteB =
        {
            "AnalysisProviderColor8", "AnalysisProviderColor7", "AnalysisProviderColor6", "AnalysisProviderColor5", "AnalysisProviderColor4",
            "AnalysisProviderColor3", "AnalysisProviderColor2", "AnalysisProviderColor1", "AnalysisProviderColor8", "AnalysisProviderColor7"
        };

        private static readonly string[] DefaultBarPaletteA =
        {
            "GridHeaderBackgroundBrush", "GridSelectionBackgroundBrush", "PanelBackgroundBrush", "BorderBrush"
        };

        private static readonly string[] DefaultBarPaletteB =
        {
            "GridSelectionBackgroundBrush", "GridHeaderBackgroundBrush", "InputBackgroundBrush", "BorderBrush"
        };

        private AnalysisResult _currentResult = new();
        private UserConfig _currentGraphConfig = new();
        private string _currentMachine = string.Empty;
        private string _currentLevelsLabel = string.Empty;
        private int _currentMaxEvents = 500;

        public AnalysisWindow(AnalysisResult result, string machine, string levelsLabel, int maxEvents)
        {
            InitializeComponent();
            ProviderPiePlot.Controller = new PlotController();
            SetResult(result, machine, levelsLabel, maxEvents);
        }

        public void SetResult(AnalysisResult result, string machine, string levelsLabel, int maxEvents)
        {
            _currentResult = result ?? new AnalysisResult();
            _currentMachine = machine ?? string.Empty;
            _currentLevelsLabel = levelsLabel ?? string.Empty;
            _currentMaxEvents = maxEvents > 0 ? maxEvents : 500;
            Render();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            Render();
        }

        private void Render()
        {
            _currentGraphConfig = LoadUserConfig();
            int total = _currentResult.Total;
            int errorCount = _currentResult.ErrorCount;

            TxtTarget.Text = string.Format(
                CultureInfo.CurrentUICulture,
                GetText("Analysis.TargetFormat"),
                ToSafeMachine(_currentMachine),
                ToSafeLabel(_currentResult.LogsLabel));

            TxtRange.Text = string.Format(
                CultureInfo.CurrentUICulture,
                GetText("Analysis.PeriodFormat"),
                FormatRange(_currentResult.RangeStart),
                FormatRange(_currentResult.RangeEnd),
                GetDayCount(_currentResult.RangeStart, _currentResult.RangeEnd));

            TxtFilter.Text = string.Format(
                CultureInfo.CurrentUICulture,
                GetText("Analysis.FilterFormat"),
                ToSafeLabel(_currentLevelsLabel),
                _currentMaxEvents);
            TxtLastUpdated.Text = string.Format(
                CultureInfo.CurrentUICulture,
                GetText("Analysis.LastUpdatedFormat"),
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentUICulture));

            TxtTotal.Text = FormatSummary("Analysis.Total", _currentResult.Total, total);
            TxtCritical.Text = FormatSummary("Analysis.Critical", _currentResult.CriticalCount, total);
            TxtError.Text = FormatSummary("Analysis.Error", _currentResult.ErrorCount, total);
            TxtWarning.Text = FormatSummary("Analysis.Warning", _currentResult.WarningCount, total);
            TxtInfo.Text = FormatSummary("Analysis.Info", _currentResult.InfoCount, total);
            TxtOther.Text = FormatSummary("Analysis.Other", _currentResult.OtherLevelCount, total);

            TxtHeaderTotal.Text = $"{GetText("Analysis.HeaderTotal")}: {string.Format(CultureInfo.CurrentUICulture, GetText("Analysis.TotalOnlyFormat"), total)}";
            TxtHeaderErrorRate.Text = $"{GetText("Analysis.HeaderErrorRate")}: {string.Format(CultureInfo.CurrentUICulture, GetText("Analysis.ErrorRateFormat"), ToShareDisplay(errorCount, total), errorCount, total)}";
            TxtHeaderConcentration.Text = $"{GetText("Analysis.HeaderConcentration")}: {string.Format(CultureInfo.CurrentUICulture, GetText("Analysis.ConcentrationFormat"), _currentResult.ConcentrationScore.ToString("0.00", CultureInfo.CurrentUICulture))}";
            TxtHeaderTop3Share.Text = $"{GetText("Analysis.HeaderTop3Share")}: {string.Format(CultureInfo.CurrentUICulture, GetText("Analysis.Top3ShareFormat"), _currentResult.Top3ProviderShare.ToString("0.00", CultureInfo.CurrentUICulture))}";

            if (_currentResult.PeakHour >= 0)
            {
                TxtPeakHourStrong.Text = string.Format(
                    CultureInfo.CurrentUICulture,
                    GetText("Analysis.PeakHourStrongFormat"),
                    _currentResult.PeakHour.ToString("00", CultureInfo.InvariantCulture),
                    _currentResult.PeakHourCount);
            }
            else
            {
                TxtPeakHourStrong.Text = GetText("Analysis.None");
            }

            if (!string.IsNullOrWhiteSpace(_currentResult.BurstProvider) && _currentResult.BurstCount > 1)
            {
                TxtBurst.Text = string.Format(
                    CultureInfo.CurrentUICulture,
                    GetText("Analysis.BurstFormat"),
                    _currentResult.BurstProvider,
                    _currentResult.BurstCount);
            }
            else
            {
                TxtBurst.Text = GetText("Analysis.BurstNone");
            }

            GridTopProviders.ItemsSource = BuildProviderRows(_currentResult.TopProviders, total);
            GridTopEventIds.ItemsSource = BuildEventIdRows(_currentResult.TopEventIds, total);

            PieBuildResult pieBuild = BuildProviderPieModel(_currentResult.ProviderDistribution, total);
            ProviderPiePlot.Model = pieBuild.Model;
            ProviderLegendItems.ItemsSource = pieBuild.Legends;
            HourlyBarPlot.Model = BuildHourlyBarModel(_currentResult.HourlyCounts);
            ProviderPiePlot.InvalidatePlot(true);
            HourlyBarPlot.InvalidatePlot(true);

            string peakForSummary = _currentResult.PeakHour >= 0
                ? string.Format(CultureInfo.CurrentUICulture, GetText("Analysis.PeakHourStrongFormat"), _currentResult.PeakHour.ToString("00", CultureInfo.InvariantCulture), _currentResult.PeakHourCount)
                : GetText("Analysis.None");
            string burstForSummary = TxtBurst.Text;
            TxtAnalysisSummary.Text = string.Format(
                CultureInfo.CurrentUICulture,
                GetText("Analysis.SummaryFormatLong"),
                string.Format(CultureInfo.CurrentUICulture, GetText("Analysis.ErrorRateFormat"), ToShareDisplay(errorCount, total), errorCount, total),
                peakForSummary,
                string.Format(CultureInfo.CurrentUICulture, GetText("Analysis.ConcentrationFormat"), _currentResult.ConcentrationScore.ToString("0.00", CultureInfo.CurrentUICulture)),
                string.Format(CultureInfo.CurrentUICulture, GetText("Analysis.Top3ShareFormat"), _currentResult.Top3ProviderShare.ToString("0.00", CultureInfo.CurrentUICulture)),
                burstForSummary);

            string peakForInsight = _currentResult.PeakHour >= 0
                ? string.Format(CultureInfo.CurrentUICulture, GetText("Analysis.PeakHourStrongFormat"), _currentResult.PeakHour.ToString("00", CultureInfo.InvariantCulture), _currentResult.PeakHourCount)
                : GetText("Analysis.None");
            TxtInsightPeak.Text = string.Format(
                CultureInfo.CurrentUICulture,
                GetText("Analysis.InsightPeakFormat"),
                peakForInsight);
            TxtInsightProviderConcentration.Text = string.Format(
                CultureInfo.CurrentUICulture,
                GetText("Analysis.InsightProviderConcentrationFormat"),
                ComputeTopProviderShare(1, total),
                ComputeTopProviderShare(3, total),
                ComputeTopProviderShare(5, total));
            TxtInsightEventIdConcentration.Text = string.Format(
                CultureInfo.CurrentUICulture,
                GetText("Analysis.InsightEventIdConcentrationFormat"),
                ComputeTopEventIdShare(1, total),
                ComputeTopEventIdShare(3, total));
            TxtInsightDiversity.Text = string.Format(
                CultureInfo.CurrentUICulture,
                GetText("Analysis.InsightDiversityFormat"),
                _currentResult.UniqueProviderCount,
                _currentResult.UniqueEventIdCount);
            TxtInsightBurst.Text = string.Format(
                CultureInfo.CurrentUICulture,
                GetText("Analysis.InsightBurstFormat"),
                burstForSummary);

            GridAnalysisAggregation.ItemsSource = BuildAggregationRows(total);
            DataContext = _currentResult;
        }

        private PieBuildResult BuildProviderPieModel(List<KeyValuePair<string, int>> providers, int total)
        {
            PlotModel model = new()
            {
                Padding = new OxyThickness(0),
                PlotMargins = new OxyThickness(0),
                PlotAreaBorderThickness = new OxyThickness(0),
                PlotAreaBorderColor = OxyColors.Transparent,
                Background = OxyColors.Transparent
            };
            model.Axes.Clear();

            PieSeries pie = new()
            {
                StrokeThickness = 0,
                AngleSpan = 360,
                StartAngle = 270,
                InsideLabelFormat = string.Empty,
                OutsideLabelFormat = string.Empty,
                TickHorizontalLength = 0,
                TickRadialLength = 0,
                Diameter = 0.95,
                TrackerFormatString = string.Empty
            };

            List<ProviderPieLegendItem> legends = new();
            List<ProviderSlice> finalSlices = BuildFinalProviderSlices(providers, total);
            if (finalSlices.Count == 0)
            {
                OxyColor emptyColor = GetThemeColor("BorderBrush", OxyColors.Automatic);
                pie.Slices.Add(new PieSlice(GetText("Analysis.None"), 1) { Fill = emptyColor });
                legends.Add(new ProviderPieLegendItem(
                    string.Format(CultureInfo.CurrentUICulture, GetText("Analysis.PieLegendItemFormat"), GetText("Analysis.None"), "100.00"),
                    ToBrush(emptyColor)));
                model.Series.Add(pie);
                return new PieBuildResult(model, legends);
            }

            List<OxyColor> palette = BuildThemePalette();
            OxyColor otherColor = GetThemeColor("AnalysisProviderColorOther", GetThemeColor("BorderBrush", OxyColors.Automatic));

            for (int i = 0; i < finalSlices.Count; i++)
            {
                ProviderSlice item = finalSlices[i];
                OxyColor fill = item.IsOther ? otherColor : palette[i % palette.Count];
                string safeLabel = string.IsNullOrWhiteSpace(item.Name) ? GetText("Analysis.None") : item.Name;
                pie.Slices.Add(new PieSlice(safeLabel, item.Count) { Fill = fill });
                legends.Add(new ProviderPieLegendItem(BuildPieLegendText(safeLabel, item.Count, total), ToBrush(fill)));
            }

            model.Series.Add(pie);
            return new PieBuildResult(model, legends);
        }

        private List<ProviderSlice> BuildFinalProviderSlices(List<KeyValuePair<string, int>> providers, int total)
        {
            const int topCount = 8;

            List<ProviderSlice> ranked = (providers ?? new List<KeyValuePair<string, int>>())
                .Where(static x => x.Value > 0)
                .Select(x => new ProviderSlice(
                    string.IsNullOrWhiteSpace(x.Key) ? GetText("Analysis.None") : x.Key,
                    x.Value,
                    total > 0 ? x.Value * 100d / total : 0d,
                    false))
                .OrderByDescending(static x => x.Count)
                .ThenBy(static x => x.Name, StringComparer.CurrentCulture)
                .ToList();

            if (ranked.Count == 0)
            {
                return new List<ProviderSlice>();
            }

            List<ProviderSlice> reRanked = ranked
                .OrderByDescending(static x => x.Count)
                .ThenBy(static x => x.Name, StringComparer.CurrentCulture)
                .ToList();

            // Provider composition chart follows the "top K only" rule to keep
            // small slices visible instead of collapsing into Other.
            return reRanked.Take(topCount).ToList();
        }

        private PlotModel BuildHourlyBarModel(int[]? hourlyCounts)
        {
            int[] values = hourlyCounts is { Length: 24 } ? hourlyCounts : new int[24];
            double leftMargin = GetDoubleResource("HourlyPlotLeftMargin", 84d);
            double rightMargin = GetDoubleResource("HourlyPlotRightMargin", 42d);
            double bottomMargin = GetDoubleResource("HourlyPlotBottomMargin", 42d);
            PlotModel model = new()
            {
                DefaultFontSize = 14,
                PlotMargins = new OxyThickness(leftMargin, 12, rightMargin, bottomMargin),
                Padding = new OxyThickness(8)
            };

            CategoryAxis categoryAxis = new()
            {
                Position = AxisPosition.Left,
                GapWidth = 0.2,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                FontSize = 13,
                MajorStep = 2
            };

            int peakHour = -1;
            int peak = -1;
            for (int i = 0; i < values.Length; i++)
            {
                categoryAxis.Labels.Add(i % 2 == 0 ? i.ToString("00", CultureInfo.InvariantCulture) : string.Empty);
                if (values[i] > peak)
                {
                    peak = values[i];
                    peakHour = i;
                }
            }

            LinearAxis valueAxis = new()
            {
                Position = AxisPosition.Bottom,
                Minimum = 0,
                MaximumPadding = 0.12,
                MinimumPadding = 0.02,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                MajorGridlineStyle = LineStyle.Solid,
                MinorGridlineStyle = LineStyle.None,
                FontSize = 13,
                LabelFormatter = static _ => string.Empty
            };

            BarSeries bars = new()
            {
                StrokeColor = OxyColors.Transparent,
                StrokeThickness = 0,
                LabelFormatString = null,
                TrackerFormatString = string.Empty,
                TextColor = ResolveOxyColorFromSlot(
                    _currentGraphConfig.BarLabelTextSlot,
                    "InputForegroundBrush")
            };

            string[] barKeys = GetConfiguredBarPaletteKeys();
            OxyColor normalBarColor = ResolveOxyColorFromSlot(barKeys[0], "GridHeaderBackgroundBrush");
            OxyColor peakBarColor = ResolveOxyColorFromSlot(
                barKeys.Length > 1 ? barKeys[1] : barKeys[0],
                "GridSelectionBackgroundBrush");

            for (int i = 0; i < values.Length; i++)
            {
                bars.Items.Add(new BarItem(values[i])
                {
                    Color = i == peakHour ? peakBarColor : normalBarColor
                });
            }

            model.Axes.Add(categoryAxis);
            model.Axes.Add(valueAxis);
            model.Series.Add(bars);
            return model;
        }

        private string ComputeTopProviderShare(int top, int total)
        {
            if (total <= 0 || top <= 0)
            {
                return "0.00";
            }

            int count = _currentResult.ProviderDistribution.Take(top).Sum(static x => x.Value);
            return ToShareDisplay(count, total);
        }

        private string ComputeTopEventIdShare(int top, int total)
        {
            if (total <= 0 || top <= 0)
            {
                return "0.00";
            }

            int count = _currentResult.TopEventIds.Take(top).Sum(static x => x.Value);
            return ToShareDisplay(count, total);
        }

        private string BuildPieLegendText(string providerName, int count, int total)
        {
            string name = string.IsNullOrWhiteSpace(providerName) ? GetText("Analysis.None") : providerName;
            string wrappedName = BuildLegendLabel(name, LegendWrapMaxLength);
            return string.Format(CultureInfo.CurrentUICulture, GetText("Analysis.PieLegendItemFormat"), wrappedName, ToShareDisplay(count, total));
        }

        private static string BuildLegendLabel(string label, int wrapAt)
        {
            if (string.IsNullOrWhiteSpace(label) || wrapAt <= 0 || label.Length <= wrapAt)
            {
                return label;
            }

            int split = FindLegendSplitIndex(label, wrapAt);
            if (split <= 0 || split >= label.Length - 1)
            {
                return label;
            }

            string head = label[..split].TrimEnd();
            string tail = label[split..].TrimStart();
            return $"{head}{Environment.NewLine}{tail}";
        }

        private static int FindLegendSplitIndex(string text, int preferred)
        {
            char[] separators = { ' ', '-', '.', '/' };
            for (int i = Math.Min(preferred, text.Length - 1); i >= Math.Max(1, preferred - 10); i--)
            {
                if (separators.Contains(text[i]))
                {
                    return i + 1;
                }
            }

            return preferred;
        }

        private List<OxyColor> BuildThemePalette()
        {
            string[] keys = GetConfiguredPiePaletteKeys();

            List<OxyColor> colors = new(keys.Length);
            foreach (string key in keys)
            {
                colors.Add(ResolveOxyColorFromSlot(key, "BorderBrush"));
            }

            if (colors.Count == 0)
            {
                colors.Add(GetThemeColor("AnalysisProviderColor1", GetThemeColor("BorderBrush", OxyColors.Automatic)));
            }

            return colors;
        }

        private string[] GetConfiguredPiePaletteKeys()
        {
            UserConfig config = _currentGraphConfig;
            string paletteId = NormalizePaletteId(config.SelectedPiePaletteId);
            GraphPalette? palette = config.PiePalettes.FirstOrDefault(p => string.Equals(NormalizePaletteId(p.Id), paletteId, StringComparison.OrdinalIgnoreCase))
                ?? config.PiePalettes.FirstOrDefault();
            string[] defaults = paletteId == "B" ? DefaultPiePaletteB : DefaultPiePaletteA;
            return BuildPaletteFromConfig(palette, 10, defaults);
        }

        private string[] GetConfiguredBarPaletteKeys()
        {
            UserConfig config = _currentGraphConfig;
            string paletteId = NormalizePaletteId(config.SelectedBarPaletteId);
            GraphPalette? palette = config.BarPalettes.FirstOrDefault(p => string.Equals(NormalizePaletteId(p.Id), paletteId, StringComparison.OrdinalIgnoreCase))
                ?? config.BarPalettes.FirstOrDefault();
            string[] defaults = paletteId == "B" ? DefaultBarPaletteB : DefaultBarPaletteA;
            return BuildPaletteFromConfig(palette, 4, defaults);
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
                Debug.WriteLine($"Missing settings key: {key}");
            }

            return fallback;
        }

        private UserConfig LoadUserConfig()
        {
            (UserConfig config, _) = UserConfigService.LoadOrDefault(CreateDefaultGraphConfig);
            if (config.PiePalettes.Count == 0 || config.BarPalettes.Count == 0)
            {
                return CreateDefaultGraphConfig();
            }

            if (string.IsNullOrWhiteSpace(config.BarLabelTextSlot))
            {
                config.BarLabelTextSlot = $"{ThemePrefix}InputForegroundBrush";
            }

            return config;
        }

        private static string[] BuildPaletteFromConfig(GraphPalette? palette, int count, string[] defaults)
        {
            string[] result = new string[count];
            for (int i = 0; i < count; i++)
            {
                string fallback = defaults[Math.Min(i, defaults.Length - 1)];
                string source = palette != null && i < palette.Colors.Count ? palette.Colors[i] : fallback;
                result[i] = NormalizeSlotValue(source, fallback);
            }

            return result;
        }

        private UserConfig CreateDefaultGraphConfig()
        {
            return new UserConfig
            {
                SelectedPiePaletteId = "A",
                SelectedBarPaletteId = "A",
                BarLabelTextSlot = $"{ThemePrefix}InputForegroundBrush",
                PiePalettes = new List<GraphPalette>
                {
                    new() { Id = "A", Name = string.Format(GetText("SettingsPalettePieNameFormat"), "A"), Colors = DefaultPiePaletteA.ToList() },
                    new() { Id = "B", Name = string.Format(GetText("SettingsPalettePieNameFormat"), "B"), Colors = DefaultPiePaletteB.ToList() }
                },
                BarPalettes = new List<GraphPalette>
                {
                    new() { Id = "A", Name = string.Format(GetText("SettingsPaletteBarNameFormat"), "A"), Colors = DefaultBarPaletteA.ToList() },
                    new() { Id = "B", Name = string.Format(GetText("SettingsPaletteBarNameFormat"), "B"), Colors = DefaultBarPaletteB.ToList() }
                }
            };
        }

        private OxyColor GetThemeColor(string key, OxyColor fallback)
        {
            object? resource = TryFindResource(key);
            if (resource is SolidColorBrush brush)
            {
                return OxyColor.FromArgb(brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B);
            }

            return fallback;
        }

        private OxyColor ResolveOxyColorFromSlot(string slotValue, string fallbackBrushKey)
        {
            string normalized = NormalizeSlotValue(slotValue, $"{ThemePrefix}{fallbackBrushKey}");
            if (normalized.StartsWith(ThemePrefix, StringComparison.OrdinalIgnoreCase))
            {
                string key = normalized[ThemePrefix.Length..];
                return GetThemeColor(key, GetThemeColor(fallbackBrushKey, OxyColors.Automatic));
            }

            if (normalized.StartsWith(ArgbPrefix, StringComparison.OrdinalIgnoreCase))
            {
                string argb = normalized[ArgbPrefix.Length..];
                try
                {
                    object? colorObj = System.Windows.Media.ColorConverter.ConvertFromString(argb);
                    if (colorObj is System.Windows.Media.Color color)
                    {
                        return OxyColor.FromArgb(color.A, color.R, color.G, color.B);
                    }
                }
                catch
                {
                    // fall through
                }
            }

            return GetThemeColor(fallbackBrushKey, OxyColors.Automatic);
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

        private double GetDoubleResource(string key, double fallback)
        {
            object? resource = TryFindResource(key);
            if (resource is double d)
            {
                return d;
            }

            return fallback;
        }


        private List<AggregationMetricRow> BuildAggregationRows(int total)
        {
            string peakDisplay = _currentResult.PeakHour >= 0
                ? string.Format(
                    CultureInfo.CurrentUICulture,
                    GetText("Analysis.PeakHourStrongFormat"),
                    _currentResult.PeakHour.ToString("00", CultureInfo.InvariantCulture),
                    _currentResult.PeakHourCount)
                : GetText("Analysis.None");

            return new List<AggregationMetricRow>
            {
                new(GetText("Analysis.Aggregation.UniqueProviders"), _currentResult.UniqueProviderCount.ToString(CultureInfo.CurrentUICulture)),
                new(GetText("Analysis.Aggregation.UniqueEventIds"), _currentResult.UniqueEventIdCount.ToString(CultureInfo.CurrentUICulture)),
                new(GetText("Analysis.Aggregation.ProviderTop1Share"), $"{ComputeTopProviderShare(1, total)}%"),
                new(GetText("Analysis.Aggregation.ProviderTop3Share"), $"{ComputeTopProviderShare(3, total)}%"),
                new(GetText("Analysis.Aggregation.ProviderTop5Share"), $"{ComputeTopProviderShare(5, total)}%"),
                new(GetText("Analysis.Aggregation.EventIdTop1Share"), $"{ComputeTopEventIdShare(1, total)}%"),
                new(GetText("Analysis.Aggregation.EventIdTop3Share"), $"{ComputeTopEventIdShare(3, total)}%"),
                new(GetText("Analysis.Aggregation.PeakHour"), peakDisplay),
                new(GetText("Analysis.Aggregation.Concentration"), string.Format(
                    CultureInfo.CurrentUICulture,
                    GetText("Analysis.ConcentrationFormat"),
                    _currentResult.ConcentrationScore.ToString("0.00", CultureInfo.CurrentUICulture)))
            };
        }

        private List<ProviderShareRow> BuildProviderRows(IEnumerable<KeyValuePair<string, int>> source, int total)
        {
            return (source ?? Enumerable.Empty<KeyValuePair<string, int>>())
                .Select(x => new ProviderShareRow(x.Key, x.Value, ToShareDisplay(x.Value, total)))
                .ToList();
        }

        private List<EventIdShareRow> BuildEventIdRows(IEnumerable<KeyValuePair<int, int>> source, int total)
        {
            return (source ?? Enumerable.Empty<KeyValuePair<int, int>>())
                .Select(x => new EventIdShareRow(x.Key, x.Value, ToShareDisplay(x.Value, total)))
                .ToList();
        }

        private static string ToShareDisplay(int count, int total)
        {
            if (total <= 0)
            {
                return "0.00";
            }

            double ratio = (double)count / total * 100d;
            return ratio.ToString("0.00", CultureInfo.InvariantCulture);
        }

        private string FormatSummary(string labelKey, int count, int total)
        {
            return string.Format(
                CultureInfo.CurrentUICulture,
                GetText("Analysis.SummaryFormat"),
                GetText(labelKey),
                count,
                ToShareDisplay(count, total));
        }

        private string FormatRange(DateTime? value)
        {
            return value.HasValue
                ? value.Value.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentUICulture)
                : GetText("Analysis.Na");
        }

        private static int GetDayCount(DateTime? start, DateTime? end)
        {
            if (!start.HasValue || !end.HasValue || end < start)
            {
                return 0;
            }

            TimeSpan span = end.Value - start.Value;
            int days = (int)Math.Ceiling(span.TotalDays);
            return Math.Max(1, days);
        }

        private string ToSafeLabel(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? GetText("Analysis.None") : value;
        }

        private string ToSafeMachine(string machine)
        {
            return string.IsNullOrWhiteSpace(machine) ? GetText("Analysis.LocalMachine") : machine;
        }

        private string GetText(string key)
        {
            return TryFindResource(key) as string ?? key;
        }

        private static SolidColorBrush ToBrush(OxyColor color)
        {
            return new SolidColorBrush(System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
        }

        private sealed record ProviderShareRow(string Provider, int Count, string ShareDisplay);
        private sealed record EventIdShareRow(int EventId, int Count, string ShareDisplay);
        private sealed record AggregationMetricRow(string Metric, string Value);
        private sealed record ProviderPieLegendItem(string Label, MediaBrush Color);
        private sealed record PieBuildResult(PlotModel Model, List<ProviderPieLegendItem> Legends);
        private sealed record ProviderSlice(string Name, int Count, double Share, bool IsOther);
    }
}
