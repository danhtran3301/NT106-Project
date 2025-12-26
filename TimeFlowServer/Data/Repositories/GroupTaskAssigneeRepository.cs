using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using TimeFlow.Models;

namespace TimeFlow.Data.Repositories
{
    // Repository xu ly GroupTaskAssignees table (multiple assignees per group task)
    public class GroupTaskAssigneeRepository : BaseRepository
    {
        public GroupTaskAssigneeRepository() : base() { }
        public GroupTaskAssigneeRepository(DatabaseHelper db) : base(db) { }

        // ================== READ OPERATIONS ==================

        // Lay tat ca assignees cua group task
        public List<int> GetAssigneeIds(int groupTaskId)
        {
            var query = @"SELECT UserId FROM GroupTaskAssignees 
                         WHERE GroupTaskId = @groupTaskId";
            
            var parameters = CreateParameters(("@groupTaskId", groupTaskId));
            var rows = GetRows(query, parameters);
            
            var userIds = new List<int>();
            foreach (DataRow row in rows)
            {
                userIds.Add(GetValue<int>(row, "UserId"));
            }
            return userIds;
        }

        // Lay assignees theo TaskId (thông qua GroupTaskId)
        public List<int> GetAssigneeIdsByTaskId(int taskId)
        {
            var query = @"SELECT gta.UserId 
                         FROM GroupTaskAssignees gta
                         INNER JOIN GroupTasks gt ON gta.GroupTaskId = gt.GroupTaskId
                         WHERE gt.TaskId = @taskId";
            
            var parameters = CreateParameters(("@taskId", taskId));
            var rows = GetRows(query, parameters);
            
            var userIds = new List<int>();
            foreach (DataRow row in rows)
            {
                userIds.Add(GetValue<int>(row, "UserId"));
            }
            return userIds;
        }

        // Kiem tra user da duoc assign cho group task chua
        public bool IsAssigned(int groupTaskId, int userId)
        {
            var query = @"SELECT COUNT(*) FROM GroupTaskAssignees 
                         WHERE GroupTaskId = @groupTaskId AND UserId = @userId";
            
            var parameters = CreateParameters(
                ("@groupTaskId", groupTaskId),
                ("@userId", userId)
            );
            
            var result = _db.ExecuteScalar(query, parameters);
            return result != null && Convert.ToInt32(result) > 0;
        }

        // ================== CREATE OPERATIONS ==================

        // Assign user cho group task
        public bool AddAssignee(int groupTaskId, int userId, int? assignedBy = null)
        {
            // Kiem tra da ton tai chua
            if (IsAssigned(groupTaskId, userId))
            {
                return false; // Da ton tai
            }

            var query = @"INSERT INTO GroupTaskAssignees 
                         (GroupTaskId, UserId, AssignedBy, AssignedAt)
                         VALUES 
                         (@groupTaskId, @userId, @assignedBy, GETDATE())";
            
            var parameters = CreateParameters(
                ("@groupTaskId", groupTaskId),
                ("@userId", userId),
                ("@assignedBy", assignedBy)
            );
            
            return Execute(query, parameters) > 0;
        }

        // Assign nhieu users cho group task (batch)
        public int AddAssignees(int groupTaskId, List<int> userIds, int? assignedBy = null)
        {
            int count = 0;
            foreach (var userId in userIds)
            {
                if (AddAssignee(groupTaskId, userId, assignedBy))
                {
                    count++;
                }
            }
            return count;
        }

        // ================== DELETE OPERATIONS ==================

        // Xoa assignment (unassign user)
        public bool RemoveAssignee(int groupTaskId, int userId)
        {
            var query = @"DELETE FROM GroupTaskAssignees 
                         WHERE GroupTaskId = @groupTaskId AND UserId = @userId";
            
            var parameters = CreateParameters(
                ("@groupTaskId", groupTaskId),
                ("@userId", userId)
            );
            
            return Execute(query, parameters) > 0;
        }

        // Xoa tat ca assignees cua group task
        public bool RemoveAllAssignees(int groupTaskId)
        {
            var query = @"DELETE FROM GroupTaskAssignees 
                         WHERE GroupTaskId = @groupTaskId";
            
            var parameters = CreateParameters(("@groupTaskId", groupTaskId));
            
            return Execute(query, parameters) >= 0;
        }

        // ================== UPDATE OPERATIONS ==================

        // Update assignees: xoa het, them lai theo danh sach moi
        public bool UpdateAssignees(int groupTaskId, List<int> userIds, int? assignedBy = null)
        {
            return ExecuteInTransaction((conn, trans) =>
            {
                // Xoa tat ca assignees cu
                var deleteQuery = @"DELETE FROM GroupTaskAssignees 
                                   WHERE GroupTaskId = @groupTaskId";
                var deleteParams = CreateParameters(("@groupTaskId", groupTaskId));
                
                using (var cmd = new SqlCommand(deleteQuery, conn, trans))
                {
                    foreach (var param in deleteParams)
                    {
                        cmd.Parameters.Add(param);
                    }
                    cmd.ExecuteNonQuery();
                }

                // Them assignees moi
                if (userIds != null && userIds.Count > 0)
                {
                    foreach (var userId in userIds)
                    {
                        var insertQuery = @"INSERT INTO GroupTaskAssignees 
                                           (GroupTaskId, UserId, AssignedBy, AssignedAt)
                                           VALUES 
                                           (@groupTaskId, @userId, @assignedBy, GETDATE())";
                        var insertParams = CreateParameters(
                            ("@groupTaskId", groupTaskId),
                            ("@userId", userId),
                            ("@assignedBy", assignedBy)
                        );
                        
                        using (var cmd = new SqlCommand(insertQuery, conn, trans))
                        {
                            foreach (var param in insertParams)
                            {
                                cmd.Parameters.Add(param);
                            }
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                return true;
            });
        }
    }
}

