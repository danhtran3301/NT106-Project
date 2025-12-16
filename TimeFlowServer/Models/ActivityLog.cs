using System;

namespace TimeFlow.Models
{
    // Cac loai hoat dong trong he thong
    public enum ActivityType
    {
        Created,
        Updated,
        StatusChanged,
        Assigned,
        Completed,
        Commented,
        Deleted
    }

    // Ghi log cac hoat dong trong he thong
    public class ActivityLog
    {
        public int LogId { get; set; }
        public int? TaskId { get; set; }
        public int UserId { get; set; }
        public string ActivityType { get; set; } = string.Empty;
        public string ActivityDescription { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public TaskItem? Task { get; set; }
        public User User { get; set; } = null!;

        public ActivityLog()
        {
            CreatedAt = DateTime.Now;
        }

        // Tao activity log moi
        public static ActivityLog Create(int userId, int? taskId, string activityType, string description)
        {
            return new ActivityLog
            {
                UserId = userId,
                TaskId = taskId,
                ActivityType = activityType,
                ActivityDescription = description,
                CreatedAt = DateTime.Now
            };
        }

        // Hien thi thoi gian dang "5m ago"
        public string TimeAgo
        {
            get
            {
                var timeSpan = DateTime.Now - CreatedAt;
                
                if (timeSpan.TotalMinutes < 1)
                    return "just now";
                if (timeSpan.TotalMinutes < 60)
                    return $"{(int)timeSpan.TotalMinutes}m ago";
                if (timeSpan.TotalHours < 24)
                    return $"{(int)timeSpan.TotalHours}h ago";
                if (timeSpan.TotalDays < 30)
                    return $"{(int)timeSpan.TotalDays}d ago";
                
                return CreatedAt.ToString("MMM dd, yyyy");
            }
        }

        // Mo ta hoat dong co dinh dang
        public string FormattedDescription => $"{User?.DisplayName ?? "User"} {ActivityDescription}";
    }
}
