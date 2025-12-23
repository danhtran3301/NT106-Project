using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using TimeFlow.Models;
using TimeFlow.Data.Exceptions;

namespace TimeFlow.Data.Repositories
{
    // Repository xu ly Tasks table
    public class TaskRepository : BaseRepository
    {
        private readonly ActivityLogRepository _activityLogRepo;
        private readonly CommentRepository _commentRepo;
        private readonly GroupTaskRepository _groupTaskRepo;
        private readonly GroupMemberRepository _groupMemberRepo;

        public TaskRepository() : base() 
        {
            _activityLogRepo = new ActivityLogRepository(_db);
            _commentRepo = new CommentRepository(_db);
            _groupTaskRepo = new GroupTaskRepository(_db);
            _groupMemberRepo = new GroupMemberRepository(_db);
        }

        public TaskRepository(DatabaseHelper db) : base(db) 
        {
            _activityLogRepo = new ActivityLogRepository(db);
            _commentRepo = new CommentRepository(db);
            _groupTaskRepo = new GroupTaskRepository(db);
            _groupMemberRepo = new GroupMemberRepository(db);
        }

        // ================== MAPPING ==================

        private TaskItem MapToTask(DataRow row)
        {
            return new TaskItem
            {
                TaskId = GetValue<int>(row, "TaskId"),
                Title = GetValue<string>(row, "Title", string.Empty),
                Description = GetString(row, "Description"),
                DueDate = GetNullableValue<DateTime>(row, "DueDate"),
                Priority = (TaskPriority)GetValue<int>(row, "Priority", 2),
                Status = (Models.TaskStatus)GetValue<int>(row, "Status", 1),
                CategoryId = GetNullableValue<int>(row, "CategoryId"),
                CreatedBy = GetValue<int>(row, "CreatedBy"),
                IsGroupTask = GetValue<bool>(row, "IsGroupTask", false),
                CompletedAt = GetNullableValue<DateTime>(row, "CompletedAt"),
                CreatedAt = GetValue<DateTime>(row, "CreatedAt"),
                UpdatedAt = GetNullableValue<DateTime>(row, "UpdatedAt")
            };
        }

        // ================== VALIDATION ==================

        /// <summary>
        /// Validate task data truoc khi insert/update
        /// </summary>
        private void ValidateTask(TaskItem task, bool isUpdate = false)
        {
            // Validate Title
            if (string.IsNullOrWhiteSpace(task.Title))
            {
                throw new ValidationException("Title", task.Title, "Task title is required and cannot be empty");
            }

            if (task.Title.Length > 200)
            {
                throw new ValidationException("Title", task.Title, "Task title cannot exceed 200 characters");
            }

            // Validate Priority
            if (!Enum.IsDefined(typeof(TaskPriority), task.Priority))
            {
                throw new ValidationException("Priority", task.Priority, "Invalid priority value");
            }

            // Validate Status
            if (!Enum.IsDefined(typeof(Models.TaskStatus), task.Status))
            {
                throw new ValidationException("Status", task.Status, "Invalid status value");
            }

            // Validate DueDate (neu co)
            if (task.DueDate.HasValue && !isUpdate)
            {
                // Chi check qua khu khi tao moi
                if (task.DueDate.Value < DateTime.Now.AddHours(-24))
                {
                    throw new ValidationException("DueDate", task.DueDate, 
                        "Due date cannot be more than 24 hours in the past");
                }
            }

            // Validate CreatedBy
            if (task.CreatedBy <= 0)
            {
                throw new ValidationException("CreatedBy", task.CreatedBy, "Invalid user ID");
            }

            // Check user exists
            if (!Exists("Users", "UserId = @userId AND IsActive = 1", 
                CreateParameters(("@userId", task.CreatedBy))))
            {
                throw new ValidationException("CreatedBy", task.CreatedBy, 
                    $"User with ID {task.CreatedBy} does not exist or is inactive");
            }

            // Validate CategoryId (neu co)
            if (task.CategoryId.HasValue && task.CategoryId.Value > 0)
            {
                if (!Exists("Categories", "CategoryId = @categoryId", 
                    CreateParameters(("@categoryId", task.CategoryId.Value))))
                {
                    throw new ValidationException("CategoryId", task.CategoryId, 
                        $"Category with ID {task.CategoryId} does not exist");
                }
            }

            // Validate Description length
            if (!string.IsNullOrEmpty(task.Description) && task.Description.Length > 5000)
            {
                throw new ValidationException("Description", null, 
                    "Task description cannot exceed 5000 characters");
            }
        }

        // ================== AUTHORIZATION ==================

        /// <summary>
        /// Kiem tra user co quyen edit task khong
        /// </summary>
        public bool CanUserEdit(int taskId, int userId)
        {
            var task = GetById(taskId);
            if (task == null) return false;

            // Creator luon co quyen edit
            if (task.CreatedBy == userId) return true;

            // Neu la group task, check membership
            if (task.IsGroupTask)
            {
                var groupTask = _groupTaskRepo.GetByTaskId(taskId);
                if (groupTask != null)
                {
                    return _groupMemberRepo.IsUserInGroup(userId, groupTask.GroupId);
                }
            }

            return false;
        }

        /// <summary>
        /// Kiem tra user co quyen delete task khong
        /// </summary>
        public bool CanUserDelete(int taskId, int userId)
        {
            var task = GetById(taskId);
            if (task == null) return false;

            // Chi creator moi co quyen xoa
            return task.CreatedBy == userId;
        }

        /// <summary>
        /// Kiem tra user co quyen view task khong
        /// </summary>
        public bool CanUserView(int taskId, int userId)
        {
            var task = GetById(taskId);
            if (task == null) return false;

            // Creator luon co quyen view
            if (task.CreatedBy == userId) return true;

            // Neu la group task, check membership
            if (task.IsGroupTask)
            {
                var groupTask = _groupTaskRepo.GetByTaskId(taskId);
                if (groupTask != null)
                {
                    return _groupMemberRepo.IsUserInGroup(userId, groupTask.GroupId);
                }
            }

            return false;
        }

        // ================== READ OPERATIONS ==================

        // Lay task theo ID
        public TaskItem? GetById(int taskId)
        {
            var query = "SELECT * FROM Tasks WHERE TaskId = @id";
            var parameters = CreateParameters(("@id", taskId));
            
            var row = GetSingleRow(query, parameters);
            return row != null ? MapToTask(row) : null;
        }

        /// <summary>
        /// Lay task theo ID voi authorization check
        /// </summary>
        public TaskItem? GetById(int taskId, int requestingUserId)
        {
            var task = GetById(taskId);
            
            if (task == null) return null;

            if (!CanUserView(taskId, requestingUserId))
            {
                throw new UnauthorizedException(requestingUserId, "view", taskId, 
                    $"User {requestingUserId} does not have permission to view task {taskId}");
            }

            return task;
        }

        // Lay tat ca tasks cua user
        public List<TaskItem> GetByUserId(int userId)
        {
            var query = @"SELECT * FROM Tasks 
                         WHERE CreatedBy = @userId 
                         ORDER BY DueDate ASC, Priority DESC";
            
            var parameters = CreateParameters(("@userId", userId));
            var rows = GetRows(query, parameters);
            
            var tasks = new List<TaskItem>();
            foreach (DataRow row in rows)
            {
                tasks.Add(MapToTask(row));
            }
            return tasks;
        }

        // Lay tasks theo status
        public List<TaskItem> GetByStatus(int userId, Models.TaskStatus status)
        {
            var query = @"SELECT * FROM Tasks 
                         WHERE CreatedBy = @userId 
                         AND Status = @status
                         ORDER BY DueDate ASC, Priority DESC";
            
            var parameters = CreateParameters(
                ("@userId", userId),
                ("@status", (int)status)
            );
            var rows = GetRows(query, parameters);
            
            var tasks = new List<TaskItem>();
            foreach (DataRow row in rows)
            {
                tasks.Add(MapToTask(row));
            }
            return tasks;
        }

        // Lay tasks theo priority
        public List<TaskItem> GetByPriority(int userId, TaskPriority priority)
        {
            var query = @"SELECT * FROM Tasks 
                         WHERE CreatedBy = @userId 
                         AND Priority = @priority
                         ORDER BY DueDate ASC";
            
            var parameters = CreateParameters(
                ("@userId", userId),
                ("@priority", (int)priority)
            );
            var rows = GetRows(query, parameters);
            
            var tasks = new List<TaskItem>();
            foreach (DataRow row in rows)
            {
                tasks.Add(MapToTask(row));
            }
            return tasks;
        }

        // Lay tasks theo category
        public List<TaskItem> GetByCategory(int userId, int categoryId)
        {
            var query = @"SELECT * FROM Tasks 
                         WHERE CreatedBy = @userId 
                         AND CategoryId = @categoryId
                         ORDER BY DueDate ASC, Priority DESC";
            
            var parameters = CreateParameters(
                ("@userId", userId),
                ("@categoryId", categoryId)
            );
            var rows = GetRows(query, parameters);
            
            var tasks = new List<TaskItem>();
            foreach (DataRow row in rows)
            {
                tasks.Add(MapToTask(row));
            }
            return tasks;
        }

        // Lay cac tasks qua han
        public List<TaskItem> GetOverdueTasks(int userId)
        {
            var query = @"SELECT * FROM Tasks 
                         WHERE CreatedBy = @userId 
                         AND DueDate < GETDATE()
                         AND Status NOT IN (3, 4)
                         ORDER BY DueDate ASC";
            
            var parameters = CreateParameters(("@userId", userId));
            var rows = GetRows(query, parameters);
            
            var tasks = new List<TaskItem>();
            foreach (DataRow row in rows)
            {
                tasks.Add(MapToTask(row));
            }
            return tasks;
        }

        // Lay tasks hom nay
        public List<TaskItem> GetTasksToday(int userId)
        {
            var query = @"SELECT * FROM Tasks 
                         WHERE CreatedBy = @userId 
                         AND CAST(DueDate AS DATE) = CAST(GETDATE() AS DATE)
                         ORDER BY Priority DESC";
            
            var parameters = CreateParameters(("@userId", userId));
            var rows = GetRows(query, parameters);
            
            var tasks = new List<TaskItem>();
            foreach (DataRow row in rows)
            {
                tasks.Add(MapToTask(row));
            }
            return tasks;
        }

        // Lay tasks tuan nay
        public List<TaskItem> GetTasksThisWeek(int userId)
        {
            var query = @"SELECT * FROM Tasks 
                         WHERE CreatedBy = @userId 
                         AND DueDate >= DATEADD(day, 1-DATEPART(weekday, GETDATE()), CAST(GETDATE() AS DATE))
                         AND DueDate < DATEADD(day, 8-DATEPART(weekday, GETDATE()), CAST(GETDATE() AS DATE))
                         ORDER BY DueDate ASC, Priority DESC";
            
            var parameters = CreateParameters(("@userId", userId));
            var rows = GetRows(query, parameters);
            
            var tasks = new List<TaskItem>();
            foreach (DataRow row in rows)
            {
                tasks.Add(MapToTask(row));
            }
            return tasks;
        }

        // Tim kiem tasks
        public List<TaskItem> Search(int userId, string keyword)
        {
            var query = @"SELECT * FROM Tasks 
                         WHERE CreatedBy = @userId 
                         AND (Title LIKE @keyword OR Description LIKE @keyword)
                         ORDER BY DueDate ASC, Priority DESC";
            
            var parameters = CreateParameters(
                ("@userId", userId),
                ("@keyword", $"%{keyword}%")
            );
            var rows = GetRows(query, parameters);
            
            var tasks = new List<TaskItem>();
            foreach (DataRow row in rows)
            {
                tasks.Add(MapToTask(row));
            }
            return tasks;
        }

        // ================== CREATE OPERATIONS ==================

        /// <summary>
        /// Tao task moi voi validation va transaction
        /// </summary>
        public int Create(TaskItem task)
        {
            // Step 1: Validate
            ValidateTask(task);

            // Step 2: Create trong transaction
            return ExecuteInTransaction((conn, trans) =>
            {
                // Insert task
                var query = @"INSERT INTO Tasks 
                             (Title, Description, DueDate, Priority, Status, CategoryId, 
                              CreatedBy, IsGroupTask, CreatedAt)
                             VALUES 
                             (@title, @desc, @due, @priority, @status, @category, 
                              @createdBy, @isGroup, GETDATE());
                             SELECT CAST(SCOPE_IDENTITY() AS INT);";
                
                var parameters = CreateParameters(
                    ("@title", task.Title),
                    ("@desc", task.Description),
                    ("@due", task.DueDate),
                    ("@priority", (int)task.Priority),
                    ("@status", (int)task.Status),
                    ("@category", task.CategoryId),
                    ("@createdBy", task.CreatedBy),
                    ("@isGroup", task.IsGroupTask)
                );

                using var cmd = new SqlCommand(query, conn, trans);
                cmd.Parameters.AddRange(parameters);
                var taskId = (int)cmd.ExecuteScalar();

                // Log activity
                var logQuery = @"INSERT INTO ActivityLog 
                                (TaskId, UserId, ActivityType, ActivityDescription, CreatedAt)
                                VALUES (@taskId, @userId, @type, @desc, GETDATE())";
                
                var logParams = CreateParameters(
                    ("@taskId", taskId),
                    ("@userId", task.CreatedBy),
                    ("@type", "CreateTask"),
                    ("@desc", $"Created task: {task.Title}")
                );

                using var logCmd = new SqlCommand(logQuery, conn, trans);
                logCmd.Parameters.AddRange(logParams);
                logCmd.ExecuteNonQuery();

                return taskId;
            });
        }

        // ================== UPDATE OPERATIONS ==================

        /// <summary>
        /// Cap nhat task voi authorization va validation
        /// </summary>
        public bool Update(TaskItem task, int requestingUserId)
        {
            // Authorization check
            if (!CanUserEdit(task.TaskId, requestingUserId))
            {
                throw new UnauthorizedException(requestingUserId, "update", task.TaskId,
                    $"User {requestingUserId} does not have permission to update task {task.TaskId}");
            }

            // Validate
            ValidateTask(task, isUpdate: true);

            // Update trong transaction
            return ExecuteInTransaction((conn, trans) =>
            {
                var query = @"UPDATE Tasks SET 
                             Title = @title,
                             Description = @desc,
                             DueDate = @due,
                             Priority = @priority,
                             Status = @status,
                             CategoryId = @category,
                             UpdatedAt = GETDATE()
                             WHERE TaskId = @id";
                
                var parameters = CreateParameters(
                    ("@id", task.TaskId),
                    ("@title", task.Title),
                    ("@desc", task.Description),
                    ("@due", task.DueDate),
                    ("@priority", (int)task.Priority),
                    ("@status", (int)task.Status),
                    ("@category", task.CategoryId)
                );

                using var cmd = new SqlCommand(query, conn, trans);
                cmd.Parameters.AddRange(parameters);
                var rowsAffected = cmd.ExecuteNonQuery();

                // Log activity
                if (rowsAffected > 0)
                {
                    var logQuery = @"INSERT INTO ActivityLog 
                                    (TaskId, UserId, ActivityType, ActivityDescription, CreatedAt)
                                    VALUES (@taskId, @userId, @type, @desc, GETDATE())";
                    
                    var logParams = CreateParameters(
                        ("@taskId", task.TaskId),
                        ("@userId", requestingUserId),
                        ("@type", "UpdateTask"),
                        ("@desc", $"Updated task: {task.Title}")
                    );

                    using var logCmd = new SqlCommand(logQuery, conn, trans);
                    logCmd.Parameters.AddRange(logParams);
                    logCmd.ExecuteNonQuery();
                }

                return rowsAffected > 0;
            });
        }

        /// <summary>
        /// Cap nhat task (backward compatible)
        /// </summary>
        public bool Update(TaskItem task)
        {
            ValidateTask(task, isUpdate: true);

            var query = @"UPDATE Tasks SET 
                         Title = @title,
                         Description = @desc,
                         DueDate = @due,
                         Priority = @priority,
                         Status = @status,
                         CategoryId = @category,
                         UpdatedAt = GETDATE()
                         WHERE TaskId = @id";
            
            var parameters = CreateParameters(
                ("@id", task.TaskId),
                ("@title", task.Title),
                ("@desc", task.Description),
                ("@due", task.DueDate),
                ("@priority", (int)task.Priority),
                ("@status", (int)task.Status),
                ("@category", task.CategoryId)
            );
            
            return Execute(query, parameters) > 0;
        }

        /// <summary>
        /// Cap nhat status voi transaction va logging
        /// </summary>
        public bool UpdateStatus(int taskId, Models.TaskStatus status, int requestingUserId)
        {
            // Authorization check
            if (!CanUserEdit(taskId, requestingUserId))
            {
                throw new UnauthorizedException(requestingUserId, "update_status", taskId,
                    $"User {requestingUserId} does not have permission to update task {taskId} status");
            }

            // Validate status
            if (!Enum.IsDefined(typeof(Models.TaskStatus), status))
            {
                throw new ValidationException("Status", status, "Invalid status value");
            }

            return ExecuteInTransaction((conn, trans) =>
            {
                var query = @"UPDATE Tasks SET 
                             Status = @status,
                             UpdatedAt = GETDATE(),
                             CompletedAt = CASE WHEN @status = 3 THEN GETDATE() ELSE NULL END
                             WHERE TaskId = @id";
                
                var parameters = CreateParameters(
                    ("@id", taskId),
                    ("@status", (int)status)
                );

                using var cmd = new SqlCommand(query, conn, trans);
                cmd.Parameters.AddRange(parameters);
                var rowsAffected = cmd.ExecuteNonQuery();

                // Log activity
                if (rowsAffected > 0)
                {
                    var statusText = status.ToString();

                    var logQuery = @"INSERT INTO ActivityLog 
                                    (TaskId, UserId, ActivityType, ActivityDescription, CreatedAt)
                                    VALUES (@taskId, @userId, @type, @desc, GETDATE())";
                    
                    var logParams = CreateParameters(
                        ("@taskId", taskId),
                        ("@userId", requestingUserId),
                        ("@type", "UpdateTaskStatus"),
                        ("@desc", $"Changed status to {statusText}")
                    );

                    using var logCmd = new SqlCommand(logQuery, conn, trans);
                    logCmd.Parameters.AddRange(logParams);
                    logCmd.ExecuteNonQuery();
                }

                return rowsAffected > 0;
            });
        }

        /// <summary>
        /// Cap nhat status (backward compatible)
        /// </summary>
        public bool UpdateStatus(int taskId, Models.TaskStatus status)
        {
            var query = @"UPDATE Tasks SET 
                         Status = @status,
                         UpdatedAt = GETDATE(),
                         CompletedAt = CASE WHEN @status = 3 THEN GETDATE() ELSE NULL END
                         WHERE TaskId = @id";
            
            var parameters = CreateParameters(
                ("@id", taskId),
                ("@status", (int)status)
            );
            
            return Execute(query, parameters) > 0;
        }

        // Danh dau task hoan thanh
        public bool MarkAsCompleted(int taskId)
        {
            return UpdateStatus(taskId, Models.TaskStatus.Completed);
        }

        /// <summary>
        /// Danh dau task hoan thanh voi auth check
        /// </summary>
        public bool MarkAsCompleted(int taskId, int requestingUserId)
        {
            return UpdateStatus(taskId, Models.TaskStatus.Completed, requestingUserId);
        }

        // Cap nhat priority
        public bool UpdatePriority(int taskId, TaskPriority priority)
        {
            var query = @"UPDATE Tasks SET 
                         Priority = @priority,
                         UpdatedAt = GETDATE()
                         WHERE TaskId = @id";
            
            var parameters = CreateParameters(
                ("@id", taskId),
                ("@priority", (int)priority)
            );
            
            return Execute(query, parameters) > 0;
        }

        // ================== DELETE OPERATIONS ==================

        /// <summary>
        /// Xoa task voi cascade delete va transaction
        /// </summary>
        public bool DeleteWithCascade(int taskId, int requestingUserId)
        {
            // Authorization check
            if (!CanUserDelete(taskId, requestingUserId))
            {
                throw new UnauthorizedException(requestingUserId, "delete", taskId,
                    $"User {requestingUserId} does not have permission to delete task {taskId}");
            }

            // Get task info truoc khi xoa
            var task = GetById(taskId);
            if (task == null) return false;

            // Delete trong transaction voi cascade
            return ExecuteInTransaction((conn, trans) =>
            {
                // Log activity TRUOC KHI xoa
                var logQuery = @"INSERT INTO ActivityLog 
                                (TaskId, UserId, ActivityType, ActivityDescription, CreatedAt)
                                VALUES (@taskId, @userId, @type, @desc, GETDATE())";
                
                var logParams = CreateParameters(
                    ("@taskId", taskId),
                    ("@userId", requestingUserId),
                    ("@type", "DeleteTask"),
                    ("@desc", $"Deleted task: {task.Title}")
                );

                using var logCmd = new SqlCommand(logQuery, conn, trans);
                logCmd.Parameters.AddRange(logParams);
                logCmd.ExecuteNonQuery();

                // Delete Comments
                var deleteCommentsQuery = "DELETE FROM Comments WHERE TaskId = @taskId";
                using var delCommentsCmd = new SqlCommand(deleteCommentsQuery, conn, trans);
                delCommentsCmd.Parameters.AddWithValue("@taskId", taskId);
                delCommentsCmd.ExecuteNonQuery();

                // Delete ActivityLogs (tru cai vua log)
                var deleteLogsQuery = "DELETE FROM ActivityLog WHERE TaskId = @taskId AND ActivityType != 'DeleteTask'";
                using var delLogsCmd = new SqlCommand(deleteLogsQuery, conn, trans);
                delLogsCmd.Parameters.AddWithValue("@taskId", taskId);
                delLogsCmd.ExecuteNonQuery();

                // Delete GroupTasks
                var deleteGroupTaskQuery = "DELETE FROM GroupTasks WHERE TaskId = @taskId";
                using var delGroupTaskCmd = new SqlCommand(deleteGroupTaskQuery, conn, trans);
                delGroupTaskCmd.Parameters.AddWithValue("@taskId", taskId);
                delGroupTaskCmd.ExecuteNonQuery();

                // Delete Task
                var deleteTaskQuery = "DELETE FROM Tasks WHERE TaskId = @taskId";
                using var delTaskCmd = new SqlCommand(deleteTaskQuery, conn, trans);
                delTaskCmd.Parameters.AddWithValue("@taskId", taskId);
                var rowsAffected = delTaskCmd.ExecuteNonQuery();

                return rowsAffected > 0;
            });
        }

        /// <summary>
        /// Xoa task (backward compatible)
        /// </summary>
        public bool Delete(int taskId)
        {
            var query = "DELETE FROM Tasks WHERE TaskId = @id";
            var parameters = CreateParameters(("@id", taskId));
            
            return Execute(query, parameters) > 0;
        }

        // ================== STATISTICS ==================

        // Dem tasks theo status
        public int CountByStatus(int userId, Models.TaskStatus status)
        {
            var query = "SELECT COUNT(*) FROM Tasks WHERE CreatedBy = @userId AND Status = @status";
            var parameters = CreateParameters(
                ("@userId", userId),
                ("@status", (int)status)
            );
            
            var result = _db.ExecuteScalar(query, parameters);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        // Dem tasks qua han
        public int CountOverdue(int userId)
        {
            var query = @"SELECT COUNT(*) FROM Tasks 
                         WHERE CreatedBy = @userId 
                         AND DueDate < GETDATE()
                         AND Status NOT IN (3, 4)";
            
            var parameters = CreateParameters(("@userId", userId));
            
            var result = _db.ExecuteScalar(query, parameters);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        // Dem tong so tasks
        public int CountTotal(int userId)
        {
            var query = "SELECT COUNT(*) FROM Tasks WHERE CreatedBy = @userId";
            var parameters = CreateParameters(("@userId", userId));
            
            var result = _db.ExecuteScalar(query, parameters);
            return result != null ? Convert.ToInt32(result) : 0;
        }
    }
}
