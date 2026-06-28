namespace HyperVMonitor.Models
{
    public class HistoryEventModel
    {
        public string Timestamp { get; set; } = string.Empty;
        public string ServerName { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }
}
