using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using BT3_LTMCB;
using TimeFlow;
using System.Threading.Tasks;

namespace DOANNT106
{
    public partial class GiaoDien : Form
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
        public GiaoDien()
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
            // 1. Chỉ đếm các task CHƯA HOÀN THÀNH cho nút "Your Task"
            // (Logic cũ: totalTaskCount = userTasks.Count; -> Sẽ bị đếm sai)
            totalTaskCount = userTasks.Count(t => !t.IsCompleted);

            // 2. Đếm số task đã hoàn thành (để hiện ở góc dưới bên trái)
            completedTaskCount = userTasks.Count(t => t.IsCompleted);

            // --- Cập nhật UI ---

            // Cập nhật text cho Button chính
            button1.Text = $"Your Task ({totalTaskCount})";

            // Cập nhật text cho Label góc dưới (My Calendar)
            // Bạn có thể để label12 hiện tổng task đang chờ xử lý
            label12.Text = $"Pending tasks: {totalTaskCount} ⏳";
            label13.Text = $"Completed: {completedTaskCount} ✓";

            // Logic đổi màu nút (nếu còn task cần làm thì sáng màu)
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

            // Lấy ngày bắt đầu hiển thị. 
            // Nếu bạn muốn Lịch luôn bắt đầu từ Thứ 2 của tuần hiện tại thì dùng dòng comment bên dưới:
            // DateTime startDate = currentSelectedDate.AddDays(-(int)currentSelectedDate.DayOfWeek + 1);

            // Còn nếu muốn ngày được chọn (currentSelectedDate) nằm ở cột đầu tiên:
            DateTime startDate = currentSelectedDate;

            // --- DUYỆT QUA 7 CỘT (7 NGÀY) ---
            for (int col = 0; col < 7; col++) // Giả sử TableLayoutPanel có 7 cột
            {
                // Tính ngày cho cột hiện tại
                DateTime columnDate = startDate.AddDays(col);

                // 1. TẠO HEADER (HÀNG 0) - Hiển thị ngày cụ thể (VD: 16/12/2025)
                Label lblHeader = new Label
                {
                    Text = columnDate.ToString("dd/MM/yyyy"), // Format ngày/tháng/năm
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 10, FontStyle.Bold),
                    BackColor = Color.LightGray,
                    ForeColor = Color.Black,
                    BorderStyle = BorderStyle.FixedSingle
                };

                // Highlight nếu là ngày hôm nay
                if (columnDate.Date == DateTime.Today)
                {
                    lblHeader.BackColor = Color.Orange;
                    lblHeader.ForeColor = Color.White;
                    lblHeader.Text += "\n(Today)";
                }

                tableLayoutPanel2.Controls.Add(lblHeader, col, 0);

                // 2. TẠO CÁC Ô TASK (TỪ HÀNG 1 ĐẾN HẾT)
                // Lấy danh sách task của ngày này
              
                var dailyTasks = userTasks
                     .Where(t => t.Date.Date == columnDate.Date && !t.IsCompleted) // 👈 QUAN TRỌNG: Thêm "&& !t.IsCompleted"
                     .OrderBy(t => t.Id)
                     .ToList();

                for (int row = 1; row < tableLayoutPanel2.RowCount; row++)
                {
                    // Logic: Hàng 1 hiện Task 1, Hàng 2 hiện Task 2...
                    // index trong list = row - 1 (vì row bắt đầu từ 1)
                    int taskIndex = row - 1;

                    TaskItem taskToShow = null;
                    if (taskIndex < dailyTasks.Count)
                    {
                        taskToShow = dailyTasks[taskIndex];
                    }

                    // Gọi hàm vẽ ô
                    AddDayCell(columnDate, col, row, taskToShow);
                }
            }

            tableLayoutPanel2.ResumeLayout();
        }


        private void AddDayCell(DateTime date, int col, int row, TaskItem task)
        {
            // 1. Khai báo dayCell và lblContent NGAY ĐẦU HÀM
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

            // 2. Kiểm tra task để hiển thị nội dung & màu sắc
            if (task != null)
            {
                // --- CÓ TASK ---
                lblContent.Text = task.Title; // Hoặc "📝 " + task.Title;
                lblContent.Font = new Font("Segoe UI", 9, FontStyle.Regular);

                // Logic Highlight: Nếu Task này đang được chọn -> Màu Cam
                if (currentSelectedTask != null && task.Id == currentSelectedTask.Id)
                {
                    lblContent.BackColor = Color.Orange;
                    lblContent.ForeColor = Color.White;
                    lblContent.BorderStyle = BorderStyle.FixedSingle;
                }
                else
                {
                    // Màu mặc định (Hoàn thành: Xanh lá, Chưa: Xanh dương)
                    lblContent.BackColor = task.IsCompleted ? Color.LightGreen : Color.LightBlue;
                    lblContent.ForeColor = Color.Black;
                    lblContent.BorderStyle = BorderStyle.None;
                }
            }
            else
            {
                // --- Ô TRỐNG ---
                lblContent.Text = "+ Click to add";
                lblContent.ForeColor = Color.Gray;
                lblContent.Font = new Font("Segoe UI", 8, FontStyle.Italic);
                lblContent.BackColor = Color.White;
            }

            // 3. Gán sự kiện Click (Đoạn này phải nằm TRONG hàm, trước ngoặc đóng cuối cùng)
            // Truyền biến 'task' vào để xử lý chọn hoặc thêm mới
            lblContent.Click += (s, e) => OnCalendarCellClick(date, task);
            dayCell.Click += (s, e) => OnCalendarCellClick(date, task);

            // 4. Thêm Control vào bảng
            dayCell.Controls.Add(lblContent);
            tableLayoutPanel2.Controls.Add(dayCell, col, row);

        }


        private void OnCalendarCellClick(DateTime date, TaskItem task)
        {
            if (task != null)
            {
                // --- TRƯỜNG HỢP CLICK VÀO TASK ---

                // 1. Lưu task đang chọn vào biến toàn cục
                currentSelectedTask = task;

                // 2. Load lại giao diện để hiển thị hiệu ứng "Đang chọn" (Highlight)
                UpdateCalendarView();

                // (Tùy chọn) Vẫn hiện thông báo chi tiết nếu muốn, 
                // nhưng nếu bạn muốn thao tác nhanh để Submit thì có thể bỏ dòng MessageBox này đi
                // MessageBox.Show($"Đã chọn task: {task.Title}\nNhấn 'Submit task' để hoàn thành.", "Selected");
            }
            else
            {
                // --- TRƯỜNG HỢP CLICK VÀO Ô TRỐNG ---
                // Bỏ chọn task cũ (nếu có)
                currentSelectedTask = null;
                UpdateCalendarView(); // Xóa highlight cũ

                // Mở form thêm mới
                OpenNewTaskFormForDate(date);
            }
        }


        // ✅ METHOD MỚI: Mở form thêm task với ngày được chọn trước
        private void OpenNewTaskFormForDate(DateTime selectedDate)
        {
            FormThemTask newTaskForm = new FormThemTask(this, selectedDate);
            if (newTaskForm.ShowDialog() == DialogResult.OK)
            {
                UpdateCalendarView();
                LoadTaskCountBadges();
            }
        }

        // ✅ THÊM TASK VÀ CẬP NHẬT UI
        public void AddTaskFromForm(TaskItem newTask)
        {
            // Đảm bảo Id là duy nhất
            newTask.Id = userTasks.Count > 0 ? userTasks.Max(t => t.Id) + 1 : 1;
            userTasks.Add(newTask);

            // ✅ CẬP NHẬT UI NGAY LẬP TỨC
            UpdateCalendarView();
            LoadTaskCountBadges();

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

        // Event Handlers
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
        {// 1. Lọc ra danh sách các task CHƯA HOÀN THÀNH
            var pendingTasks = userTasks.Where(t => !t.IsCompleted).ToList();

            if (pendingTasks.Any())
            {
                string taskList = "📋 Your Pending Tasks:\n\n";
                foreach (var task in pendingTasks)
                {
                    // Hiển thị: Tên - Ngày hết hạn (hoặc ngày thực hiện)
                    taskList += $"• {task.Title} (Ngày: {task.Date.ToString("dd/MM")}) ⏳\n";
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

        // Empty event handlers (có thể xóa nếu không cần)
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
        private void button4_Click(object sender, EventArgs e) {
            if (currentSelectedTask == null)
            {
                MessageBox.Show("Vui lòng chọn task cần submit!");
                return;
            }

            DialogResult result = MessageBox.Show($"Xác nhận hoàn thành task: {currentSelectedTask.Title}?",
                "Xác nhận", MessageBoxButtons.YesNo);

            if (result == DialogResult.Yes)
            {
                // 1. Đánh dấu task đã xong
                currentSelectedTask.IsCompleted = true;

                // 2. Reset biến chọn về null (để không còn task nào đang được chọn)
                currentSelectedTask = null;

                // 3. Cập nhật lại giao diện (Lúc này hàm UpdateCalendarView sẽ chạy và lọc bỏ task vừa xong)
                UpdateCalendarView();

                // 4. Cập nhật số lượng task hoàn thành ở góc dưới
                LoadTaskCountBadges();
            }
        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void label13_Click_1(object sender, EventArgs e)
        {

        }
    }

    // ✅ TaskItem class với EndDate
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsCompleted { get; set; }
        public string AssignedTo { get; set; }
    }
}
