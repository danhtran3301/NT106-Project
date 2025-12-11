using System;
using System.Collections.Generic;

namespace TimeFlow.Models
{
    /// <summary>
    /// Model ??i di?n cho m?t Task trong h? th?ng
    /// </summary>
    public class TaskModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public TaskState Status { get; set; }
        public TaskPriorityLevel Priority { get; set; }
        public int AssigneeCount { get; set; }
        public List<string> Assignees { get; set; }
        public int Progress { get; set; } // 0-100
        public List<TaskComment> Comments { get; set; }
        public List<TaskActivity> Activities { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? CompletedDate { get; set; }

        public TaskModel()
        {
            Assignees = new List<string>();
            Comments = new List<TaskComment>();
            Activities = new List<TaskActivity>();
            CreatedDate = DateTime.Now;
        }

        // Helper properties
        public string StatusText => Status switch
        {
            TaskState.Pending => "Pending",
            TaskState.InProgress => "In Progress",
            TaskState.Completed => "Completed",
            TaskState.Cancelled => "Cancelled",
            _ => "Unknown"
        };

        public string PriorityText => Priority switch
        {
            TaskPriorityLevel.Low => "Low",
            TaskPriorityLevel.Medium => "Medium",
            TaskPriorityLevel.High => "High",
            TaskPriorityLevel.Critical => "Critical",
            _ => "Unknown"
        };

        public string DueDateText => DueDate.ToString("MMM dd, yyyy");
    }

    public enum TaskState
    {
        Pending,
        InProgress,
        Completed,
        Cancelled
    }

    public enum TaskPriorityLevel
    {
        Low,
        Medium,
        High,
        Critical
    }

    public class TaskComment
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }

        public string TimeAgo
        {
            get
            {
                var span = DateTime.Now - CreatedDate;
                if (span.TotalMinutes < 1) return "just now";
                if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} minutes ago";
                if (span.TotalHours < 24) return $"{(int)span.TotalHours} hours ago";
                if (span.TotalDays < 7) return $"{(int)span.TotalDays} days ago";
                return CreatedDate.ToString("MMM dd, yyyy");
            }
        }
    }

    public class TaskActivity
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }

        public string TimeAgo
        {
            get
            {
                var span = DateTime.Now - CreatedDate;
                if (span.TotalMinutes < 1) return "just now";
                if (span.TotalMinutes < 60) return $"{(int)span.TotalMinutes} minutes ago";
                if (span.TotalHours < 24) return $"{(int)span.TotalHours} hours ago";
                if (span.TotalDays < 7) return $"{(int)span.TotalDays} days ago";
                return CreatedDate.ToString("MMM dd, yyyy");
            }
        }
    }
}
