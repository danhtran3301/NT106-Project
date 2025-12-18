using Microsoft.Extensions.Configuration;
using Serilog;
using TimeFlowServer.ServerCore;

namespace TimeFlowServer
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            // Setup Serilog
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            try
            {
                PrintBanner();
                Log.Information("=== TimeFlow Server Starting ===");
                Log.Information($"Environment: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}");
                Log.Information($"Machine Name: {Environment.MachineName}");
                Log.Information($"OS: {Environment.OSVersion}");
                Log.Information($".NET Version: {Environment.Version}");
                Log.Information($"Working Directory: {Directory.GetCurrentDirectory()}");

                // Get configuration values
                var connectionString = configuration.GetConnectionString("Default");
                var port = configuration.GetValue<int>("ServerSettings:Port", 1010);
                var maxConnections = configuration.GetValue<int>("ServerSettings:MaxConnections", 100);

                // --- THÊM ĐOẠN NÀY ĐỂ LẤY KEY TỪ APPSETTINGS ---
                var jwtKey = configuration.GetValue<string>("ServerSettings:JwtSecretKey");

                // Kiểm tra an toàn
                if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
                {
                    // Fallback nếu quên config hoặc key quá ngắn
                    Log.Warning("JWT Key trong config bị thiếu hoặc quá ngắn. Đang sử dụng Key mặc định an toàn.");
                    jwtKey = "Key_Mac_Dinh_Nay_Phai_Dai_Hon_32_Ky_Tu_De_Tranh_Bi_Crash_Server_Khi_Khoi_Tao_JWT_123456";
                }
                if (string.IsNullOrEmpty(connectionString))
                {
                    Log.Fatal("Connection string is not configured!");
                    return 1;
                }

                Log.Information($"Server Port: {port}");
                Log.Information($"Max Connections: {maxConnections}");
                Log.Information($"Database: {GetDatabaseInfo(connectionString)}");
                Log.Information($"[DEBUG] KEY HIEN TAI: '{jwtKey}' - DO DAI: {jwtKey?.Length ?? 0}");

                // Initialize and start server
                var server = new TcpServerManager(connectionString, port, jwtKey);

                // Setup graceful shutdown
                var cancellationTokenSource = new CancellationTokenSource();
                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    Log.Information("Shutdown signal received (Ctrl+C)...");
                    server.Stop();
                    cancellationTokenSource.Cancel();
                };

                // Handle SIGTERM for Docker
                AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
                {
                    Log.Information("Process exit signal received...");
                    server.Stop();
                };

                // Start server
                await server.StartAsync();

                // Keep running until cancellation
                Log.Information("Server is running. Press Ctrl+C to stop.");
                Log.Information("================================================");

                try
                {
                    await Task.Delay(Timeout.Infinite, cancellationTokenSource.Token);
                }
                catch (TaskCanceledException)
                {
                    // Normal shutdown
                }

                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Server terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.Information("=== TimeFlow Server Stopped ===");
                await Log.CloseAndFlushAsync();
            }
        }

        static void PrintBanner()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
╔════════════════════════════════════════════╗
║                                            ║
║         TimeFlow Server v1.0.0             ║
║       Task Management & Chat System        ║
║                                            ║
╚════════════════════════════════════════════╝
            ");
            Console.ResetColor();
        }

        static string GetDatabaseInfo(string? connString)
        {
            if (string.IsNullOrEmpty(connString)) return "Not configured";

            try
            {
                var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connString);
                return $"{builder.DataSource}/{builder.InitialCatalog}";
            }
            catch
            {
                return "Invalid connection string";
            }
        }
    }
}
