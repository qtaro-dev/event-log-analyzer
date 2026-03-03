using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using LogAnalyzer.Models;
using LogAnalyzer.Properties;

namespace LogAnalyzer.Services
{
    public static class SettingsIniService
    {
        private const string AppSettingsSection = "AppSettings";
        private const string GraphConfigSection = "GraphConfig";
        private const string GraphConfigKey = "JsonBase64";

        private static readonly JsonSerializerOptions GraphJsonOptions = new()
        {
            WriteIndented = true
        };

        public static string ConfigPath => Path.Combine(AppContext.BaseDirectory, "Settings.ini");

        public static void LoadIntoSettings()
        {
            IniDocument document = LoadDocument();
            foreach (SettingsProperty property in Settings.Default.Properties)
            {
                if (!document.TryGetValue(AppSettingsSection, property.Name, out string? rawValue))
                {
                    continue;
                }

                if (rawValue == null)
                {
                    continue;
                }

                object? parsed = ParseSettingValue(property.PropertyType, rawValue);
                if (parsed != null)
                {
                    Settings.Default[property.Name] = parsed;
                }
            }
        }

        public static void SaveSettings()
        {
            IniDocument document = LoadDocument();

            foreach (SettingsProperty property in Settings.Default.Properties)
            {
                object? value = Settings.Default[property.Name];
                document.SetValue(AppSettingsSection, property.Name, FormatSettingValue(property.PropertyType, value));
            }

            SaveDocument(document);
        }

        public static (UserConfig Config, bool Loaded) LoadGraphConfig()
        {
            IniDocument document = LoadDocument();
            if (!document.TryGetValue(GraphConfigSection, GraphConfigKey, out string? rawValue) || string.IsNullOrWhiteSpace(rawValue))
            {
                return (new UserConfig(), false);
            }

            try
            {
                byte[] bytes = Convert.FromBase64String(rawValue);
                UserConfig? parsed = JsonSerializer.Deserialize<UserConfig>(bytes, GraphJsonOptions);
                return parsed == null ? (new UserConfig(), false) : (parsed, true);
            }
            catch
            {
                return (new UserConfig(), false);
            }
        }

        public static bool SaveGraphConfig(UserConfig config)
        {
            IniDocument document = LoadDocument();
            string json = JsonSerializer.Serialize(config, GraphJsonOptions);
            string base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
            document.SetValue(GraphConfigSection, GraphConfigKey, base64);
            SaveDocument(document);
            return true;
        }

        private static object? ParseSettingValue(Type propertyType, string rawValue)
        {
            if (propertyType == typeof(string))
            {
                return rawValue;
            }

            if (propertyType == typeof(bool) && bool.TryParse(rawValue, out bool boolValue))
            {
                return boolValue;
            }

            if (propertyType == typeof(int) && int.TryParse(rawValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue))
            {
                return intValue;
            }

            if (propertyType == typeof(DateTime) && DateTime.TryParse(rawValue, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime dateValue))
            {
                return dateValue;
            }

            return null;
        }

        private static string FormatSettingValue(Type propertyType, object? value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (propertyType == typeof(DateTime) && value is DateTime dateTime)
            {
                return dateTime.ToString("o", CultureInfo.InvariantCulture);
            }

            if (value is IFormattable formattable)
            {
                return formattable.ToString(null, CultureInfo.InvariantCulture);
            }

            return value.ToString() ?? string.Empty;
        }

        private static IniDocument LoadDocument()
        {
            IniDocument document = new();
            if (!File.Exists(ConfigPath))
            {
                return document;
            }

            string? currentSection = null;
            foreach (string rawLine in File.ReadAllLines(ConfigPath))
            {
                string line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith(";", StringComparison.Ordinal))
                {
                    continue;
                }

                if (line.StartsWith("[", StringComparison.Ordinal) && line.EndsWith("]", StringComparison.Ordinal))
                {
                    currentSection = line[1..^1].Trim();
                    document.EnsureSection(currentSection);
                    continue;
                }

                int separatorIndex = line.IndexOf('=');
                if (separatorIndex <= 0 || string.IsNullOrWhiteSpace(currentSection))
                {
                    continue;
                }

                string key = line[..separatorIndex].Trim();
                string value = line[(separatorIndex + 1)..];
                document.SetValue(currentSection, key, value);
            }

            return document;
        }

        private static void SaveDocument(IniDocument document)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath) ?? AppContext.BaseDirectory);

            List<string> lines = new();
            foreach (KeyValuePair<string, Dictionary<string, string>> section in document.Sections.OrderBy(static x => x.Key, StringComparer.OrdinalIgnoreCase))
            {
                lines.Add($"[{section.Key}]");
                foreach (KeyValuePair<string, string> entry in section.Value.OrderBy(static x => x.Key, StringComparer.OrdinalIgnoreCase))
                {
                    lines.Add($"{entry.Key}={entry.Value}");
                }

                lines.Add(string.Empty);
            }

            File.WriteAllLines(ConfigPath, lines, new UTF8Encoding(false));
        }

        private sealed class IniDocument
        {
            public Dictionary<string, Dictionary<string, string>> Sections { get; } = new(StringComparer.OrdinalIgnoreCase);

            public void EnsureSection(string section)
            {
                if (!Sections.ContainsKey(section))
                {
                    Sections[section] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }
            }

            public void SetValue(string section, string key, string value)
            {
                EnsureSection(section);
                Sections[section][key] = value ?? string.Empty;
            }

            public bool TryGetValue(string section, string key, out string? value)
            {
                value = null;
                if (!Sections.TryGetValue(section, out Dictionary<string, string>? sectionValues))
                {
                    return false;
                }

                return sectionValues.TryGetValue(key, out value);
            }
        }
    }
}
