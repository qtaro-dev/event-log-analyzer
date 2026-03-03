using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LogAnalyzer.Models;

namespace LogAnalyzer.Services
{
    public static class AnalysisService
    {
        public static AnalysisResult Analyze(IEnumerable<EventRecordInfo>? events, string logsLabel, DateTime? from, DateTime? to)
        {
            List<EventRecordInfo> source = events?.ToList() ?? new List<EventRecordInfo>();

            AnalysisResult result = new()
            {
                Total = source.Count,
                RangeStart = from,
                RangeEnd = to,
                LogsLabel = logsLabel ?? string.Empty
            };

            foreach (EventRecordInfo item in source)
            {
                string level = (item.Level ?? string.Empty).Trim();
                if (level.Equals("Critical", StringComparison.OrdinalIgnoreCase))
                {
                    result.CriticalCount++;
                }
                else if (level.Equals("Error", StringComparison.OrdinalIgnoreCase))
                {
                    result.ErrorCount++;
                }
                else if (level.Equals("Warning", StringComparison.OrdinalIgnoreCase))
                {
                    result.WarningCount++;
                }
                else if (level.Equals("Information", StringComparison.OrdinalIgnoreCase) ||
                         level.Equals("Info", StringComparison.OrdinalIgnoreCase))
                {
                    result.InfoCount++;
                }
                else
                {
                    result.OtherLevelCount++;
                }

                DateTime localTime = item.TimeCreated.LocalDateTime;
                if (localTime > DateTime.MinValue && localTime.Hour >= 0 && localTime.Hour <= 23)
                {
                    result.HourlyCounts[localTime.Hour]++;
                }
            }

            result.TopProviders = source
                .GroupBy(static x => string.IsNullOrWhiteSpace(x.ProviderName) ? "(Unknown)" : x.ProviderName)
                .Select(static g => new KeyValuePair<string, int>(g.Key, g.Count()))
                .OrderByDescending(static kv => kv.Value)
                .ThenBy(static kv => kv.Key, StringComparer.CurrentCulture)
                .ToList();

            result.ProviderDistribution = result.TopProviders
                .Select(static kv => new KeyValuePair<string, int>(kv.Key, kv.Value))
                .ToList();
            result.UniqueProviderCount = result.ProviderDistribution.Count;

            result.TopProviders = result.TopProviders
                .Take(10)
                .ToList();

            result.TopEventIds = source
                .GroupBy(static x => x.Id)
                .Select(static g => new KeyValuePair<int, int>(g.Key, g.Count()))
                .OrderByDescending(static kv => kv.Value)
                .ThenBy(static kv => kv.Key)
                .ToList();

            result.UniqueEventIdCount = result.TopEventIds.Count;
            result.TopEventIds = result.TopEventIds
                .Take(10)
                .ToList();

            if (result.HourlyCounts.Length == 24)
            {
                int peakHour = 0;
                int peakCount = result.HourlyCounts[0];
                for (int i = 1; i < result.HourlyCounts.Length; i++)
                {
                    if (result.HourlyCounts[i] > peakCount)
                    {
                        peakHour = i;
                        peakCount = result.HourlyCounts[i];
                    }
                }

                result.PeakHour = result.Total > 0 ? peakHour : -1;
                result.PeakHourCount = result.Total > 0 ? peakCount : 0;
                double average = result.Total / 24d;
                result.ConcentrationScore = average > 0 ? peakCount / average : 0d;
            }

            if (result.Total > 0 && result.ProviderDistribution.Count > 0)
            {
                int top3Count = result.ProviderDistribution.Take(3).Sum(static x => x.Value);
                result.Top3ProviderShare = top3Count * 100d / result.Total;
            }

            List<EventRecordInfo> orderedByTime = source
                .OrderBy(static x => x.TimeCreated)
                .ToList();
            string currentProvider = string.Empty;
            int runLength = 0;
            int maxRun = 0;
            string maxProvider = string.Empty;
            foreach (EventRecordInfo item in orderedByTime)
            {
                string provider = string.IsNullOrWhiteSpace(item.ProviderName) ? "(Unknown)" : item.ProviderName;
                if (provider.Equals(currentProvider, StringComparison.Ordinal))
                {
                    runLength++;
                }
                else
                {
                    currentProvider = provider;
                    runLength = 1;
                }

                if (runLength > maxRun)
                {
                    maxRun = runLength;
                    maxProvider = provider;
                }
            }

            result.BurstProvider = maxProvider;
            result.BurstCount = maxRun;

            return result;
        }

        public static List<KeyValuePair<string, int>> BuildHourlyRows(int[]? hourlyCounts)
        {
            int[] values = hourlyCounts is { Length: 24 } ? hourlyCounts : new int[24];
            List<KeyValuePair<string, int>> rows = new(24);

            for (int hour = 0; hour < 24; hour++)
            {
                rows.Add(new KeyValuePair<string, int>(hour.ToString("00", CultureInfo.InvariantCulture), values[hour]));
            }

            return rows;
        }
    }
}
