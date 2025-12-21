using System.Net;
using System.Net.Sockets;
using Serilog;
using TimeFlowServer.Security;
using TimeFlow.Data.Repositories;
using TimeFlow.Data;

namespace TimeFlowServer.ServerCore
{
    public class TcpServerManager
    {
        private readonly string _connectionString;
        private readonly int _port;
        private TcpListener? _listener;
        private bool _isRunning;
        private readonly CancellationTokenSource _cancellationTokenSource;

        private readonly UserRepository _userRepo;
        private readonly ActivityLogRepository _activityLogRepo;
        private readonly MessageRepository _messageRepo;
        private readonly GroupRepository _groupRepo;         // MỚI BỔ SUNG
        private readonly GroupMemberRepository _groupMemberRepo; // MỚI BỔ SUNG

        private readonly JwtManager _jwtManager;

        private readonly Dictionary<string, TcpClient> _onlineClients;
        private readonly object _clientsLock = new object();

        public TcpServerManager(string connectionString, int port, string secretKey)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _port = port;
            _jwtManager = new JwtManager(secretKey);
            _cancellationTokenSource = new CancellationTokenSource();
            _onlineClients = new Dictionary<string, TcpClient>();

            var dbHelper = new DatabaseHelper(connectionString);

            _userRepo = new UserRepository(dbHelper);
            _activityLogRepo = new ActivityLogRepository(dbHelper);
            _messageRepo = new MessageRepository(connectionString);
            _groupRepo = new GroupRepository(dbHelper);            
            _groupMemberRepo = new GroupMemberRepository(dbHelper);
        }

        public async Task StartAsync()
        {
            try
            {
                Log.Information("Testing database connection...");
                var testDb = new DatabaseHelper(_connectionString);
                if (testDb.TestConnection())
                {
                    Log.Information("✓ Database connection successful!");
                    var usersCount = testDb.ExecuteScalar("SELECT COUNT(*) FROM Users", null);
                    Log.Information($"✓ Users table accessible with {usersCount} users");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "✗ Database connection failed!");
                throw;
            }

            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
            _isRunning = true;

            Log.Information($"✓ Server started on port {_port}");
            await AcceptClientsAsync(_cancellationTokenSource.Token);
        }

        public void Stop()
        {
            if (!_isRunning) return;
            _isRunning = false;
            _cancellationTokenSource.Cancel();
            lock (_clientsLock)
            {
                foreach (var client in _onlineClients.Values)
                {
                    try { client.Close(); } catch { }
                }
                _onlineClients.Clear();
            }
            _listener?.Stop();
        }

        private async Task AcceptClientsAsync(CancellationToken cancellationToken)
        {
            while (_isRunning && !cancellationToken.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = await _listener!.AcceptTcpClientAsync(cancellationToken);
                    _ = Task.Run(() => HandleClientAsync(client, cancellationToken), cancellationToken);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex) { if (_isRunning) Log.Error(ex, "Error accepting client"); }
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
                _groupMemberRepo, 
                _groupRepo,      
                _jwtManager,     
                _onlineClients,  
                _clientsLock     
            );

            try { await handler.HandleAsync(cancellationToken); }
            catch (Exception ex) { Log.Error(ex, $"Error handling client {clientEndpoint}"); }
            finally { Log.Information($"[DISCONNECTED] Client {clientEndpoint}"); }
        }

        public int GetOnlineCount() { lock (_clientsLock) return _onlineClients.Count; }
        public bool IsRunning => _isRunning;
    }
}