using System;
using System.Collections.Generic;

namespace LogAnalyzer.Models
{
    public class AnalysisResult
    {
        public int Total { get; set; }
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
        public int InfoCount { get; set; }
        public int CriticalCount { get; set; }
        public int OtherLevelCount { get; set; }
        public List<KeyValuePair<string, int>> TopProviders { get; set; } = new();
        public List<KeyValuePair<int, int>> TopEventIds { get; set; } = new();
        public int[] HourlyCounts { get; set; } = new int[24];
        public DateTime? RangeStart { get; set; }
        public DateTime? RangeEnd { get; set; }
        public string LogsLabel { get; set; } = string.Empty;
        public List<KeyValuePair<string, int>> ProviderDistribution { get; set; } = new();
        public int UniqueProviderCount { get; set; }
        public int UniqueEventIdCount { get; set; }
        public int PeakHour { get; set; } = -1;
        public int PeakHourCount { get; set; }
        public double ConcentrationScore { get; set; }
        public double Top3ProviderShare { get; set; }
        public string BurstProvider { get; set; } = string.Empty;
        public int BurstCount { get; set; }
    }
}
