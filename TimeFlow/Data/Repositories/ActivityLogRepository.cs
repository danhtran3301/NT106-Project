using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using TimeFlow.Models;

namespace TimeFlow.Data.Repositories
{
    // Repository xu ly ActivityLog table
    public class ActivityLogRepository : BaseRepository
    {
        public ActivityLogRepository() : base() { }
        public ActivityLogRepository(DatabaseHelper db) : base(db) { }

        // ================== MAPPING ==================

        private ActivityLog MapToActivityLog(DataRow row)
        {
            return new ActivityLog
            {
                LogId = GetValue<int>(row, "LogId"),
                TaskId = GetNullableValue<int>(row, "TaskId"),
                UserId = GetValue<int>(row, "UserId"),
                ActivityType = GetValue<string>(row, "ActivityType", string.Empty),
                ActivityDescription = GetValue<string>(row, "ActivityDescription", string.Empty),
                CreatedAt = GetValue<DateTime>(row, "CreatedAt")
            };
        }

        // ================== READ OPERATIONS ==================

        // Lay log theo ID
        public ActivityLog? GetById(int logId)
        {
            var query = "SELECT * FROM ActivityLog WHERE LogId = @id";
            var parameters = CreateParameters(("@id", logId));
            
            var row = GetSingleRow(query, parameters);
            return row != null ? MapToActivityLog(row) : null;
        }

        // Lay logs cua task
        public List<ActivityLog> GetByTaskId(int taskId)
        {
            var query = @"SELECT * FROM ActivityLog 
                         WHERE TaskId = @taskId
                         ORDER BY CreatedAt DESC";
            
            var parameters = CreateParameters(("@taskId", taskId));
            var rows = GetRows(query, parameters);
            
            var logs = new List<ActivityLog>();
            foreach (DataRow row in rows)
            {
                logs.Add(MapToActivityLog(row));
            }
            return logs;
        }

        // Lay logs cua user
        public List<ActivityLog> GetByUserId(int userId, int limit = 50)
        {
            var query = @"SELECT TOP(@limit) * FROM ActivityLog 
                         WHERE UserId = @userId
                         ORDER BY CreatedAt DESC";
            
            var parameters = CreateParameters(
                ("@userId", userId),
                ("@limit", limit)
            );
            var rows = GetRows(query, parameters);
            
            var logs = new List<ActivityLog>();
            foreach (DataRow row in rows)
            {
                logs.Add(MapToActivityLog(row));
            }
            return logs;
        }

        // Lay logs gan day (tat ca users)
        public List<ActivityLog> GetRecentLogs(int limit = 100)
        {
            var query = @"SELECT TOP(@limit) * FROM ActivityLog 
                         ORDER BY CreatedAt DESC";
            
            var parameters = CreateParameters(("@limit", limit));
            var rows = GetRows(query, parameters);
            
            var logs = new List<ActivityLog>();
            foreach (DataRow row in rows)
            {
                logs.Add(MapToActivityLog(row));
            }
            return logs;
        }

        // Lay logs theo activity type
        public List<ActivityLog> GetByActivityType(string activityType, int limit = 50)
        {
            var query = @"SELECT TOP(@limit) * FROM ActivityLog 
                         WHERE ActivityType = @type
                         ORDER BY CreatedAt DESC";
            
            var parameters = CreateParameters(
                ("@type", activityType),
                ("@limit", limit)
            );
            var rows = GetRows(query, parameters);
            
            var logs = new List<ActivityLog>();
            foreach (DataRow row in rows)
            {
                logs.Add(MapToActivityLog(row));
            }
            return logs;
        }

        // Lay logs trong khoang thoi gian
        public List<ActivityLog> GetByDateRange(DateTime fromDate, DateTime toDate, int? userId = null)
        {
            var query = userId.HasValue
                ? @"SELECT * FROM ActivityLog 
                   WHERE CreatedAt >= @from AND CreatedAt <= @to AND UserId = @userId
                   ORDER BY CreatedAt DESC"
                : @"SELECT * FROM ActivityLog 
                   WHERE CreatedAt >= @from AND CreatedAt <= @to
                   ORDER BY CreatedAt DESC";
            
            var parameters = userId.HasValue
                ? CreateParameters(
                    ("@from", fromDate),
                    ("@to", toDate),
                    ("@userId", userId.Value))
                : CreateParameters(
                    ("@from", fromDate),
                    ("@to", toDate));
            
            var rows = GetRows(query, parameters);
            
            var logs = new List<ActivityLog>();
            foreach (DataRow row in rows)
            {
                logs.Add(MapToActivityLog(row));
            }
            return logs;
        }

        // ================== CREATE OPERATIONS ==================

        // Them log moi
        public int Create(ActivityLog log)
        {
            var query = @"INSERT INTO ActivityLog 
                         (TaskId, UserId, ActivityType, ActivityDescription, CreatedAt)
                         VALUES 
                         (@taskId, @userId, @type, @desc, GETDATE())";
            
            var parameters = CreateParameters(
                ("@taskId", log.TaskId),
                ("@userId", log.UserId),
                ("@type", log.ActivityType),
                ("@desc", log.ActivityDescription)
            );
            
            return InsertAndGetId(query, parameters);
        }

        // Them log nhanh (helper method)
        public int LogActivity(int userId, int? taskId, string activityType, string description)
        {
            var log = new ActivityLog
            {
                UserId = userId,
                TaskId = taskId,
                ActivityType = activityType,
                ActivityDescription = description
            };
            
            return Create(log);
        }

        // ================== DELETE OPERATIONS ==================

        // Xoa log (chi dung cho cleanup data cu)
        public bool Delete(int logId)
        {
            var query = "DELETE FROM ActivityLog WHERE LogId = @id";
            var parameters = CreateParameters(("@id", logId));
            
            return Execute(query, parameters) > 0;
        }

        // Xoa logs cu hon X ngay
        public int DeleteOlderThan(int days)
        {
            var query = @"DELETE FROM ActivityLog 
                         WHERE CreatedAt < DATEADD(day, -@days, GETDATE())";
            
            var parameters = CreateParameters(("@days", days));
            
            return Execute(query, parameters);
        }

        // Xoa tat ca logs cua task
        public bool DeleteByTaskId(int taskId)
        {
            var query = "DELETE FROM ActivityLog WHERE TaskId = @taskId";
            var parameters = CreateParameters(("@taskId", taskId));
            
            return Execute(query, parameters) > 0;
        }

        // ================== STATISTICS ==================

        // Dem so luong logs
        public int CountLogs(int? userId = null)
        {
            var query = userId.HasValue
                ? "SELECT COUNT(*) FROM ActivityLog WHERE UserId = @userId"
                : "SELECT COUNT(*) FROM ActivityLog";
            
            var parameters = userId.HasValue
                ? CreateParameters(("@userId", userId.Value))
                : null;
            
            var result = _db.ExecuteScalar(query, parameters);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        // Dem logs theo activity type
        public int CountByActivityType(string activityType)
        {
            var query = "SELECT COUNT(*) FROM ActivityLog WHERE ActivityType = @type";
            var parameters = CreateParameters(("@type", activityType));
            
            var result = _db.ExecuteScalar(query, parameters);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        // Dem logs hom nay
        public int CountToday(int? userId = null)
        {
            var query = userId.HasValue
                ? @"SELECT COUNT(*) FROM ActivityLog 
                   WHERE CAST(CreatedAt AS DATE) = CAST(GETDATE() AS DATE) 
                   AND UserId = @userId"
                : @"SELECT COUNT(*) FROM ActivityLog 
                   WHERE CAST(CreatedAt AS DATE) = CAST(GETDATE() AS DATE)";
            
            var parameters = userId.HasValue
                ? CreateParameters(("@userId", userId.Value))
                : null;
            
            var result = _db.ExecuteScalar(query, parameters);
            return result != null ? Convert.ToInt32(result) : 0;
        }
    }
}
