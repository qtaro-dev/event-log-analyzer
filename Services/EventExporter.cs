using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using LogAnalyzer.Models;

namespace LogAnalyzer.Services
{
    public static class EventExporter
    {
        public static void WriteJsonl(IEnumerable<EventRecordInfo> events, string path)
        {
            JsonSerializerOptions options = new()
            {
                WriteIndented = false
            };

            using StreamWriter writer = new(path, false, new UTF8Encoding(false));
            foreach (EventRecordInfo item in events)
            {
                string json = JsonSerializer.Serialize(item, options);
                writer.WriteLine(json);
            }
        }
    }
}
