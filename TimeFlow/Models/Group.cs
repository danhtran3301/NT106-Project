using System;

namespace TimeFlow.Models
{
    // Dai dien cho mot nhom lam viec
    public class Group
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }

        // Navigation properties
        public User Creator { get; set; } = null!;
        public List<GroupMember> Members { get; set; } = new();
        public List<GroupTask> GroupTasks { get; set; } = new();

        public Group()
        {
            CreatedAt = DateTime.Now;
            IsActive = true;
        }

        // So luong thanh vien dang hoat dong
        public int ActiveMemberCount => Members.Count(m => m.IsActive);

        // So luong task trong nhom
        public int TaskCount => GroupTasks.Count;

        // Kiem tra user co phai member cua nhom khong
        public bool IsMember(int userId)
        {
            return Members.Any(m => m.UserId == userId && m.IsActive);
        }

        // Kiem tra user co phai admin cua nhom khong
        public bool IsAdmin(int userId)
        {
            return Members.Any(m => m.UserId == userId && m.Role == GroupRole.Admin && m.IsActive);
        }
    }
}
