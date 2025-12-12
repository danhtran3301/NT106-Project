using System;

namespace TimeFlow.Models
{
    // Vai tro trong nhom
    public enum GroupRole
    {
        Member,
        Admin
    }

    // Dai dien cho thanh vien trong nhom
    public class GroupMember
    {
        public int GroupMemberId { get; set; }
        public int GroupId { get; set; }
        public int UserId { get; set; }
        public GroupRole Role { get; set; }
        public DateTime JoinedAt { get; set; }
        public bool IsActive { get; set; }

        // Navigation properties
        public Group Group { get; set; } = null!;
        public User User { get; set; } = null!;

        public GroupMember()
        {
            JoinedAt = DateTime.Now;
            Role = GroupRole.Member;
            IsActive = true;
        }

        // Text hien thi vai tro
        public string RoleText => Role == GroupRole.Admin ? "Admin" : "Member";

        // Kiem tra co phai admin khong
        public bool IsAdmin => Role == GroupRole.Admin;

        // So ngay da tham gia
        public int MembershipDays => (int)(DateTime.Now - JoinedAt).TotalDays;
    }
}
