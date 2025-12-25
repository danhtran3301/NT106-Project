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
        private readonly GroupRepository _groupRepo;
        private readonly GroupMemberRepository _groupMemberRepo;
        private readonly MessageRepository _messageRepo;
        private readonly JwtManager _jwtManager;
        private readonly Dictionary<string, TcpClient> _onlineClients;
        private readonly object _clientsLock;

        private string? _currentUsername;
        private int? _currentUserId;
        private readonly string _clientId;
        private readonly GroupTaskRepository _groupTaskRepo;

        // Constructor tổng hợp (Phải tiêm đủ Dependency từ Program.cs)
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
            GroupTaskRepository groupTaskRepo,  // ✅ Thêm parameter
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
            _groupRepo = groupRepo;
            _groupMemberRepo = groupMemberRepo;
            _messageRepo = messageRepo;
            _groupTaskRepo = groupTaskRepo;  // ✅ Gán từ parameter thay vì new
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

                    case "add_group_member":
                        await HandleAddGroupMemberAsync(root);
                        break;

                    case "get_group_members":
                        await HandleGetGroupMembersAsync(root);
                        break;

                    // ✅ MỚI: Lấy lịch sử chat nhóm
                    case "get_group_chat_history":
                        await HandleGetGroupChatHistoryAsync(root);
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

        // ✅ MỚI: Handler lấy lịch sử chat nhóm
        private async Task HandleGetGroupChatHistoryAsync(JsonElement root)
        {
            try
            {
                // Authenticate từ token
                string? token = root.TryGetProperty("token", out var tokenElem) ? tokenElem.GetString() : null;
                if (string.IsNullOrEmpty(token))
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { type = "group_chat_history", status = "error", message = "Not authenticated" }));
                    return;
                }

                if (!_jwtManager.ValidateToken(token, out string username) || string.IsNullOrEmpty(username))
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { type = "group_chat_history", status = "error", message = "Not authenticated" }));
                    return;
                }

                var user = _userRepo.GetByUsername(username);
                if (user == null)
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { type = "group_chat_history", status = "error", message = "User not found" }));
                    return;
                }

                int groupId = root.GetProperty("groupId").GetInt32();
                Log.Information($"[{_clientId}] Getting chat history for GroupId: {groupId}");

                // Kiểm tra user có phải member của group không
                if (!_groupMemberRepo.IsMember(groupId, user.UserId))
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { type = "group_chat_history", status = "error", message = "You are not a member of this group" }));
                    return;
                }

                // Lấy lịch sử chat
                var messages = _messageRepo.GetGroupHistory(groupId);
                
                // Build response
                var messageList = new List<object>();
                foreach (var msg in messages)
                {
                    messageList.Add(new
                    {
                        sender = msg.Sender,
                        content = msg.Content,
                        timestamp = msg.Time.ToString("HH:mm dd/MM")
                    });
                }

                var response = new
                {
                    type = "group_chat_history",
                    status = "success",
                    groupId = groupId,
                    messages = messageList
                };

                await SendResponseAsync(JsonSerializer.Serialize(response));
                Log.Information($"[{_clientId}] ✓ Returned {messageList.Count} messages for GroupId={groupId}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[{_clientId}] Error getting group chat history");
                await SendResponseAsync(JsonSerializer.Serialize(new { type = "group_chat_history", status = "error", message = ex.Message }));
            }
        }

        private async Task HandleCreateGroupAsync(JsonElement root)
        {
            try
            {
                // Authenticate từ token (giống các handler khác)
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
                string groupName = data.GetProperty("groupName").GetString() ?? string.Empty;
                string description = data.TryGetProperty("description", out var desc) ? desc.GetString() : "";

                // Validate
                if (string.IsNullOrWhiteSpace(groupName))
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = "Group name cannot be empty" }));
                    return;
                }

                if (_groupRepo.GroupNameExists(groupName))
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = "Group name already exists" }));
                    return;
                }

                // Tạo Group Model
                var newGroup = new Group
                {
                    GroupName = groupName,
                    Description = description,
                    CreatedBy = user.UserId,
                    IsActive = true
                };

                // Lưu vào DB
                int groupId = _groupRepo.Create(newGroup);

                if (groupId > 0)
                {
                    // Đảm bảo creator là admin
                    try
                    {
                        _groupMemberRepo.AddMember(groupId, user.UserId, GroupRole.Admin);
                    }
                    catch
                    {
                        // ignore - repo sẽ trả 0 nếu đã tồn tại
                    }

                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "success", groupId = groupId, groupName = groupName }));
                    Log.Information($"[{_clientId}] Created group '{groupName}' (ID: {groupId}) by {username}");
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
                string? token = root.TryGetProperty("token", out var tokenElem) ? tokenElem.GetString() : null;
                if (string.IsNullOrEmpty(token) || !_jwtManager.ValidateToken(token, out string username) || string.IsNullOrEmpty(username))
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

                Log.Information($"[{_clientId}] Getting groups for UserID: {user.UserId} ({username})");

                var groups = _groupRepo.GetByUserId(user.UserId);

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
                Log.Information($"[{_clientId}] ✓ Returned {groups.Count} groups for {username}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting groups");
                await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = ex.Message }));
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
                        _currentUserId = user.UserId;  // ✅ SỬA: Set _currentUserId

                        // Cap nhat last login
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

                        Log.Information($"[{_clientId}] ✓ User '{username}' (ID: {user.UserId}) auto-logged in successfully");

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
            if (string.IsNullOrEmpty(_currentUsername) || !_currentUserId.HasValue) return;

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
                    if (!int.TryParse(receiver, out int groupId))
                    {
                        await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = "Invalid group id" }));
                        return;
                    }

                    // Kiểm tra group tồn tại
                    var group = _groupRepo.GetById(groupId);
                    if (group == null || !group.IsActive)
                    {
                        await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = "Group not found" }));
                        return;
                    }

                    // Kiểm tra sender có phải là member
                    if (!_groupMemberRepo.IsMember(groupId, _currentUserId.Value))
                    {
                        await SendResponseAsync(JsonSerializer.Serialize(new { status = "unauthorized", message = "You are not a member of this group" }));
                        return;
                    }

                    // 1. Lưu vào Database
                    _messageRepo.AddGroupMessage(_currentUsername, groupId, content);
                    Log.Information($"[GROUP CHAT] {_currentUsername} sent to Group {groupId}: {content}");

                    // 2. Lấy danh sách thành viên trong nhóm
                    var members = _groupMemberRepo.GetByGroupId(groupId);

                    // 3. Gửi cho từng thành viên đang online (KHÔNG gửi lại cho người gửi)
                    int sentCount = 0;
                    foreach (var member in members)
                    {
                        var user = _userRepo.GetById(member.UserId);
                        if (user == null) continue;

                        string memberUsername = user.Username;
                        
                        // ✅ KHÔNG gửi lại cho người gửi - client đã tự hiển thị rồi
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
                                sentCount++;
                                Log.Information($"[GROUP CHAT] Delivered to {memberUsername}");
                            }
                            catch (Exception ex)
                            {
                                Log.Warning(ex, $"Failed to send group message to {memberUsername}");
                            }
                        }
                        else
                        {
                            Log.Information($"[GROUP CHAT] {memberUsername} is offline, message saved to DB");
                        }
                    }
                    
                    Log.Information($"[GROUP CHAT] Message delivered to {sentCount}/{members.Count - 1} online members");
                }
                else
                {
                    // --- XỬ LÝ CHAT 1-1 ---

                    // 1. Lưu vào DB
                    _messageRepo.AddMessage(_currentUsername, receiver, content);
                    Log.Information($"[CHAT 1-1] {_currentUsername} -> {receiver}: {content}");

                    // 2. Gửi cho người nhận nếu đang online
                    TcpClient? receiverClient = null;
                    lock (_clientsLock)
                    {
                        if (_onlineClients.TryGetValue(receiver, out var c)) receiverClient = c;
                    }

                    if (receiverClient != null && receiverClient.Connected)
                    {
                        try
                        {
                            var packet = new
                            {
                                type = "receive_message",
                                sender = _currentUsername,
                                content = content,
                                timestamp = DateTime.Now.ToString("HH:mm")
                            };

                            string json = JsonSerializer.Serialize(packet);
                            byte[] bytes = Encoding.UTF8.GetBytes(json);
                            await receiverClient.GetStream().WriteAsync(bytes, 0, bytes.Length);
                            
                            Log.Information($"[CHAT 1-1] Message delivered to {receiver}");
                        }
                        catch (Exception ex)
                        {
                            Log.Warning(ex, $"Failed to send 1-1 message to {receiver}");
                        }
                    }
                    else
                    {
                        Log.Information($"[CHAT 1-1] {receiver} is offline, message saved to DB");
                    }
                }
            }
                }
            }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Chat error");
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
                var tasks = _taskRepo.GetByUserId(userId);

                // ✅ Build response với GroupTask info
                var taskDataList = new List<object>();
                foreach (var t in tasks)

                var response = new

                var response = new
                {
                    // Lấy GroupTask info nếu là group task
                    object groupTaskInfo = null;
                    if (t.IsGroupTask)
                    {
                        var groupTask = _groupTaskRepo.GetByTaskId(t.TaskId);
                        if (groupTask != null)
                        {
                            // ✅ Lấy username của assignee
                            string? assignedToUsername = null;
                            string? assignedToFullName = null;
                            if (groupTask.AssignedTo.HasValue)
                            {
                                var assignedUser = _userRepo.GetById(groupTask.AssignedTo.Value);
                                if (assignedUser != null)
                                {
                                    assignedToUsername = assignedUser.Username;
                                    assignedToFullName = assignedUser.FullName;
                                }
                            }

                            // ✅ Lấy group name
                            string? groupName = null;
                            var group = _groupRepo.GetById(groupTask.GroupId);
                            if (group != null)
                            {
                                groupName = group.GroupName;
                            }

                            groupTaskInfo = new
                            {
                                groupTaskId = groupTask.GroupTaskId,
                                groupId = groupTask.GroupId,
                                groupName = groupName,
                                assignedTo = groupTask.AssignedTo,
                                assignedToUsername = assignedToUsername,
                                assignedToFullName = assignedToFullName,
                                assignedBy = groupTask.AssignedBy,
                                assignedAt = groupTask.AssignedAt?.ToString("yyyy-MM-dd HH:mm:ss")
                            };
                        }
                    }

                    taskDataList.Add(new
                    {
                        taskId = t.TaskId,
                        title = t.Title,
                        description = t.Description,
                        dueDate = t.DueDate?.ToString("yyyy-MM-dd HH:mm:ss"),
                        createdAt = t.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                        updatedAt = t.UpdatedAt?.ToString("yyyy-MM-dd HH:mm:ss"),
                        groupTask = groupTaskInfo
                    });
                }

                var response = new
                {
                    status = "success",
                    data = taskDataList
                        updatedAt = t.UpdatedAt?.ToString("yyyy-MM-dd HH:mm:ss")
                    })
                        updatedAt = t.UpdatedAt?.ToString("yyyy-MM-dd HH:mm:ss")
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
                
                // Parse categoryId
                
                if (data.TryGetProperty("categoryId", out var catIdElem) && 
                    catIdElem.ValueKind != JsonValueKind.Null && 
                    catIdElem.ValueKind == JsonValueKind.Number)
                {
                    categoryId = catIdElem.GetInt32();
                }

                // Parse groupId
                int? groupId = null;
                if (data.TryGetProperty("groupId", out var groupIdElem) && 
                    groupIdElem.ValueKind == JsonValueKind.Number)
                {
                    groupId = groupIdElem.GetInt32();
                }

                // Parse assignedTo
                int? assignedTo = null;
                if (data.TryGetProperty("assignedTo", out var assignedToElem) && 
                    assignedToElem.ValueKind == JsonValueKind.Number)
                {
                    assignedTo = assignedToElem.GetInt32();
                }

                bool isGroupTask = data.TryGetProperty("isGroupTask", out var isGroup) && isGroup.GetBoolean();
                
                }
                
                }
                
                var newTask = new TaskItem
                {
                    Status = data.TryGetProperty("status", out var statusProp) ? (TimeFlow.Models.TaskStatus)statusProp.GetInt32() : TimeFlow.Models.TaskStatus.Pending,
                    CategoryId = categoryId,
                    CreatedBy = user.UserId,
                    IsGroupTask = isGroupTask
                    CreatedBy = user.UserId, // ✅ Always use authenticated user's ID
                    IsGroupTask = data.TryGetProperty("isGroupTask", out var isGroup) && isGroup.GetBoolean()
                Log.Information($"[{_clientId}] Creating task: {newTask.Title} for user {username} (IsGroupTask: {isGroupTask}, GroupId: {groupId}, AssignedTo: {assignedTo})");

                // Create task

                // ✅ Create task (validation + activity logging done inside transaction)

                // ✅ Create task (validation + activity logging done inside transaction)
                int taskId = _taskRepo.Create(newTask);

                if (taskId > 0)
                {
                    // If group task, create GroupTask record to link task to group with assignee
                    if (isGroupTask && groupId.HasValue)
                    {
                        int groupTaskId = _groupTaskRepo.Create(taskId, groupId.Value, assignedTo, user.UserId);
                        Log.Information($"[{_clientId}] ✓ Created GroupTask link: TaskId={taskId}, GroupId={groupId}, AssignedTo={assignedTo}, GroupTaskId={groupTaskId}");
                        
                        // Log activity nếu có assignee
                        if (assignedTo.HasValue)
                        {
                            var assignedUser = _userRepo.GetById(assignedTo.Value);
                            string assigneeName = assignedUser?.Username ?? assignedTo.Value.ToString();
                            _activityLogRepo.LogActivity(user.UserId, taskId, "Assigned", $"Task assigned to {assigneeName}");
                        }
                    }

                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "success", taskId = taskId }));
                    Log.Information($"[{_clientId}] ✓ Task created with ID={taskId}");
                }
                else
                {
                    await SendErrorAsync("Failed to create task");
                await SendResponseAsync(JsonSerializer.Serialize(new { 
                    status = "validation_error",
                    field = ex.Field,
                    message = ex.Message 
                }));
                Log.Warning($"[{_clientId}] Validation error creating task: Field={ex.Field}, Message={ex.Message}");
            }
            catch (TimeFlow.Data.Exceptions.UnauthorizedException ex)
            {
                await SendResponseAsync(JsonSerializer.Serialize(new { 
                    status = "unauthorized",
                    message = ex.Message 
                }));
                Log.Warning($"[{_clientId}] Unauthorized task creation: {ex.Message}");
                    status = "unauthorized",
                    message = ex.Message 
                }));
                Log.Error(ex, $"[{_clientId}] Error creating task");
                await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = ex.Message }));
                    message = ex.Message 
                }));
                // Generic errors
                Log.Error(ex, $"[{_clientId}] Error creating task");
                await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = ex.Message }));
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
                if (!response.EndsWith("\n")) response += "\n";
                byte[] bytes = Encoding.UTF8.GetBytes(response);
                Log.Warning(ex, "Failed to get assignee names");
            }
            return assignees;
        }
                byte[] bytes = Encoding.UTF8.GetBytes(response);
        private async Task SendResponseAsync(string json)
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
            catch (Exception ex)
            {
                Log.Warning(ex, $"[{_clientId}] Cleanup error");
            }
        }

        private async Task HandleAddGroupMemberAsync(JsonElement root)
        {
            try
            {
                // Authenticate từ token (giống các handler khác)
                string token = root.TryGetProperty("token", out var tokenElem) ? tokenElem.GetString() : null;
                
                if (string.IsNullOrEmpty(token) || !_jwtManager.ValidateToken(token, out string username))
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = "Not authenticated" }));
                    return;
                }

                var currentUser = _userRepo.GetByUsername(username);
                if (currentUser == null)
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = "User not found" }));
                    return;
                }

                var data = root.GetProperty("data");
                int groupId = data.GetProperty("groupId").GetInt32();
                string usernameToAdd = data.GetProperty("username").GetString() ?? "";

                Log.Information($"[{_clientId}] Adding member '{usernameToAdd}' to GroupId={groupId}");

                // Kiểm tra quyền admin
                if (!_groupMemberRepo.IsAdmin(groupId, currentUser.UserId))
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = "Only admins can add members" }));
                    Log.Warning($"[{_clientId}] User {currentUser.UserId} is not admin of group {groupId}");
                    return;
                }

                // Tìm user theo username
                var userToAdd = _userRepo.GetByUsername(usernameToAdd);
                if (userToAdd == null)
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = "User not found" }));
                    Log.Warning($"[{_clientId}] User '{usernameToAdd}' not found");
                    return;
                }

                // Thêm member
                int result = _groupMemberRepo.AddMember(groupId, userToAdd.UserId);
                
                if (result > 0)
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "success", message = $"Added {usernameToAdd} to group" }));
                    Log.Information($"[{_clientId}] ✓ Added {usernameToAdd} to GroupId={groupId}");
                    
                    // Broadcast event "group_member_added" to all group members
                    await BroadcastGroupMemberAddedAsync(groupId, userToAdd.UserId, usernameToAdd);
                }
                else
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = "User already in group" }));
                    Log.Warning($"[{_clientId}] User {usernameToAdd} already in group {groupId}");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[{_clientId}] Error adding member");
                await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = ex.Message }));
            }
        }

        private async Task BroadcastGroupMemberAddedAsync(int groupId, int newUserId, string newUsername)
        {
            try
            {
                // Get all members of the group
                var members = _groupMemberRepo.GetByGroupId(groupId);
                var group = _groupRepo.GetById(groupId);

                if (group == null) return;

                // Broadcast to all online members
                foreach (var member in members)
                {
                    var user = _userRepo.GetById(member.UserId);
                    if (user == null) continue;

                    TcpClient? clientToSend = null;
                    lock (_clientsLock)
                    {
                        if (_onlineClients.TryGetValue(user.Username, out var c))
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
                                type = "group_member_added",
                                groupId = groupId,
                                groupName = group.GroupName,
                                newUserId = newUserId,
                                newUsername = newUsername
                            };
                            string json = JsonSerializer.Serialize(packet);
                            byte[] bytes = Encoding.UTF8.GetBytes(json);
                            await clientToSend.GetStream().WriteAsync(bytes, 0, bytes.Length);
                        }
                        catch (Exception ex)
                        {
                            Log.Warning(ex, $"Failed to broadcast group_member_added to {user.Username}");
                        }
                    }
                }

                Log.Information($"[BROADCAST] group_member_added: GroupId={groupId}, NewUser={newUsername}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error broadcasting group_member_added");
            }
        }

        private async Task HandleGetGroupMembersAsync(JsonElement root)
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

                int groupId = root.GetProperty("groupId").GetInt32();
                Log.Information($"[{_clientId}] Getting members for GroupId: {groupId}");

                // Kiểm tra user có phải member của group không
                if (!_groupMemberRepo.IsMember(groupId, user.UserId))
                {
                    await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = "You are not a member of this group" }));
                    return;
                }

                // Lấy danh sách members
                var members = _groupMemberRepo.GetByGroupId(groupId);
                
                // Build response với user info
                var memberList = new List<object>();
                foreach (var member in members)
                {
                    var memberUser = _userRepo.GetById(member.UserId);
                    if (memberUser != null)
                    {
                        memberList.Add(new
                        {
                            userId = memberUser.UserId,
                            username = memberUser.Username,
                            fullName = memberUser.FullName,
                            email = memberUser.Email,
                            role = member.Role.ToString()
                        });
                    }
                }

                var response = new
                {
                    status = "success",
                    data = memberList
                };

                await SendResponseAsync(JsonSerializer.Serialize(response));
                Log.Information($"[{_clientId}] ✓ Returned {memberList.Count} members for GroupId={groupId}");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"[{_clientId}] Error getting group members");
                await SendResponseAsync(JsonSerializer.Serialize(new { status = "error", message = ex.Message }));
            }
        }

            catch (Exception ex)
            {
                Log.Warning(ex, $"[{_clientId}] Cleanup error");
            }
        }
            catch (Exception ex)
            {
                Log.Warning(ex, $"[{_clientId}] Cleanup error");
            }
        }
    }
}