using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using TimeFlow.Models;

namespace TimeFlow.Data.Repositories
{
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

        public Group? GetById(int groupId)
        {
            var query = "SELECT * FROM Groups WHERE GroupId = @id";
            var parameters = CreateParameters(("@id", groupId));

            var row = GetSingleRow(query, parameters);
            return row != null ? MapToGroup(row) : null;
        }

        public List<Group> GetByUserId(int userId)
        {
            var query = @"SELECT DISTINCT g.* FROM Groups g
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

        public List<Group> Search(int userId, string keyword)
        {
            var query = @"SELECT DISTINCT g.* FROM Groups g
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


        // Tao group moi VA them nguoi tao lam Admin ngay lap tuc
        public int Create(Group group)
        {
            return ExecuteInTransaction((conn, trans) =>
            {
                // Buoc 1: Insert Group
                var insertGroupSql = @"INSERT INTO Groups 
                                       (GroupName, Description, CreatedBy, IsActive, CreatedAt)
                                       VALUES 
                                       (@name, @desc, @createdBy, 1, GETDATE());
                                       SELECT CAST(SCOPE_IDENTITY() as int);";

                int newGroupId;
                using (var cmdGroup = new SqlCommand(insertGroupSql, conn, trans))
                {
                    cmdGroup.Parameters.AddWithValue("@name", group.GroupName);
                    cmdGroup.Parameters.AddWithValue("@desc", (object)group.Description ?? DBNull.Value);
                    cmdGroup.Parameters.AddWithValue("@createdBy", group.CreatedBy);

                    newGroupId = (int)cmdGroup.ExecuteScalar();
                }

                var insertMemberSql = @"INSERT INTO GroupMembers 
                                        (GroupId, UserId, Role, JoinedAt, IsActive)
                                        VALUES 
                                        (@groupId, @userId, 'Admin', GETDATE(), 1)";

                using (var cmdMember = new SqlCommand(insertMemberSql, conn, trans))
                {
                    cmdMember.Parameters.AddWithValue("@groupId", newGroupId);
                    cmdMember.Parameters.AddWithValue("@userId", group.CreatedBy);
                    cmdMember.ExecuteNonQuery();
                }

                return newGroupId;
            });
        }

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

        public bool SoftDelete(int groupId)
        {
            return UpdateActiveStatus(groupId, false);
        }

        public bool Delete(int groupId)
        {
            var query = "DELETE FROM Groups WHERE GroupId = @id";
            var parameters = CreateParameters(("@id", groupId));

            return Execute(query, parameters) > 0;
        }

        // ================== CHECK OPERATIONS ==================

        public bool IsUserMember(int groupId, int userId)
        {
            var query = @"SELECT COUNT(1) FROM GroupMembers 
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

        public bool IsUserAdmin(int groupId, int userId)
        {
            var query = @"SELECT COUNT(1) FROM GroupMembers 
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

        public bool GroupNameExists(string groupName)
        {
            // Chi check cac group dang active
            return Exists("Groups", "GroupName = @name AND IsActive = 1",
                CreateParameters(("@name", groupName)));
        }

        // ================== STATISTICS ==================

        public int CountMembers(int groupId)
        {
            var query = "SELECT COUNT(1) FROM GroupMembers WHERE GroupId = @groupId AND IsActive = 1";
            var parameters = CreateParameters(("@groupId", groupId));
            var result = _db.ExecuteScalar(query, parameters);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public int CountTasks(int groupId)
        {
            var query = "SELECT COUNT(1) FROM GroupTasks WHERE GroupId = @groupId";
            var parameters = CreateParameters(("@groupId", groupId));
            var result = _db.ExecuteScalar(query, parameters);
            return result != null ? Convert.ToInt32(result) : 0;
        }

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