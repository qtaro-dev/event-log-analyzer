using System;
using System.IO;
using System.Text.Json;
using LogAnalyzer.Models;

namespace LogAnalyzer.Services
{
    public static class UserConfigService
    {
        private const string ConfigFileName = "LogAnalyzer.graph.config.json";

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        public static string PrimaryPath => Path.Combine(AppContext.BaseDirectory, ConfigFileName);

        public static string FallbackPath
        {
            get
            {
                string baseDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "LogAnalyzer");
                return Path.Combine(baseDir, ConfigFileName);
            }
        }

        public static (UserConfig Config, string LoadedFrom) LoadOrDefault(Func<UserConfig> defaultFactory)
        {
            if (TryLoadFromPath(PrimaryPath, out UserConfig? primaryConfig))
            {
                return (primaryConfig!, PrimaryPath);
            }

            if (TryLoadFromPath(FallbackPath, out UserConfig? fallbackConfig))
            {
                return (fallbackConfig!, FallbackPath);
            }

            return (defaultFactory(), PrimaryPath);
        }

        public static SaveResult Save(UserConfig config)
        {
            string json = JsonSerializer.Serialize(config, JsonOptions);

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(PrimaryPath) ?? AppContext.BaseDirectory);
                File.WriteAllText(PrimaryPath, json);
                return new SaveResult(true, PrimaryPath, false, null);
            }
            catch (Exception primaryEx)
            {
                try
                {
                    string? fallbackDir = Path.GetDirectoryName(FallbackPath);
                    if (!string.IsNullOrWhiteSpace(fallbackDir))
                    {
                        Directory.CreateDirectory(fallbackDir);
                    }

                    File.WriteAllText(FallbackPath, json);
                    return new SaveResult(true, FallbackPath, true, primaryEx.Message);
                }
                catch (Exception fallbackEx)
                {
                    return new SaveResult(false, string.Empty, true, $"{primaryEx.Message} / {fallbackEx.Message}");
                }
            }
        }

        private static bool TryLoadFromPath(string path, out UserConfig? config)
        {
            config = null;
            try
            {
                if (!File.Exists(path))
                {
                    return false;
                }

                string json = File.ReadAllText(path);
                UserConfig? parsed = JsonSerializer.Deserialize<UserConfig>(json, JsonOptions);
                if (parsed == null)
                {
                    return false;
                }

                config = parsed;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public sealed record SaveResult(bool Success, string SavedPath, bool UsedFallback, string? ErrorMessage);
    }
}
