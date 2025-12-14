using System;
using System.Drawing;
using System.Windows.Forms;
using TimeFlow.Models;
using TimeFlow.UI;
using System.Linq; 
using TimeFlow.Services;
using TimeFlow.UI.Components;

namespace TimeFlow.Tasks
{
    public partial class FormTaskDetail : Form
    {
        private readonly Font FontRegular = new Font("Segoe UI", 10F, FontStyle.Regular);
        private readonly Font FontBold = new Font("Segoe UI", 10F, FontStyle.Bold);
        private readonly Font FontTitle = new Font("Segoe UI", 16F, FontStyle.Bold);
        private readonly Font FontHeaderTitle = new Font("Segoe UI", 12F, FontStyle.Bold);
        private readonly Color HeaderIconColor = AppColors.Gray600;

        private TaskModel _currentTask;

        public FormTaskDetail()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.DoubleBuffer |
                          ControlStyles.UserPaint |
                          ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
        }

        // Constructor nhận task data
        public FormTaskDetail(TaskModel task) : this()
        {
            _currentTask = task ?? throw new ArgumentNullException(nameof(task));
        }

        private TimeFlow.UI.Components.CustomButton CreateMenuButton(string text, Color backColor, Color foreColor, int width, int height, Color? hoverColor)
        {
            return new TimeFlow.UI.Components.CustomButton
            {
                Text = text,
                BackColor = backColor,
                ForeColor = foreColor,
                HoverColor = hoverColor ?? AppColors.Blue600,
                BorderRadius = 8,
                Width = width,
                Height = height,
                Font = FontBold,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 0, 0, 12)
            };
        }

        private TimeFlow.UI.Components.CustomButton CreateMenuButton(string text, Color backColor, Color foreColor, int width, int height)
        {
            return CreateMenuButton(text, backColor, foreColor, width, height, null);
        }

        private Control CreateComment(string user, string text, string time)
        {
            FlowLayoutPanel comment = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = false,
                Width = 800,
                Margin = new Padding(0, 0, 0, 16),
                Padding = new Padding(0, 0, 0, 8),
                BackColor = AppColors.Gray50,
                BorderStyle = BorderStyle.FixedSingle
            };
            comment.AutoSize = true;
            comment.WrapContents = false;

            FlowLayoutPanel header = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 4)
            };
            header.Controls.Add(new Label { Text = user, Font = FontBold, ForeColor = AppColors.Gray800, AutoSize = true, Margin = new Padding(0, 0, 8, 0) });
            header.Controls.Add(new Label { Text = time, Font = FontRegular, ForeColor = AppColors.Gray500, AutoSize = true });
            comment.Controls.Add(header);

            Label content = new Label
            {
                Text = text,
                Font = FontRegular,
                ForeColor = AppColors.Gray600,
                MaximumSize = new Size(comment.Width, 0),
                AutoSize = true
            };
            comment.Controls.Add(content);

            return comment;
        }
        private void ShowExistingForm<T>() where T : Form, new()
        {
            System.Windows.Forms.Form existingForm = System.Windows.Forms.Application.OpenForms.OfType<T>().FirstOrDefault();
            if (existingForm != null)
            {
                existingForm.Show();
                existingForm.BringToFront();
            }
            else
            {
                T newForm = new T();
                newForm.Show();
            }
            this.Close();
        }

        private void CloseAndNavigateToTaskList(object sender, EventArgs e)
        {
            ShowExistingForm<FormTaskList>();
        }
        private void LoadTaskDetail(int taskId)
        {
            var updatedTask = Services.TaskManager.GetTaskById(taskId);
            if (updatedTask != null)
            {
                _currentTask = updatedTask;
                this.Controls.Clear();
                InitializeComponent();
            }
            else
            {
                MessageBox.Show("Task này không còn tồn tại.", "Thông báo");
                ShowExistingForm<FormTaskList>();
            }
        }
        private void BtnYourTask_Click(object sender, EventArgs e)
        {
            CloseAndNavigateToTaskList(sender, e);
        }

        private void BtnNewTask_Click(object sender, EventArgs e)
        {
            try
            {
                FormThemTask newTaskForm = new FormThemTask();
                newTaskForm.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi mở Form Tạo Task: {ex.Message}", "Lỗi");
            }
        }

        private void BtnSubmitTask_Click(object sender, EventArgs e)
        {
            if (_currentTask == null) return;
            if (_currentTask.Progress < 100)
            {
                MessageBox.Show("Vui lòng hoàn thành 100% tiến độ trước khi nộp Task.", "Cảnh báo");
                return;
            }

            _currentTask.Status = TaskState.Completed;
            if (Services.TaskManager.UpdateTask(_currentTask))
            {
                LoadTaskDetail(_currentTask.Id);
                MessageBox.Show("Task đã được nộp và chuyển sang trạng thái HOÀN THÀNH.", "Thành công");
            }
        }
        private void EditItem_Click(object sender, EventArgs e)
        {
            if (_currentTask == null) return;
            try
            {
                FormThemTask editForm = new FormThemTask(_currentTask.Id);
                if (editForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    LoadTaskDetail(_currentTask.Id);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi mở form chỉnh sửa: {ex.Message}", "Lỗi");
            }
        }

        // Hàm Options Menu: Xóa
        private void DeleteItem_Click(object sender, EventArgs e)
        {
            if (_currentTask == null) return;
            if (Services.TaskManager.DeleteTask(_currentTask.Id))
            {
                ShowExistingForm<FormTaskList>();
            }
            else
            {
                MessageBox.Show("Xóa Task thất bại.", "Lỗi");
            }
        }

        // Hàm Options Menu: Thay đổi Trạng thái (Cần CreateStatusSubMenu để gọi hàm này)
        private void ChangeStatusItem_Click(TaskState newStatus)
        {
            if (_currentTask == null) return;

            TaskState oldStatus = _currentTask.Status;
            _currentTask.Status = newStatus;

            if (Services.TaskManager.UpdateTask(_currentTask))
            {
                LoadTaskDetail(_currentTask.Id);
            }
            else
            {
                _currentTask.Status = oldStatus; // Rollback
            }
        }

        private void CreateStatusSubMenu(System.Windows.Forms.ToolStripMenuItem statusMenu)
        {
            System.Array statusValues = System.Enum.GetValues(typeof(TaskState));
            foreach (TaskState status in statusValues)
            {
                System.Windows.Forms.ToolStripMenuItem item = new System.Windows.Forms.ToolStripMenuItem(status.ToString());
                if (status == _currentTask.Status) item.Checked = true;
                item.Click += (sender, e) => ChangeStatusItem_Click(status);
                statusMenu.DropDownItems.Add(item);
            }
        }
        private Control CreateActivityLog(string activity, string time, int width)
        {
            FlowLayoutPanel logItem = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = false,
                Width = width,
                Margin = new Padding(0, 0, 0, 12)
            };
            logItem.AutoSize = true;

            Label lblActivity = new Label
            {
                Text = activity,
                Font = FontRegular,
                ForeColor = AppColors.Gray600,
                MaximumSize = new Size(width, 0),
                AutoSize = true
            };
            Label lblTime = new Label
            {
                Text = time,
                Font = new Font("Segoe UI", 8F, FontStyle.Regular),
                ForeColor = AppColors.Gray400,
                AutoSize = true
            };

            logItem.Controls.Add(lblActivity);
            logItem.Controls.Add(lblTime);
            return logItem;
        }

        private void FormTaskDetail_Load(object sender, EventArgs e)
        {

        }
    }
}