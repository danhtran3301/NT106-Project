using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
using TimeFlow.Authentication;
using TimeFlow.Models;
using TimeFlow.UI;

namespace TimeFlow
{
    public partial class FormGiaoDien : Form
    {
        // --- KHAI BÁO BIẾN ---
        private static Dictionary<Type, Form> _openForms = new Dictionary<Type, Form>();
        private string _currentUsername;
        private string _currentEmail;

        private List<TaskItem> userTasks = new List<TaskItem>();
        private DateTime currentSelectedDate = DateTime.Now;
        private TaskItem currentSelectedTask = null;
        private int totalTaskCount = 0;
        private int completedTaskCount = 0;

        // --- CONSTRUCTOR ---
        public FormGiaoDien(string username = "Guest", string email = "")
        {
            InitializeComponent(); // Designer sẽ tự gán sự kiện tại đây
            _currentUsername = username;
            _currentEmail = email;

            InitializeForm();
            PopulateTimeline();
        }

        private void InitializeForm()
        {
            if (label1 != null && !string.IsNullOrEmpty(_currentUsername))
            {
                label1.Text = _currentUsername;
            }
            if (label10 != null)
                label10.Text = currentSelectedDate.ToString("MMMM yyyy");
        }

        private void GiaoDien_Load(object sender, EventArgs e)
        {
            if (tableLayoutPanel2 != null) EnableDoubleBuffered(tableLayoutPanel2);
            UpdateCalendarView();
            LoadTaskCountBadges();
        }

        // --- CÁC HÀM SỰ KIỆN CLICK (Tên khớp với Designer) ---

        // Nút "Your Task"
        private void button1_Click(object sender, EventArgs e)
        {
            NavigateToTaskList();
        }

        // Nút "Group" (Mở Chat)
        private void button2_Click(object sender, EventArgs e)
        {
            OpenChatForm();
        }

        // Nút "New Task"
        private void button3_Click(object sender, EventArgs e)
        {
            OpenNewTaskFormForDate(DateTime.Today);
        }

        // Nút "Submit Task"
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
                currentSelectedTask.Status = Models.TaskStatus.Completed;
                currentSelectedTask = null;
                UpdateCalendarView();
                LoadTaskCountBadges();
            }
        }

        // Sự kiện click vào Avatar (pictureBox1)
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            // Mở form thông tin người dùng (tái sử dụng hàm Label1_Click)
            Label1_Click(sender, e);
        }

        // --- CÁC HÀM LOGIC CHỨC NĂNG ---

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

                TcpClient client = new TcpClient();
                // Lưu ý: Đảm bảo Port 1010 khớp với Server Docker
                client.Connect("127.0.0.1", 1010);

                // Gửi login tự động
                var loginPayload = new
                {
                    type = "login",
                    data = new { username = _currentUsername, password = "123" }
                };
                byte[] data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(loginPayload));
                client.GetStream().Write(data, 0, data.Length);

                System.Threading.Thread.Sleep(200);

                ChatForm chatForm = new ChatForm(client, _currentUsername);
                _openForms[typeof(ChatForm)] = chatForm;

                chatForm.FormClosed += (s, args) =>
                {
                    _openForms.Remove(typeof(ChatForm));
                    this.Show();
                    this.Activate();
                };

                this.Hide();
                chatForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối Chat Server: " + ex.Message + "\nHãy đảm bảo Docker Server đang chạy.");
                this.Show();
            }
        }

        public void AddTaskFromForm(TaskItem newTask)
        {
            userTasks.Add(newTask);
            UpdateCalendarView();
            LoadTaskCountBadges();
            MessageBox.Show($"Task '{newTask.Title}' added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void PopulateTimeline()
        {
            // Dữ liệu mẫu
            if (userTasks.Count == 0)
            {
                userTasks.Add(new TaskItem { TaskId = 1, Title = "Họp Team", DueDate = DateTime.Now, Status = Models.TaskStatus.InProgress });
            }
            UpdateCalendarView();
        }

        private void LoadTaskCountBadges()
        {
            if (userTasks == null) return;
            totalTaskCount = userTasks.Count(t => t.Status != Models.TaskStatus.Completed);
            completedTaskCount = userTasks.Count(t => t.Status == Models.TaskStatus.Completed);

            if (button1 != null)
            {
                button1.Text = $"Your Task ({totalTaskCount})";
                button1.BackColor = totalTaskCount > 0 ? Color.LightBlue : SystemColors.ActiveCaption;
                button1.ForeColor = totalTaskCount > 0 ? Color.DarkBlue : Color.Black;
            }

            if (label12 != null) label12.Text = $"Pending tasks: {totalTaskCount} ⏳";
            if (label13 != null) label13.Text = $"Completed: {completedTaskCount} ✓";
        }

        private void UpdateCalendarView()
        {
            if (tableLayoutPanel2 == null) return;

            tableLayoutPanel2.SuspendLayout();
            tableLayoutPanel2.Controls.Clear();

            DateTime startDate = currentSelectedDate;

            for (int col = 0; col < 7; col++)
            {
                DateTime columnDate = startDate.AddDays(col);

                Label lblHeader = new Label
                {
                    Text = columnDate.ToString("dd/MM/yyyy"),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    BackColor = columnDate.Date == DateTime.Today ? Color.Orange : Color.LightGray,
                    ForeColor = columnDate.Date == DateTime.Today ? Color.White : Color.Black,
                    BorderStyle = BorderStyle.FixedSingle
                };
                if (columnDate.Date == DateTime.Today) lblHeader.Text += "\n(Today)";

                tableLayoutPanel2.Controls.Add(lblHeader, col, 0);

                var dailyTasks = userTasks
                   .Where(t => t.DueDate.HasValue && t.DueDate.Value.Date == columnDate.Date && t.Status != Models.TaskStatus.Completed)
                   .OrderBy(t => t.TaskId)
                   .ToList();

                for (int row = 1; row < tableLayoutPanel2.RowCount; row++)
                {
                    TaskItem taskToShow = (row - 1 < dailyTasks.Count) ? dailyTasks[row - 1] : null;
                    AddDayCell(columnDate, col, row, taskToShow);
                }
            }
            tableLayoutPanel2.ResumeLayout();
        }

        private void AddDayCell(DateTime date, int col, int row, TaskItem task)
        {
            Panel dayCell = new Panel { Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, BackColor = Color.White, Margin = new Padding(0) };
            Label lblContent = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Cursor = Cursors.Hand };

            if (task != null)
            {
                lblContent.Text = task.Title;
                lblContent.Font = new Font("Segoe UI", 9, FontStyle.Regular);
                if (currentSelectedTask != null && task.TaskId == currentSelectedTask.TaskId)
                {
                    lblContent.BackColor = Color.Orange;
                    lblContent.ForeColor = Color.White;
                }
            }
            else
            {
                lblContent.Text = "+";
                lblContent.ForeColor = Color.Gray;
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
            FormThemTask newTaskForm = new FormThemTask(this); // Đảm bảo FormThemTask có constructor này
            if (newTaskForm.ShowDialog() == DialogResult.OK)
            {
                UpdateCalendarView();
                LoadTaskCountBadges();
            }
        }

        private void NavigateToTaskList()
        {
            var pendingTasks = userTasks.Where(t => t.Status != Models.TaskStatus.Completed).ToList();
            string msg = pendingTasks.Any() ? "📋 Pending Tasks:\n" + string.Join("\n", pendingTasks.Select(t => "- " + t.Title)) : "Bạn không còn task nào!";
            MessageBox.Show(msg);
        }

        public static void EnableDoubleBuffered(Control control)
        {
            typeof(Control).GetProperty("DoubleBuffered", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(control, true, null);
        }

        // --- CÁC SỰ KIỆN KHÁC (Giữ nguyên để tránh lỗi Designer) ---
        private void monthCalendar1_DateChanged(object sender, DateRangeEventArgs e)
        {
            currentSelectedDate = e.Start;
            if (label10 != null) label10.Text = currentSelectedDate.ToString("dd/MM/yyyy");
            UpdateCalendarView();
        }
        private void pictureBox2_Click(object sender, EventArgs e) { monthCalendar1?.SetDate(monthCalendar1.SelectionStart.AddMonths(-1)); UpdateCalendarView(); }
        private void pictureBox3_Click(object sender, EventArgs e) { monthCalendar1?.SetDate(monthCalendar1.SelectionStart.AddMonths(1)); UpdateCalendarView(); }
        private void PictureBox4_Click(object sender, EventArgs e) { new FormSettings().ShowDialog(); }
        private void Label1_Click(object sender, EventArgs e) { new FormThongTinNguoiDung(_currentUsername, _currentEmail).ShowDialog(); }

        // Empty event handlers
        private void label1_Click(object sender, EventArgs e) { Label1_Click(sender, e); }
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
    }
}