using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using TimeFlow.Models;

namespace TimeFlow.Data.Repositories
{
    // Repository xu ly GroupTasks table
    public class GroupTaskRepository : BaseRepository
    {
        public GroupTaskRepository() : base() { }
        public GroupTaskRepository(DatabaseHelper db) : base(db) { }

        // ================== MAPPING ==================

        private GroupTask MapToGroupTask(DataRow row)
        {
            return new GroupTask
            {
                GroupTaskId = GetValue<int>(row, "GroupTaskId"),
                TaskId = GetValue<int>(row, "TaskId"),
                GroupId = GetValue<int>(row, "GroupId"),
                AssignedTo = GetNullableValue<int>(row, "AssignedTo"),
                AssignedAt = GetNullableValue<DateTime>(row, "AssignedAt"),
                AssignedBy = GetNullableValue<int>(row, "AssignedBy")
            };
        }

        // ================== READ OPERATIONS ==================

        public GroupTask? GetById(int groupTaskId)
        {
            var query = "SELECT * FROM GroupTasks WHERE GroupTaskId = @id";
            var parameters = CreateParameters(("@id", groupTaskId));

            var row = GetSingleRow(query, parameters);
            return row != null ? MapToGroupTask(row) : null;
        }

        public GroupTask? GetByTaskId(int taskId)
        {
            var query = "SELECT * FROM GroupTasks WHERE TaskId = @taskId";
            var parameters = CreateParameters(("@taskId", taskId));

            var row = GetSingleRow(query, parameters);
            return row != null ? MapToGroupTask(row) : null;
        }

        public List<GroupTask> GetByGroupId(int groupId)
        {
            var query = @"SELECT * FROM GroupTasks 
                          WHERE GroupId = @groupId
                          ORDER BY AssignedAt DESC";

            var parameters = CreateParameters(("@groupId", groupId));
            var rows = GetRows(query, parameters);

            var groupTasks = new List<GroupTask>();
            foreach (DataRow row in rows)
            {
                groupTasks.Add(MapToGroupTask(row));
            }
            return groupTasks;
        }

        public List<GroupTask> GetAssignedToUser(int userId)
        {
            var query = @"SELECT * FROM GroupTasks 
                          WHERE AssignedTo = @userId
                          ORDER BY AssignedAt DESC";

            var parameters = CreateParameters(("@userId", userId));
            var rows = GetRows(query, parameters);

            var groupTasks = new List<GroupTask>();
            foreach (DataRow row in rows)
            {
                groupTasks.Add(MapToGroupTask(row));
            }
            return groupTasks;
        }

        public List<GroupTask> GetUnassignedTasks(int groupId)
        {
            var query = @"SELECT * FROM GroupTasks 
                          WHERE GroupId = @groupId AND AssignedTo IS NULL
                          ORDER BY GroupTaskId DESC";

            var parameters = CreateParameters(("@groupId", groupId));
            var rows = GetRows(query, parameters);

            var groupTasks = new List<GroupTask>();
            foreach (DataRow row in rows)
            {
                groupTasks.Add(MapToGroupTask(row));
            }
            return groupTasks;
        }

        // ================== CREATE OPERATIONS ==================

        public int Create(int taskId, int groupId, int? assignedTo = null, int? assignedBy = null)
        {
            var query = @"INSERT INTO GroupTasks 
                          (TaskId, GroupId, AssignedTo, AssignedBy, AssignedAt)
                          VALUES 
                          (@taskId, @groupId, @assignedTo, @assignedBy, 
                           CASE WHEN @assignedTo IS NOT NULL THEN GETDATE() ELSE NULL END)";

            var parameters = CreateParameters(
                ("@taskId", taskId),
                ("@groupId", groupId),
                ("@assignedTo", assignedTo),
                ("@assignedBy", assignedBy)
            );

            return InsertAndGetId(query, parameters);
        }

        // ================== UPDATE OPERATIONS ==================

        // 1. Assign Task dựa trên GroupTaskId (Code cũ của bạn)
        public bool AssignTask(int groupTaskId, int userId, int assignedByUserId)
        {
            var query = @"UPDATE GroupTasks SET 
                          AssignedTo = @userId,
                          AssignedBy = @assignedBy,
                          AssignedAt = GETDATE()
                          WHERE GroupTaskId = @id";

            var parameters = CreateParameters(
                ("@id", groupTaskId),
                ("@userId", userId),
                ("@assignedBy", assignedByUserId)
            );

            return Execute(query, parameters) > 0;
        }

        // 2. [QUAN TRỌNG] Assign Task dựa trên TaskId + GroupId (Bổ sung để fix lỗi ClientHandler)
        // Đây là hàm Overload mà ClientHandler đang thiếu
        public bool AssignTask(int taskId, int groupId, int assignedToUserId, int assignedByUserId)
        {
            // Logic: Cập nhật nếu đã có dòng liên kết, nếu chưa có thì Insert mới (Upsert)
            var query = @"
                UPDATE GroupTasks 
                SET AssignedTo = @assignedTo, 
                    AssignedBy = @assignedBy, 
                    AssignedAt = GETDATE()
                WHERE TaskId = @taskId AND GroupId = @groupId;

                IF @@ROWCOUNT = 0
                BEGIN
                    INSERT INTO GroupTasks (TaskId, GroupId, AssignedTo, AssignedBy, AssignedAt)
                    VALUES (@taskId, @groupId, @assignedTo, @assignedBy, GETDATE());
                END";

            var parameters = CreateParameters(
                ("@taskId", taskId),
                ("@groupId", groupId),
                ("@assignedTo", assignedToUserId),
                ("@assignedBy", assignedByUserId)
            );

            return Execute(query, parameters) > 0;
        }

        public bool UnassignTask(int groupTaskId)
        {
            var query = @"UPDATE GroupTasks SET 
                          AssignedTo = NULL,
                          AssignedBy = NULL,
                          AssignedAt = NULL
                          WHERE GroupTaskId = @id";

            var parameters = CreateParameters(("@id", groupTaskId));

            return Execute(query, parameters) > 0;
        }

        public bool ReassignTask(int groupTaskId, int newUserId, int reassignedByUserId)
        {
            return AssignTask(groupTaskId, newUserId, reassignedByUserId);
        }

        // ================== DELETE OPERATIONS ==================

        public bool Delete(int groupTaskId)
        {
            var query = "DELETE FROM GroupTasks WHERE GroupTaskId = @id";
            var parameters = CreateParameters(("@id", groupTaskId));

            return Execute(query, parameters) > 0;
        }

        public bool DeleteByTaskId(int taskId)
        {
            var query = "DELETE FROM GroupTasks WHERE TaskId = @taskId";
            var parameters = CreateParameters(("@taskId", taskId));

            return Execute(query, parameters) > 0;
        }

        // ================== CHECK OPERATIONS ==================

        public bool IsTaskInGroup(int taskId, int groupId)
        {
            var query = @"SELECT COUNT(*) FROM GroupTasks 
                          WHERE TaskId = @taskId AND GroupId = @groupId";

            var parameters = CreateParameters(
                ("@taskId", taskId),
                ("@groupId", groupId)
            );

            var result = _db.ExecuteScalar(query, parameters);
            return result != null && Convert.ToInt32(result) > 0;
        }

        public bool IsTaskAssigned(int taskId)
        {
            var query = @"SELECT COUNT(*) FROM GroupTasks 
                          WHERE TaskId = @taskId AND AssignedTo IS NOT NULL";

            var parameters = CreateParameters(("@taskId", taskId));

            var result = _db.ExecuteScalar(query, parameters);
            return result != null && Convert.ToInt32(result) > 0;
        }

        // ================== STATISTICS ==================

        public int CountGroupTasks(int groupId)
        {
            var query = "SELECT COUNT(*) FROM GroupTasks WHERE GroupId = @groupId";
            var parameters = CreateParameters(("@groupId", groupId));
            var result = _db.ExecuteScalar(query, parameters);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public int CountAssignedTasks(int groupId)
        {
            var query = @"SELECT COUNT(*) FROM GroupTasks 
                          WHERE GroupId = @groupId AND AssignedTo IS NOT NULL";

            var parameters = CreateParameters(("@groupId", groupId));
            var result = _db.ExecuteScalar(query, parameters);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public int CountUnassignedTasks(int groupId)
        {
            var query = @"SELECT COUNT(*) FROM GroupTasks 
                          WHERE GroupId = @groupId AND AssignedTo IS NULL";

            var parameters = CreateParameters(("@groupId", groupId));
            var result = _db.ExecuteScalar(query, parameters);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public int CountUserAssignedTasks(int userId)
        {
            var query = "SELECT COUNT(*) FROM GroupTasks WHERE AssignedTo = @userId";
            var parameters = CreateParameters(("@userId", userId));
            var result = _db.ExecuteScalar(query, parameters);
            return result != null ? Convert.ToInt32(result) : 0;
        }
    }
}