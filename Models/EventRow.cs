namespace LogAnalyzer.Models
{
    public class EventRow
    {
        public DateTime TimeCreated { get; set; }
        public string TimeDisplay { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public string ProviderName { get; set; } = string.Empty;
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public string MessageShort { get; set; } = string.Empty;
    }
}
