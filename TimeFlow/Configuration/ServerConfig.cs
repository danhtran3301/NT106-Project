using System;
using System.IO;
using System.Text.Json;

namespace TimeFlow.Configuration
{
 
    public static class ServerConfig
    {
        private const string DefaultHost = "127.0.0.1";
        private const int DefaultPort = 1010;
        private const int DefaultTimeout = 5000;

        
        public static string Host { get; private set; } = DefaultHost;
        public static int Port { get; private set; } = DefaultPort;
        public static int Timeout { get; private set; } = DefaultTimeout;
        private static bool _isLoaded = false;

        public static void Load()
        {
            if (_isLoaded) return;

            try
            {
               
                var configPath = FindConfigFile();

                if (!string.IsNullOrEmpty(configPath) && File.Exists(configPath))
                {
                    var json = File.ReadAllText(configPath);
                    var config = JsonSerializer.Deserialize<AppSettings>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (config?.ServerSettings != null)
                    {
                        if (!string.IsNullOrWhiteSpace(config.ServerSettings.Host))
                            Host = config.ServerSettings.Host;

                        if (config.ServerSettings.Port > 0 && config.ServerSettings.Port <= 65535)
                            Port = config.ServerSettings.Port;

                        if (config.ServerSettings.Timeout > 0)
                            Timeout = config.ServerSettings.Timeout;
                    }
                }
            }
            catch (Exception ex)
            {
                
                System.Diagnostics.Debug.WriteLine($"[ServerConfig] Failed to load config: {ex.Message}");
            }

            _isLoaded = true;
        }
        private static string? FindConfigFile()
        {
            
            var possiblePaths = new[]
            {
                // thu muc hien tai
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json"),
                //thu muc goc
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "appsettings.json"),
                // thu muc lam viec
                Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json")
            };

            foreach (var path in possiblePaths)
            {
                var fullPath = Path.GetFullPath(path);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }

            return null;
        }
        public static void SetConfig(string host, int port)
        {
            if (!string.IsNullOrWhiteSpace(host))
                Host = host;

            if (port > 0 && port <= 65535)
                Port = port;
        }
        public static void Save()
        {
            try
            {
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
                
                var config = new AppSettings
                {
                    ServerSettings = new ServerSettingsModel
                    {
                        Host = Host,
                        Port = Port,
                        Timeout = Timeout
                    }
                };

                var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(configPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ServerConfig] Failed to save config: {ex.Message}");
                throw;
            }
        }
        public static void Reset()
        {
            Host = DefaultHost;
            Port = DefaultPort;
            Timeout = DefaultTimeout;
        }
        // Model classes cho JSON deserialization
        private class AppSettings
        {
            public ServerSettingsModel? ServerSettings { get; set; }
        }

        private class ServerSettingsModel
        {
            public string Host { get; set; } = DefaultHost;
            public int Port { get; set; } = DefaultPort;
            public int Timeout { get; set; } = DefaultTimeout;
        }
    }
}
