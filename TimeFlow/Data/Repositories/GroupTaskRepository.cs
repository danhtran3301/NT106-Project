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

        // Lay group task theo ID
        public GroupTask? GetById(int groupTaskId)
        {
            var query = "SELECT * FROM GroupTasks WHERE GroupTaskId = @id";
            var parameters = CreateParameters(("@id", groupTaskId));
            
            var row = GetSingleRow(query, parameters);
            return row != null ? MapToGroupTask(row) : null;
        }

        // Lay group task theo TaskId
        public GroupTask? GetByTaskId(int taskId)
        {
            var query = "SELECT * FROM GroupTasks WHERE TaskId = @taskId";
            var parameters = CreateParameters(("@taskId", taskId));
            
            var row = GetSingleRow(query, parameters);
            return row != null ? MapToGroupTask(row) : null;
        }

        // Lay tat ca tasks cua group
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

        // Lay tasks duoc assign cho user
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

        // Lay tasks chua assign trong group
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

        // Gan task vao group
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

        // Assign task cho user
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

        // Unassign task
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

        // Reassign task cho user khac
        public bool ReassignTask(int groupTaskId, int newUserId, int reassignedByUserId)
        {
            return AssignTask(groupTaskId, newUserId, reassignedByUserId);
        }

        // ================== DELETE OPERATIONS ==================

        // Xoa group task (khi xoa task khoi group)
        public bool Delete(int groupTaskId)
        {
            var query = "DELETE FROM GroupTasks WHERE GroupTaskId = @id";
            var parameters = CreateParameters(("@id", groupTaskId));
            
            return Execute(query, parameters) > 0;
        }

        // Xoa theo TaskId
        public bool DeleteByTaskId(int taskId)
        {
            var query = "DELETE FROM GroupTasks WHERE TaskId = @taskId";
            var parameters = CreateParameters(("@taskId", taskId));
            
            return Execute(query, parameters) > 0;
        }

        // ================== CHECK OPERATIONS ==================

        // Kiem tra task co thuoc group khong
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

        // Kiem tra task da duoc assign chua
        public bool IsTaskAssigned(int taskId)
        {
            var query = @"SELECT COUNT(*) FROM GroupTasks 
                         WHERE TaskId = @taskId AND AssignedTo IS NOT NULL";
            
            var parameters = CreateParameters(("@taskId", taskId));
            
            var result = _db.ExecuteScalar(query, parameters);
            return result != null && Convert.ToInt32(result) > 0;
        }

        // ================== STATISTICS ==================

        // Dem so luong tasks cua group
        public int CountGroupTasks(int groupId)
        {
            var query = "SELECT COUNT(*) FROM GroupTasks WHERE GroupId = @groupId";
            var parameters = CreateParameters(("@groupId", groupId));
            
            var result = _db.ExecuteScalar(query, parameters);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        // Dem so luong tasks da assign
        public int CountAssignedTasks(int groupId)
        {
            var query = @"SELECT COUNT(*) FROM GroupTasks 
                         WHERE GroupId = @groupId AND AssignedTo IS NOT NULL";
            
            var parameters = CreateParameters(("@groupId", groupId));
            
            var result = _db.ExecuteScalar(query, parameters);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        // Dem so luong tasks chua assign
        public int CountUnassignedTasks(int groupId)
        {
            var query = @"SELECT COUNT(*) FROM GroupTasks 
                         WHERE GroupId = @groupId AND AssignedTo IS NULL";
            
            var parameters = CreateParameters(("@groupId", groupId));
            
            var result = _db.ExecuteScalar(query, parameters);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        // Dem so luong tasks duoc assign cho user
        public int CountUserAssignedTasks(int userId)
        {
            var query = "SELECT COUNT(*) FROM GroupTasks WHERE AssignedTo = @userId";
            var parameters = CreateParameters(("@userId", userId));
            
            var result = _db.ExecuteScalar(query, parameters);
            return result != null ? Convert.ToInt32(result) : 0;
        }
    }
}
