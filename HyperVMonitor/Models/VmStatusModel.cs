namespace HyperVMonitor.Models
{
    public class VmStatusModel
    {
        public string VmName { get; set; } = string.Empty;
        public string ServerName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string LastSync { get; set; } = string.Empty;
    }
}
