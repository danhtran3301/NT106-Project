using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using TimeFlow.Services;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TimeFlow.Models;
using TimeFlow.UI;
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
            SetupLayout();
            this.SetStyle(ControlStyles.DoubleBuffer |
                          ControlStyles.UserPaint |
                          ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
        }

        public FormTaskDetail(int taskId) : this()
        {
            _currentTask = Services.TaskManager.GetTaskById(taskId);

            if (_currentTask == null)
            {
                MessageBox.Show("Không tìm thấy thông tin công việc!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            SetupLayout();
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
        private GroupTask _groupTaskDetails;
        private TimeFlow.Models.Group _currentGroup;

        private void LoadGroupData(int taskId)
        {
            _groupTaskDetails = Services.TaskManager.GetGroupTaskByTaskId(taskId);

            if (_groupTaskDetails != null)
            {
                _currentGroup = _groupTaskDetails.Group;
            }
        }
        private bool UserHasPermission()
        {
            if (_currentTask == null) return false;
            int currentUserId = 1; // ID người dùng hiện tại (Giả định)

            if (_groupTaskDetails == null)
            {
                return _currentTask.CreatorId == currentUserId;
            }

            bool isCreator = _currentTask.CreatorId == currentUserId;
            bool isAdmin = _currentGroup != null && _currentGroup.IsAdmin(currentUserId);

            return isCreator || isAdmin;
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



        private void LoadTaskDetail(int taskId)
        {
            var updatedTask = Services.TaskManager.GetTaskById(taskId);
            if (updatedTask != null)
            {
                _currentTask = updatedTask;
                LoadGroupData(taskId);
                this.Controls.Clear();
                InitializeComponent();
            }
            else
            {
                MessageBox.Show("Task này không còn tồn tại.", "Thông báo");
            }
        }
        private void BtnYourTask_Click(object sender, EventArgs e)
        {
            FormTaskList newTasklist = new FormTaskList();
            newTasklist.ShowDialog();
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
        private void BtnGroup_Click(object sender, EventArgs e)
        {
            /* try
             {
                 FromGroupList groupListForm = new FormGroupList();
                 groupListForm.ShowDialog();
             }
             catch (Exception ex)
             {
                 MessageBox.Show($"Không thể mở danh sách nhóm: {ex.Message}", "Lỗi");
             }*/
        }
        private void EditItem_Click(object sender, EventArgs e)
        {
            if (_currentTask == null) return;

            using (FormThemTask editForm = new FormThemTask(_currentTask.Id))
            {
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    LoadTaskDetail(_currentTask.Id);
                }
            }
        }
        private void DeleteItem_Click(object sender, EventArgs e)
        {
            if (_currentTask == null) return;

            var confirm = MessageBox.Show($"Bạn có chắc chắn muốn xóa task '{_currentTask.Name}'?",
                                        "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                if (Services.TaskManager.DeleteTask(_currentTask.Id))
                {
                    this.Close();
                }
            }
        }
        private void ChangeStatusItem_Click(TaskState newStatus)
        {
            if (_currentTask == null || _statusBadge == null) return;

            _currentTask.Status = newStatus;

            Color newColor = newStatus switch
            {
                TaskState.Pending => AppColors.Yellow500,
                TaskState.InProgress => AppColors.Blue500, //
                TaskState.Completed => AppColors.Green500,
                _ => AppColors.Gray400
            };

            _statusBadge.Text = _currentTask.StatusText;
            _statusBadge.BackColor = newColor;
            _statusBadge.ForeColor = newColor == AppColors.Yellow500 ? AppColors.Gray800 : Color.White;

            Services.TaskManager.UpdateTask(_currentTask);

            Services.TaskManager.AddActivity(_currentTask.Id, $"Trạng thái đổi sang {newStatus}");
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