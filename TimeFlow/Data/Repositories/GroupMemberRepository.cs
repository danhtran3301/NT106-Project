using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using TimeFlow.Models;

namespace TimeFlow.Data.Repositories
{
    // Repository xu ly GroupMembers table
    public class GroupMemberRepository : BaseRepository
    {
        public GroupMemberRepository() : base() { }
        public GroupMemberRepository(DatabaseHelper db) : base(db) { }

        // ================== MAPPING ==================

        private GroupMember MapToGroupMember(DataRow row)
        {
            return new GroupMember
            {
                GroupMemberId = GetValue<int>(row, "GroupMemberId"),
                GroupId = GetValue<int>(row, "GroupId"),
                UserId = GetValue<int>(row, "UserId"),
                Role = GetValue<string>(row, "Role") == "Admin" ? GroupRole.Admin : GroupRole.Member,
                JoinedAt = GetValue<DateTime>(row, "JoinedAt"),
                IsActive = GetValue<bool>(row, "IsActive", true)
            };
        }

        // ================== READ OPERATIONS ==================

        // Lay member theo ID
        public GroupMember? GetById(int groupMemberId)
        {
            var query = "SELECT * FROM GroupMembers WHERE GroupMemberId = @id";
            var parameters = CreateParameters(("@id", groupMemberId));
            
            var row = GetSingleRow(query, parameters);
            return row != null ? MapToGroupMember(row) : null;
        }

        // Lay member theo GroupId va UserId
        public GroupMember? GetByGroupAndUser(int groupId, int userId)
        {
            var query = @"SELECT * FROM GroupMembers 
                         WHERE GroupId = @groupId AND UserId = @userId";
            
            var parameters = CreateParameters(
                ("@groupId", groupId),
                ("@userId", userId)
            );
            
            var row = GetSingleRow(query, parameters);
            return row != null ? MapToGroupMember(row) : null;
        }

        // Lay tat ca members cua group
        public List<GroupMember> GetByGroupId(int groupId)
        {
            var query = @"SELECT * FROM GroupMembers 
                         WHERE GroupId = @groupId AND IsActive = 1
                         ORDER BY 
                         CASE WHEN Role = 'Admin' THEN 0 ELSE 1 END,
                         JoinedAt ASC";
            
            var parameters = CreateParameters(("@groupId", groupId));
            var rows = GetRows(query, parameters);
            
            var members = new List<GroupMember>();
            foreach (DataRow row in rows)
            {
                members.Add(MapToGroupMember(row));
            }
            return members;
        }

        // Lay cac groups ma user tham gia
        public List<GroupMember> GetByUserId(int userId)
        {
            var query = @"SELECT * FROM GroupMembers 
                         WHERE UserId = @userId AND IsActive = 1
                         ORDER BY JoinedAt DESC";
            
            var parameters = CreateParameters(("@userId", userId));
            var rows = GetRows(query, parameters);
            
            var members = new List<GroupMember>();
            foreach (DataRow row in rows)
            {
                members.Add(MapToGroupMember(row));
            }
            return members;
        }

        // Lay danh sach admins cua group
        public List<GroupMember> GetAdmins(int groupId)
        {
            var query = @"SELECT * FROM GroupMembers 
                         WHERE GroupId = @groupId 
                         AND Role = 'Admin' 
                         AND IsActive = 1
                         ORDER BY JoinedAt ASC";
            
            var parameters = CreateParameters(("@groupId", groupId));
            var rows = GetRows(query, parameters);
            
            var members = new List<GroupMember>();
            foreach (DataRow row in rows)
            {
                members.Add(MapToGroupMember(row));
            }
            return members;
        }

        // ================== CREATE OPERATIONS ==================

        // Them member vao group
        public int AddMember(int groupId, int userId, GroupRole role = GroupRole.Member)
        {
            // Kiem tra xem da la member chua
            var existing = GetByGroupAndUser(groupId, userId);
            if (existing != null)
            {
                // Neu da ton tai nhung inactive, reactive lai
                if (!existing.IsActive)
                {
                    UpdateActiveStatus(existing.GroupMemberId, true);
                    return existing.GroupMemberId;
                }
                return 0; // Da la member roi
            }

            var query = @"INSERT INTO GroupMembers 
                         (GroupId, UserId, Role, IsActive, JoinedAt)
                         VALUES 
                         (@groupId, @userId, @role, @active, GETDATE())";
            
            var parameters = CreateParameters(
                ("@groupId", groupId),
                ("@userId", userId),
                ("@role", role == GroupRole.Admin ? "Admin" : "Member"),
                ("@active", true)
            );
            
            return InsertAndGetId(query, parameters);
        }

        // ================== UPDATE OPERATIONS ==================

        // Cap nhat role cua member
        public bool UpdateRole(int groupMemberId, GroupRole newRole)
        {
            var query = @"UPDATE GroupMembers SET 
                         Role = @role
                         WHERE GroupMemberId = @id";
            
            var parameters = CreateParameters(
                ("@id", groupMemberId),
                ("@role", newRole == GroupRole.Admin ? "Admin" : "Member")
            );
            
            return Execute(query, parameters) > 0;
        }

        // Promote member thanh admin
        public bool PromoteToAdmin(int groupId, int userId)
        {
            var member = GetByGroupAndUser(groupId, userId);
            if (member == null) return false;
            
            return UpdateRole(member.GroupMemberId, GroupRole.Admin);
        }

        // Demote admin xuong member
        public bool DemoteToMember(int groupId, int userId)
        {
            var member = GetByGroupAndUser(groupId, userId);
            if (member == null) return false;
            
            // Kiem tra xem con admin nao khac khong
            var adminCount = CountAdmins(groupId);
            if (adminCount <= 1)
                return false; // Khong cho demote admin cuoi cung
            
            return UpdateRole(member.GroupMemberId, GroupRole.Member);
        }

        // Cap nhat trang thai active
        public bool UpdateActiveStatus(int groupMemberId, bool isActive)
        {
            var query = @"UPDATE GroupMembers SET 
                         IsActive = @active
                         WHERE GroupMemberId = @id";
            
            var parameters = CreateParameters(
                ("@id", groupMemberId),
                ("@active", isActive)
            );
            
            return Execute(query, parameters) > 0;
        }

        // ================== DELETE OPERATIONS ==================

        // Xoa member khoi group (soft delete)
        public bool RemoveMember(int groupId, int userId)
        {
            // Neu la admin, kiem tra xem con admin nao khac khong
            var member = GetByGroupAndUser(groupId, userId);
            if (member == null) return false;
            
            if (member.Role == GroupRole.Admin)
            {
                var adminCount = CountAdmins(groupId);
                if (adminCount <= 1)
                    return false; // Khong cho xoa admin cuoi cung
            }
            
            return UpdateActiveStatus(member.GroupMemberId, false);
        }

        // Xoa member (hard delete)
        public bool Delete(int groupMemberId)
        {
            var query = "DELETE FROM GroupMembers WHERE GroupMemberId = @id";
            var parameters = CreateParameters(("@id", groupMemberId));
            
            return Execute(query, parameters) > 0;
        }

        // ================== CHECK OPERATIONS ==================

        // Kiem tra user co phai member cua group khong
        public bool IsMember(int groupId, int userId)
        {
            var member = GetByGroupAndUser(groupId, userId);
            return member != null && member.IsActive;
        }

        // Kiem tra user co phai admin cua group khong
        public bool IsAdmin(int groupId, int userId)
        {
            var member = GetByGroupAndUser(groupId, userId);
            return member != null && member.IsActive && member.Role == GroupRole.Admin;
        }

        // ================== STATISTICS ==================

        // Dem so luong members trong group
        public int CountMembers(int groupId, bool activeOnly = true)
        {
            var query = activeOnly
                ? "SELECT COUNT(*) FROM GroupMembers WHERE GroupId = @groupId AND IsActive = 1"
                : "SELECT COUNT(*) FROM GroupMembers WHERE GroupId = @groupId";
            
            var parameters = CreateParameters(("@groupId", groupId));
            
            var result = _db.ExecuteScalar(query, parameters);
            return result != null ? Convert.ToInt32(result) : 0;
        }

        // Dem so luong admins trong group
        public int CountAdmins(int groupId)
        {
            var query = @"SELECT COUNT(*) FROM GroupMembers 
                         WHERE GroupId = @groupId 
                         AND Role = 'Admin' 
                         AND IsActive = 1";
            
            var parameters = CreateParameters(("@groupId", groupId));
            
            var result = _db.ExecuteScalar(query, parameters);
            return result != null ? Convert.ToInt32(result) : 0;
        }
    }
}
