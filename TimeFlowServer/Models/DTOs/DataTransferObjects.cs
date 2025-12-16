using System;

namespace TimeFlow.Models.DTOs
{
    // DTO cho yeu cau dang nhap
    public class LoginRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // DTO cho yeu cau dang ky
    public class RegisterRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? FullName { get; set; }
    }

    // DTO cho ket qua xac thuc
    public class AuthResponse
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Message { get; set; }
    }

    // DTO cho yeu cau tao task moi
    public class CreateTaskRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public int Priority { get; set; } = 2; // Mac dinh: Medium
        public int Status { get; set; } = 1;   // Mac dinh: Pending
        public int? CategoryId { get; set; }
        public bool IsGroupTask { get; set; }
        public int? GroupId { get; set; }
        public int? AssignedTo { get; set; }
    }

    // DTO cho yeu cau cap nhat task
    public class UpdateTaskRequest
    {
        public int TaskId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public int? Priority { get; set; }
        public int? Status { get; set; }
        public int? CategoryId { get; set; }
    }

    // DTO cho thong tin tom tat task
    public class TaskSummary
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        public string? CategoryColor { get; set; }
        public bool IsGroupTask { get; set; }
        public string? GroupName { get; set; }
        public int? AssignedTo { get; set; }
        public string? AssignedToName { get; set; }
    }

    // DTO cho thong ke cua user
    public class UserStatistics
    {
        public int TotalTasks { get; set; }
        public int PendingTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int OverdueTasks { get; set; }
        public int GroupsCount { get; set; }
    }

    // DTO cho yeu cau tao nhom moi
    public class CreateGroupRequest
    {
        public string GroupName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    // DTO cho yeu cau them thanh vien vao nhom
    public class AddGroupMemberRequest
    {
        public int GroupId { get; set; }
        public int UserId { get; set; }
        public string Role { get; set; } = "Member"; // "Admin" hoac "Member"
    }

    // DTO cho thong tin thanh vien nhom
    public class GroupMemberInfo
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
    }

    // DTO cho thong tin nhom voi so luong thanh vien
    public class GroupInfo
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int MemberCount { get; set; }
        public int TaskCount { get; set; }
        public string CreatedByUsername { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string UserRole { get; set; } = string.Empty; // Vai tro cua user hien tai trong nhom
    }

    // DTO cho yeu cau them binh luan
    public class AddCommentRequest
    {
        public int TaskId { get; set; }
        public string CommentText { get; set; } = string.Empty;
    }

    // DTO cho hien thi binh luan
    public class CommentDisplay
    {
        public int CommentId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string CommentText { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string TimeAgo { get; set; } = string.Empty;
        public bool IsEdited { get; set; }
    }
}
