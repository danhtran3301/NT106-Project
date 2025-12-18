using System;

namespace TimeFlow.Models
{
    // Muc do uu tien cua task
    public enum TaskPriority
    {
        Low = 1,
        Medium = 2,
        High = 3
    }

    // Trang thai cua task
    public enum TaskStatus
    {
        Pending = 1,
        InProgress = 2,
        Completed = 3,
        Cancelled = 4
    }

    // Dai dien cho mot cong viec (ca nhan hoac nhom)
    public class TaskItem
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public TaskPriority Priority { get; set; }
        public TaskStatus Status { get; set; }
        public int? CategoryId { get; set; }
        public int CreatedBy { get; set; }
        public bool IsGroupTask { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Category? Category { get; set; }
        public User Creator { get; set; } = null!;
        public List<Comment> Comments { get; set; } = new();
        public List<ActivityLog> ActivityLogs { get; set; } = new();
        public GroupTask? GroupTask { get; set; }

        public TaskItem()
        {
            CreatedAt = DateTime.Now;
            Priority = TaskPriority.Medium;
            Status = TaskStatus.Pending;
            IsGroupTask = false;
        }

        // Kiem tra task co qua han khong
        public bool IsOverdue => DueDate.HasValue && 
                                 DueDate.Value < DateTime.Now && 
                                 Status != TaskStatus.Completed && 
                                 Status != TaskStatus.Cancelled;

        // So ngay con lai den han (am neu qua han)
        public int? DaysUntilDue => DueDate.HasValue ? (int)(DueDate.Value - DateTime.Now).TotalDays : null;

        // Text hien thi trang thai
        public string StatusText => Status switch
        {
            TaskStatus.Pending => "Pending",
            TaskStatus.InProgress => "In Progress",
            TaskStatus.Completed => "Completed",
            TaskStatus.Cancelled => "Cancelled",
            _ => "Unknown"
        };

        // Text hien thi do uu tien
        public string PriorityText => Priority switch
        {
            TaskPriority.Low => "Low",
            TaskPriority.Medium => "Medium",
            TaskPriority.High => "High",
            _ => "Unknown"
        };

        // Mau hien thi priority (hex)
        public string PriorityColor => Priority switch
        {
            TaskPriority.Low => "#10B981",    // Green
            TaskPriority.Medium => "#F97316", // Orange
            TaskPriority.High => "#EF4444",   // Red
            _ => "#6B7280"
        };

        // Mau hien thi status (hex)
        public string StatusColor => Status switch
        {
            TaskStatus.Pending => "#F59E0B",    // Yellow
            TaskStatus.InProgress => "#3B82F6", // Blue
            TaskStatus.Completed => "#10B981",  // Green
            TaskStatus.Cancelled => "#6B7280",  // Gray
            _ => "#6B7280"
        };

        // Danh dau task hoan thanh
        public void MarkAsCompleted()
        {
            Status = TaskStatus.Completed;
            CompletedAt = DateTime.Now;
            UpdatedAt = DateTime.Now;
        }

        // Kiem tra user co quyen edit task khong
        public bool CanEdit(int userId)
        {
            return CreatedBy == userId || IsGroupTask;
        }
    }
}
