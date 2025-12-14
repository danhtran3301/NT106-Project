using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using TimeFlow.Models;

namespace TimeFlow.Data.Repositories
{
    // Repository xu ly Comments table
    public class CommentRepository : BaseRepository
    {
        public CommentRepository() : base() { }
        public CommentRepository(DatabaseHelper db) : base(db) { }

        // ================== MAPPING ==================

        private Comment MapToComment(DataRow row)
        {
            return new Comment
            {
                CommentId = GetValue<int>(row, "CommentId"),
                TaskId = GetValue<int>(row, "TaskId"),
                UserId = GetValue<int>(row, "UserId"),
                CommentText = GetValue<string>(row, "CommentText", string.Empty),
                CreatedAt = GetValue<DateTime>(row, "CreatedAt"),
                UpdatedAt = GetNullableValue<DateTime>(row, "UpdatedAt"),
                IsEdited = GetValue<bool>(row, "IsEdited", false)
            };
        }

        // ================== READ OPERATIONS ==================

        // Lay comment theo ID
        public Comment? GetById(int commentId)
        {
            var query = "SELECT * FROM Comments WHERE CommentId = @id";
            var parameters = CreateParameters(("@id", commentId));
            
            var row = GetSingleRow(query, parameters);
            return row != null ? MapToComment(row) : null;
        }

        // Lay tat ca comments cua task
        public List<Comment> GetByTaskId(int taskId)
        {
            var query = @"SELECT * FROM Comments 
                         WHERE TaskId = @taskId
                         ORDER BY CreatedAt ASC";
            
            var parameters = CreateParameters(("@taskId", taskId));
            var rows = GetRows(query, parameters);
            
            var comments = new List<Comment>();
            foreach (DataRow row in rows)
            {
                comments.Add(MapToComment(row));
            }
            return comments;
        }

        // Lay comments cua user
        public List<Comment> GetByUserId(int userId)
        {
            var query = @"SELECT * FROM Comments 
                         WHERE UserId = @userId
                         ORDER BY CreatedAt DESC";
            
            var parameters = CreateParameters(("@userId", userId));
            var rows = GetRows(query, parameters);
            
            var comments = new List<Comment>();
            foreach (DataRow row in rows)
            {
                comments.Add(MapToComment(row));
            }
            return comments;
        }

        // ================== CREATE OPERATIONS ==================

        // Them comment moi
        public int Create(Comment comment)
        {
            var query = @"INSERT INTO Comments 
                         (TaskId, UserId, CommentText, CreatedAt, IsEdited)
                         VALUES 
                         (@taskId, @userId, @text, GETDATE(), 0)";
            
            var parameters = CreateParameters(
                ("@taskId", comment.TaskId),
                ("@userId", comment.UserId),
                ("@text", comment.CommentText)
            );
            
            return InsertAndGetId(query, parameters);
        }

        // ================== UPDATE OPERATIONS ==================

        // Cap nhat comment
        public bool Update(int commentId, string newText)
        {
            var query = @"UPDATE Comments SET 
                         CommentText = @text,
                         UpdatedAt = GETDATE(),
                         IsEdited = 1
                         WHERE CommentId = @id";
            
            var parameters = CreateParameters(
                ("@id", commentId),
                ("@text", newText)
            );
            
            return Execute(query, parameters) > 0;
        }

        // ================== DELETE OPERATIONS ==================

        // Xoa comment
        public bool Delete(int commentId)
        {
            var query = "DELETE FROM Comments WHERE CommentId = @id";
            var parameters = CreateParameters(("@id", commentId));
            
            return Execute(query, parameters) > 0;
        }

        // Xoa tat ca comments cua task
        public bool DeleteByTaskId(int taskId)
        {
            var query = "DELETE FROM Comments WHERE TaskId = @taskId";
            var parameters = CreateParameters(("@taskId", taskId));
            
            return Execute(query, parameters) > 0;
        }

        // ================== CHECK OPERATIONS ==================

        // Kiem tra user co the edit comment khong
        public bool CanUserEdit(int commentId, int userId)
        {
            var comment = GetById(commentId);
            return comment != null && comment.UserId == userId;
        }

        // ================== STATISTICS ==================

        // Dem so luong comments cua task
        public int CountByTaskId(int taskId)
        {
            var query = "SELECT COUNT(*) FROM Comments WHERE TaskId = @taskId";
            var parameters = CreateParameters(("@taskId", taskId));
            
            var result = _db.ExecuteScalar(query, parameters);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        // Dem so luong comments cua user
        public int CountByUserId(int userId)
        {
            var query = "SELECT COUNT(*) FROM Comments WHERE UserId = @userId";
            var parameters = CreateParameters(("@userId", userId));
            
            var result = _db.ExecuteScalar(query, parameters);
            return result != null ? Convert.ToInt32(result) : 0;
        }
    }
}
