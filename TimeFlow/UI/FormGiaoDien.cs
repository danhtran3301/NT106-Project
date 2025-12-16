using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TimeFlow.Authentication;
using TimeFlow.Server;
using TimeFlow.UI;
using TimeFlow.Tasks;
using TimeFlow.Models; // ✅ Đã dùng Model chuẩn của nhóm
using TimeFlow.Services;

namespace TimeFlow
{
    public partial class FormGiaoDien : Form
    {
        public static void EnableDoubleBuffered(Control control)
        {
            typeof(Control).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic,
                null, control, new object[] { true });
        }

        private int totalTaskCount = 0;
        private int completedTaskCount = 0;
        private List<TaskItem> userTasks = new List<TaskItem>();
        private DateTime currentSelectedDate = DateTime.Now;
        private TaskItem currentSelectedTask = null;

        public FormGiaoDien()
        {
            InitializeComponent();
            InitializeForm();
        }

        private void InitializeForm()
        {
            // Hiển thị thông tin user
            if (!string.IsNullOrEmpty(SessionManager.Username))
            {
                label1.Text = SessionManager.Username;
            }

            // Thiết lập ngày hiện tại
            label10.Text = currentSelectedDate.ToString("MMMM yyyy");

            // Setup calendar events
            monthCalendar1.DateChanged += monthCalendar1_DateChanged;
        }

        private void GiaoDien_Load(object sender, EventArgs e)
        {
            // 🔥 CHO PHÉP TEST KHÔNG LOGIN
            if (string.IsNullOrEmpty(SessionManager.Username))
            {
                SessionManager.Username = "TEST_USER";
            }
            // ✅ HẾT BỊ NHÁY/GIẬT
            EnableDoubleBuffered(tableLayoutPanel2);

            UpdateCalendarView();
            LoadTaskCountBadges();
        }

        private void LoadTaskCountBadges()
        {
            // ⚠️ LƯU Ý: Nếu 'IsCompleted' báo đỏ, hãy đổi thành tên biến đúng của nhóm (VD: Status == "Completed")
            totalTaskCount = userTasks.Count(t => t.Status != TimeFlow.Models.TaskStatus.Completed);
            completedTaskCount = userTasks.Count(t => t.Status == TimeFlow.Models.TaskStatus.Completed);
            button1.Text = $"Your Task ({totalTaskCount})";
            label12.Text = $"Pending tasks: {totalTaskCount} ⏳";
            label13.Text = $"Completed: {completedTaskCount} ✓";

            if (totalTaskCount > 0)
            {
                button1.BackColor = Color.LightBlue;
                button1.ForeColor = Color.DarkBlue;
            }
            else
            {
                button1.BackColor = SystemColors.ActiveCaption;
                button1.ForeColor = Color.Black;
            }
        }

        private void UpdateCalendarView()
        {
            tableLayoutPanel2.SuspendLayout();
            tableLayoutPanel2.Controls.Clear();

            DateTime startDate = currentSelectedDate;

            // --- DUYỆT QUA 7 CỘT (7 NGÀY) ---
            for (int col = 0; col < 7; col++)
            {
                DateTime columnDate = startDate.AddDays(col);

                Label lblHeader = new Label
                {
                    Text = columnDate.ToString("dd/MM/yyyy"),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    BackColor = Color.LightGray,
                    ForeColor = Color.Black,
                    BorderStyle = BorderStyle.FixedSingle
                };

                if (columnDate.Date == DateTime.Today)
                {
                    lblHeader.BackColor = Color.Orange;
                    lblHeader.ForeColor = Color.White;
                    lblHeader.Text += "\n(Today)";
                }

                tableLayoutPanel2.Controls.Add(lblHeader, col, 0);

                // ⚠️ LƯU Ý: Nếu 'Date' báo đỏ, kiểm tra xem nhóm dùng tên gì (VD: DueDate, CreatedAt)
                var dailyTasks = userTasks
                   .Where(t => t.DueDate.HasValue
                     && t.DueDate.Value.Date == columnDate.Date
                     && t.Status != TimeFlow.Models.TaskStatus.Completed)
                   .OrderBy(t => t.TaskId) // Đổi Id -> TaskId
                   .ToList();

                for (int row = 1; row < tableLayoutPanel2.RowCount; row++)
                {
                    int taskIndex = row - 1;
                    TaskItem taskToShow = null;
                    if (taskIndex < dailyTasks.Count)
                    {
                        taskToShow = dailyTasks[taskIndex];
                    }
                    AddDayCell(columnDate, col, row, taskToShow);
                }
            }

            tableLayoutPanel2.ResumeLayout();
        }

        private void AddDayCell(DateTime date, int col, int row, TaskItem task)
        {
            Panel dayCell = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Margin = new Padding(0)
            };

            Label lblContent = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand
            };

            if (task != null)
            {
                // ⚠️ LƯU Ý: Nếu 'Title' báo đỏ -> Đổi thành 'TaskName'
                lblContent.Text = task.Title;
                lblContent.Font = new Font("Segoe UI", 9, FontStyle.Regular);

                // ⚠️ Nếu 'Id' đỏ -> Đổi thành 'TaskId'
                if (currentSelectedTask != null && task.TaskId == currentSelectedTask.TaskId)
                {
                    lblContent.BackColor = Color.Orange;
                    lblContent.ForeColor = Color.White;
                    lblContent.BorderStyle = BorderStyle.FixedSingle;
                }
                else
                {
                    // ⚠️ Nếu 'IsCompleted' đỏ -> Kiểm tra logic Status của nhóm
                    bool isDone = task.Status == TimeFlow.Models.TaskStatus.Completed;
                    lblContent.BorderStyle = BorderStyle.None;
                }
            }
            else
            {
                lblContent.Text = "+ Click to add";
                lblContent.ForeColor = Color.Gray;
                lblContent.Font = new Font("Segoe UI", 8, FontStyle.Italic);
                lblContent.BackColor = Color.White;
            }

            lblContent.Click += (s, e) => OnCalendarCellClick(date, task);
            dayCell.Click += (s, e) => OnCalendarCellClick(date, task);

            dayCell.Controls.Add(lblContent);
            tableLayoutPanel2.Controls.Add(dayCell, col, row);
        }

        private void OnCalendarCellClick(DateTime date, TaskItem task)
        {
            if (task != null)
            {
                currentSelectedTask = task;
                UpdateCalendarView();
            }
            else
            {
                currentSelectedTask = null;
                UpdateCalendarView();
                OpenNewTaskFormForDate(date);
            }
        }

        private void OpenNewTaskFormForDate(DateTime selectedDate)
        {
            FormThemTask newTaskForm = new FormThemTask(this, selectedDate);
            if (newTaskForm.ShowDialog() == DialogResult.OK)
            {
                UpdateCalendarView();
                LoadTaskCountBadges();
            }
        }

        public void AddTaskFromForm(TaskItem newTask)
        {
            // ⚠️ LƯU Ý: Kiểm tra 'Id' có phải là 'TaskId' không?
            newTask.TaskId = userTasks.Count > 0 ? userTasks.Max(t => t.TaskId) + 1 : 1;
            userTasks.Add(newTask);

            UpdateCalendarView();
            LoadTaskCountBadges();

            // ⚠️ LƯU Ý: Kiểm tra 'Title' có phải là 'TaskName' không?
            MessageBox.Show($"Task '{newTask.Title}' added successfully!",
                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void OpenNewTaskForm()
        {
            FormThemTask newTaskForm = new FormThemTask(this);
            if (newTaskForm.ShowDialog() == DialogResult.OK)
            {
                UpdateCalendarView();
                LoadTaskCountBadges();
            }
        }

        private void monthCalendar1_DateChanged(object sender, DateRangeEventArgs e)
        {
            currentSelectedDate = e.Start;
            label10.Text = currentSelectedDate.ToString("dd/MM/yyyy");
            UpdateCalendarView();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            monthCalendar1.SetDate(monthCalendar1.SelectionStart.AddMonths(-1));
            UpdateCalendarView();
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            monthCalendar1.SetDate(monthCalendar1.SelectionStart.AddMonths(1));
            UpdateCalendarView();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenNewTaskForm();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            NavigateToTaskList();
        }

        private void NavigateToTaskList()
        {
            // Lọc những task có trạng thái KHÁC "Completed" (Đã xong)
            var pendingTasks = userTasks
                .Where(t => t.Status != TimeFlow.Models.TaskStatus.Completed)
                .ToList();

            if (pendingTasks.Any())
            {
                string taskList = "📋 Your Pending Tasks:\n\n";
                foreach (var task in pendingTasks)
                {
                    string dateString = task.DueDate.HasValue
                        ? task.DueDate.Value.ToString("dd/MM")
                        : "N/A";

                    // Hiển thị Title (Model nhóm dùng Title nên giữ nguyên, nếu lỗi đổi thành TaskName)
                    taskList += $"• {task.Title} (Hạn: {dateString}) ⏳\n";
                }

                MessageBox.Show(taskList, "Task List", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Tuyệt vời! Bạn không còn task nào cần xử lý.",
                    "All Clear", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            FormThongTinNguoiDung profileForm = new FormThongTinNguoiDung(SessionManager.Username, SessionManager.Email);
            profileForm.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "Choose an option:\n\nYes = Groups Management\nNo = Chat",
                "Group Features",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);

            switch (result)
            {
                case DialogResult.Yes:
                    NavigateToGroups();
                    break;
                case DialogResult.No:
                    NavigateToChat();
                    break;
                case DialogResult.Cancel:
                    break;
            }
        }

        private void NavigateToGroups()
        {
            MessageBox.Show("Navigate to Groups Management - Feature coming soon!",
                "Groups", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void NavigateToChat()
        {
            MessageBox.Show("Navigate to Chat - Feature coming soon!",
                "Chat", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Empty event handlers
        private void label1_Click(object sender, EventArgs e) { }
        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e) { }
        private void label2_Click(object sender, EventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }
        private void panel3_Paint(object sender, PaintEventArgs e) { }
        private void label11_Click(object sender, EventArgs e) { }
        private void label12_Click(object sender, EventArgs e) { }
        private void label13_Click(object sender, EventArgs e) { }
        private void panel2_Paint(object sender, PaintEventArgs e) { }
        private void label4_Click(object sender, EventArgs e) { }
        private void label10_Click(object sender, EventArgs e) { }
        private void label13_Click_1(object sender, EventArgs e) { }

        private void button4_Click(object sender, EventArgs e)
        {
            if (currentSelectedTask == null)
            {
                MessageBox.Show("Vui lòng chọn task cần submit!");
                return;
            }

            DialogResult result = MessageBox.Show($"Xác nhận hoàn thành task: {currentSelectedTask.Title}?",
                "Xác nhận", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                // 1. Đánh dấu task đã xong (⚠️ Có thể cần đổi IsCompleted thành Status = "Done")
                currentSelectedTask.Status = TimeFlow.Models.TaskStatus.Completed;
                // 2. Reset biến chọn
                currentSelectedTask = null;

                // 3. Cập nhật UI
                UpdateCalendarView();
                LoadTaskCountBadges();
            }
        }
    }

}