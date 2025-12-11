using System;
using System.Collections.Generic;
using System.Linq;
using TimeFlow.Models;

namespace TimeFlow.Services
{
    /// <summary>
    /// Service quản lý tasks - hiện tại dùng in-memory data
    /// TODO: Tích hợp với database/API
    /// </summary>
    public static class TaskManager
    {
        private static List<TaskModel> _tasks;
        private static int _nextId = 11;

        // Event để notify khi task thay đổi
        public static event EventHandler<TaskModel> TaskCreated;
        public static event EventHandler<TaskModel> TaskUpdated;
        public static event EventHandler<int> TaskDeleted;

        static TaskManager()
        {
            InitializeMockData();
        }

        /// <summary>
        /// Khởi tạo dữ liệu task giả
        /// </summary>
        private static void InitializeMockData()
        {
            _tasks = new List<TaskModel>
            {
                new TaskModel
                {
                    Id = 1,
                    Name = "Design a new dashboard for the mobile app",
                    Description = "The current dashboard design is outdated and doesn't provide a good user experience. We need to create a new design that is modern, intuitive, and visually appealing. The new design should include a clear information hierarchy, data visualizations, and easy navigation.\n\nKey requirements:\n• User-friendly interface with a clean layout\n• Interactive charts and graphs for data visualization\n• Customizable widgets for personalization\n• Responsive design for various screen sizes",
                    DueDate = new DateTime(2025, 12, 15),
                    Status = TaskState.InProgress,
                    Priority = TaskPriorityLevel.High,
                    AssigneeCount = 4,
                    Assignees = new List<string> { "Alice", "Bob", "Charlie", "Diana" },
                    Progress = 75,
                    CreatedDate = DateTime.Now.AddDays(-10),
                    Comments = new List<TaskComment>
                    {
                        new TaskComment { Id = 1, Username = "Diana", Content = "Can we make sure the dark mode colors are consistent with the web version?", CreatedDate = DateTime.Now.AddHours(-3) },
                        new TaskComment { Id = 2, Username = "Charlie", Content = "Great progress! I've attached the latest wireframes.", CreatedDate = DateTime.Now.AddHours(-1) }
                    },
                    Activities = new List<TaskActivity>
                    {
                        new TaskActivity { Id = 1, Description = "Alice assigned this task to Bob.", CreatedDate = DateTime.Now.AddDays(-2) },
                        new TaskActivity { Id = 2, Description = "Charlie changed the due date to Dec 15, 2025.", CreatedDate = DateTime.Now.AddDays(-1) },
                        new TaskActivity { Id = 3, Description = "Diana left a comment.", CreatedDate = DateTime.Now.AddHours(-3) }
                    }
                },
                new TaskModel
                {
                    Id = 2,
                    Name = "Database Schema (CSDL) Deadline",
                    Description = "Complete the database schema design for the project. This includes defining tables, relationships, indexes, and constraints. Review with the team before implementation.",
                    DueDate = new DateTime(2025, 11, 1),
                    Status = TaskState.Pending,
                    Priority = TaskPriorityLevel.Medium,
                    AssigneeCount = 1,
                    Assignees = new List<string> { "Bob" },
                    Progress = 25,
                    CreatedDate = DateTime.Now.AddDays(-15),
                    Comments = new List<TaskComment>
                    {
                        new TaskComment { Id = 3, Username = "Bob", Content = "I've started working on the ERD diagram.", CreatedDate = DateTime.Now.AddHours(-5) }
                    },
                    Activities = new List<TaskActivity>
                    {
                        new TaskActivity { Id = 4, Description = "Task created by Alice.", CreatedDate = DateTime.Now.AddDays(-15) },
                        new TaskActivity { Id = 5, Description = "Bob accepted the assignment.", CreatedDate = DateTime.Now.AddDays(-14) }
                    }
                },
                new TaskModel
                {
                    Id = 3,
                    Name = "Philosophy Theory Preparation",
                    Description = "Prepare presentation materials for philosophy theory class. Include key concepts, historical context, and practical examples.",
                    DueDate = new DateTime(2025, 12, 21),
                    Status = TaskState.Completed,
                    Priority = TaskPriorityLevel.Low,
                    AssigneeCount = 2,
                    Assignees = new List<string> { "Charlie", "Diana" },
                    Progress = 100,
                    CreatedDate = DateTime.Now.AddDays(-20),
                    CompletedDate = DateTime.Now.AddDays(-1),
                    Comments = new List<TaskComment>
                    {
                        new TaskComment { Id = 4, Username = "Charlie", Content = "Presentation is ready for review!", CreatedDate = DateTime.Now.AddDays(-2) }
                    },
                    Activities = new List<TaskActivity>
                    {
                        new TaskActivity { Id = 6, Description = "Charlie completed the task.", CreatedDate = DateTime.Now.AddDays(-1) }
                    }
                },
                new TaskModel
                {
                    Id = 4,
                    Name = "Submit Q4 Report",
                    Description = "Compile and submit the quarterly report including financial data, project updates, and performance metrics.",
                    DueDate = new DateTime(2025, 11, 30),
                    Status = TaskState.Pending,
                    Priority = TaskPriorityLevel.High,
                    AssigneeCount = 1,
                    Assignees = new List<string> { "Alice" },
                    Progress = 10,
                    CreatedDate = DateTime.Now.AddDays(-5),
                    Comments = new List<TaskComment>(),
                    Activities = new List<TaskActivity>
                    {
                        new TaskActivity { Id = 7, Description = "Task assigned to Alice.", CreatedDate = DateTime.Now.AddDays(-5) }
                    }
                },
                new TaskModel
                {
                    Id = 5,
                    Name = "Team Building Event Planning",
                    Description = "Organize team building event including venue booking, activities planning, and budget management. Aim for December first week.",
                    DueDate = new DateTime(2025, 12, 5),
                    Status = TaskState.InProgress,
                    Priority = TaskPriorityLevel.Medium,
                    AssigneeCount = 4,
                    Assignees = new List<string> { "Alice", "Bob", "Charlie", "Diana" },
                    Progress = 60,
                    CreatedDate = DateTime.Now.AddDays(-12),
                    Comments = new List<TaskComment>
                    {
                        new TaskComment { Id = 5, Username = "Alice", Content = "I've found a great venue! Sending details.", CreatedDate = DateTime.Now.AddHours(-8) }
                    },
                    Activities = new List<TaskActivity>
                    {
                        new TaskActivity { Id = 8, Description = "Venue options researched.", CreatedDate = DateTime.Now.AddDays(-3) }
                    }
                },
                new TaskModel
                {
                    Id = 6,
                    Name = "Review design docs",
                    Description = "Review the latest design documentation and provide feedback on UI/UX improvements.",
                    DueDate = new DateTime(2025, 12, 10),
                    Status = TaskState.InProgress,
                    Priority = TaskPriorityLevel.Medium,
                    AssigneeCount = 1,
                    Assignees = new List<string> { "Bob" },
                    Progress = 40,
                    CreatedDate = DateTime.Now.AddDays(-8),
                    Comments = new List<TaskComment>(),
                    Activities = new List<TaskActivity>()
                },
                new TaskModel
                {
                    Id = 7,
                    Name = "Prepare presentation slides",
                    Description = "Create PowerPoint presentation for the upcoming client meeting. Include project overview, timeline, and deliverables.",
                    DueDate = new DateTime(2025, 12, 12),
                    Status = TaskState.Pending,
                    Priority = TaskPriorityLevel.Low,
                    AssigneeCount = 2,
                    Assignees = new List<string> { "Charlie", "Diana" },
                    Progress = 0,
                    CreatedDate = DateTime.Now.AddDays(-4),
                    Comments = new List<TaskComment>(),
                    Activities = new List<TaskActivity>()
                },
                new TaskModel
                {
                    Id = 8,
                    Name = "Final API integration",
                    Description = "Complete the integration of payment gateway API with error handling and logging. Test thoroughly before deployment.",
                    DueDate = new DateTime(2025, 12, 18),
                    Status = TaskState.InProgress,
                    Priority = TaskPriorityLevel.High,
                    AssigneeCount = 3,
                    Assignees = new List<string> { "Alice", "Bob", "Charlie" },
                    Progress = 85,
                    CreatedDate = DateTime.Now.AddDays(-18),
                    Comments = new List<TaskComment>
                    {
                        new TaskComment { Id = 6, Username = "Bob", Content = "Payment flow is working, testing edge cases now.", CreatedDate = DateTime.Now.AddHours(-6) }
                    },
                    Activities = new List<TaskActivity>
                    {
                        new TaskActivity { Id = 9, Description = "Initial integration completed.", CreatedDate = DateTime.Now.AddDays(-5) }
                    }
                },
                new TaskModel
                {
                    Id = 9,
                    Name = "Database backup",
                    Description = "Set up automated daily database backup system with 30-day retention policy.",
                    DueDate = new DateTime(2025, 12, 25),
                    Status = TaskState.Completed,
                    Priority = TaskPriorityLevel.Low,
                    AssigneeCount = 1,
                    Assignees = new List<string> { "Bob" },
                    Progress = 100,
                    CreatedDate = DateTime.Now.AddDays(-25),
                    CompletedDate = DateTime.Now.AddDays(-3),
                    Comments = new List<TaskComment>
                    {
                        new TaskComment { Id = 7, Username = "Bob", Content = "Backup system is live and tested!", CreatedDate = DateTime.Now.AddDays(-3) }
                    },
                    Activities = new List<TaskActivity>
                    {
                        new TaskActivity { Id = 10, Description = "Backup system deployed.", CreatedDate = DateTime.Now.AddDays(-3) }
                    }
                },
                new TaskModel
                {
                    Id = 10,
                    Name = "Holiday Planning",
                    Description = "Plan team holiday schedule, coordinate with HR, and ensure project coverage during holiday season.",
                    DueDate = new DateTime(2026, 1, 1),
                    Status = TaskState.Pending,
                    Priority = TaskPriorityLevel.Medium,
                    AssigneeCount = 4,
                    Assignees = new List<string> { "Alice", "Bob", "Charlie", "Diana" },
                    Progress = 15,
                    CreatedDate = DateTime.Now.AddDays(-7),
                    Comments = new List<TaskComment>(),
                    Activities = new List<TaskActivity>
                    {
                        new TaskActivity { Id = 11, Description = "Task created for holiday coordination.", CreatedDate = DateTime.Now.AddDays(-7) }
                    }
                }
            };
        }

        /// <summary>
        /// Lấy tất cả tasks
        /// </summary>
        public static List<TaskModel> GetAllTasks()
        {
            return _tasks.ToList();
        }

        /// <summary>
        /// Lấy task theo ID
        /// </summary>
        public static TaskModel GetTaskById(int id)
        {
            return _tasks.FirstOrDefault(t => t.Id == id);
        }

        /// <summary>
        /// Lấy tasks theo status
        /// </summary>
        public static List<TaskModel> GetTasksByStatus(TaskState status)
        {
            return _tasks.Where(t => t.Status == status).ToList();
        }

        /// <summary>
        /// Lấy tasks theo priority
        /// </summary>
        public static List<TaskModel> GetTasksByPriority(TaskPriorityLevel priority)
        {
            return _tasks.Where(t => t.Priority == priority).ToList();
        }

        /// <summary>
        /// Tạo task mới
        /// </summary>
        public static TaskModel CreateTask(TaskModel task)
        {
            task.Id = _nextId++;
            task.CreatedDate = DateTime.Now;
            _tasks.Add(task);
            TaskCreated?.Invoke(null, task);
            return task;
        }

        /// <summary>
        /// Cập nhật task
        /// </summary>
        public static bool UpdateTask(TaskModel task)
        {
            var existingTask = _tasks.FirstOrDefault(t => t.Id == task.Id);
            if (existingTask == null) return false;

            var index = _tasks.IndexOf(existingTask);
            _tasks[index] = task;
            TaskUpdated?.Invoke(null, task);
            return true;
        }

        /// <summary>
        /// Xóa task
        /// </summary>
        public static bool DeleteTask(int id)
        {
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task == null) return false;

            _tasks.Remove(task);
            TaskDeleted?.Invoke(null, id);
            return true;
        }

        /// <summary>
        /// Thêm comment vào task
        /// </summary>
        public static bool AddComment(int taskId, string username, string content)
        {
            var task = GetTaskById(taskId);
            if (task == null) return false;

            var comment = new TaskComment
            {
                Id = task.Comments.Count + 1,
                Username = username,
                Content = content,
                CreatedDate = DateTime.Now
            };

            task.Comments.Add(comment);
            TaskUpdated?.Invoke(null, task);
            return true;
        }

        /// <summary>
        /// Thêm activity vào task
        /// </summary>
        public static bool AddActivity(int taskId, string description)
        {
            var task = GetTaskById(taskId);
            if (task == null) return false;

            var activity = new TaskActivity
            {
                Id = task.Activities.Count + 1,
                Description = description,
                CreatedDate = DateTime.Now
            };

            task.Activities.Add(activity);
            TaskUpdated?.Invoke(null, task);
            return true;
        }
    }
}
