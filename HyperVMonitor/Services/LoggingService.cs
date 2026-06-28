namespace HyperVMonitor.Services
{
    public class LoggingService
    {
        private readonly string _logDirectory = @"C:\HyperV-Monitor\Logs";
        private readonly string _logFile;
        private readonly object _lockObject = new object();

        public LoggingService()
        {
            // Create log directory if it doesn't exist
            if (!Directory.Exists(_logDirectory))
            {
                Directory.CreateDirectory(_logDirectory);
            }

            _logFile = Path.Combine(_logDirectory, $"log_{DateTime.Now:yyyy-MM-dd}.log");
            
            // Clean up old logs (older than 3 days)
            CleanupOldLogs();
        }

        public void Log(string message)
        {
            lock (_lockObject)
            {
                try
                {
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    var logEntry = $"[{timestamp}] {message}";
                    
                    File.AppendAllText(_logFile, logEntry + Environment.NewLine);
                    
                    // Also log to console for debugging
                    Console.WriteLine(logEntry);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Naplózás hiba: {ex.Message}");
                }
            }
        }

        private void CleanupOldLogs()
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-3);
                var files = Directory.GetFiles(_logDirectory, "log_*.log");

                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        File.Delete(file);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Log cleanup hiba: {ex.Message}");
            }
        }

        public List<string> GetRecentLogs(int lines = 100)
        {
            try
            {
                var allLines = File.ReadAllLines(_logFile);
                return allLines.Skip(Math.Max(0, allLines.Length - lines)).ToList();
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}
