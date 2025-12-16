using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using TimeFlow.Models;

namespace TimeFlow.Data.Repositories
{
    // Repository xu ly Tasks table
    public class TaskRepository : BaseRepository
    {
        public TaskRepository() : base() { }
        public TaskRepository(DatabaseHelper db) : base(db) { }

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

        // ================== READ OPERATIONS ==================

        // Lay task theo ID
        public TaskItem? GetById(int taskId)
        {
            var query = "SELECT * FROM Tasks WHERE TaskId = @id";
            var parameters = CreateParameters(("@id", taskId));
            
            var row = GetSingleRow(query, parameters);
            return row != null ? MapToTask(row) : null;
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

        // Tao task moi
        public int Create(TaskItem task)
        {
            var query = @"INSERT INTO Tasks 
                         (Title, Description, DueDate, Priority, Status, CategoryId, 
                          CreatedBy, IsGroupTask, CreatedAt)
                         VALUES 
                         (@title, @desc, @due, @priority, @status, @category, 
                          @createdBy, @isGroup, GETDATE())";
            
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
            
            return InsertAndGetId(query, parameters);
        }

        // ================== UPDATE OPERATIONS ==================

        // Cap nhat task
        public bool Update(TaskItem task)
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
            
            return Execute(query, parameters) > 0;
        }

        // Cap nhat status
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

        // Xoa task
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
