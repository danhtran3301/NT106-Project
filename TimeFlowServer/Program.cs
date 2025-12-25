using Microsoft.Extensions.Configuration;
using Serilog;
using TimeFlow.Data; // Chứa DatabaseHelper
using TimeFlow.Data.Repositories;
using TimeFlowServer.Security;
using TimeFlowServer.ServerCore;

namespace TimeFlowServer
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            // 1. Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            // 2. Setup Serilog
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            try
            {
                PrintBanner();
                Log.Information("=== TimeFlow Server Starting ===");

                // 3. Lấy thông tin từ Config
                var connectionString = configuration.GetConnectionString("Default");
                var port = configuration.GetValue<int>("ServerSettings:Port", 1010);
                var jwtKey = configuration.GetValue<string>("ServerSettings:JwtSecretKey");

                // Validate Config
                if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
                {
                    Log.Warning("JWT Key thiếu hoặc quá ngắn. Đang dùng Key mặc định (Chỉ dùng cho Dev).");
                    jwtKey = "Key_Mac_Dinh_Nay_Phai_Dai_Hon_32_Ky_Tu_De_Dam_Bao_An_Toan_123456";
                }

                if (string.IsNullOrEmpty(connectionString))
                {
                    Log.Fatal("Connection string chưa được cấu hình!");
                    return 1;
                }

                Log.Information($"Server Port: {port}");
                Log.Information($"Database: {GetDatabaseInfo(connectionString)}");
                TimeFlow.Data.Configuration.DbConfig.SetConnectionString(connectionString);
                var dbHelper = new DatabaseHelper();

                // B. Khởi tạo tất cả Repositories
                var userRepo = new UserRepository(dbHelper);
                var activityRepo = new ActivityLogRepository(dbHelper);
                var taskRepo = new TaskRepository(dbHelper);
                var categoryRepo = new CategoryRepository(dbHelper);
                var commentRepo = new CommentRepository(dbHelper);

                // Các Repo cho Chat/Group
                var messageRepo = new MessageRepository(dbHelper);
                var groupTaskRepo = new GroupTaskRepository(dbHelper);
                var groupRepo = new GroupRepository(dbHelper);
                var groupMemberRepo = new GroupMemberRepository(dbHelper);
                var contactRepo = new ContactRepository();
                var jwtManager = new JwtManager(jwtKey);

    
                var server = new TcpServerManager(
                    "0.0.0.0", // Lắng nghe mọi IP (quan trọng nếu chạy Docker)
                    port,
                    jwtManager,
                    userRepo,
                    activityRepo,
                    taskRepo,
                    categoryRepo,
                    commentRepo,
                    messageRepo,
                    groupTaskRepo,
                    groupRepo,
                    groupMemberRepo,
                    contactRepo
                );

                // ============================================================

                // Setup graceful shutdown
                var cancellationTokenSource = new CancellationTokenSource();
                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    Log.Information("Shutdown signal received (Ctrl+C)...");
                    server.Stop();
                    cancellationTokenSource.Cancel();
                };

                // Start server
                server.Start(); // Lưu ý: Hàm Start của TcpServerManager cũ là void (chạy Task.Run bên trong)
                                // Nếu bạn đã đổi thành async Task StartAsync() thì thêm await.

                Log.Information("Server is running. Press Ctrl+C to stop.");
                Log.Information("================================================");

                // Giữ ứng dụng chạy
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