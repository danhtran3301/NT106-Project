using System;
using System.Collections.Generic;

namespace TimeFlow.Models
{
    // View model cho task detail với đầy đủ thông tin
    public class TaskDetailViewModel
    {
        // Basic task info
        public int TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public TaskPriority Priority { get; set; }
        public TimeFlow.Models.TaskStatus Status { get; set; }
        public bool IsGroupTask { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? CategoryId { get; set; }

        // Extended info
        public string CategoryName { get; set; } = "Other";
        public string CategoryColor { get; set; } = "#6B7280";
        public List<string> Assignees { get; set; } = new();
        public int Progress { get; set; }

        // Comments
        public List<CommentViewModel> Comments { get; set; } = new();

        // Activities
        public List<ActivityViewModel> Activities { get; set; } = new();

        // Computed properties
        public string StatusText => Status switch
        {
            TimeFlow.Models.TaskStatus.Pending => "Pending",
            TimeFlow.Models.TaskStatus.InProgress => "In Progress",
            TimeFlow.Models.TaskStatus.Completed => "Completed",
            TimeFlow.Models.TaskStatus.Cancelled => "Cancelled",
            _ => "Unknown"
        };

        public string PriorityText => Priority switch
        {
            TaskPriority.Low => "Low",
            TaskPriority.Medium => "Medium",
            TaskPriority.High => "High",
            _ => "Unknown"
        };

        public string DueDateText => DueDate.HasValue 
            ? DueDate.Value.ToString("MMM dd, yyyy") 
            : "No due date";

        public bool HasAssignees => Assignees.Count > 0;
        public bool HasComments => Comments.Count > 0;
        public bool HasActivities => Activities.Count > 0;
    }

    public class CommentViewModel
    {
        public int CommentId { get; set; }
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? FullName { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsEdited { get; set; }

        public string DisplayName => !string.IsNullOrEmpty(FullName) ? FullName : Username ?? "Unknown";

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
                if (timeSpan.TotalDays < 7)
                    return $"{(int)timeSpan.TotalDays} days ago";
                
                return CreatedAt.ToString("MMM dd, yyyy");
            }
        }
    }

    public class ActivityViewModel
    {
        public int LogId { get; set; }
        public int UserId { get; set; }
        public string ActivityType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

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
                if (timeSpan.TotalDays < 7)
                    return $"{(int)timeSpan.TotalDays} days ago";
                
                return CreatedAt.ToString("MMM dd, yyyy");
            }
        }
    }
}
