using System.Net;
using System.Net.Sockets;
using Serilog;
using TimeFlowServer.Security;
using TimeFlow.Data.Repositories;

namespace TimeFlowServer.ServerCore
{
    // Quan ly TCP Server chinh - xu ly ket noi client va lifecycle cua server
    public class TcpServerManager
    {
        private readonly string _connectionString;
        private readonly int _port;
        private TcpListener? _listener;
        private bool _isRunning;
        private readonly CancellationTokenSource _cancellationTokenSource;

        // Repositories
        private readonly UserRepository _userRepo;
        private readonly ActivityLogRepository _activityLogRepo;

        // Security
        private readonly JwtManager _jwtManager;

        // Quan ly cac client dang online
        private readonly Dictionary<string, TcpClient> _onlineClients;
        private readonly object _clientsLock = new object();
        private readonly MessageRepository _messageRepo;

        public TcpServerManager(string connectionString, int port, string secretKey)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _port = port;
            _jwtManager = new JwtManager(secretKey);
            _cancellationTokenSource = new CancellationTokenSource();
            _onlineClients = new Dictionary<string, TcpClient>();

            // Khoi tao repositories voi connection string
            var dbHelper = new TimeFlow.Data.DatabaseHelper(connectionString);
            _userRepo = new UserRepository(dbHelper);
            _activityLogRepo = new ActivityLogRepository(dbHelper);

            // Khoi tao JWT manager (lay secret tu config)
            _jwtManager = new JwtManager("your_super_secret_jwt_key_change_in_production_minimum_32_characters_long_for_security");
            _messageRepo = new MessageRepository(dbHelper.ToString());
        }

        // Khoi dong TCP server
        public async Task StartAsync()
        {
            try
            {
                // Kiem tra ket noi database truoc
                Log.Information("Testing database connection...");
                var testDb = new TimeFlow.Data.DatabaseHelper(_connectionString);
                if (testDb.TestConnection())
                {
                    Log.Information("✓ Database connection successful!");
                    Log.Information($"✓ Connection: {GetDatabaseInfo()}");

                    // Kiem tra bang Users
                    var usersCount = testDb.ExecuteScalar("SELECT COUNT(*) FROM Users", null);
                    Log.Information($"✓ Users table accessible with {usersCount} users");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "✗ Database connection failed!");
                throw;
            }

            // Khoi dong TCP listener
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
            _isRunning = true;

            Log.Information($"✓ Server started successfully on port {_port}");
            Log.Information($"✓ Listening on all network interfaces (0.0.0.0:{_port})");
            Log.Information("Waiting for client connections...");

            // Vong lap chap nhan clients
            await AcceptClientsAsync(_cancellationTokenSource.Token);
        }

        // Dung server mot cach graceful
        public void Stop()
        {
            if (!_isRunning) return;

            Log.Information("Stopping server...");
            _isRunning = false;
            _cancellationTokenSource.Cancel();

            // Ngat ket noi tat ca clients
            lock (_clientsLock)
            {
                foreach (var client in _onlineClients.Values)
                {
                    try { client.Close(); } catch { }
                }
                _onlineClients.Clear();
            }

            // Dung listener
            _listener?.Stop();
            Log.Information("Server stopped successfully");
        }

        private async Task AcceptClientsAsync(CancellationToken cancellationToken)
        {
            while (_isRunning && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = await _listener!.AcceptTcpClientAsync(cancellationToken);
                    
                    var clientEndpoint = client.Client.RemoteEndPoint?.ToString() ?? "Unknown";
                    Log.Information($"[NEW CONNECTION] Client connected from {clientEndpoint}");

                    // Xu ly client trong task rieng
                    _ = Task.Run(() => HandleClientAsync(client, cancellationToken), cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    if (_isRunning)
                    {
                        Log.Error(ex, "Error accepting client connection");
                    }
                }
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            var clientEndpoint = client.Client.RemoteEndPoint?.ToString() ?? "Unknown";
            var handler = new ClientHandler(
                client,
                _userRepo,
                _activityLogRepo,
                _messageRepo,
                _jwtManager,
                _onlineClients,
                _clientsLock
            );

            try
            {
                await handler.HandleAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Error handling client {clientEndpoint}");
            }
            finally
            {
                Log.Information($"[DISCONNECTED] Client {clientEndpoint} disconnected");
            }
        }

        public int GetOnlineCount()
        {
            lock (_clientsLock)
            {
                return _onlineClients.Count;
            }
        }

        public bool IsRunning => _isRunning;

        private string GetDatabaseInfo()
        {
            try
            {
                var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(_connectionString);
                return $"{builder.DataSource}/{builder.InitialCatalog}";
            }
            catch
            {
                return "Connection string parsing failed";
            }
        }
    }
}
