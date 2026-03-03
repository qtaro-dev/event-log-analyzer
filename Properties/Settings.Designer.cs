namespace LogAnalyzer.Properties
{
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Manual", "1.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase
    {
        private static readonly Settings defaultInstance =
            (Settings)global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings());

        public static Settings Default => defaultInstance;

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool AgreeToAdminLogAccess
        {
            get => (bool)this[nameof(AgreeToAdminLogAccess)];
            set => this[nameof(AgreeToAdminLogAccess)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("en")]
        public string UiLanguage
        {
            get => (string)this[nameof(UiLanguage)];
            set => this[nameof(UiLanguage)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string OutputDirectory
        {
            get => (string)this[nameof(OutputDirectory)];
            set => this[nameof(OutputDirectory)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool WriteJsonl
        {
            get => (bool)this[nameof(WriteJsonl)];
            set => this[nameof(WriteJsonl)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool WriteCsv
        {
            get => (bool)this[nameof(WriteCsv)];
            set => this[nameof(WriteCsv)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool WriteSummary
        {
            get => (bool)this[nameof(WriteSummary)];
            set => this[nameof(WriteSummary)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool DedupMessage
        {
            get => (bool)this[nameof(DedupMessage)];
            set => this[nameof(DedupMessage)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool IncludeXml
        {
            get => (bool)this[nameof(IncludeXml)];
            set => this[nameof(IncludeXml)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool ShowOnlyNew
        {
            get => (bool)this[nameof(ShowOnlyNew)];
            set => this[nameof(ShowOnlyNew)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Light")]
        public string UiTheme
        {
            get => (string)this[nameof(UiTheme)];
            set => this[nameof(UiTheme)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("24h")]
        public string TimeFormat
        {
            get => (string)this[nameof(TimeFormat)];
            set => this[nameof(TimeFormat)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("500")]
        public int LastTopN
        {
            get => (int)this[nameof(LastTopN)];
            set => this[nameof(LastTopN)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool LastLogsSystem
        {
            get => (bool)this[nameof(LastLogsSystem)];
            set => this[nameof(LastLogsSystem)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool LastLogsApplication
        {
            get => (bool)this[nameof(LastLogsApplication)];
            set => this[nameof(LastLogsApplication)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool LastLevelCritical
        {
            get => (bool)this[nameof(LastLevelCritical)];
            set => this[nameof(LastLevelCritical)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool LastLevelError
        {
            get => (bool)this[nameof(LastLevelError)];
            set => this[nameof(LastLevelError)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool LastLevelWarning
        {
            get => (bool)this[nameof(LastLevelWarning)];
            set => this[nameof(LastLevelWarning)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool LastLevelInformation
        {
            get => (bool)this[nameof(LastLevelInformation)];
            set => this[nameof(LastLevelInformation)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0001-01-01T00:00:00")]
        public global::System.DateTime LastStartTime
        {
            get => (global::System.DateTime)this[nameof(LastStartTime)];
            set => this[nameof(LastStartTime)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0001-01-01T00:00:00")]
        public global::System.DateTime LastEndTime
        {
            get => (global::System.DateTime)this[nameof(LastEndTime)];
            set => this[nameof(LastEndTime)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool LastUseTimeRange
        {
            get => (bool)this[nameof(LastUseTimeRange)];
            set => this[nameof(LastUseTimeRange)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("PaletteA")]
        public string PiePaletteId
        {
            get => (string)this[nameof(PiePaletteId)];
            set => this[nameof(PiePaletteId)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("GridHeaderBackgroundBrush")]
        public string HourBarFillKey
        {
            get => (string)this[nameof(HourBarFillKey)];
            set => this[nameof(HourBarFillKey)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("GridSelectionBackgroundBrush")]
        public string HourPeakFillKey
        {
            get => (string)this[nameof(HourPeakFillKey)];
            set => this[nameof(HourPeakFillKey)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("InputForegroundBrush")]
        public string HourBarLabelTextKey
        {
            get => (string)this[nameof(HourBarLabelTextKey)];
            set => this[nameof(HourBarLabelTextKey)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("theme:GridHeaderBackgroundBrush")]
        public string OutputListSelectionKey
        {
            get => (string)this[nameof(OutputListSelectionKey)];
            set => this[nameof(OutputListSelectionKey)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AnalysisProviderColor1|AnalysisProviderColor2|AnalysisProviderColor3|AnalysisProviderColor4|AnalysisProviderColor5|AnalysisProviderColor6|AnalysisProviderColor7|AnalysisProviderColor8|AnalysisProviderColor1|AnalysisProviderColor2")]
        public string PiePaletteAColorKeys
        {
            get => (string)this[nameof(PiePaletteAColorKeys)];
            set => this[nameof(PiePaletteAColorKeys)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AnalysisProviderColor8|AnalysisProviderColor7|AnalysisProviderColor6|AnalysisProviderColor5|AnalysisProviderColor4|AnalysisProviderColor3|AnalysisProviderColor2|AnalysisProviderColor1|AnalysisProviderColor8|AnalysisProviderColor7")]
        public string PiePaletteBColorKeys
        {
            get => (string)this[nameof(PiePaletteBColorKeys)];
            set => this[nameof(PiePaletteBColorKeys)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("PaletteA")]
        public string BarPaletteSelectedId
        {
            get => (string)this[nameof(BarPaletteSelectedId)];
            set => this[nameof(BarPaletteSelectedId)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("GridHeaderBackgroundBrush|GridSelectionBackgroundBrush|PanelBackgroundBrush|BorderBrush")]
        public string BarPaletteAColorKeys
        {
            get => (string)this[nameof(BarPaletteAColorKeys)];
            set => this[nameof(BarPaletteAColorKeys)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("GridSelectionBackgroundBrush|GridHeaderBackgroundBrush|InputBackgroundBrush|BorderBrush")]
        public string BarPaletteBColorKeys
        {
            get => (string)this[nameof(BarPaletteBColorKeys)];
            set => this[nameof(BarPaletteBColorKeys)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("A")]
        public string PiePaletteSelectedId
        {
            get => (string)this[nameof(PiePaletteSelectedId)];
            set => this[nameof(PiePaletteSelectedId)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AnalysisProviderColor1")]
        public string PiePaletteA_ColorKey1
        {
            get => (string)this[nameof(PiePaletteA_ColorKey1)];
            set => this[nameof(PiePaletteA_ColorKey1)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AnalysisProviderColor2")]
        public string PiePaletteA_ColorKey2
        {
            get => (string)this[nameof(PiePaletteA_ColorKey2)];
            set => this[nameof(PiePaletteA_ColorKey2)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AnalysisProviderColor3")]
        public string PiePaletteA_ColorKey3
        {
            get => (string)this[nameof(PiePaletteA_ColorKey3)];
            set => this[nameof(PiePaletteA_ColorKey3)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AnalysisProviderColor4")]
        public string PiePaletteA_ColorKey4
        {
            get => (string)this[nameof(PiePaletteA_ColorKey4)];
            set => this[nameof(PiePaletteA_ColorKey4)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AnalysisProviderColor5")]
        public string PiePaletteA_ColorKey5
        {
            get => (string)this[nameof(PiePaletteA_ColorKey5)];
            set => this[nameof(PiePaletteA_ColorKey5)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AnalysisProviderColor6")]
        public string PiePaletteA_ColorKey6
        {
            get => (string)this[nameof(PiePaletteA_ColorKey6)];
            set => this[nameof(PiePaletteA_ColorKey6)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AnalysisProviderColor7")]
        public string PiePaletteA_ColorKey7
        {
            get => (string)this[nameof(PiePaletteA_ColorKey7)];
            set => this[nameof(PiePaletteA_ColorKey7)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AnalysisProviderColor8")]
        public string PiePaletteA_ColorKey8
        {
            get => (string)this[nameof(PiePaletteA_ColorKey8)];
            set => this[nameof(PiePaletteA_ColorKey8)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AnalysisProviderColor1")]
        public string PiePaletteA_ColorKey9
        {
            get => (string)this[nameof(PiePaletteA_ColorKey9)];
            set => this[nameof(PiePaletteA_ColorKey9)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AnalysisProviderColor2")]
        public string PiePaletteA_ColorKey10
        {
            get => (string)this[nameof(PiePaletteA_ColorKey10)];
            set => this[nameof(PiePaletteA_ColorKey10)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AnalysisProviderColor8")]
        public string PiePaletteB_ColorKey1
        {
            get => (string)this[nameof(PiePaletteB_ColorKey1)];
            set => this[nameof(PiePaletteB_ColorKey1)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AnalysisProviderColor7")]
        public string PiePaletteB_ColorKey2
        {
            get => (string)this[nameof(PiePaletteB_ColorKey2)];
            set => this[nameof(PiePaletteB_ColorKey2)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AnalysisProviderColor6")]
        public string PiePaletteB_ColorKey3
        {
            get => (string)this[nameof(PiePaletteB_ColorKey3)];
            set => this[nameof(PiePaletteB_ColorKey3)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AnalysisProviderColor5")]
        public string PiePaletteB_ColorKey4
        {
            get => (string)this[nameof(PiePaletteB_ColorKey4)];
            set => this[nameof(PiePaletteB_ColorKey4)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AnalysisProviderColor4")]
        public string PiePaletteB_ColorKey5
        {
            get => (string)this[nameof(PiePaletteB_ColorKey5)];
            set => this[nameof(PiePaletteB_ColorKey5)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AnalysisProviderColor3")]
        public string PiePaletteB_ColorKey6
        {
            get => (string)this[nameof(PiePaletteB_ColorKey6)];
            set => this[nameof(PiePaletteB_ColorKey6)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AnalysisProviderColor2")]
        public string PiePaletteB_ColorKey7
        {
            get => (string)this[nameof(PiePaletteB_ColorKey7)];
            set => this[nameof(PiePaletteB_ColorKey7)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AnalysisProviderColor1")]
        public string PiePaletteB_ColorKey8
        {
            get => (string)this[nameof(PiePaletteB_ColorKey8)];
            set => this[nameof(PiePaletteB_ColorKey8)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AnalysisProviderColor8")]
        public string PiePaletteB_ColorKey9
        {
            get => (string)this[nameof(PiePaletteB_ColorKey9)];
            set => this[nameof(PiePaletteB_ColorKey9)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("AnalysisProviderColor7")]
        public string PiePaletteB_ColorKey10
        {
            get => (string)this[nameof(PiePaletteB_ColorKey10)];
            set => this[nameof(PiePaletteB_ColorKey10)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("GridHeaderBackgroundBrush")]
        public string BarPaletteA_ColorKey1
        {
            get => (string)this[nameof(BarPaletteA_ColorKey1)];
            set => this[nameof(BarPaletteA_ColorKey1)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("GridSelectionBackgroundBrush")]
        public string BarPaletteA_ColorKey2
        {
            get => (string)this[nameof(BarPaletteA_ColorKey2)];
            set => this[nameof(BarPaletteA_ColorKey2)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("PanelBackgroundBrush")]
        public string BarPaletteA_ColorKey3
        {
            get => (string)this[nameof(BarPaletteA_ColorKey3)];
            set => this[nameof(BarPaletteA_ColorKey3)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("BorderBrush")]
        public string BarPaletteA_ColorKey4
        {
            get => (string)this[nameof(BarPaletteA_ColorKey4)];
            set => this[nameof(BarPaletteA_ColorKey4)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("GridSelectionBackgroundBrush")]
        public string BarPaletteB_ColorKey1
        {
            get => (string)this[nameof(BarPaletteB_ColorKey1)];
            set => this[nameof(BarPaletteB_ColorKey1)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("GridHeaderBackgroundBrush")]
        public string BarPaletteB_ColorKey2
        {
            get => (string)this[nameof(BarPaletteB_ColorKey2)];
            set => this[nameof(BarPaletteB_ColorKey2)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("InputBackgroundBrush")]
        public string BarPaletteB_ColorKey3
        {
            get => (string)this[nameof(BarPaletteB_ColorKey3)];
            set => this[nameof(BarPaletteB_ColorKey3)] = value;
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("BorderBrush")]
        public string BarPaletteB_ColorKey4
        {
            get => (string)this[nameof(BarPaletteB_ColorKey4)];
            set => this[nameof(BarPaletteB_ColorKey4)] = value;
        }
    }
}
