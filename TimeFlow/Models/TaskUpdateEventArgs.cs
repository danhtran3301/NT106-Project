using System;

namespace TimeFlow.Models
{
    // Event args for task updates
    public class TaskUpdateEventArgs : EventArgs
    {
        public int TaskId { get; set; }
        public TimeFlow.Models.TaskStatus Status { get; set; }
    }
}
