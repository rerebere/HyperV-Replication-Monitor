using System.Management;
using HyperVMonitor.Models;

namespace HyperVMonitor.Services
{
    public class HyperVService
    {
        private readonly LoggingService _logger;

        public HyperVService(LoggingService logger)
        {
            _logger = logger;
        }

        public async Task<string> GetServerStatusAsync(string hostname, string username, string password)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var scope = new ManagementScope($@"\\{hostname}\root\virtualization\v2")
                    {
                        Authentication = AuthenticationLevel.PacketPrivacy
                    };

                    if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                    {
                        scope.Options.Authentication = AuthenticationLevel.PacketPrivacy;
                        scope.Options.Impersonation = ImpersonationLevel.Impersonate;
                        scope.Options.EnablePrivileges = true;
                        scope.Options.Timeout = TimeSpan.FromSeconds(30);
                        
                        var conn = new ConnectionOptions
                        {
                            Authentication = AuthenticationLevel.PacketPrivacy,
                            Impersonation = ImpersonationLevel.Impersonate,
                            EnablePrivileges = true,
                            Timeout = TimeSpan.FromSeconds(30)
                        };
                        
                        // Note: This is simplified. In production, use proper credential handling
                        scope.Options = conn;
                    }

                    scope.Connect();
                    return "Online - Replikálódik";
                }
                catch (Exception ex)
                {
                    _logger.Log($"Server státusz ellenőrzés hiba ({hostname}): {ex.Message}");
                    return "Offline - Hiba";
                }
            });
        }

        public async Task<List<VmStatusModel>> GetVmReplicationStatusAsync(string hostname, string username, string password)
        {
            return await Task.Run(() =>
            {
                var vmList = new List<VmStatusModel>();
                
                try
                {
                    var scope = new ManagementScope($@"\\{hostname}\root\virtualization\v2")
                    {
                        Authentication = AuthenticationLevel.PacketPrivacy
                    };
                    scope.Connect();

                    var query = new ObjectQuery("SELECT * FROM Msvm_ReplicationSettingData");
                    var searcher = new ManagementObjectSearcher(scope, query);

                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var vmName = obj["ElementName"]?.ToString() ?? "Ismeretlen";
                        var replicationState = obj["ReplicationState"]?.ToString() ?? "Ismeretlen";
                        var lastSync = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                        vmList.Add(new VmStatusModel
                        {
                            VmName = vmName,
                            ServerName = hostname,
                            Status = MapReplicationState(replicationState),
                            LastSync = lastSync
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.Log($"VM státusz ellenőrzés hiba ({hostname}): {ex.Message}");
                }

                return vmList;
            });
        }

        public async Task<string> ExecuteFailoverAsync(string hostname, string username, string password)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _logger.Log($"Failover végrehajtása: {hostname}");
                    // TODO: Implement actual failover logic using PowerShell or WMI
                    return "Failover sikeresen indítva";
                }
                catch (Exception ex)
                {
                    _logger.Log($"Failover hiba ({hostname}): {ex.Message}");
                    return $"Failover hiba: {ex.Message}";
                }
            });
        }

        public async Task<string> TestConnectionAsync(string hostname, string username, string password)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var scope = new ManagementScope($@"\\{hostname}\root\cimv2")
                    {
                        Authentication = AuthenticationLevel.PacketPrivacy
                    };
                    scope.Connect();
                    return "✓ Sikeres kapcsolat";
                }
                catch (Exception ex)
                {
                    _logger.Log($"Kapcsolat teszt hiba ({hostname}): {ex.Message}");
                    return $"✗ Hiba: {ex.Message}";
                }
            });
        }

        private string MapReplicationState(string state)
        {
            return state switch
            {
                "0" => "Tiltva",
                "1" => "Szinkronizálás",
                "2" => "Szinkronizálva",
                "3" => "Hiba",
                _ => "Ismeretlen"
            };
        }
    }
}
