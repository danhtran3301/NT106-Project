using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TimeFlow.Models;

namespace TimeFlow.Services
{
    public class TaskApiClient
    {
        private readonly string _serverHost;
        private readonly int _serverPort;
        private readonly int _timeout;

        public TaskApiClient(string serverHost = "127.0.0.1", int serverPort = 1010, int timeout = 5000)
        {
            _serverHost = serverHost;
            _serverPort = serverPort;
            _timeout = timeout;
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

                    byte[] buffer = new byte[8192];
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    string responseJson = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();

                    return responseJson;
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
                // Gửi token để authenticate
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
                        tasks.Add(new TaskItem
                        {
                            TaskId = item.GetProperty("taskId").GetInt32(),
                            Title = item.GetProperty("title").GetString() ?? "",
                            Description = item.TryGetProperty("description", out var desc) ? desc.GetString() : null,
                            DueDate = item.TryGetProperty("dueDate", out var dueDate) && !string.IsNullOrEmpty(dueDate.GetString())
                                ? DateTime.Parse(dueDate.GetString()!) : null,
                            Priority = (TaskPriority)item.GetProperty("priority").GetInt32(),
                            Status = (TimeFlow.Models.TaskStatus)item.GetProperty("status").GetInt32(),
                            CategoryId = item.TryGetProperty("categoryId", out var catId) ? catId.GetInt32() : null,
                            IsGroupTask = item.GetProperty("isGroupTask").GetBoolean(),
                            CreatedAt = DateTime.Parse(item.GetProperty("createdAt").GetString()!),
                            UpdatedAt = item.TryGetProperty("updatedAt", out var updatedAt) && !string.IsNullOrEmpty(updatedAt.GetString())
                                ? DateTime.Parse(updatedAt.GetString()!) : null
                        });
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

        public async Task<int> CreateTaskAsync(TaskItem task)
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
                        isGroupTask = task.IsGroupTask
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
                throw; // Re-throw to preserve exception type
            }
            catch (Data.Exceptions.UnauthorizedException)
            {
                throw; // Re-throw to preserve exception type
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
                        
                        // Extended info
                        CategoryName = data.GetProperty("categoryName").GetString() ?? "Other",
                        CategoryColor = data.GetProperty("categoryColor").GetString() ?? "#6B7280",
                        Progress = data.GetProperty("progress").GetInt32()
                    };

                    // Parse assignees
                    if (data.TryGetProperty("assignees", out var assignees))
                    {
                        foreach (var assignee in assignees.EnumerateArray())
                        {
                            taskDetail.Assignees.Add(assignee.GetString() ?? "");
                        }
                    }

                    // Parse comments
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

                    // Parse activities
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
    }
}
