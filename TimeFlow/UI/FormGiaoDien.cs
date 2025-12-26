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
using TimeFlow.UI.Components;
using TimeFlow.Tasks;
using TimeFlow.Models;
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

        private readonly TaskApiClient _taskApi;
        private int totalTaskCount = 0;
        private int completedTaskCount = 0;
        private List<TaskItem> _currentTasks = new List<TaskItem>();
        private DateTime currentSelectedDate = DateTime.Now;
        private TaskItem currentSelectedTask = null;
        private bool _isLoading = false;

        public FormGiaoDien()
        {
            InitializeComponent();
            _taskApi = new TaskApiClient();
            InitializeForm();
        }

        private void InitializeForm()
        {
            if (!string.IsNullOrEmpty(SessionManager.Username))
            {
                label1.Text = SessionManager.Username;
            }

            UpdateTimelineHeaderLabel();
            monthCalendar1.DateChanged += monthCalendar1_DateChanged;

            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.WindowState = FormWindowState.Maximized;
            pictureBox2.BringToFront();
            pictureBox3.BringToFront();
        }

        private async void GiaoDien_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(SessionManager.Username))
            {
                SessionManager.Username = "TEST_USER";
            }

            EnableDoubleBuffered(tableLayoutPanel2);

            await LoadTasksFromServerAsync();
        }

        private async System.Threading.Tasks.Task LoadTasksFromServerAsync()
        {
            try
            {
                _isLoading = true;
                ShowLoadingIndicator();

                _currentTasks = await _taskApi.GetTasksAsync();

                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        UpdateCalendarView();
                        LoadTaskCountBadges();
                    }));
                }
                else
                {
                    UpdateCalendarView();
                    LoadTaskCountBadges();
                }

                HideLoadingIndicator();
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();

                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() =>
                    {
                        MessageBox.Show($"Không thể tải tasks: {ex.Message}\n\nVui lòng kiểm tra:\n1. Server đang chạy\n2. Đã đăng nhập",
                            "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        UpdateCalendarView();
                        LoadTaskCountBadges();
                    }));
                }
                else
                {
                    MessageBox.Show($"Không thể tải tasks: {ex.Message}\n\nVui lòng kiểm tra:\n1. Server đang chạy\n2. Đã đăng nhập",
                        "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    UpdateCalendarView();
                    LoadTaskCountBadges();
                }
            }
            finally
            {
                _isLoading = false;
            }
        }

        private void ShowLoadingIndicator()
        {
            if (!this.IsHandleCreated)
                return;

            if (this.InvokeRequired)
            {
                this.Invoke(new Action(ShowLoadingIndicator));
                return;
            }

            tableLayoutPanel2.SuspendLayout();
            tableLayoutPanel2.Controls.Clear();

            Label loadingLabel = new Label
            {
                Text = "⏳ Loading your tasks...\n\nPlease wait...",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 14F),
                ForeColor = AppColors.Gray600,
                BackColor = Color.White
            };

            tableLayoutPanel2.Controls.Add(loadingLabel, 0, 0);
            tableLayoutPanel2.SetColumnSpan(loadingLabel, 7);
            tableLayoutPanel2.SetRowSpan(loadingLabel, tableLayoutPanel2.RowCount);

            tableLayoutPanel2.ResumeLayout();
        }

        private void HideLoadingIndicator()
        {
        }

        private async void RefreshCalendar()
        {
            if (!this.IsHandleCreated || this.IsDisposed)
                return;

            await LoadTasksFromServerAsync();
        }

        private void LoadTaskCountBadges()
        {
            totalTaskCount = _currentTasks.Count(t => t.Status != TimeFlow.Models.TaskStatus.Completed);
            completedTaskCount = _currentTasks.Count(t => t.Status == TimeFlow.Models.TaskStatus.Completed);
            int inProgressCount = _currentTasks.Count(t => t.Status == TimeFlow.Models.TaskStatus.InProgress);
            int cancelledCount = _currentTasks.Count(t => t.Status == TimeFlow.Models.TaskStatus.Cancelled);


            button1.Text = $"Your Task ({totalTaskCount})";
            label13.Text = $"Completed: {completedTaskCount} ✓";
            label15.Text = $"In progress: {inProgressCount} 🟠";
            label14.Text = $"Cancelled: {cancelledCount} ❌";



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
            if (!this.IsHandleCreated)
                return;

            if (this.InvokeRequired)
            {
                this.Invoke(new Action(UpdateCalendarView));
                return;
            }

            tableLayoutPanel2.SuspendLayout();
            tableLayoutPanel2.Controls.Clear();

            DateTime startDate = currentSelectedDate;

            for (int col = 0; col < 7; col++)
            {
                DateTime columnDate = startDate.AddDays(col);
            Label lblHeader = new Label
            {
                Text = columnDate.ToString("dd/MM/yyyy") + Environment.NewLine + 
                       columnDate.ToString("ddd", new System.Globalization.CultureInfo("en-US")), // Thêm thứ trong tuần
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BackColor = Color.FromArgb(245, 245, 250),
                ForeColor = Color.Black,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(2)
            };

            if (columnDate.Date == DateTime.Today)
            {
                lblHeader.BackColor = Color.FromArgb(255, 165, 0); // Màu cam cho hôm nay
                lblHeader.ForeColor = Color.White;
                lblHeader.Text = columnDate.ToString("dd/MM/yyyy") + Environment.NewLine + 
                                columnDate.ToString("ddd", new System.Globalization.CultureInfo("en-US")) + 
                                Environment.NewLine + "(Today)";
            }
            else if (columnDate.DayOfWeek == DayOfWeek.Saturday || columnDate.DayOfWeek == DayOfWeek.Sunday)
            {
                lblHeader.BackColor = Color.FromArgb(240, 240, 245);
            }

                tableLayoutPanel2.Controls.Add(lblHeader, col, 0);

                var dailyTasks = _currentTasks
                   .Where(t => t.DueDate.HasValue
                     && t.DueDate.Value.Date == columnDate.Date
                     && t.Status != TimeFlow.Models.TaskStatus.Completed)
                   .OrderBy(t => t.TaskId)
                   .ToList();

                // hien thi tat ca cac o task 
                // neu co task thi hien thi, khong co thi "+ Click to add"
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
                lblContent.Text = task.Title;
                lblContent.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                Color statusColor = GetStatusColor(task.Status);
                lblContent.BackColor = statusColor;
                lblContent.ForeColor = Color.FromArgb(25, 25, 25);
                lblContent.Padding = new Padding(5, 3, 5, 3);

                if (currentSelectedTask != null && task.TaskId == currentSelectedTask.TaskId)
                {
                    lblContent.BorderStyle = BorderStyle.FixedSingle;
                    dayCell.BackColor = Color.LightGray;
                }
                else
                {
                    lblContent.BorderStyle = BorderStyle.None;
                    dayCell.BackColor = Color.White;
                }
            }
            else
            {
                lblContent.Text = "+ Click to add";
                lblContent.ForeColor = Color.FromArgb(128, 128, 128); // Màu xám đẹp hơn
                lblContent.Font = new Font("Segoe UI", 9, FontStyle.Italic);
                lblContent.BackColor = Color.FromArgb(250, 250, 250); // Nền hơi xám nhẹ để dễ nhận biết
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
                OpenTaskDetail(task);
            }
            else
            {
                currentSelectedTask = null;
                UpdateCalendarView();
                OpenNewTaskFormForDate(date);
            }
        }

        private void OpenTaskDetail(TaskItem task)
        {
            FormTaskDetail detailForm = new FormTaskDetail(task);

            detailForm.TaskUpdated += (s, e) =>
            {
                // Cách 1: Nạp lại toàn bộ từ Server (An toàn nhất)
                RefreshCalendar();

            };

            detailForm.TaskDeleted += (s, e) =>
            {
                RefreshCalendar();
            };

            detailForm.Show();

        }
            
       
        

        private Color GetStatusColor(TimeFlow.Models.TaskStatus status)
        {
            return status switch
            {
                TimeFlow.Models.TaskStatus.Pending => Color.FromArgb(173, 216, 230),        // LightBlue - nền sáng, chữ đen
                TimeFlow.Models.TaskStatus.InProgress => Color.FromArgb(255, 255, 180),     // Yellow nhạt hơn - nền sáng, chữ đen
                TimeFlow.Models.TaskStatus.Completed => Color.FromArgb(144, 238, 144),      // LightGreen - nền sáng, chữ đen
                TimeFlow.Models.TaskStatus.Cancelled => Color.FromArgb(255, 200, 200),      // LightPink - nền sáng, chữ đen
                _ => Color.FromArgb(220, 220, 220)                                          // LightGray - nền sáng, chữ đen
            };
        }

        private bool IsLightColor(Color color)
        {
            // Tính độ sáng của màu (luminance)
            // Nếu > 0.5 thì là màu sáng
            double luminance = (0.299 * color.R + 0.587 * color.G + 0.114 * color.B) / 255.0;
            return luminance > 0.5;
        }

        private void OpenNewTaskFormForDate(DateTime selectedDate)
        {
            FormThemTask newTaskForm = new FormThemTask(selectedDate);
            if (newTaskForm.ShowDialog() == DialogResult.OK)
            {
                RefreshCalendar();
            }
        }

        private void OpenNewTaskForm()
        {
            FormThemTask newTaskForm = new FormThemTask();
            if (newTaskForm.ShowDialog() == DialogResult.OK)
            {
                RefreshCalendar();
            }
        }

        private void monthCalendar1_DateChanged(object sender, DateRangeEventArgs e)
        {
            currentSelectedDate = e.Start;
            UpdateTimelineHeaderLabel();
            UpdateCalendarView();
        }

        private void UpdateTimelineHeaderLabel()
        {
            //hien thi khoang thoi gian 7 nga7: "DD/MM/YYYY - DD/MM/YYYY"
            DateTime endDate = currentSelectedDate.AddDays(6);
            label10.Text = $"{currentSelectedDate.ToString("dd/MM/yyyy")} - {endDate.ToString("dd/MM/yyyy")}";
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            currentSelectedDate = currentSelectedDate.AddDays(-7);
            monthCalendar1.SetDate(currentSelectedDate);
            UpdateCalendarView();
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            currentSelectedDate = currentSelectedDate.AddDays(7);
            monthCalendar1.SetDate(currentSelectedDate);
            UpdateCalendarView();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenNewTaskForm();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FormTaskList formTaskList = new FormTaskList();
            formTaskList.TasksChanged += (s, ev) => RefreshCalendar(); 
            formTaskList.Show();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            FormThongTinNguoiDung profileForm = new FormThongTinNguoiDung(SessionManager.Username, SessionManager.Email);
            profileForm.Show();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FormGroupTaskList groupTasksForm = new FormGroupTaskList();
            groupTasksForm.Show();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (_currentTasks.Count > 0)
            {
                OpenTaskDetail(_currentTasks[0]);
            }
            else
            {
                MessageBox.Show("No tasks to display!", "Info");
            }
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            FormSettings formSettings = new FormSettings();
            formSettings.Show();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (!_isLoading)
                RefreshCalendar();
        }


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
        private void panel1_Paint(object sender, PaintEventArgs e) { }

        private void label15_Click(object sender, EventArgs e)
        {

        }

        private void label14_Click(object sender, EventArgs e)
        {

        }
    }
}