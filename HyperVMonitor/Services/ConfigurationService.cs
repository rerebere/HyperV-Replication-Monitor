using System.Text.Json;
using HyperVMonitor.Models;

namespace HyperVMonitor.Services
{
    public class ConfigurationService
    {
        private readonly string _configDirectory = @"C:\HyperV-Monitor";
        private readonly string _configFile;
        private readonly object _lockObject = new object();

        public ConfigurationService()
        {
            if (!Directory.Exists(_configDirectory))
            {
                Directory.CreateDirectory(_configDirectory);
            }

            _configFile = Path.Combine(_configDirectory, "config.json");
        }

        public void SaveCredentials(string serverName, string username, string password)
        {
            lock (_lockObject)
            {
                try
                {
                    var config = LoadConfig();
                    
                    if (!config.ContainsKey("servers"))
                    {
                        config["servers"] = new Dictionary<string, object>();
                    }

                    var servers = (Dictionary<string, object>)config["servers"];
                    
                    // Encrypt password before storing
                    var encryptedPassword = EncryptPassword(password);
                    
                    servers[serverName] = new
                    {
                        username = username,
                        password = encryptedPassword
                    };

                    SaveConfig(config);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Konfigurációs hiba: {ex.Message}");
                }
            }
        }

        public CredentialsModel? GetCredentials(string serverName)
        {
            lock (_lockObject)
            {
                try
                {
                    var config = LoadConfig();
                    
                    if (!config.ContainsKey("servers"))
                        return null;

                    var servers = (Dictionary<string, object>)config["servers"];
                    
                    if (!servers.ContainsKey(serverName))
                        return null;

                    var serverConfig = (JsonElement)servers[serverName];
                    var username = serverConfig.GetProperty("username").GetString();
                    var encryptedPassword = serverConfig.GetProperty("password").GetString();
                    
                    var password = DecryptPassword(encryptedPassword!);
                    
                    return new CredentialsModel
                    {
                        Username = username!,
                        Password = password
                    };
                }
                catch
                {
                    return null;
                }
            }
        }

        private Dictionary<string, object> LoadConfig()
        {
            try
            {
                if (!File.Exists(_configFile))
                    return new Dictionary<string, object>();

                var json = File.ReadAllText(_configFile);
                return JsonSerializer.Deserialize<Dictionary<string, object>>(json) ?? new Dictionary<string, object>();
            }
            catch
            {
                return new Dictionary<string, object>();
            }
        }

        private void SaveConfig(Dictionary<string, object> config)
        {
            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_configFile, json);
        }

        // Simple XOR encryption (in production, use DPAPI or stronger encryption)
        private string EncryptPassword(string password)
        {
            const string key = "HyperVMonitor2024";
            var result = new System.Text.StringBuilder();
            
            for (int i = 0; i < password.Length; i++)
            {
                result.Append((char)(password[i] ^ key[i % key.Length]));
            }
            
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(result.ToString()));
        }

        private string DecryptPassword(string encryptedPassword)
        {
            const string key = "HyperVMonitor2024";
            var decoded = Convert.FromBase64String(encryptedPassword);
            var password = System.Text.Encoding.UTF8.GetString(decoded);
            var result = new System.Text.StringBuilder();
            
            for (int i = 0; i < password.Length; i++)
            {
                result.Append((char)(password[i] ^ key[i % key.Length]));
            }
            
            return result.ToString();
        }
    }
}
