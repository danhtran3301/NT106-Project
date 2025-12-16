using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using TimeFlow.Models;

namespace TimeFlow.Data.Repositories
{
    // Repository xu ly Groups table
    public class GroupRepository : BaseRepository
    {
        public GroupRepository() : base() { }
        public GroupRepository(DatabaseHelper db) : base(db) { }

        // ================== MAPPING ==================

        private Group MapToGroup(DataRow row)
        {
            return new Group
            {
                GroupId = GetValue<int>(row, "GroupId"),
                GroupName = GetValue<string>(row, "GroupName", string.Empty),
                Description = GetString(row, "Description"),
                CreatedBy = GetValue<int>(row, "CreatedBy"),
                CreatedAt = GetValue<DateTime>(row, "CreatedAt"),
                UpdatedAt = GetNullableValue<DateTime>(row, "UpdatedAt"),
                IsActive = GetValue<bool>(row, "IsActive", true)
            };
        }

        // ================== READ OPERATIONS ==================

        // Lay group theo ID
        public Group? GetById(int groupId)
        {
            var query = "SELECT * FROM Groups WHERE GroupId = @id";
            var parameters = CreateParameters(("@id", groupId));
            
            var row = GetSingleRow(query, parameters);
            return row != null ? MapToGroup(row) : null;
        }

        // Lay groups cua user (user la member hoac creator)
        public List<Group> GetByUserId(int userId)
        {
            var query = @"SELECT DISTINCT g.* 
                         FROM Groups g
                         INNER JOIN GroupMembers gm ON g.GroupId = gm.GroupId
                         WHERE gm.UserId = @userId 
                         AND g.IsActive = 1 
                         AND gm.IsActive = 1
                         ORDER BY g.GroupName";
            
            var parameters = CreateParameters(("@userId", userId));
            var rows = GetRows(query, parameters);
            
            var groups = new List<Group>();
            foreach (DataRow row in rows)
            {
                groups.Add(MapToGroup(row));
            }
            return groups;
        }

        // Lay groups ma user la admin
        public List<Group> GetGroupsWhereUserIsAdmin(int userId)
        {
            var query = @"SELECT g.* 
                         FROM Groups g
                         INNER JOIN GroupMembers gm ON g.GroupId = gm.GroupId
                         WHERE gm.UserId = @userId 
                         AND gm.Role = 'Admin'
                         AND g.IsActive = 1 
                         AND gm.IsActive = 1
                         ORDER BY g.GroupName";
            
            var parameters = CreateParameters(("@userId", userId));
            var rows = GetRows(query, parameters);
            
            var groups = new List<Group>();
            foreach (DataRow row in rows)
            {
                groups.Add(MapToGroup(row));
            }
            return groups;
        }

        // Tim kiem groups
        public List<Group> Search(int userId, string keyword)
        {
            var query = @"SELECT DISTINCT g.* 
                         FROM Groups g
                         INNER JOIN GroupMembers gm ON g.GroupId = gm.GroupId
                         WHERE gm.UserId = @userId 
                         AND g.IsActive = 1 
                         AND gm.IsActive = 1
                         AND (g.GroupName LIKE @keyword OR g.Description LIKE @keyword)
                         ORDER BY g.GroupName";
            
            var parameters = CreateParameters(
                ("@userId", userId),
                ("@keyword", $"%{keyword}%")
            );
            var rows = GetRows(query, parameters);
            
            var groups = new List<Group>();
            foreach (DataRow row in rows)
            {
                groups.Add(MapToGroup(row));
            }
            return groups;
        }

        // ================== CREATE OPERATIONS ==================

        // Tao group moi (auto them creator vao GroupMembers voi role Admin)
        public int Create(Group group)
        {
            return ExecuteInTransaction((conn, trans) =>
            {
                // Buoc 1: Tao group
                var insertGroup = @"INSERT INTO Groups 
                                   (GroupName, Description, CreatedBy, IsActive, CreatedAt)
                                   VALUES 
                                   (@name, @desc, @createdBy, @active, GETDATE());
                                   SELECT SCOPE_IDENTITY();";
                
                using var cmd1 = new SqlCommand(insertGroup, conn, trans);
                cmd1.Parameters.AddRange(CreateParameters(
                    ("@name", group.GroupName),
                    ("@desc", group.Description),
                    ("@createdBy", group.CreatedBy),
                    ("@active", group.IsActive)
                ));
                
                int groupId = Convert.ToInt32(cmd1.ExecuteScalar());

                // Buoc 2: Tu dong them creator vao GroupMembers voi role Admin
                // (Trigger trg_Groups_AutoAddCreator se xu ly phan nay)
                // Nhung de chac chan, ta co the them manually neu muon
                
                return groupId;
            });
        }

        // ================== UPDATE OPERATIONS ==================

        // Cap nhat group info
        public bool Update(Group group)
        {
            var query = @"UPDATE Groups SET 
                         GroupName = @name,
                         Description = @desc,
                         UpdatedAt = GETDATE()
                         WHERE GroupId = @id";
            
            var parameters = CreateParameters(
                ("@id", group.GroupId),
                ("@name", group.GroupName),
                ("@desc", group.Description)
            );
            
            return Execute(query, parameters) > 0;
        }

        // Cap nhat trang thai active
        public bool UpdateActiveStatus(int groupId, bool isActive)
        {
            var query = @"UPDATE Groups SET 
                         IsActive = @active,
                         UpdatedAt = GETDATE()
                         WHERE GroupId = @id";
            
            var parameters = CreateParameters(
                ("@id", groupId),
                ("@active", isActive)
            );
            
            return Execute(query, parameters) > 0;
        }

        // ================== DELETE OPERATIONS ==================

        // Xoa group (soft delete)
        public bool SoftDelete(int groupId)
        {
            return UpdateActiveStatus(groupId, false);
        }

        // Xoa group (hard delete)
        public bool Delete(int groupId)
        {
            var query = "DELETE FROM Groups WHERE GroupId = @id";
            var parameters = CreateParameters(("@id", groupId));
            
            return Execute(query, parameters) > 0;
        }

        // ================== CHECK OPERATIONS ==================

        // Kiem tra user co phai member cua group khong
        public bool IsUserMember(int groupId, int userId)
        {
            var query = @"SELECT COUNT(*) FROM GroupMembers 
                         WHERE GroupId = @groupId 
                         AND UserId = @userId 
                         AND IsActive = 1";
            
            var parameters = CreateParameters(
                ("@groupId", groupId),
                ("@userId", userId)
            );
            
            var result = _db.ExecuteScalar(query, parameters);
            return result != null && Convert.ToInt32(result) > 0;
        }

        // Kiem tra user co phai admin cua group khong
        public bool IsUserAdmin(int groupId, int userId)
        {
            var query = @"SELECT COUNT(*) FROM GroupMembers 
                         WHERE GroupId = @groupId 
                         AND UserId = @userId 
                         AND Role = 'Admin'
                         AND IsActive = 1";
            
            var parameters = CreateParameters(
                ("@groupId", groupId),
                ("@userId", userId)
            );
            
            var result = _db.ExecuteScalar(query, parameters);
            return result != null && Convert.ToInt32(result) > 0;
        }

        // Kiem tra ten group da ton tai chua
        public bool GroupNameExists(string groupName)
        {
            return Exists("Groups", "GroupName = @name AND IsActive = 1", 
                CreateParameters(("@name", groupName)));
        }

        // ================== STATISTICS ==================

        // Dem so luong members trong group
        public int CountMembers(int groupId)
        {
            var query = @"SELECT COUNT(*) FROM GroupMembers 
                         WHERE GroupId = @groupId AND IsActive = 1";
            
            var parameters = CreateParameters(("@groupId", groupId));
            
            var result = _db.ExecuteScalar(query, parameters);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        // Dem so luong tasks trong group
        public int CountTasks(int groupId)
        {
            var query = @"SELECT COUNT(*) FROM GroupTasks 
                         WHERE GroupId = @groupId";
            
            var parameters = CreateParameters(("@groupId", groupId));
            
            var result = _db.ExecuteScalar(query, parameters);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        // Dem so luong groups cua user
        public int CountUserGroups(int userId)
        {
            var query = @"SELECT COUNT(DISTINCT gm.GroupId) 
                         FROM GroupMembers gm
                         INNER JOIN Groups g ON gm.GroupId = g.GroupId
                         WHERE gm.UserId = @userId 
                         AND g.IsActive = 1 
                         AND gm.IsActive = 1";
            
            var parameters = CreateParameters(("@userId", userId));
            
            var result = _db.ExecuteScalar(query, parameters);
            return result != null ? Convert.ToInt32(result) : 0;
        }
    }
}
