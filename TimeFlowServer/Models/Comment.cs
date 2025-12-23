using System;

namespace TimeFlow.Models
{
    // Binh luan tren task
    public class Comment
    {
        public int CommentId { get; set; }
        public int TaskId { get; set; }
        public int UserId { get; set; }
        public string CommentText { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsEdited { get; set; }

        // User info for display
        public string? Username { get; set; }
        public string? FullName { get; set; }

        // Navigation properties
        public TaskItem Task { get; set; } = null!;
        public User User { get; set; } = null!;

        public Comment()
        {
            CreatedAt = DateTime.Now;
            IsEdited = false;
        }

        // Hien thi ten user (uu tien FullName)
        public string DisplayName => !string.IsNullOrEmpty(FullName) ? FullName : Username ?? "Unknown";

        // Hien thi thoi gian dang "5 minutes ago"
        public string TimeAgo
        {
            get
            {
                var timeSpan = DateTime.Now - CreatedAt;
                
                if (timeSpan.TotalMinutes < 1)
                    return "just now";
                if (timeSpan.TotalMinutes < 60)
                    return $"{(int)timeSpan.TotalMinutes} minutes ago";
                if (timeSpan.TotalHours < 24)
                    return $"{(int)timeSpan.TotalHours} hours ago";
                if (timeSpan.TotalDays < 30)
                    return $"{(int)timeSpan.TotalDays} days ago";
                if (timeSpan.TotalDays < 365)
                    return $"{(int)(timeSpan.TotalDays / 30)} months ago";
                
                return $"{(int)(timeSpan.TotalDays / 365)} years ago";
            }
        }

        // Cap nhat text va danh dau la da sua
        public void UpdateText(string newText)
        {
            CommentText = newText;
            UpdatedAt = DateTime.Now;
            IsEdited = true;
        }

        // Kiem tra user co quyen edit comment khong
        public bool CanEdit(int userId)
        {
            return UserId == userId;
        }
    }
}
