using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Linq;
using LogAnalyzer.Models;

namespace LogAnalyzer.Services
{
    public class EventLogCollector
    {
        public List<EventRecordInfo> Collect(string logName, DateTime startTime, DateTime endTime, IReadOnlyList<int> levels, int maxEvents)
        {
            int limitedMax = maxEvents > 0 ? maxEvents : 500;
            string queryString = BuildQuery(startTime, endTime, levels);

            EventLogQuery query = new(logName, PathType.LogName, queryString)
            {
                ReverseDirection = true,
                TolerateQueryErrors = true
            };

            List<EventRecordInfo> rows = new(limitedMax);

            using EventLogReader reader = new(query);
            for (int i = 0; i < limitedMax; i++)
            {
                using EventRecord? record = reader.ReadEvent();
                if (record == null)
                {
                    break;
                }

                string message = string.Empty;
                try
                {
                    message = record.FormatDescription() ?? string.Empty;
                }
                catch
                {
                    message = string.Empty;
                }

                string normalizedMessage = NormalizeMessage(message);

                rows.Add(new EventRecordInfo
                {
                    TimeCreated = ToDateTimeOffset(record.TimeCreated),
                    LogName = logName,
                    Level = ToLevelText(record.Level),
                    ProviderName = record.ProviderName ?? string.Empty,
                    Id = record.Id,
                    MachineName = record.MachineName ?? string.Empty,
                    Message = normalizedMessage
                });
            }

            rows.Sort((a, b) => b.TimeCreated.CompareTo(a.TimeCreated));
            return rows;
        }

        private static string BuildQuery(DateTime startTime, DateTime endTime, IReadOnlyList<int> levels)
        {
            string startUtc = startTime.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
            string endUtc = endTime.ToUniversalTime().ToString("o", CultureInfo.InvariantCulture);
            string levelClause = string.Join(" or ", levels.Select(static l => $"Level={l}"));

            return $"*[System[({levelClause}) and TimeCreated[@SystemTime >= '{startUtc}' and @SystemTime <= '{endUtc}']]]";
        }

        private static string ToLevelText(byte? level)
        {
            return level switch
            {
                1 => "Critical",
                2 => "Error",
                3 => "Warning",
                4 => "Information",
                _ => string.Empty
            };
        }

        private static DateTimeOffset ToDateTimeOffset(DateTime? value)
        {
            if (!value.HasValue)
            {
                return DateTimeOffset.MinValue;
            }

            DateTime dateTime = value.Value;
            if (dateTime.Kind == DateTimeKind.Unspecified)
            {
                dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Local);
            }

            return new DateTimeOffset(dateTime);
        }

        private static string NormalizeMessage(string text)
        {
            return (text ?? string.Empty).Replace("\r\n", "\n").Replace('\r', '\n');
        }
    }
}
