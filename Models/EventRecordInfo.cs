namespace LogAnalyzer.Models
{
    public class EventRecordInfo
    {
        public DateTimeOffset TimeCreated { get; set; }
        public string LogName { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public string ProviderName { get; set; } = string.Empty;
        public int Id { get; set; }
        public string MachineName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
