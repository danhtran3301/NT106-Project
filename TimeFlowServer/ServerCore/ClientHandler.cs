using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Serilog;
using TimeFlow.Data.Repositories;
using TimeFlow.Models;
using TimeFlowServer.Security;

namespace TimeFlowServer.ServerCore
{
    // Xu ly tung ket noi client va xu ly cac message
    public class ClientHandler
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;
        private readonly UserRepository _userRepo;
        private readonly ActivityLogRepository _activityLogRepo;
        private readonly TaskRepository _taskRepo;
        private readonly CategoryRepository _categoryRepo;
        private readonly CommentRepository _commentRepo;
        private readonly GroupRepository _groupRepo;
        private readonly GroupMemberRepository _groupMemberRepo;
        private readonly MessageRepository _messageRepo;
        private readonly JwtManager _jwtManager;
        private readonly Dictionary<string, TcpClient> _onlineClients;
        private readonly object _clientsLock;
        
        private string? _currentUsername;
        private int? _currentUserId;
        private readonly string _clientId;

        public ClientHandler(
            TcpClient client,
            UserRepository userRepo,
            ActivityLogRepository activityLogRepo,
            TaskRepository taskRepo,
            CategoryRepository categoryRepo,
            CommentRepository commentRepo,
            GroupRepository groupRepo,             
            GroupMemberRepository groupMemberRepo, 
            MessageRepository messageRepo,
            JwtManager jwtManager,
            Dictionary<string, TcpClient> onlineClients,
            object clientsLock)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _stream = client.GetStream();
            _userRepo = userRepo;
            _activityLogRepo = activityLogRepo;
            _taskRepo = taskRepo;
            _categoryRepo = categoryRepo;
            _commentRepo = commentRepo;
            _groupRepo = groupRepo;
            _groupMemberRepo = groupMemberRepo;
            _messageRepo = messageRepo;
            _jwtManager = jwtManager;
            _onlineClients = onlineClients;
            _clientsLock = clientsLock;
            _clientId = client.Client.RemoteEndPoint?.ToString() ?? Guid.NewGuid().ToString();
        }

        public async Task HandleAsync(CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[4096];

            try
            {
                while (_client.Connected && !cancellationToken.IsCancellationRequested)
                {
                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    
                    if (bytesRead == 0)
                    {
                        Log.Information($"[{_clientId}] Client disconnected (zero bytes)");
                        break;
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    await ProcessMessageAsync(message);
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, $"[{_clientId}] Connection error");
            }
            finally
            {
                Cleanup();
            }
        }

        private async Task ProcessMessageAsync(string json)
        {
            try
            {
                using JsonDocument doc = JsonDocument.Parse(json);
                JsonElement root = doc.RootElement;

                if (!root.TryGetProperty("type", out JsonElement typeElem))
                {
                    Log.Warning($"[{_clientId}] Message without 'type' field");
                    return;
                }

                string type = typeElem.GetString() ?? "";

                switch (type)
                {
                    case "login":
                        await HandleLoginAsync(root);
                        break;

                    case "register":
                        await HandleRegisterAsync(root);
                        break;

                    case "autologin":
                        await HandleAutoLoginAsync(root);
                        break;

                    case "chat":
                        await HandleChatAsync(root);
                        break;

                    // Task operations
                    case "get_tasks":
                        await HandleGetTasksAsync(root);
                        break;

                    case "get_task_detail":
                        await HandleGetTaskDetailAsync(root);
                        break;

                    case "get_task_detail_full":
                        await HandleGetTaskDetailFullAsync(root);
                        break;

                    case "create_task":
                        await HandleCreateTaskAsync(root);
                        break;

                    case "update_task":
                        await HandleUpdateTaskAsync(root);
                        break;

                    case "delete_task":
                        await HandleDeleteTaskAsync(root);
                        break;

                    case "update_task_status":
                        await HandleUpdateTaskStatusAsync(root);
                        break;

                    case "get_categories":
                        await HandleGetCategoriesAsync(root);
                        break;

                    case "get_my_groups":
                        await HandleGetMyGroupsAsync(root);
                        break;

                    case "create_group":
                        await HandleCreateGroupAsync(root);
                        break;

                    default:
                        Log.Warning($"[{_clientId}] Unknown message type: {type}");
                        break;
                }
            }
            catch (JsonException ex)
            {
                Log.Warning(ex, $"[{_clientId}] Invalid JSON received");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[{_clientId}] Error processing message");
            }
        }


        private async Task HandleCreateGroupAsync(JsonElement root)
        {
            try
            {
                // 1. Kiểm tra đăng nhập
                if (!_currentUserId.HasValue)
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = "Not logged in" }));
                    return;
                }

                var data = root.GetProperty("data");
                string groupName = data.GetProperty("groupName").GetString();
                string description = data.TryGetProperty("description", out var desc) ? desc.GetString() : "";

                // 2. Tạo Group Model
                var newGroup = new Group
                {
                    GroupName = groupName,
                    Description = description,
                    CreatedBy = _currentUserId.Value,
                    IsActive = true
                };

                // 3. Lưu vào DB (Hàm Create trong GroupRepo đã xử lý việc thêm Group và add Admin)
                int groupId = _groupRepo.Create(newGroup);

                if (groupId > 0)
                {
                    // Tự động add người tạo vào làm Admin (nếu Repo chưa làm, ta làm thủ công ở đây cho chắc)
                    // _groupMemberRepo.AddMember(groupId, _currentUserId.Value, GroupRole.Admin);

                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "success", groupId = groupId, groupName = groupName }));
                    Log.Information($"[{_clientId}] Created group '{groupName}' (ID: {groupId})");
                }
                else
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = "Failed to create group" }));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error creating group");
                await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = ex.Message }));
            }
        }

        private async Task HandleGetMyGroupsAsync(JsonElement root)
        {
            try
            {
                if (!_currentUserId.HasValue)
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = "Not logged in" }));
                    return;
                }

                Log.Information($"[{_clientId}] Getting groups for UserID: {_currentUserId}");

                // Gọi Repo lấy danh sách nhóm
                var groups = _groupRepo.GetByUserId(_currentUserId.Value);

                // Tạo response JSON
                var response = new
                {
                    type = "my_groups_list",
                    status = "success",
                    data = groups.Select(g => new
                    {
                        groupId = g.GroupId,
                        groupName = g.GroupName,
                        description = g.Description
                    })
                };

                await SendResponseAsync(JsonSerializer.Serialize(response));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting groups");
            }
        }
        private async Task HandleLoginAsync(JsonElement root)
        {
            try
            {
                var data = root.GetProperty("data");
                string username = data.GetProperty("username").GetString() ?? "";
                string password = data.GetProperty("password").GetString() ?? "";

                Log.Information($"[{_clientId}] Login attempt for user: {username}");

                string hashedPassword = PasswordHasher.HashPassword(password);
                var user = _userRepo.ValidateLogin(username, hashedPassword);

                if (user != null && user.IsActive)
                {
                    _currentUsername = user.Username;
                    _currentUserId = user.UserId;

                    // Cap nhat last login
                    _userRepo.UpdateLastLogin(user.UserId);

                    // Them vao danh sach online clients
                    lock (_clientsLock)
                    {
                        if (_onlineClients.ContainsKey(_currentUsername))
                        {
                            Log.Information($"[{_clientId}] User {_currentUsername} already online, replacing connection");
                            try { _onlineClients[_currentUsername].Close(); } catch { }
                        }
                        _onlineClients[_currentUsername] = _client;
                    }

                    // Tao JWT token
                    string token = _jwtManager.CreateToken(_currentUsername);

                    // Gui response thanh cong
                    var response = new
                    {
                        status = "success",
                        token = token,
                        user = new
                        {
                            userId = user.UserId,
                            username = user.Username,
                            email = user.Email,
                            fullName = user.FullName
                        }
                    };

                    await SendResponseAsync(JsonSerializer.Serialize(response));

                    Log.Information($"[{_clientId}] ✓ User '{_currentUsername}' logged in successfully");

                    // Log activity
                    _activityLogRepo.LogActivity(user.UserId, null, "Login", "User logged in via TCP");
                }
                else
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "fail" }));
                    Log.Warning($"[{_clientId}] ✗ Login failed for user: {username}");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[{_clientId}] Login error");
                await SendResponseAsync(JsonSerializer.Serialize(new { status = "error" }));
            }
        }

        private async Task HandleRegisterAsync(JsonElement root)
        {
            try
            {
                var data = root.GetProperty("data");
                string username = data.GetProperty("username").GetString() ?? "";
                string password = data.GetProperty("password").GetString() ?? "";
                string email = data.GetProperty("email").GetString() ?? "";

                Log.Information($"[{_clientId}] Register attempt for user: {username}");

                // Kiem tra username hoac email da ton tai
                if (_userRepo.UsernameOrEmailExists(username, email))
                {
                    await SendResponseAsync("exists");
                    Log.Warning($"[{_clientId}] ✗ Registration failed: Username or email already exists");
                    return;
                }

                // Tao user moi
                var newUser = new User
                {
                    Username = username,
                    Email = email,
                    PasswordHash = PasswordHasher.HashPassword(password),
                    IsActive = true
                };

                int userId = _userRepo.Create(newUser);

                if (userId > 0)
                {
                    await SendResponseAsync("registered");
                    Log.Information($"[{_clientId}] ✓ User '{username}' registered successfully (ID: {userId})");

                    // Log activity
                    _activityLogRepo.LogActivity(userId, null, "Register", "New user registered via TCP");
                }
                else
                {
                    await SendResponseAsync("error");
                    Log.Error($"[{_clientId}] ✗ Registration failed for user: {username}");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[{_clientId}] Register error");
                await SendResponseAsync("error");
            }
        }

        private async Task HandleAutoLoginAsync(JsonElement root)
        {
            try
            {
                string token = root.GetProperty("token").GetString() ?? "";

                if (_jwtManager.ValidateToken(token, out string? username) && !string.IsNullOrEmpty(username))
                {
                    var user = _userRepo.GetByUsername(username);

                    if (user != null && user.IsActive)
                    {
                        _currentUsername = username;

                        // Cap nhat last login
                        _userRepo.UpdateLastLogin(user.UserId);

                        // Them vao danh sach online clients
                        lock (_clientsLock)
                        {
                            if (_onlineClients.ContainsKey(_currentUsername))
                            {
                                try { _onlineClients[_currentUsername].Close(); } catch { }
                            }
                            _onlineClients[_currentUsername] = _client;
                        }

                        var response = new
                        {
                            status = "autologin_success",
                            user = new
                            {
                                userId = user.UserId,
                                username = user.Username,
                                email = user.Email,
                                fullName = user.FullName
                            }
                        };

                        await SendResponseAsync(JsonSerializer.Serialize(response));

                        Log.Information($"[{_clientId}] ✓ User '{username}' auto-logged in successfully");

                        // Log activity
                        _activityLogRepo.LogActivity(user.UserId, null, "AutoLogin", "User auto-logged in via token");
                    }
                    else
                    {
                        await SendResponseAsync(JsonSerializer.Serialize(new { status = "autologin_fail" }));
                        Log.Warning($"[{_clientId}] ✗ Auto-login failed: User not found or inactive");
                    }
                }
                else
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "autologin_fail" }));
                    Log.Warning($"[{_clientId}] ✗ Auto-login failed: Invalid token");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[{_clientId}] Auto-login error");
                await SendResponseAsync(JsonSerializer.Serialize(new { status = "autologin_fail" }));
            }
        }

        private async Task HandleChatAsync(JsonElement root)
        {
            if (string.IsNullOrEmpty(_currentUsername)) return;

            try
            {
                string content = root.GetProperty("content").GetString() ?? "";
                string receiver = root.GetProperty("receiver").GetString() ?? "";

                // Kiểm tra xem có phải chat nhóm không (Client gửi cờ isGroup = true)
                bool isGroup = false;
                if (root.TryGetProperty("isGroup", out JsonElement groupElem))
                {
                    isGroup = groupElem.GetBoolean();
                }

                if (isGroup)
                {
                    // --- XỬ LÝ CHAT NHÓM ---
                    int groupId = int.Parse(receiver); // Receiver lúc này là GroupId

                    // 1. Lưu vào Database
                    _messageRepo.AddGroupMessage(_currentUsername, groupId, content);

                    // 2. Lấy danh sách thành viên trong nhóm (Dùng GroupMemberRepo đã có)
                    var members = _groupMemberRepo.GetByGroupId(groupId);

                    // 3. Gửi cho từng thành viên đang online
                    foreach (var member in members)
                    {
                        // Lấy username của member (Cần join bảng User hoặc lấy từ cache nếu có)
                        // Giả sử GroupMember có property User hoặc ta query thêm
                        // Ở đây ta cần Username để tra trong _onlineClients
                        var user = _userRepo.GetById(member.UserId);
                        if (user == null) continue;

                        string memberUsername = user.Username;

                        // Không gửi lại cho chính mình (tuỳ chọn)
                        if (memberUsername == _currentUsername) continue;

                        TcpClient? clientToSend = null;
                        lock (_clientsLock)
                        {
                            if (_onlineClients.TryGetValue(memberUsername, out var c))
                            {
                                clientToSend = c;
                            }
                        }

                        if (clientToSend != null && clientToSend.Connected)
                        {
                            try
                            {
                                var packet = new
                                {
                                    type = "receive_group_message",
                                    groupId = groupId,
                                    sender = _currentUsername,
                                    content = content,
                                    timestamp = DateTime.Now.ToString("HH:mm")
                                };
                                string json = JsonSerializer.Serialize(packet);
                                byte[] bytes = Encoding.UTF8.GetBytes(json);
                                await clientToSend.GetStream().WriteAsync(bytes, 0, bytes.Length);
                            }
                            catch { /* Log error */ }
                        }
                    }
                    Log.Information($"[GROUP CHAT] {_currentUsername} sent to Group {groupId}");
                }
                else
                {
                    // --- XỬ LÝ CHAT 1-1 (CŨ) ---

                    // 1. Lưu vào DB
                    _messageRepo.AddMessage(_currentUsername, receiver, content);

                    // 2. Gửi cho người nhận
                    TcpClient? receiverClient = null;
                    lock (_clientsLock)
                    {
                        if (_onlineClients.TryGetValue(receiver, out var c)) receiverClient = c;
                    }

                    if (receiverClient != null)
                    {
                        var packet = new
                        {
                            type = "receive_message",
                            sender = _currentUsername,
                            content = content,
                            timestamp = DateTime.Now.ToString("HH:mm")
                        };
                        // ... Gửi packet ...
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Chat error");
            }
        }

        // ================== TASK HANDLERS ==================

        private async Task HandleGetTasksAsync(JsonElement root)
        {
            try
            {
                // Authenticate từ token
                string token = root.TryGetProperty("token", out var tokenElem) ? tokenElem.GetString() : null;
                
                if (string.IsNullOrEmpty(token) || !_jwtManager.ValidateToken(token, out string username))
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = "Not authenticated" }));
                    return;
                }

                // Lấy user info từ username
                var user = _userRepo.GetByUsername(username);
                if (user == null)
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = "User not found" }));
                    return;
                }

                int userId = user.UserId;

                Log.Information($"[{_clientId}] Get tasks for user: {userId} ({username})");

                var tasks = _taskRepo.GetByUserId(userId);

                var response = new
                {
                    status = "success",
                    data = tasks.Select(t => new
                    {
                        taskId = t.TaskId,
                        title = t.Title,
                        description = t.Description,
                        dueDate = t.DueDate?.ToString("yyyy-MM-dd HH:mm:ss"),
                        priority = (int)t.Priority,
                        status = (int)t.Status,
                        categoryId = t.CategoryId,
                        isGroupTask = t.IsGroupTask,
                        createdAt = t.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                        updatedAt = t.UpdatedAt?.ToString("yyyy-MM-dd HH:mm:ss")
                    })
                };

                await SendResponseAsync(JsonSerializer.Serialize(response));
                Log.Information($"[{_clientId}] ✓ Returned {tasks.Count} tasks for {username}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[{_clientId}] Error getting tasks");
                await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = ex.Message }));
            }
        }

        private async Task HandleGetTaskDetailAsync(JsonElement root)
        {
            try
            {
                // Authenticate từ token
                string token = root.TryGetProperty("token", out var tokenElem) ? tokenElem.GetString() : null;
                
                if (string.IsNullOrEmpty(token) || !_jwtManager.ValidateToken(token, out string username))
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = "Not authenticated" }));
                    return;
                }

                var user = _userRepo.GetByUsername(username);
                if (user == null)
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = "User not found" }));
                    return;
                }

                int taskId = root.GetProperty("taskId").GetInt32();
                Log.Information($"[{_clientId}] Get task detail: {taskId}");

                var task = _taskRepo.GetById(taskId);

                if (task == null)
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = "Task not found" }));
                    return;
                }

                // Check ownership
                if (task.CreatedBy != user.UserId && !task.IsGroupTask)
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = "Access denied" }));
                    return;
                }

                var response = new
                {
                    status = "success",
                    data = new
                    {
                        taskId = task.TaskId,
                        title = task.Title,
                        description = task.Description,
                        dueDate = task.DueDate?.ToString("yyyy-MM-dd HH:mm:ss"),
                        priority = (int)task.Priority,
                        status = (int)task.Status,
                        categoryId = task.CategoryId,
                        isGroupTask = task.IsGroupTask,
                        createdBy = task.CreatedBy,
                        completedAt = task.CompletedAt?.ToString("yyyy-MM-dd HH:mm:ss"),
                        createdAt = task.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                        updatedAt = task.UpdatedAt?.ToString("yyyy-MM-dd HH:mm:ss")
                    }
                };

                await SendResponseAsync(JsonSerializer.Serialize(response));
                Log.Information($"[{_clientId}] ✓ Returned task detail for TaskId={taskId}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[{_clientId}] Error getting task detail");
                await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = ex.Message }));
            }
        }

        private async Task HandleCreateTaskAsync(JsonElement root)
        {
            try
            {
                // Authenticate từ token
                string token = root.TryGetProperty("token", out var tokenElem) ? tokenElem.GetString() : null;
                
                if (string.IsNullOrEmpty(token) || !_jwtManager.ValidateToken(token, out string username))
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = "Not authenticated" }));
                    return;
                }

                var user = _userRepo.GetByUsername(username);
                if (user == null)
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = "User not found" }));
                    return;
                }

                var data = root.GetProperty("data");
                
                // ✅ FIX: Properly handle nullable categoryId
                int? categoryId = null;
                if (data.TryGetProperty("categoryId", out var catIdElem) && 
                    catIdElem.ValueKind != JsonValueKind.Null && 
                    catIdElem.ValueKind == JsonValueKind.Number)
                {
                    categoryId = catIdElem.GetInt32();
                }
                
                var newTask = new TaskItem
                {
                    Title = data.GetProperty("title").GetString() ?? "",
                    Description = data.TryGetProperty("description", out var desc) ? desc.GetString() : null,
                    DueDate = data.TryGetProperty("dueDate", out var dueDate) && !string.IsNullOrEmpty(dueDate.GetString()) 
                        ? DateTime.Parse(dueDate.GetString()!) : null,
                    Priority = (TaskPriority)data.GetProperty("priority").GetInt32(),
                    Status = data.TryGetProperty("status", out var statusProp) ? (TimeFlow.Models.TaskStatus)statusProp.GetInt32() : TimeFlow.Models.TaskStatus.Pending,
                    CategoryId = categoryId, // ✅ Use safely parsed categoryId
                    CreatedBy = user.UserId, // ✅ Always use authenticated user's ID
                    IsGroupTask = data.TryGetProperty("isGroupTask", out var isGroup) && isGroup.GetBoolean()
                };

                Log.Information($"[{_clientId}] Creating task: {newTask.Title} for user {username} (CategoryId: {categoryId})");

                // ✅ Create task (validation + activity logging done inside transaction)
                int taskId = _taskRepo.Create(newTask);

                if (taskId > 0)
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "success", taskId = taskId }));
                    Log.Information($"[{_clientId}] ✓ Task created with ID={taskId}");
                }
                else
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = "Failed to create task" }));
                    Log.Warning($"[{_clientId}] ✗ Task creation returned 0");
                }
            }
            catch (TimeFlow.Data.Exceptions.ValidationException ex)
            {
                // ✅ Handle validation errors with detailed info
                await SendResponseAsync(JsonSerializer.Serialize(new { 
                    status = "validation_error",
                    field = ex.Field,
                    message = ex.Message 
                }));
                Log.Warning($"[{_clientId}] Validation error creating task: Field={ex.Field}, Message={ex.Message}");
            }
            catch (TimeFlow.Data.Exceptions.UnauthorizedException ex)
            {
                // ✅ Handle authorization errors
                await SendResponseAsync(JsonSerializer.Serialize(new { 
                    status = "unauthorized",
                    message = ex.Message 
                }));
                Log.Warning($"[{_clientId}] Unauthorized task creation: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Generic errors
                Log.Error(ex, $"[{_clientId}] Error creating task");
                await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = ex.Message }));
            }
        }

        private async Task HandleUpdateTaskAsync(JsonElement root)
        {
            try
            {
                var data = root.GetProperty("data");
                int taskId = data.GetProperty("taskId").GetInt32();

                string token = root.GetProperty("token").GetString();
                if (!_jwtManager.ValidateToken(token, out string username))
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "unauthorized", message = "Session expired" }));
                    return;
                }

                var user = _userRepo.GetByUsername(username);
                if (user == null) return;

                var existingTask = _taskRepo.GetById(taskId);
                if (existingTask == null || existingTask.CreatedBy != user.UserId)
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = "No permission or task not found" }));
                    return;
                }

                existingTask.Title = data.GetProperty("title").GetString();
                existingTask.Description = data.TryGetProperty("description", out var desc) ? desc.GetString() : null;
                if (data.TryGetProperty("dueDate", out var due) && !string.IsNullOrEmpty(due.GetString()))
                    existingTask.DueDate = DateTime.Parse(due.GetString());

                existingTask.Priority = (TaskPriority)data.GetProperty("priority").GetInt32();
                existingTask.Status = (TimeFlow.Models.TaskStatus)data.GetProperty("status").GetInt32();
                existingTask.CategoryId = data.TryGetProperty("categoryId", out var cat) ? cat.GetInt32() : null;
                existingTask.UpdatedAt = DateTime.Now;

                bool success = _taskRepo.Update(existingTask, user.UserId);

                if (success)
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "success" }));
                }
                else
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = "Database update failed" }));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in HandleUpdateTaskAsync");
                await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = ex.Message }));
            }
        }

        private async Task HandleDeleteTaskAsync(JsonElement root)
        {
            try
            {
                // Authenticate từ token
                string token = root.TryGetProperty("token", out var tokenElem) ? tokenElem.GetString() : null;
                
                if (string.IsNullOrEmpty(token) || !_jwtManager.ValidateToken(token, out string username))
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = "Not authenticated" }));
                    return;
                }

                var user = _userRepo.GetByUsername(username);
                if (user == null)
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = "User not found" }));
                    return;
                }

                int taskId = root.GetProperty("taskId").GetInt32();
                Log.Information($"[{_clientId}] Deleting task: {taskId}");

                // ✅ Use DeleteWithCascade (handles authorization, cascade delete, activity logging)
                bool success = _taskRepo.DeleteWithCascade(taskId, user.UserId);

                if (success)
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "success" }));
                    Log.Information($"[{_clientId}] ✓ Task deleted with cascade: {taskId}");
                    
                    // ✅ REMOVED: Activity log (already logged in DeleteWithCascade)
                }
                else
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = "Failed to delete task" }));
                    Log.Warning($"[{_clientId}] ✗ Task deletion failed: {taskId}");
                }
            }
            catch (TimeFlow.Data.Exceptions.UnauthorizedException ex)
            {
                await SendResponseAsync(JsonSerializer.Serialize(new { 
                    status = "unauthorized",
                    message = ex.Message 
                }));
                Log.Warning($"[{_clientId}] Unauthorized task deletion: {ex.Message}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[{_clientId}] Error deleting task");
                await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = ex.Message }));
            }
        }

        private async Task HandleUpdateTaskStatusAsync(JsonElement root)
        {
            try
            {
                // Authenticate từ token
                string token = root.TryGetProperty("token", out var tokenElem) ? tokenElem.GetString() : null;
                
                if (string.IsNullOrEmpty(token) || !_jwtManager.ValidateToken(token, out string username))
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = "Not authenticated" }));
                    return;
                }

                var user = _userRepo.GetByUsername(username);
                if (user == null)
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = "User not found" }));
                    return;
                }

                int taskId = root.GetProperty("taskId").GetInt32();
                int statusValue = root.GetProperty("status").GetInt32();
                var newStatus = (TimeFlow.Models.TaskStatus)statusValue;

                Log.Information($"[{_clientId}] Updating task status: TaskId={taskId}, Status={newStatus}");

                // ✅ Use UpdateStatus with authorization (validation + activity logging done inside)
                bool success = _taskRepo.UpdateStatus(taskId, newStatus, user.UserId);

                if (success)
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "success" }));
                    Log.Information($"[{_clientId}] ✓ Task status updated: {taskId} -> {newStatus}");
                }
                else
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = "Failed to update status" }));
                }
            }
            catch (TimeFlow.Data.Exceptions.ValidationException ex)
            {
                await SendResponseAsync(JsonSerializer.Serialize(new { 
                    status = "validation_error",
                    field = ex.Field,
                    message = ex.Message 
                }));
                Log.Warning($"[{_clientId}] Validation error updating status: {ex.Message}");
            }
            catch (TimeFlow.Data.Exceptions.UnauthorizedException ex)
            {
                await SendResponseAsync(JsonSerializer.Serialize(new { 
                    status = "unauthorized",
                    message = ex.Message 
                }));
                Log.Warning($"[{_clientId}] Unauthorized status update: {ex.Message}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[{_clientId}] Error updating task status");
                await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = ex.Message }));
            }
        }

        private async Task HandleGetCategoriesAsync(JsonElement root)
        {
            try
            {
                Log.Information($"[{_clientId}] Get categories");

                var categories = _categoryRepo.GetAll();

                var response = new
                {
                    status = "success",
                    data = categories.Select(c => new
                    {
                        categoryId = c.CategoryId,
                        categoryName = c.CategoryName,
                        color = c.Color,
                        iconName = c.IconName,
                        isDefault = c.IsDefault
                    })
                };

                await SendResponseAsync(JsonSerializer.Serialize(response));
                Log.Information($"[{_clientId}] ✓ Returned {categories.Count} categories");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[{_clientId}] Error getting categories");
                await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = ex.Message }));
            }
        }

        private async Task HandleGetTaskDetailFullAsync(JsonElement root)
        {
            try
            {
                // Authenticate từ token
                string token = root.TryGetProperty("token", out var tokenElem) ? tokenElem.GetString() : null;
                
                if (string.IsNullOrEmpty(token) || !_jwtManager.ValidateToken(token, out string username))
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = "Not authenticated" }));
                    return;
                }

                var user = _userRepo.GetByUsername(username);
                if (user == null)
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = "User not found" }));
                    return;
                }

                int taskId = root.GetProperty("taskId").GetInt32();
                Log.Information($"[{_clientId}] Get full task detail: {taskId}");

                var task = _taskRepo.GetById(taskId);

                if (task == null)
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = "Task not found" }));
                    return;
                }

                // Check ownership
                if (task.CreatedBy != user.UserId && !task.IsGroupTask)
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = "Access denied" }));
                    return;
                }

                // Get category info
                var category = task.CategoryId.HasValue ? _categoryRepo.GetById(task.CategoryId.Value) : null;

                // Get comments
                var comments = _commentRepo.GetByTaskId(taskId);

                // Get activities
                var activities = _activityLogRepo.GetByTaskId(taskId);

                // Get assignees (nếu là group task)
                var assignees = new List<string>();
                if (task.IsGroupTask)
                {
                    assignees = GetAssigneeNames(taskId);
                }

                // Calculate progress based on status
                int progress = task.Status switch
                {
                    TimeFlow.Models.TaskStatus.Pending => 0,
                    TimeFlow.Models.TaskStatus.InProgress => 50,
                    TimeFlow.Models.TaskStatus.Completed => 100,
                    TimeFlow.Models.TaskStatus.Cancelled => 0,
                    _ => 0
                };

                var response = new
                {
                    status = "success",
                    data = new
                    {
                        // Basic task info
                        taskId = task.TaskId,
                        title = task.Title,
                        description = task.Description,
                        dueDate = task.DueDate?.ToString("yyyy-MM-dd HH:mm:ss"),
                        priority = (int)task.Priority,
                        status = (int)task.Status,
                        isGroupTask = task.IsGroupTask,
                        createdBy = task.CreatedBy,
                        completedAt = task.CompletedAt?.ToString("yyyy-MM-dd HH:mm:ss"),
                        createdAt = task.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                        updatedAt = task.UpdatedAt?.ToString("yyyy-MM-dd HH:mm:ss"),
                        
                        // Extended info
                        categoryName = category?.CategoryName ?? "Other",
                        categoryColor = category?.Color ?? "#6B7280",
                        assignees = assignees,
                        progress = progress,
                        
                        // Comments
                        comments = comments.Select(c => new
                        {
                            commentId = c.CommentId,
                            userId = c.UserId,
                            username = c.Username,
                            fullName = c.FullName,
                            content = c.CommentText,
                            createdAt = c.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                            isEdited = c.IsEdited
                        }),
                        
                        // Activities
                        activities = activities.Select(a => new
                        {
                            logId = a.LogId,
                            userId = a.UserId,
                            activityType = a.ActivityType,
                            description = a.ActivityDescription,
                            createdAt = a.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                        })
                    }
                };

                await SendResponseAsync(JsonSerializer.Serialize(response));
                Log.Information($"[{_clientId}] ✓ Returned full task detail for TaskId={taskId}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[{_clientId}] Error getting full task detail");
                await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = ex.Message }));
            }
        }

        private List<string> GetAssigneeNames(int taskId)
        {
            var assignees = new List<string>();
            
            try
            {
                using (var conn = new Microsoft.Data.SqlClient.SqlConnection(TimeFlow.Data.Configuration.DbConfig.GetConnectionString()))
                {
                    conn.Open();
                    var query = @"
                        SELECT u.Username, u.FullName
                        FROM GroupTasks gt
                        INNER JOIN Users u ON gt.AssignedTo = u.UserId
                        WHERE gt.TaskId = @TaskId AND gt.AssignedTo IS NOT NULL
                    ";
                    
                    using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@TaskId", taskId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string fullName = reader.IsDBNull(1) ? null : reader.GetString(1);
                                string username = reader.GetString(0);
                                assignees.Add(!string.IsNullOrEmpty(fullName) ? fullName : username);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to get assignee names");
            }
            
            return assignees;
        }

        private async Task SendResponseAsync(string response)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(response);
                await _stream.WriteAsync(bytes, 0, bytes.Length);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, $"[{_clientId}] Failed to send response");
            }
        }

        private void Cleanup()
        {
            try
            {
                if (!string.IsNullOrEmpty(_currentUsername))
                {
                    lock (_clientsLock)
                    {
                        if (_onlineClients.TryGetValue(_currentUsername, out var client) && client == _client)
                        {
                            _onlineClients.Remove(_currentUsername);
                            Log.Information($"[{_clientId}] User '{_currentUsername}' removed from online list");
                        }
                    }

                    // Log logout activity
                    try
                    {
                        var user = _userRepo.GetByUsername(_currentUsername);
                        if (user != null)
                        {
                            _activityLogRepo.LogActivity(user.UserId, null, "Logout", "User disconnected from TCP");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Failed to log logout activity");
                    }
                }

                _client.Close();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, $"[{_clientId}] Cleanup error");
            }
        }
    }
}
