using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TimeFlow.Models;
using TimeFlow.Configuration;

namespace TimeFlow.Services
{
    // DTO cho Group Member
    public class GroupMemberDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = "";
        public string? FullName { get; set; }
        public string Role { get; set; } = "Member";
    }

    public class TaskApiClient
    {
        private readonly string _serverHost;
        private readonly int _serverPort;
        private readonly int _timeout;

        public TaskApiClient(string? serverHost = null, int? serverPort = null, int? timeout = null)
        {
            _serverHost = serverHost ?? ServerConfig.Host;
            _serverPort = serverPort ?? ServerConfig.Port;
            _timeout = timeout ?? ServerConfig.Timeout;
        }

        private async Task<string> SendRequestAsync(object request)
        {
            using (TcpClient client = new TcpClient())
            {
                client.ReceiveTimeout = _timeout;
                client.SendTimeout = _timeout;

                await client.ConnectAsync(_serverHost, _serverPort);

                using (NetworkStream stream = client.GetStream())
                {
                    string json = JsonSerializer.Serialize(request);
                    byte[] sendBytes = Encoding.UTF8.GetBytes(json);
                    await stream.WriteAsync(sendBytes, 0, sendBytes.Length);
                    await stream.FlushAsync();

                   
                    const int bufferSize = 65536; 
                    byte[] buffer = new byte[bufferSize];
                    using (var memoryStream = new System.IO.MemoryStream())
                    {
                        int bytesRead;
                        
                       
                        bytesRead = await stream.ReadAsync(buffer, 0, bufferSize);
                        
                        if (bytesRead > 0)
                        {
                            await memoryStream.WriteAsync(buffer, 0, bytesRead);
                            
                            
                            while (bytesRead == bufferSize)
                            {
                               
                                bytesRead = await stream.ReadAsync(buffer, 0, bufferSize);
                                if (bytesRead > 0)
                                {
                                    await memoryStream.WriteAsync(buffer, 0, bytesRead);
                                }
                                else
                                {
                                    break; 
                                }
                            }
                            
                            
                            await System.Threading.Tasks.Task.Delay(150);
                        }
                        
                     
                        if (memoryStream.Length == 0)
                        {
                            throw new Exception("No response received from server");
                        }
                        
                        string responseJson = Encoding.UTF8.GetString(memoryStream.GetBuffer(), 0, (int)memoryStream.Length).Trim();
                        return responseJson;
                    }
                }
            }
        }

        private string GetErrorMessage(JsonElement root)
        {
            if (root.TryGetProperty("message", out var msg))
            {
                return msg.GetString() ?? "Unknown error";
            }
            return "Unknown error";
        }

        public async Task<List<TaskItem>> GetTasksAsync()
        {
            try
            {
                
                var request = new 
                { 
                    type = "get_tasks",
                    token = SessionManager.Token,
                    userId = SessionManager.UserId
                };
                string responseJson = await SendRequestAsync(request);

                using var doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;

                string status = root.GetProperty("status").GetString();
                if (status == "success")
                {
                    var tasks = new List<TaskItem>();
                    var dataArray = root.GetProperty("data");

                    foreach (var item in dataArray.EnumerateArray())
                    {
                        var task = new TaskItem
                        {
                            TaskId = item.GetProperty("taskId").GetInt32(),
                            Title = item.GetProperty("title").GetString() ?? "",
                            Description = item.TryGetProperty("description", out var desc) ? desc.GetString() : null,
                            DueDate = item.TryGetProperty("dueDate", out var dueDate) && !string.IsNullOrEmpty(dueDate.GetString())
                                ? DateTime.Parse(dueDate.GetString()!) : null,
                            Priority = (TaskPriority)item.GetProperty("priority").GetInt32(),
                            Status = (TimeFlow.Models.TaskStatus)item.GetProperty("status").GetInt32(),
                            CategoryId = item.TryGetProperty("categoryId", out var catId) && catId.ValueKind == JsonValueKind.Number 
                                ? catId.GetInt32() : null,
                            IsGroupTask = item.GetProperty("isGroupTask").GetBoolean(),
                            CreatedAt = DateTime.Parse(item.GetProperty("createdAt").GetString()!),
                            UpdatedAt = item.TryGetProperty("updatedAt", out var updatedAt) && !string.IsNullOrEmpty(updatedAt.GetString())
                                ? DateTime.Parse(updatedAt.GetString()!) : null
                        };

                     
                        if (item.TryGetProperty("groupTask", out var groupTaskElem) && groupTaskElem.ValueKind == JsonValueKind.Object)
                        {
                            task.GroupTask = new GroupTask
                            {
                                GroupTaskId = groupTaskElem.TryGetProperty("groupTaskId", out var gtId) ? gtId.GetInt32() : 0,
                                GroupId = groupTaskElem.TryGetProperty("groupId", out var gId) ? gId.GetInt32() : 0,
                                TaskId = task.TaskId,
                                AssignedTo = groupTaskElem.TryGetProperty("assignedTo", out var assignedTo) && assignedTo.ValueKind == JsonValueKind.Number 
                                    ? assignedTo.GetInt32() : null,
                                AssignedBy = groupTaskElem.TryGetProperty("assignedBy", out var assignedBy) && assignedBy.ValueKind == JsonValueKind.Number 
                                    ? assignedBy.GetInt32() : null,
                                AssignedAt = groupTaskElem.TryGetProperty("assignedAt", out var assignedAt) && !string.IsNullOrEmpty(assignedAt.GetString())
                                    ? DateTime.Parse(assignedAt.GetString()!) : null
                            };
                            
                           
                            if (groupTaskElem.TryGetProperty("groupName", out var groupNameElem) && groupNameElem.ValueKind == JsonValueKind.String)
                            {
                               
                                task.GroupTask.Group = new Group { GroupName = groupNameElem.GetString() ?? "" };
                            }
                            
                            if (groupTaskElem.TryGetProperty("assignedToUsername", out var atUsername) && atUsername.ValueKind == JsonValueKind.String)
                            {
                                var username = atUsername.GetString();
                                var fullName = groupTaskElem.TryGetProperty("assignedToFullName", out var atFullName) && atFullName.ValueKind == JsonValueKind.String
                                    ? atFullName.GetString() : null;
                                
                                task.GroupTask.AssignedUser = new User 
                                { 
                                    Username = username ?? "", 
                                    FullName = fullName 
                                };
                            }
                        }

                        tasks.Add(task);
                    }

                    return tasks;
                }
                else
                {
                    string errorMsg = GetErrorMessage(root);
                    throw new Exception("Server error: " + errorMsg);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get tasks: " + ex.Message, ex);
            }
        }

        public async Task<int> CreateTaskAsync(TaskItem task, int? groupId = null)
        {
            try
            {
                var request = new
                {
                    type = "create_task",
                    token = SessionManager.Token,
                    data = new
                    {
                        title = task.Title,
                        description = task.Description,
                        dueDate = task.DueDate?.ToString("yyyy-MM-dd HH:mm:ss"),
                        priority = (int)task.Priority,
                        status = (int)task.Status,
                        categoryId = task.CategoryId,
                        isGroupTask = task.IsGroupTask,
                        groupId = groupId 
                    }
                };

                string responseJson = await SendRequestAsync(request);

                using var doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;

                string status = root.GetProperty("status").GetString();
                
                if (status == "success")
                {
                    return root.GetProperty("taskId").GetInt32();
                }
                else if (status == "validation_error")
                {
                    string field = root.TryGetProperty("field", out var f) ? f.GetString() : "";
                    string message = GetErrorMessage(root);
                    throw new Data.Exceptions.ValidationException(field, message);
                }
                else if (status == "unauthorized")
                {
                    string message = GetErrorMessage(root);
                    throw new Data.Exceptions.UnauthorizedException(message);
                }
                else
                {
                    string errorMsg = GetErrorMessage(root);
                    throw new Exception("Server error: " + errorMsg);
                }
            }
            catch (Data.Exceptions.ValidationException)
            {
                throw; 
            }
            catch (Data.Exceptions.UnauthorizedException)
            {
                throw; 
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to create task: " + ex.Message, ex);
            }
        }

        public async Task<bool> UpdateTaskAsync(TaskItem task)
        {
            try
            {
                var request = new
                {
                    type = "update_task",
                    token = SessionManager.Token,
                    data = new
                    {
                        taskId = task.TaskId,
                        title = task.Title,
                        description = task.Description,
                        dueDate = task.DueDate?.ToString("yyyy-MM-dd HH:mm:ss"),
                        priority = (int)task.Priority,
                        status = (int)task.Status,
                        categoryId = task.CategoryId
                    }
                };

                string responseJson = await SendRequestAsync(request);

                using var doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;

                string status = root.GetProperty("status").GetString();
                
                if (status == "success")
                {
                    return true;
                }
                else if (status == "validation_error")
                {
                    string field = root.TryGetProperty("field", out var f) ? f.GetString() : "";
                    string message = GetErrorMessage(root);
                    throw new Data.Exceptions.ValidationException(field, message);
                }
                else if (status == "unauthorized")
                {
                    string message = GetErrorMessage(root);
                    throw new Data.Exceptions.UnauthorizedException(message);
                }
                else
                {
                    return false;
                }
            }
            catch (Data.Exceptions.ValidationException)
            {
                throw;
            }
            catch (Data.Exceptions.UnauthorizedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to update task: " + ex.Message, ex);
            }
        }

        public async Task<bool> DeleteTaskAsync(int taskId)
        {
            try
            {
                var request = new 
                { 
                    type = "delete_task", 
                    token = SessionManager.Token,
                    taskId = taskId 
                };
                string responseJson = await SendRequestAsync(request);

                using var doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;

                string status = root.GetProperty("status").GetString();
                
                if (status == "success")
                {
                    return true;
                }
                else if (status == "unauthorized")
                {
                    string message = GetErrorMessage(root);
                    throw new Data.Exceptions.UnauthorizedException(message);
                }
                else
                {
                    return false;
                }
            }
            catch (Data.Exceptions.UnauthorizedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to delete task: " + ex.Message, ex);
            }
        }

        public async Task<bool> UpdateTaskStatusAsync(int taskId, TimeFlow.Models.TaskStatus newStatus)
        {
            try
            {
                var request = new
                {
                    type = "update_task_status",
                    token = SessionManager.Token,
                    taskId = taskId,
                    status = (int)newStatus
                };

                string responseJson = await SendRequestAsync(request);

                using var doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;

                string status = root.GetProperty("status").GetString();
                
                if (status == "success")
                {
                    return true;
                }
                else if (status == "validation_error")
                {
                    string field = root.TryGetProperty("field", out var f) ? f.GetString() : "";
                    string message = GetErrorMessage(root);
                    throw new Data.Exceptions.ValidationException(field, message);
                }
                else if (status == "unauthorized")
                {
                    string message = GetErrorMessage(root);
                    throw new Data.Exceptions.UnauthorizedException(message);
                }
                else
                {
                    return false;
                }
            }
            catch (Data.Exceptions.ValidationException)
            {
                throw;
            }
            catch (Data.Exceptions.UnauthorizedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to update task status: " + ex.Message, ex);
            }
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            try
            {
                var request = new { type = "get_categories" };
                string responseJson = await SendRequestAsync(request);

                using var doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;

                string status = root.GetProperty("status").GetString();
                if (status == "success")
                {
                    var categories = new List<Category>();
                    var dataArray = root.GetProperty("data");

                    foreach (var item in dataArray.EnumerateArray())
                    {
                        categories.Add(new Category
                        {
                            CategoryId = item.GetProperty("categoryId").GetInt32(),
                            CategoryName = item.GetProperty("categoryName").GetString() ?? "",
                            Color = item.GetProperty("color").GetString() ?? "#6B7280",
                            IconName = item.TryGetProperty("iconName", out var icon) ? icon.GetString() : null,
                            IsDefault = item.GetProperty("isDefault").GetBoolean()
                        });
                    }

                    return categories;
                }
                else
                {
                    string errorMsg = GetErrorMessage(root);
                    throw new Exception("Server error: " + errorMsg);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get categories: " + ex.Message, ex);
            }
        }

        public async Task<bool> MarkTaskAsCompletedAsync(int taskId)
        {
            return await UpdateTaskStatusAsync(taskId, TimeFlow.Models.TaskStatus.Completed);
        }

        public async Task<bool> MarkTaskAsInProgressAsync(int taskId)
        {
            return await UpdateTaskStatusAsync(taskId, TimeFlow.Models.TaskStatus.InProgress);
        }

        public async Task<TaskDetailViewModel> GetTaskDetailFullAsync(int taskId)
        {
            try
            {
                var request = new 
                { 
                    type = "get_task_detail_full",
                    token = SessionManager.Token,
                    taskId = taskId
                };
                
                string responseJson = await SendRequestAsync(request);

                using var doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;

                string status = root.GetProperty("status").GetString();
                if (status == "success")
                {
                    var data = root.GetProperty("data");
                    
                    var taskDetail = new TaskDetailViewModel
                    {
                        TaskId = data.GetProperty("taskId").GetInt32(),
                        Title = data.GetProperty("title").GetString() ?? "",
                        Description = data.TryGetProperty("description", out var desc) ? desc.GetString() : null,
                        DueDate = data.TryGetProperty("dueDate", out var dueDate) && !string.IsNullOrEmpty(dueDate.GetString())
                            ? DateTime.Parse(dueDate.GetString()!) : null,
                        Priority = (TaskPriority)data.GetProperty("priority").GetInt32(),
                        Status = (TimeFlow.Models.TaskStatus)data.GetProperty("status").GetInt32(),
                        IsGroupTask = data.GetProperty("isGroupTask").GetBoolean(),
                        CreatedBy = data.GetProperty("createdBy").GetInt32(),
                        CompletedAt = data.TryGetProperty("completedAt", out var completedAt) && !string.IsNullOrEmpty(completedAt.GetString())
                            ? DateTime.Parse(completedAt.GetString()!) : null,
                        CreatedAt = DateTime.Parse(data.GetProperty("createdAt").GetString()!),
                        UpdatedAt = data.TryGetProperty("updatedAt", out var updatedAt) && !string.IsNullOrEmpty(updatedAt.GetString())
                            ? DateTime.Parse(updatedAt.GetString()!) : null,
                        
                      
                        CategoryName = data.GetProperty("categoryName").GetString() ?? "Other",
                        CategoryColor = data.GetProperty("categoryColor").GetString() ?? "#6B7280",
                        Progress = data.GetProperty("progress").GetInt32()
                    };

                
                    if (data.TryGetProperty("assignees", out var assignees))
                    {
                        foreach (var assignee in assignees.EnumerateArray())
                        {
                            taskDetail.Assignees.Add(assignee.GetString() ?? "");
                        }
                    }

                   
                    if (data.TryGetProperty("comments", out var comments))
                    {
                        foreach (var comment in comments.EnumerateArray())
                        {
                            taskDetail.Comments.Add(new CommentViewModel
                            {
                                CommentId = comment.GetProperty("commentId").GetInt32(),
                                UserId = comment.GetProperty("userId").GetInt32(),
                                Username = comment.TryGetProperty("username", out var username) ? username.GetString() : null,
                                FullName = comment.TryGetProperty("fullName", out var fullName) ? fullName.GetString() : null,
                                Content = comment.GetProperty("content").GetString() ?? "",
                                CreatedAt = DateTime.Parse(comment.GetProperty("createdAt").GetString()!),
                                IsEdited = comment.GetProperty("isEdited").GetBoolean()
                            });
                        }
                    }

                  
                    if (data.TryGetProperty("activities", out var activities))
                    {
                        foreach (var activity in activities.EnumerateArray())
                        {
                            taskDetail.Activities.Add(new ActivityViewModel
                            {
                                LogId = activity.GetProperty("logId").GetInt32(),
                                UserId = activity.GetProperty("userId").GetInt32(),
                                ActivityType = activity.GetProperty("activityType").GetString() ?? "",
                                Description = activity.GetProperty("description").GetString() ?? "",
                                CreatedAt = DateTime.Parse(activity.GetProperty("createdAt").GetString()!)
                            });
                        }
                    }

                    return taskDetail;
                }
                else
                {
                    string errorMsg = GetErrorMessage(root);
                    throw new Exception("Server error: " + errorMsg);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get task detail: " + ex.Message, ex);
            }
        }

        public async Task<List<Group>> GetMyGroupsAsync()
        {
            try
            {
                var request = new
                {
                    type = "get_my_groups",
                    token = SessionManager.Token
                };

                string responseJson = await SendRequestAsync(request);
                using var doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;

               
                if (root.TryGetProperty("type", out var t) && t.GetString() == "my_groups_list")
                {
                    if (root.GetProperty("status").GetString() == "success")
                    {
                        var list = new List<Group>();
                        var data = root.GetProperty("data");
                        foreach (var item in data.EnumerateArray())
                        {
                            var g = new Group
                            {
                                GroupId = item.GetProperty("groupId").GetInt32(),
                                GroupName = item.GetProperty("groupName").GetString() ?? string.Empty,
                                Description = item.TryGetProperty("description", out var d) ? d.GetString() : null
                            };
                            list.Add(g);
                        }
                        return list;
                    }
                }

                if (root.TryGetProperty("status", out var statusElem) && statusElem.GetString() == "success")
                {
                    var list = new List<Group>();
                    var data = root.GetProperty("data");
                    foreach (var item in data.EnumerateArray())
                    {
                        var g = new Group
                        {
                            GroupId = item.GetProperty("groupId").GetInt32(),
                            GroupName = item.GetProperty("groupName").GetString() ?? string.Empty,
                            Description = item.TryGetProperty("description", out var d) ? d.GetString() : null
                        };
                        list.Add(g);
                    }
                    return list;
                }

                string err = GetErrorMessage(root);
                throw new Exception("Server error: " + err);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get groups: " + ex.Message, ex);
            }
        }

        public async Task<int> CreateGroupAsync(string groupName, string description)
        {
            try
            {
                var request = new
                {
                    type = "create_group",
                    token = SessionManager.Token,
                    data = new
                    {
                        groupName = groupName,
                        description = description
                    }
                };

                string responseJson = await SendRequestAsync(request);
                using var doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;

                if (root.TryGetProperty("status", out var statusElem))
                {
                    var status = statusElem.GetString();
                    if (status == "success")
                    {
                        if (root.TryGetProperty("groupId", out var idElem))
                        {
                            return idElem.GetInt32();
                        }
                        if (root.TryGetProperty("data", out var data) && data.TryGetProperty("groupId", out var gElem))
                        {
                            return gElem.GetInt32();
                        }
                        return 1; 
                    }
                    else if (status == "error")
                    {
                        string msg = GetErrorMessage(root);
                        throw new Exception(msg);
                    }
                }

                string errMsg = GetErrorMessage(root);
                throw new Exception(errMsg);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to create group: " + ex.Message, ex);
            }
        }

        public async Task<bool> AddGroupMemberAsync(int groupId, string username)
        {
            try
            {
                var request = new
                {
                    type = "add_group_member",
                    token = SessionManager.Token,
                    data = new
                    {
                        groupId = groupId,
                        username = username
                    }
                };

                string responseJson = await SendRequestAsync(request);
                using var doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;

                if (root.TryGetProperty("status", out var statusElem))
                {
                    var status = statusElem.GetString();
                    if (status == "success")
                    {
                        return true;
                    }
                    else if (status == "error")
                    {
                        string msg = GetErrorMessage(root);
                        throw new Exception(msg);
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to add member: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// lay danh sach thanh vien cua group
        /// </summary>
        public async Task<List<GroupMemberDto>> GetGroupMembersAsync(int groupId)
        {
            try
            {
                var request = new
                {
                    type = "get_group_members",
                    token = SessionManager.Token,
                    groupId = groupId
                };

                string responseJson = await SendRequestAsync(request);
                using var doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;

                if (root.TryGetProperty("status", out var statusElem) && statusElem.GetString() == "success")
                {
                    var list = new List<GroupMemberDto>();
                    var data = root.GetProperty("data");
                    
                    foreach (var item in data.EnumerateArray())
                    {
                        list.Add(new GroupMemberDto
                        {
                            UserId = item.GetProperty("userId").GetInt32(),
                            Username = item.GetProperty("username").GetString() ?? "",
                            FullName = item.TryGetProperty("fullName", out var fn) ? fn.GetString() : null,
                            Role = item.TryGetProperty("role", out var r) ? r.GetString() ?? "Member" : "Member"
                        });
                    }
                    return list;
                }

                string err = GetErrorMessage(root);
                throw new Exception("Server error: " + err);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get group members: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// tao Group Task voi day du thong tin tin bao gom assignee
        /// </summary>
        public async Task<int> CreateGroupTaskAsync(TaskItem task, int groupId, int? assignedTo = null)
        {
            try
            {
                var request = new
                {
                    type = "create_task",
                    token = SessionManager.Token,
                    data = new
                    {
                        title = task.Title,
                        description = task.Description,
                        dueDate = task.DueDate?.ToString("yyyy-MM-dd HH:mm:ss"),
                        priority = (int)task.Priority,
                        status = (int)task.Status,
                        categoryId = task.CategoryId,
                        isGroupTask = true,
                        groupId = groupId,
                        assignedTo = assignedTo
                    }
                };

                string responseJson = await SendRequestAsync(request);

                using var doc = JsonDocument.Parse(responseJson);
                var root = doc.RootElement;

                string status = root.GetProperty("status").GetString();
                
                if (status == "success")
                {
                    return root.GetProperty("taskId").GetInt32();
                }
                else if (status == "validation_error")
                {
                    string field = root.TryGetProperty("field", out var f) ? f.GetString() : "";
                    string message = GetErrorMessage(root);
                    throw new Data.Exceptions.ValidationException(field, message);
                }
                else if (status == "unauthorized")
                {
                    string message = GetErrorMessage(root);
                    throw new Data.Exceptions.UnauthorizedException(message);
                }
                else
                {
                    string errorMsg = GetErrorMessage(root);
                    throw new Exception("Server error: " + errorMsg);
                }
            }
            catch (Data.Exceptions.ValidationException) { throw; }
            catch (Data.Exceptions.UnauthorizedException) { throw; }
            catch (Exception ex)
            {
                throw new Exception("Failed to create group task: " + ex.Message, ex);
            }
        }
    }
}
