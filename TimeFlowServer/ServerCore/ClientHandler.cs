using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Serilog;
using TimeFlow.Data.Repositories;
using TimeFlow.Models;
using TimeFlowServer.Security;
using System.Collections.Generic;
using System.Linq;

namespace TimeFlowServer.ServerCore
{
    public class ClientHandler
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;
        private readonly string _clientId;

        // --- Repositories của Staging (Task & User) ---
        private readonly UserRepository _userRepo;
        private readonly ActivityLogRepository _activityLogRepo;
        private readonly TaskRepository _taskRepo;
        private readonly CategoryRepository _categoryRepo;
        private readonly CommentRepository _commentRepo;

        // --- Repositories bổ sung từ Chatbox (Chat & Group) ---
        private readonly MessageRepository _messageRepo;
        private readonly GroupTaskRepository _groupTaskRepo;
        private readonly GroupRepository _groupRepo;
        private readonly GroupMemberRepository _groupMemberRepo;
        private readonly ContactRepository _contactRepo;

        private readonly JwtManager _jwtManager;
        private readonly Dictionary<string, TcpClient> _onlineClients;
        private readonly object _clientsLock;

        private string? _currentUsername;
        private int? _currentUserId;

        // Constructor tổng hợp (Phải tiêm đủ Dependency từ Program.cs)
        public ClientHandler(
            TcpClient client,
            UserRepository userRepo,
            ActivityLogRepository activityLogRepo,
            TaskRepository taskRepo,
            CategoryRepository categoryRepo,
            CommentRepository commentRepo,
            // Thêm các Repo chat
            MessageRepository messageRepo,
            GroupTaskRepository groupTaskRepo,
            GroupRepository groupRepo,
            GroupMemberRepository groupMemberRepo,
            ContactRepository contactRepo,
            JwtManager jwtManager,
            Dictionary<string, TcpClient> onlineClients,
            object clientsLock)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _stream = client.GetStream();
            _clientId = client.Client.RemoteEndPoint?.ToString() ?? Guid.NewGuid().ToString();

            // Gán dependencies
            _userRepo = userRepo;
            _activityLogRepo = activityLogRepo;
            _taskRepo = taskRepo;
            _categoryRepo = categoryRepo;
            _commentRepo = commentRepo;

            _messageRepo = messageRepo;
            _groupTaskRepo = groupTaskRepo;
            _groupRepo = groupRepo;
            _groupMemberRepo = groupMemberRepo;
            _contactRepo = contactRepo;

            _jwtManager = jwtManager;
            _onlineClients = onlineClients;
            _clientsLock = clientsLock;
        }

        public async Task HandleAsync(CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[8192]; // Buffer lớn hơn cho tin nhắn dài

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
                    // --- Authentication (Staging) ---
                    case "login": await HandleLoginAsync(root); break;
                    case "register": await HandleRegisterAsync(root); break;
                    case "autologin": await HandleAutoLoginAsync(root); break;

                    // --- Chat Features (Chatbox) ---
                    case "chat": await HandleChatAsync(root); break;
                    case "get_history": await HandleGetHistoryAsync(root); break;
                    case "add_contact": await HandleAddContactAsync(root); break;

                    // --- Task Management (Staging) ---
                    case "get_tasks": await HandleGetTasksAsync(root); break;
                    case "get_task_detail": await HandleGetTaskDetailAsync(root); break;
                    case "get_task_detail_full": await HandleGetTaskDetailFullAsync(root); break;
                    case "create_task": await HandleCreateTaskAsync(root); break;
                    case "update_task": await HandleUpdateTaskAsync(root); break;
                    case "delete_task": await HandleDeleteTaskAsync(root); break;
                    case "update_task_status": await HandleUpdateTaskStatusAsync(root); break;

                    // --- Group Assign (Chatbox) ---
                    case "assign_group_task": await HandleAssignGroupTaskAsync(root); break;

                    // --- Metadata (Staging) ---
                    case "get_categories": await HandleGetCategoriesAsync(root); break;
                    case "get_groups":
                        int uId = root.GetProperty("userId").GetInt32();
                        var groups = _groupRepo.GetByUserId(uId);
                        var resp = new { status = "success", data = groups };
                        await SendResponseAsync(JsonSerializer.Serialize(resp));
                        break;

                    default:
                        Log.Warning($"[{_clientId}] Unknown message type: {type}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[{_clientId}] Error processing message");
                await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = "Internal server error" }));
            }
        }

        // =================================================================
        // 1. AUTHENTICATION HANDLERS
        // =================================================================

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
                    _userRepo.UpdateLastLogin(user.UserId);

                    lock (_clientsLock)
                    {
                        if (_onlineClients.ContainsKey(_currentUsername))
                            try { _onlineClients[_currentUsername].Close(); } catch { }
                        _onlineClients[_currentUsername] = _client;
                    }

                    string token = _jwtManager.CreateToken(_currentUsername);

                    // --- Logic Merge: Lấy thêm Groups & Contacts ---
                    var userGroups = _groupRepo.GetByUserId(user.UserId);
                    var contacts = _contactRepo.GetContactUsernames(user.UserId);

                    // Gửi response Login chuẩn
                    var response = new
                    {
                        status = "success",
                        token = token,
                        user = new { user.UserId, user.Username, user.Email, user.FullName },
                        groups = userGroups // Trả về nhóm ngay khi login
                    };
                    await SendResponseAsync(JsonSerializer.Serialize(response));

                    // Gửi danh sách bạn bè (gói tin riêng)
                    var contactResponse = new { type = "user_list", users = contacts };
                    await SendResponseAsync(JsonSerializer.Serialize(contactResponse));

                    Log.Information($"[{_clientId}] ✓ User '{_currentUsername}' logged in successfully");
                    _activityLogRepo.LogActivity(user.UserId, null, "Login", "User logged in via TCP");
                }
                else
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "fail", message = "Invalid credentials" }));
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

                if (_userRepo.UsernameOrEmailExists(username, email))
                {
                    await SendResponseAsync("exists");
                    return;
                }

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
                    Log.Information($"[{_clientId}] ✓ User '{username}' registered successfully");
                    _activityLogRepo.LogActivity(userId, null, "Register", "New user registered via TCP");
                }
                else
                {
                    await SendResponseAsync("error");
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
                        _currentUserId = user.UserId;
                        _userRepo.UpdateLastLogin(user.UserId);

                        lock (_clientsLock)
                        {
                            if (_onlineClients.ContainsKey(_currentUsername))
                                try { _onlineClients[_currentUsername].Close(); } catch { }
                            _onlineClients[_currentUsername] = _client;
                        }

                        // --- Logic Merge: Lấy Groups & Contacts ---
                        var userGroups = _groupRepo.GetByUserId(user.UserId);
                        var contacts = _contactRepo.GetContactUsernames(user.UserId);

                        var response = new
                        {
                            status = "autologin_success",
                            user = new { user.UserId, user.Username, user.Email, user.FullName },
                            groups = userGroups
                        };
                        await SendResponseAsync(JsonSerializer.Serialize(response));

                        var contactResponse = new { type = "user_list", users = contacts };
                        await SendResponseAsync(JsonSerializer.Serialize(contactResponse));

                        Log.Information($"[{_clientId}] ✓ User '{username}' auto-logged in");
                        _activityLogRepo.LogActivity(user.UserId, null, "AutoLogin", "User auto-logged in via token");
                        return;
                    }
                }
                await SendResponseAsync(JsonSerializer.Serialize(new { status = "autologin_fail" }));
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[{_clientId}] Auto-login error");
                await SendResponseAsync(JsonSerializer.Serialize(new { status = "autologin_fail" }));
            }
        }

        // =================================================================
        // 2. CHAT HANDLERS (Từ Chatbox)
        // =================================================================

        private async Task HandleChatAsync(JsonElement root)
        {
            if (string.IsNullOrEmpty(_currentUsername))
            {
                Log.Warning($"[{_clientId}] Chat message from unauthenticated client");
                return;
            }

            try
            {
                string content = root.GetProperty("content").GetString() ?? "";

                // Case 1: Chat Nhóm
                if (root.TryGetProperty("groupId", out JsonElement gElem))
                {
                    int gId = gElem.GetInt32();
                    _messageRepo.AddGroupMessage(_currentUsername, gId, content);

                    // Broadcast cho thành viên
                    var members = _groupMemberRepo.GetByGroupId(gId);
                    foreach (var m in members)
                    {
                        var uName = _userRepo.GetById(m.UserId)?.Username;
                        if (uName != null && uName != _currentUsername)
                        {
                            lock (_clientsLock)
                            {
                                if (_onlineClients.TryGetValue(uName, out var targetClient) && targetClient.Connected)
                                {
                                    var msg = new { type = "receive_group_message", groupId = gId, sender = _currentUsername, content = content, timestamp = DateTime.Now.ToString("HH:mm") };
                                    _ = SendToClientAsync(targetClient, JsonSerializer.Serialize(msg));
                                }
                            }
                        }
                    }
                    Log.Information($"[CHAT GROUP] {_currentUsername} sent to Group {gId}");
                }
                // Case 2: Chat Cá nhân
                else
                {
                    string receiver = root.GetProperty("receiver").GetString() ?? "";
                    _messageRepo.AddMessage(_currentUsername, receiver, content);

                    Log.Information($"[CHAT] {_currentUsername} → {receiver}: {content}");

                    lock (_clientsLock)
                    {
                        if (_onlineClients.TryGetValue(receiver, out var client) && client.Connected)
                        {
                            var msg = new { type = "receive_message", sender = _currentUsername, content = content, timestamp = DateTime.Now.ToString("HH:mm") };
                            _ = SendToClientAsync(client, JsonSerializer.Serialize(msg));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[{_clientId}] Chat error");
            }
        }

        private async Task HandleGetHistoryAsync(JsonElement root)
        {
            if (string.IsNullOrEmpty(_currentUsername)) return;

            if (root.TryGetProperty("groupId", out JsonElement gElem))
            {
                int gId = gElem.GetInt32();
                var history = _messageRepo.GetGroupHistory(gId);
                await SendResponseAsync(JsonSerializer.Serialize(new { type = "group_history_response", groupId = gId, messages = history }));
            }
            else
            {
                string targetUser = root.GetProperty("target_user").GetString() ?? "";
                var history = _messageRepo.GetHistory(_currentUsername, targetUser);
                await SendResponseAsync(JsonSerializer.Serialize(new { type = "history_data", data = history }));
            }
        }

        private async Task HandleAddContactAsync(JsonElement root)
        {
            string targetUsername = root.GetProperty("target_user").GetString() ?? "";
            var targetUser = _userRepo.GetByUsername(targetUsername);
            var currentUser = _userRepo.GetByUsername(_currentUsername);

            if (targetUser == null)
            {
                await SendResponseAsync(JsonSerializer.Serialize(new { type = "receive_message", sender = "System", content = "Người dùng không tồn tại!", timestamp = DateTime.Now.ToString("HH:mm") }));
                return;
            }

            bool success = _contactRepo.AddContact(currentUser.UserId, targetUser.UserId);
            if (success)
            {
                var contacts = _contactRepo.GetContactUsernames(currentUser.UserId);
                await SendResponseAsync(JsonSerializer.Serialize(new { type = "user_list", users = contacts }));
                await SendResponseAsync(JsonSerializer.Serialize(new { type = "receive_message", sender = "System", content = $"Đã thêm {targetUsername} vào danh bạ!", timestamp = DateTime.Now.ToString("HH:mm") }));
            }
            else
            {
                await SendResponseAsync(JsonSerializer.Serialize(new { type = "receive_message", sender = "System", content = "Người này đã có trong danh bạ!", timestamp = DateTime.Now.ToString("HH:mm") }));
            }
        }

        // =================================================================
        // 3. TASK HANDLERS (Từ Staging - giữ nguyên vì code rất tốt)
        // =================================================================

        private async Task HandleGetTasksAsync(JsonElement root)
        {
            try
            {
                if (!ValidateAuth(root, out string username)) return;
                var user = _userRepo.GetByUsername(username);

                var tasks = _taskRepo.GetByUserId(user.UserId);
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
                        createdAt = t.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                    })
                };
                await SendResponseAsync(JsonSerializer.Serialize(response));
            }
            catch (Exception ex)
            {
                await SendErrorAsync(ex.Message);
            }
        }

        private async Task HandleGetTaskDetailAsync(JsonElement root)
        {
            try
            {
                if (!ValidateAuth(root, out string username)) return;
                var user = _userRepo.GetByUsername(username);

                int taskId = root.GetProperty("taskId").GetInt32();
                var task = _taskRepo.GetById(taskId);

                if (task == null) { await SendErrorAsync("Task not found"); return; }
                if (task.CreatedBy != user.UserId && !task.IsGroupTask) { await SendErrorAsync("Access denied"); return; }

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
                        isGroupTask = task.IsGroupTask
                    }
                };
                await SendResponseAsync(JsonSerializer.Serialize(response));
            }
            catch (Exception ex) { await SendErrorAsync(ex.Message); }
        }

        private async Task HandleGetTaskDetailFullAsync(JsonElement root)
        {
            try
            {
                if (!ValidateAuth(root, out string username)) return;
                var user = _userRepo.GetByUsername(username);

                int taskId = root.GetProperty("taskId").GetInt32();
                var task = _taskRepo.GetById(taskId);

                if (task == null) { await SendErrorAsync("Task not found"); return; }
                if (task.CreatedBy != user.UserId && !task.IsGroupTask) { await SendErrorAsync("Access denied"); return; }

                var category = task.CategoryId.HasValue ? _categoryRepo.GetById(task.CategoryId.Value) : null;
                var comments = _commentRepo.GetByTaskId(taskId);
                var activities = _activityLogRepo.GetByTaskId(taskId);
                var assignees = task.IsGroupTask ? GetAssigneeNames(taskId) : new List<string>();

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
                        categoryName = category?.CategoryName ?? "Other",
                        categoryColor = category?.Color ?? "#6B7280",
                        assignees = assignees,
                        comments = comments.Select(c => new {
                            commentId = c.CommentId,
                            username = c.Username,
                            content = c.CommentText,
                            createdAt = c.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                        }),
                        activities = activities.Select(a => new {
                            description = a.ActivityDescription,
                            createdAt = a.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                        })
                    }
                };
                await SendResponseAsync(JsonSerializer.Serialize(response));
            }
            catch (Exception ex) { await SendErrorAsync(ex.Message); }
        }

        private async Task HandleCreateTaskAsync(JsonElement root)
        {
            try
            {
                if (!ValidateAuth(root, out string username)) return;
                var user = _userRepo.GetByUsername(username);
                var data = root.GetProperty("data");

                int? categoryId = null;
                if (data.TryGetProperty("categoryId", out var c) && c.ValueKind == JsonValueKind.Number) categoryId = c.GetInt32();

                var newTask = new TaskItem
                {
                    Title = data.GetProperty("title").GetString() ?? "",
                    Description = data.TryGetProperty("description", out var d) ? d.GetString() : null,
                    DueDate = data.TryGetProperty("dueDate", out var date) && !string.IsNullOrEmpty(date.GetString()) ? DateTime.Parse(date.GetString()!) : null,
                    Priority = (TaskPriority)data.GetProperty("priority").GetInt32(),
                    Status = TimeFlow.Models.TaskStatus.Pending,
                    CategoryId = categoryId,
                    CreatedBy = user.UserId,
                    IsGroupTask = data.TryGetProperty("isGroupTask", out var ig) && ig.GetBoolean()
                };

                int taskId = _taskRepo.Create(newTask);

                if (taskId > 0)
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "success", taskId = taskId }));
                    Log.Information($"[{_clientId}] ✓ Task created with ID={taskId}");
                }
                else
                {
                    await SendErrorAsync("Failed to create task");
                }
            }
            catch (TimeFlow.Data.Exceptions.ValidationException ex)
            {
                await SendResponseAsync(JsonSerializer.Serialize(new { status = "validation_error", message = ex.Message }));
            }
            catch (Exception ex)
            {
                await SendErrorAsync(ex.Message);
            }
        }

        private async Task HandleUpdateTaskAsync(JsonElement root)
        {
            try
            {
                if (!ValidateAuth(root, out string username)) return;
                var user = _userRepo.GetByUsername(username);
                var data = root.GetProperty("data");
                int taskId = data.GetProperty("taskId").GetInt32();

                var existingTask = _taskRepo.GetById(taskId);
                if (existingTask == null || existingTask.CreatedBy != user.UserId)
                {
                    await SendErrorAsync("No permission or task not found");
                    return;
                }

                // Update fields
                existingTask.Title = data.GetProperty("title").GetString();
                existingTask.Description = data.TryGetProperty("description", out var desc) ? desc.GetString() : null;
                if (data.TryGetProperty("dueDate", out var due) && !string.IsNullOrEmpty(due.GetString()))
                    existingTask.DueDate = DateTime.Parse(due.GetString());

                existingTask.Priority = (TaskPriority)data.GetProperty("priority").GetInt32();
                existingTask.Status = (TimeFlow.Models.TaskStatus)data.GetProperty("status").GetInt32();
                existingTask.CategoryId = data.TryGetProperty("categoryId", out var cat) ? cat.GetInt32() : null;
                existingTask.UpdatedAt = DateTime.Now;

                bool success = _taskRepo.Update(existingTask, user.UserId);
                await SendResponseAsync(JsonSerializer.Serialize(new { status = success ? "success" : "error" }));
            }
            catch (Exception ex) { await SendErrorAsync(ex.Message); }
        }

        private async Task HandleDeleteTaskAsync(JsonElement root)
        {
            try
            {
                if (!ValidateAuth(root, out string username)) return;
                var user = _userRepo.GetByUsername(username);
                int taskId = root.GetProperty("taskId").GetInt32();

                // Sử dụng DeleteWithCascade từ Staging
                bool success = _taskRepo.DeleteWithCascade(taskId, user.UserId);
                await SendResponseAsync(JsonSerializer.Serialize(new { status = success ? "success" : "error" }));
            }
            catch (Exception ex) { await SendErrorAsync(ex.Message); }
        }

        private async Task HandleUpdateTaskStatusAsync(JsonElement root)
        {
            try
            {
                if (!ValidateAuth(root, out string username)) return;
                var user = _userRepo.GetByUsername(username);

                int taskId = root.GetProperty("taskId").GetInt32();
                int statusValue = root.GetProperty("status").GetInt32();
                var newStatus = (TimeFlow.Models.TaskStatus)statusValue;

                bool success = _taskRepo.UpdateStatus(taskId, newStatus, user.UserId);
                await SendResponseAsync(JsonSerializer.Serialize(new { status = success ? "success" : "error" }));
            }
            catch (Exception ex) { await SendErrorAsync(ex.Message); }
        }

        // =================================================================
        // 4. GROUP TASK ASSIGNMENT (Từ Chatbox)
        // =================================================================

        private async Task HandleAssignGroupTaskAsync(JsonElement root)
        {
            try
            {
                if (_currentUsername == null) return;
                int taskId = root.GetProperty("taskId").GetInt32();
                int groupId = root.GetProperty("groupId").GetInt32();
                string targetUsername = root.GetProperty("assignedToUsername").GetString() ?? "";

                var currentUser = _userRepo.GetByUsername(_currentUsername);
                var targetUser = _userRepo.GetByUsername(targetUsername);

                if (targetUser == null)
                {
                    await SendErrorAsync("Người dùng không tồn tại.");
                    return;
                }

                bool success = _groupTaskRepo.AssignTask(taskId, groupId, targetUser.UserId, currentUser.UserId);

                if (success)
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new
                    {
                        type = "notification",
                        message = $"Đã giao task cho {targetUsername} thành công!"
                    }));

                    lock (_clientsLock)
                    {
                        if (_onlineClients.TryGetValue(targetUsername, out TcpClient? targetClient))
                        {
                            var notif = new { type = "notification", message = $"Bạn vừa được giao việc bởi {_currentUsername}." };
                            _ = SendToClientAsync(targetClient, JsonSerializer.Serialize(notif));
                        }
                    }
                }
                else
                {
                    await SendErrorAsync("Giao việc thất bại.");
                }
            }
            catch (Exception ex)
            {
                await SendErrorAsync("Lỗi server khi giao việc.");
            }
        }

        // =================================================================
        // 5. HELPER METHODS
        // =================================================================

        private async Task HandleGetCategoriesAsync(JsonElement root)
        {
            var categories = _categoryRepo.GetAll();
            await SendResponseAsync(JsonSerializer.Serialize(new { status = "success", data = categories }));
        }

        private bool ValidateAuth(JsonElement root, out string username)
        {
            username = "";
            string token = root.TryGetProperty("token", out var t) ? t.GetString() : null;
            if (string.IsNullOrEmpty(token) || !_jwtManager.ValidateToken(token, out username))
            {
                SendErrorAsync("Not authenticated").Wait();
                return false;
            }
            return true;
        }

        private List<string> GetAssigneeNames(int taskId)
        {
            var assignees = new List<string>();
            try
            {
                // Logic truy vấn trực tiếp DB để lấy tên người được giao việc
                // Lưu ý: Đảm bảo chuỗi kết nối chính xác trong Config
                using (var conn = new Microsoft.Data.SqlClient.SqlConnection(TimeFlow.Data.Configuration.DbConfig.GetConnectionString()))
                {
                    conn.Open();
                    var query = @"
                        SELECT u.Username, u.FullName
                        FROM GroupTasks gt
                        INNER JOIN Users u ON gt.AssignedTo = u.UserId
                        WHERE gt.TaskId = @TaskId AND gt.AssignedTo IS NOT NULL";

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

        private async Task SendResponseAsync(string json)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                await _stream.WriteAsync(bytes, 0, bytes.Length);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, $"[{_clientId}] Failed to send response");
            }
        }

        private async Task SendErrorAsync(string message)
        {
            await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = message }));
        }

        private async Task SendToClientAsync(TcpClient client, string json)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                await client.GetStream().WriteAsync(bytes, 0, bytes.Length);
            }
            catch { }
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
                        }
                    }
                    try
                    {
                        var user = _userRepo.GetByUsername(_currentUsername);
                        if (user != null) _activityLogRepo.LogActivity(user.UserId, null, "Logout", "User disconnected");
                    }
                    catch { }
                }
                _client.Close();
            }
            catch { }
        }
    }
}