using System;
using LogAnalyzer.Models;

namespace LogAnalyzer.Services
{
    public static class UserConfigService
    {
        public static string PrimaryPath => SettingsIniService.ConfigPath;
        public static string FallbackPath => SettingsIniService.ConfigPath;

        public static (UserConfig Config, string LoadedFrom) LoadOrDefault(Func<UserConfig> defaultFactory)
        {
            (UserConfig config, bool loaded) = SettingsIniService.LoadGraphConfig();
            if (loaded)
            {
                return (config, PrimaryPath);
            }

            UserConfig defaultConfig = defaultFactory();
            SaveResult saveResult = Save(defaultConfig);
            string savedPath = saveResult.Success ? saveResult.SavedPath : PrimaryPath;
            return (defaultConfig, savedPath);
        }

        public static SaveResult Save(UserConfig config)
        {
            try
            {
                SettingsIniService.SaveGraphConfig(config);
                return new SaveResult(true, PrimaryPath, false, null);
            }
            catch (Exception ex)
            {
                return new SaveResult(false, string.Empty, false, ex.Message);
            }
        }

        public sealed record SaveResult(bool Success, string SavedPath, bool UsedFallback, string? ErrorMessage);
    }
}
