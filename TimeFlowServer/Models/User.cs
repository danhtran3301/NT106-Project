using System;

namespace TimeFlow.Models
{
    // Dai dien cho nguoi dung trong he thong
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLoginAt { get; set; }

        // Navigation properties
        public List<TaskItem> CreatedTasks { get; set; } = new();
        public List<Group> CreatedGroups { get; set; } = new();
        public List<GroupMember> GroupMemberships { get; set; } = new();
        public List<Comment> Comments { get; set; } = new();
        public List<UserToken> Tokens { get; set; } = new();

        public User()
        {
            CreatedAt = DateTime.Now;
            IsActive = true;
        }

        // Lay ten hien thi (FullName neu co, khong thi Username)
        public string DisplayName => !string.IsNullOrWhiteSpace(FullName) ? FullName : Username;

        // Kiem tra user co avatar khong
        public bool HasAvatar => !string.IsNullOrWhiteSpace(AvatarUrl);
    }
}
