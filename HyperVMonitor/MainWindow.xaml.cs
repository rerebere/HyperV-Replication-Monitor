using System.Windows;
using HyperVMonitor.Services;
using HyperVMonitor.Models;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Windows.Controls;

namespace HyperVMonitor
{
    public partial class MainWindow : Window
    {
        private HyperVService _hvService;
        private LoggingService _logger;
        private DispatcherTimer _monitoringTimer;
        private ConfigurationService _configService;
        
        public ObservableCollection<VmStatusModel> VmStatusList { get; set; }
        public ObservableCollection<HistoryEventModel> HistoryList { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            
            VmStatusList = new ObservableCollection<VmStatusModel>();
            HistoryList = new ObservableCollection<HistoryEventModel>();
            
            VmStatusGrid.ItemsSource = VmStatusList;
            HistoryGrid.ItemsSource = HistoryList;
            
            _logger = new LoggingService();
            _configService = new ConfigurationService();
            _hvService = new HyperVService(_logger);
            
            InitializeMonitoring();
            LoadSettings();
        }

        private void InitializeMonitoring()
        {
            _monitoringTimer = new DispatcherTimer();
            _monitoringTimer.Interval = TimeSpan.FromMinutes(1);
            _monitoringTimer.Tick += MonitoringTimer_Tick;
            _monitoringTimer.Start();
            
            // Initial status check
            MonitoringTimer_Tick(null, null);
        }

        private async void MonitoringTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                // Get credentials from config
                var creds1 = _configService.GetCredentials("HMHUVG21MP01");
                var creds2 = _configService.GetCredentials("HMHUVG21MP02");
                
                if (creds1 == null || creds2 == null)
                {
                    UpdateServerStatus("Server1Status", "Bejelentkezési adatok hiányoznak", "#F44336");
                    UpdateServerStatus("Server2Status", "Bejelentkezési adatok hiányoznak", "#F44336");
                    return;
                }
                
                // Check Server 1
                var server1Status = await _hvService.GetServerStatusAsync("10.8.248.40", creds1.Username, creds1.Password);
                UpdateServerStatus("Server1Status", server1Status, server1Status.Contains("Online") ? "#4CAF50" : "#F44336");
                
                // Check Server 2
                var server2Status = await _hvService.GetServerStatusAsync("10.8.248.41", creds2.Username, creds2.Password);
                UpdateServerStatus("Server2Status", server2Status, server2Status.Contains("Online") ? "#4CAF50" : "#F44336");
                
                // Get VM statuses
                var vmStatuses = await _hvService.GetVmReplicationStatusAsync("10.8.248.40", creds1.Username, creds1.Password);
                UpdateVmStatus(vmStatuses);
            }
            catch (Exception ex)
            {
                _logger.Log($"Monitoring hiba: {ex.Message}");
            }
        }

        private void UpdateServerStatus(string elementName, string status, string color)
        {
            var element = this.FindName(elementName) as TextBlock;
            if (element != null)
            {
                element.Text = status;
                element.Foreground = new System.Windows.Media.SolidColorBrush(
                    (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(color)
                );
            }
        }

        private void UpdateVmStatus(List<VmStatusModel> statuses)
        {
            Dispatcher.Invoke(() =>
            {
                VmStatusList.Clear();
                foreach (var vm in statuses)
                {
                    VmStatusList.Add(vm);
                }
            });
        }

        private void Failover1_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Biztos vagy benne, hogy failovert szeretnél indítani a HMHUVG21MP01 szerveren?",
                "Failover Megerősítés",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );
            
            if (result == MessageBoxResult.Yes)
            {
                ExecuteFailover("HMHUVG21MP01");
            }
        }

        private void Failover2_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Biztos vagy benne, hogy failovert szeretnél indítani a HMHUVG21MP02 szerveren?",
                "Failover Megerősítés",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );
            
            if (result == MessageBoxResult.Yes)
            {
                ExecuteFailover("HMHUVG21MP02");
            }
        }

        private async void ExecuteFailover(string serverName)
        {
            try
            {
                var ip = serverName == "HMHUVG21MP01" ? "10.8.248.40" : "10.8.248.41";
                var creds = _configService.GetCredentials(serverName);
                
                if (creds == null)
                {
                    MessageBox.Show("Bejelentkezési adatok hiányoznak!");
                    return;
                }
                
                var result = await _hvService.ExecuteFailoverAsync(ip, creds.Username, creds.Password);
                
                _logger.Log($"Failover végrehajtva: {serverName} - {result}");
                AddHistoryEvent(serverName, "Failover", result);
                
                MessageBox.Show(result, "Failover Eredmény");
            }
            catch (Exception ex)
            {
                _logger.Log($"Failover hiba: {ex.Message}");
                MessageBox.Show($"Hiba: {ex.Message}", "Failover Hiba");
            }
        }

        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var user1 = Server1User.Text;
                var pass1 = Server1Pass.Password;
                var user2 = Server2User.Text;
                var pass2 = Server2Pass.Password;
                
                _configService.SaveCredentials("HMHUVG21MP01", user1, pass1);
                _configService.SaveCredentials("HMHUVG21MP02", user2, pass2);
                
                MessageBox.Show("Beállítások mentve!", "Siker");
                _logger.Log("Beállítások frissítve");
                AddHistoryEvent("Rendszer", "Beállítások", "Bejelentkezési adatok frissítve");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba: {ex.Message}", "Mentés Hiba");
                _logger.Log($"Beállítások mentés hiba: {ex.Message}");
            }
        }

        private async void TestConnection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var creds1 = _configService.GetCredentials("HMHUVG21MP01");
                var creds2 = _configService.GetCredentials("HMHUVG21MP02");
                
                if (creds1 == null || creds2 == null)
                {
                    MessageBox.Show("Kérjük mentsd a bejelentkezési adatokat!");
                    return;
                }
                
                var test1 = await _hvService.TestConnectionAsync("10.8.248.40", creds1.Username, creds1.Password);
                var test2 = await _hvService.TestConnectionAsync("10.8.248.41", creds2.Username, creds2.Password);
                
                var result = $"Server 1: {test1}\nServer 2: {test2}";
                MessageBox.Show(result, "Kapcsolat Teszt");
                
                _logger.Log($"Kapcsolat teszt: {result}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba: {ex.Message}");
                _logger.Log($"Kapcsolat teszt hiba: {ex.Message}");
            }
        }

        private void LoadSettings()
        {
            try
            {
                var creds1 = _configService.GetCredentials("HMHUVG21MP01");
                var creds2 = _configService.GetCredentials("HMHUVG21MP02");
                
                if (creds1 != null)
                {
                    Server1User.Text = creds1.Username;
                }
                
                if (creds2 != null)
                {
                    Server2User.Text = creds2.Username;
                }
            }
            catch
            {
                // Settings don't exist yet
            }
        }

        private void AddHistoryEvent(string serverName, string eventType, string details)
        {
            Dispatcher.Invoke(() =>
            {
                var historyEvent = new HistoryEventModel
                {
                    Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    ServerName = serverName,
                    EventType = eventType,
                    Details = details
                };
                
                HistoryList.Insert(0, historyEvent);
                
                // Keep only last 1000 items in memory
                while (HistoryList.Count > 1000)
                {
                    HistoryList.RemoveAt(HistoryList.Count - 1);
                }
            });
        }
    }
}
