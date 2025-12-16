using System;

namespace TimeFlow.Models
{
    // Dai dien cho task duoc gan cho nhom
    public class GroupTask
    {
        public int GroupTaskId { get; set; }
        public int TaskId { get; set; }
        public int GroupId { get; set; }
        public int? AssignedTo { get; set; }
        public DateTime? AssignedAt { get; set; }
        public int? AssignedBy { get; set; }

        // Navigation properties
        public TaskItem Task { get; set; } = null!;
        public Group Group { get; set; } = null!;
        public User? AssignedUser { get; set; }
        public User? AssignerUser { get; set; }

        public GroupTask()
        {
        }

        // Kiem tra task da duoc assign chua
        public bool IsAssigned => AssignedTo.HasValue;

        // Ten nguoi duoc assign hoac "Unassigned"
        public string AssignedToName => AssignedUser?.DisplayName ?? "Unassigned";

        // Gan task cho user
        public void AssignTo(int userId, int assignedByUserId)
        {
            AssignedTo = userId;
            AssignedBy = assignedByUserId;
            AssignedAt = DateTime.Now;
        }

        // Huy assignment
        public void Unassign()
        {
            AssignedTo = null;
            AssignedAt = null;
            AssignedBy = null;
        }
    }
}
