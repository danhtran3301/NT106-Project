using System;
using System.IO;
using System.Text.Json;

namespace TimeFlow.Data.Configuration
{
    // Quan ly cau hinh database connection
    public static class DbConfig
    {
        // Connection string mac dinh cho development (SQL Server Authentication)
        private const string DefaultConnectionString = 
            "Server=localhost;Database=TimeFlowDB;User Id=myuser;Password=YourStrong@Passw0rd;TrustServerCertificate=True;Integrated Security=False;";
        private static string _connectionString;
        // Timeout mac dinh (giay)
        private const int DefaultCommandTimeout = 30;

        // Lay connection string tu config file hoac mac dinh
        public static string GetConnectionString()
        {
            try
            {
                // Thu doc tu file config.json neu co
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
                
                if (File.Exists(configPath))
                {
                    var json = File.ReadAllText(configPath);
                    var config = JsonSerializer.Deserialize<ConfigModel>(json);
                    
                    if (!string.IsNullOrWhiteSpace(config?.ConnectionString))
                        return config.ConnectionString;
                }
            }
            catch
            {
                // Neu doc file loi thi dung mac dinh
            }

            return DefaultConnectionString;
        }
        public static void SetConnectionString(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Lay command timeout
        public static int GetCommandTimeout()
        {
            try
            {
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
                
                if (File.Exists(configPath))
                {
                    var json = File.ReadAllText(configPath);
                    var config = JsonSerializer.Deserialize<ConfigModel>(json);
                    
                    if (config?.CommandTimeout > 0)
                        return config.CommandTimeout;
                }
            }
            catch
            {
                // Neu doc file loi thi dung mac dinh
            }

            return DefaultCommandTimeout;
        }

        // Tao file config mau
        public static void CreateSampleConfig(string filePath)
        {
            var sampleConfig = new ConfigModel
            {
                ConnectionString = DefaultConnectionString,
                CommandTimeout = DefaultCommandTimeout,
                EnableLogging = false
            };

            var json = JsonSerializer.Serialize(sampleConfig, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });

            File.WriteAllText(filePath, json);
        }

        // Cac preset connection strings cho cac moi truong khac nhau
        public static class Presets
        {
            // Development - localhost voi Windows Authentication
            public static string LocalDevelopment =>
                "Data Source=localhost;Initial Catalog=TimeFlowDB;Integrated Security=True;TrustServerCertificate=True";

            // Development - localhost voi SQL Authentication (RECOMMENDED FOR THIS PROJECT)
            public static string LocalDevelopmentSqlAuth =>
                "Server=localhost;Database=TimeFlowDB;User Id=myuser;Password=YourStrong@Passw0rd;TrustServerCertificate=True;Integrated Security=False;";

            // Development - localhost voi SQL Authentication custom user (method with parameters)
            public static string CustomSqlAuth(string username, string password) =>
                $"Server=localhost;Database=TimeFlowDB;User Id={username};Password={password};TrustServerCertificate=True;Integrated Security=False;";

            // Production - remote server
            public static string Production(string server, string database, string username, string password) =>
                $"Data Source={server};Initial Catalog={database};User ID={username};Password={password};TrustServerCertificate=True;Encrypt=True;Integrated Security=False;";

            // Azure SQL Database
            public static string AzureSQL(string serverName, string database, string username, string password) =>
                $"Server=tcp:{serverName}.database.windows.net,1433;Initial Catalog={database};User ID={username};Password={password};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        }

        // Model cho config file
        private class ConfigModel
        {
            public string ConnectionString { get; set; } = string.Empty;
            public int CommandTimeout { get; set; }
            public bool EnableLogging { get; set; }
        }
    }
}
