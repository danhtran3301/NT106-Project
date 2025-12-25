using System.Net;
using System.Net.Sockets;
using TimeFlow.Data.Repositories;
using TimeFlowServer.Security;
using Serilog;

namespace TimeFlowServer.ServerCore
{
    public class TcpServerManager
    {
        private readonly TcpListener _listener;
        private readonly JwtManager _jwtManager;

        // --- Danh sách Repositories ---
        private readonly UserRepository _userRepo;
        private readonly ActivityLogRepository _activityRepo;
        private readonly TaskRepository _taskRepo;
        private readonly GroupRepository _groupRepo;
        private readonly GroupMemberRepository _groupMemberRepo;
        private readonly MessageRepository _messageRepo;
        private readonly GroupTaskRepository _groupTaskRepo;
        private readonly CategoryRepository _categoryRepo;
        private readonly CommentRepository _commentRepo;

        // --- Các Repo mới cho Chat/Group ---
        private readonly MessageRepository _messageRepo;
        private readonly GroupTaskRepository _groupTaskRepo;
        private readonly GroupRepository _groupRepo;
        private readonly GroupMemberRepository _groupMemberRepo;
        private readonly ContactRepository _contactRepo;

        // Quản lý client online
        private readonly Dictionary<string, TcpClient> _onlineClients = new Dictionary<string, TcpClient>();
        private readonly object _clientsLock = new object();

        private bool _isRunning;

        // Constructor cập nhật đầy đủ tham số
        public TcpServerManager(
            string ipAddress,
            int port,
            JwtManager jwtManager,
            UserRepository userRepo,
            ActivityLogRepository activityRepo,
            TaskRepository taskRepo,
            CategoryRepository categoryRepo,
            CommentRepository commentRepo,
            // Thêm tham số mới
            MessageRepository messageRepo,
            GroupTaskRepository groupTaskRepo,
            GroupRepository groupRepo,
            GroupMemberRepository groupMemberRepo,
            ContactRepository contactRepo)
        {
            _jwtManager = jwtManager;
            _userRepo = userRepo;
            _activityRepo = activityRepo;
            _taskRepo = taskRepo;
            _categoryRepo = categoryRepo;
            _commentRepo = commentRepo;

            // Khoi tao repositories voi connection string
            var dbHelper = new TimeFlow.Data.DatabaseHelper(connectionString);
            _userRepo = new UserRepository(dbHelper);
            _activityLogRepo = new ActivityLogRepository(dbHelper);
            _taskRepo = new TaskRepository(dbHelper);
            _categoryRepo = new CategoryRepository(dbHelper);
            _commentRepo = new CommentRepository(dbHelper);

            IPAddress localAddr = IPAddress.Parse(ipAddress);
            _listener = new TcpListener(localAddr, port);
        }

        public void Start()
        {
            if (_isRunning) return;

            _listener.Start();
            _isRunning = true;
            Log.Information("Server started on " + _listener.LocalEndpoint);

            // Chấp nhận kết nối liên tục
            Task.Run(async () =>
            {
                while (_isRunning)
                {
                    try
                    {
                        TcpClient client = await _listener.AcceptTcpClientAsync();
                        Log.Information($"New connection from {client.Client.RemoteEndPoint}");
                        _ = HandleClientAsync(client);
                    }
                    catch (Exception ex)
                    {
                        if (_isRunning) Log.Error(ex, "Error accepting client");
                    }
                }
            });
        }

        public void Stop()
        {
            _isRunning = false;
            _listener.Stop();
            Log.Information("Server stopped.");
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            // Tạo ClientHandler với đầy đủ Repository
            var handler = new ClientHandler(
                client,
                _userRepo,
                _activityRepo,
                _taskRepo,
                _categoryRepo,
                _commentRepo,
                _jwtManager,
                _onlineClients,
                _clientsLock
            );

            using CancellationTokenSource cts = new CancellationTokenSource();
            await handler.HandleAsync(cts.Token);
        }
    }
}