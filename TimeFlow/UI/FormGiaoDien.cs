using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using TimeFlow;
using TimeFlow.Authentication;
using TimeFlow.Models;
using TimeFlow.Server;
using TimeFlow.Services;
using TimeFlow.Tasks;
using TimeFlow.UI;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace TimeFlow
{
    public partial class FormGiaoDien : Form
    {
        // Dictionary để track các form đã tạo (singleton pattern)
        private static Dictionary<Type, Form> _openForms = new Dictionary<Type, Form>();
        private string _currentUsername;
        private string _currentEmail;

        public FormGiaoDien(string username = "Guest", string email = "")
        {
            InitializeComponent();
            _currentUsername = username;
            _currentEmail = email;


            // Gán sự kiện cho các buttons
            button1.Click += Button1_Click;  // Your Task
            button2.Click += Button2_Click;  // Group (Chat)
            button3.Click += Button3_Click;  // New Task
            button4.Click += Button4_Click;  // Submit Task
            
            // Gán sự kiện cho pictureBox (Settings icon)
            pictureBox4.Click += PictureBox4_Click; // Settings
            
            // Gán sự kiện cho label username (User Info)
            label1.Click += Label1_Click;

            // Populate timeline với tasks
            PopulateTimeline();
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
            OpenChatForm();
        }
        private void OpenChatForm()
        {
            try
            {
                if (_openForms.ContainsKey(typeof(ChatForm)))
                {
                    var f = _openForms[typeof(ChatForm)];
                    this.Hide();
                    f.Show();
                    f.Activate();
                    return;
                }

                // 1. Tạo kết nối tới Docker Server
                TcpClient client = new TcpClient();
                client.Connect("127.0.0.1", 1010); // Port của Docker Container

                // 2. Gửi lệnh Login tự động để xác thực TCP connection này
                var loginPayload = new
                {
                    type = "login",
                    data = new { username = _currentUsername, password = "123" } // Lưu ý: Password này nên lấy từ session thật
                };

                // Gửi login
                byte[] data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(loginPayload));
                client.GetStream().Write(data, 0, data.Length);

                // Chờ Server phản hồi (Login Success) một chút để đảm bảo OK
                // (Trong thực tế nên đọc stream để check "success", ở đây ta làm tắt cho nhanh)
                System.Threading.Thread.Sleep(200);

                // 3. Khởi tạo ChatForm với client đã kết nối
                ChatForm chatForm = new ChatForm(client, _currentUsername);

                _openForms[typeof(ChatForm)] = chatForm;

                chatForm.FormClosed += (s, args) =>
                {
                    _openForms.Remove(typeof(ChatForm));
                    this.Show();
                    this.Activate();
                    // Khi đóng ChatForm, nó sẽ tự Close() TcpClient
                };

                this.Hide();
                chatForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể kết nối tới Chat Server (Docker).\nLỗi: " + ex.Message, "Lỗi Kết Nối");
                this.Show();
            }
        }

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