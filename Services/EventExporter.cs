using System.Collections.Generic;
using System.Text.Encodings.Web;
using System.IO;
using System.Text;
using System.Text.Json;
using LogAnalyzer.Models;

namespace LogAnalyzer.Services
{
    public static class EventExporter
    {
        private static readonly JsonSerializerOptions JsonlOptions = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = false
        };

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        public static void WriteJsonl(IEnumerable<EventRecordInfo> events, string path)
        {
            using StreamWriter writer = new(path, false, new UTF8Encoding(false));
            foreach (EventRecordInfo item in events)
            {
                string json = JsonSerializer.Serialize(item, JsonlOptions);
                writer.WriteLine(json);
            }
        }

        public static void WriteJson<T>(T data, string path)
        {
            using FileStream stream = new(path, FileMode.Create, FileAccess.Write, FileShare.None);
            JsonSerializer.Serialize(stream, data, JsonOptions);
        }
    }
}
