using System;

namespace TimeFlow.Models
{
    // Event args để truyền dữ liệu cập nhật giữa các Form
    public class TaskUpdateEventArgs : EventArgs
    {
        public int TaskId { get; set; }
        public string Title { get; set; }
        public TimeFlow.Models.TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime? DueDate { get; set; }
    }
}