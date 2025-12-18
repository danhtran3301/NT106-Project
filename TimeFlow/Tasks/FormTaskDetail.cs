using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using TimeFlow.Services;
using System.Collections.Generic;
using TimeFlow.Models;
using TimeFlow.UI;
using TimeFlow.UI.Components;

namespace TimeFlow.Tasks
{
    public partial class FormTaskDetail : Form
    {
        // Fields
        private readonly Font FontRegular = new Font("Segoe UI", 10F, FontStyle.Regular);
        private readonly Font FontBold = new Font("Segoe UI", 10F, FontStyle.Bold);
        private readonly Font FontTitle = new Font("Segoe UI", 16F, FontStyle.Bold);
        private readonly Font FontHeaderTitle = new Font("Segoe UI", 12F, FontStyle.Bold);
        private readonly Color HeaderIconColor = AppColors.Gray600;
        private readonly TaskApiClient _taskApi;

        private TaskDetailViewModel _currentTask;
        private TaskItem _basicTaskData; // ✅ Cache basic data
        private ModernPanel _statusBadge;
        private int _taskId;
        private bool _isLoadingDetails = false;

        // ✅ Event để notify parent form
        public event EventHandler<TaskUpdateEventArgs> TaskUpdated;
        public event EventHandler TaskDeleted;

        // Constructors
        public FormTaskDetail()
        {
            InitializeComponent();
            _taskApi = new TaskApiClient();
            this.SetStyle(ControlStyles.DoubleBuffer |
                          ControlStyles.UserPaint |
                          ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
        }

        // ✅ NEW: Constructor nhận TaskItem data
        public FormTaskDetail(TaskItem basicTask) : this()
        {
            _basicTaskData = basicTask;
            _taskId = basicTask.TaskId;
            
            // ✅ Convert TaskItem to basic TaskDetailViewModel
            _currentTask = ConvertToBasicViewModel(basicTask);
            
            // ✅ Render basic info NGAY LẬP TỨC (fast)
            SetupLayout();
            
            // ✅ Load full details async (comments, activities)
            LoadFullDetailsAsync(basicTask.TaskId);
        }

        public FormTaskDetail(int taskId) : this()
        {
            _taskId = taskId;
            
            // Show loading state immediately
            this.Text = "Loading Task Details...";
            Label loadingLabel = new Label
            {
                Text = "⏳ Loading task details...\n\nPlease wait...",
                Font = new Font("Segoe UI", 14F),
                ForeColor = AppColors.Gray600,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.White
            };
            this.Controls.Add(loadingLabel);
            
            // Load data asynchronously
            LoadTaskDetailAsync(taskId);
        }

        // Business Logic Methods
        private TaskDetailViewModel ConvertToBasicViewModel(TaskItem task)
        {
            return new TaskDetailViewModel
            {
                TaskId = task.TaskId,
                Title = task.Title,
                Description = task.Description,
                DueDate = task.DueDate,
                Priority = task.Priority,
                Status = task.Status,
                IsGroupTask = task.IsGroupTask,
                CreatedBy = task.CreatedBy,
                CompletedAt = task.CompletedAt,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                CategoryName = task.Category?.CategoryName ?? "Other",
                CategoryColor = task.Category?.Color ?? "#6B7280",
                Progress = task.Status switch
                {
                    TimeFlow.Models.TaskStatus.Pending => 0,
                    TimeFlow.Models.TaskStatus.InProgress => 50,
                    TimeFlow.Models.TaskStatus.Completed => 100,
                    TimeFlow.Models.TaskStatus.Cancelled => 0,
                    _ => 0
                },
                // Comments và Activities sẽ load sau
                Comments = new List<CommentViewModel>(),
                Activities = new List<ActivityViewModel>()
            };
        }

        private async void LoadFullDetailsAsync(int taskId)
        {
            try
            {
                _isLoadingDetails = true;
                
                // ✅ Fetch full task detail từ server (comments + activities)
                var fullDetails = await _taskApi.GetTaskDetailFullAsync(taskId);

                if (fullDetails != null)
                {
                    // ✅ Update với full data
                    _currentTask = fullDetails;
                    
                    // ✅ Progressive rendering
                    this.Invoke((MethodInvoker)delegate
                    {
                        // Render comments nếu có
                        if (_currentTask.HasComments)
                        {
                            RenderComments();
                        }
                        
                        // Render activities nếu có
                        if (_currentTask.HasActivities)
                        {
                            RenderActivities();
                        }
                        
                        _isLoadingDetails = false;
                    });
                }
            }
            catch (Exception ex)
            {
                _isLoadingDetails = false;
                MessageBox.Show($"Không thể tải đầy đủ thông tin: {ex.Message}\n\nBasic info vẫn được hiển thị.", 
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void LoadTaskDetailAsync(int taskId)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                // Fetch full task detail from server
                _currentTask = await _taskApi.GetTaskDetailFullAsync(taskId);

                if (_currentTask == null)
                {
                    MessageBox.Show("Không tìm thấy thông tin công việc!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    return;
                }

                // Setup UI with loaded data
                this.Invoke((MethodInvoker)delegate
                {
                    SetupLayout();
                    this.Text = "Task Details";
                    this.Cursor = Cursors.Default;
                });
            }
            catch (Exception ex)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show($"Lỗi khi tải thông tin task: {ex.Message}\n\nVui lòng kiểm tra:\n1. Server đang chạy\n2. Đã đăng nhập\n3. Task tồn tại trong database", 
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private bool UserHasPermission()
        {
            if (_currentTask == null) return false;
            
            // Get current user ID from SessionManager
            int currentUserId = SessionManager.UserId ?? 0;
            
            if (currentUserId == 0) return false;

            // Check if user is creator
            return _currentTask.CreatedBy == currentUserId;
        }

        private void RefreshTaskDetail()
        {
            if (_taskId > 0)
            {
                LoadTaskDetailAsync(_taskId);
            }
        }

        // Event Handlers
        private void BtnYourTask_Click(object sender, EventArgs e)
        {
            FormTaskList newTasklist = new FormTaskList();
            this.Hide();
            newTasklist.FormClosed += (s, args) => this.Close();
            newTasklist.Show();
        }

        private void BtnNewTask_Click(object sender, EventArgs e)
        {
            try
            {
                FormThemTask newTaskForm = new FormThemTask();
                if (newTaskForm.ShowDialog() == DialogResult.OK)
                {
                    // Optionally refresh
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi mở Form Tạo Task: {ex.Message}", "Lỗi");
            }
        }

        private async void BtnSubmitTask_Click(object sender, EventArgs e)
        {
            if (_currentTask == null) return;
            if (_currentTask.Progress < 100)
            {
                MessageBox.Show("Vui lòng hoàn thành 100% tiến độ trước khi nộp Task.", "Cảnh báo");
                return;
            }

            try
            {
                bool success = await _taskApi.UpdateTaskStatusAsync(_currentTask.TaskId, TimeFlow.Models.TaskStatus.Completed);
                
                if (success)
                {
                    RefreshTaskDetail();
                    MessageBox.Show("Task đã được nộp và chuyển sang trạng thái HOÀN THÀNH.", "Thành công");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật task: {ex.Message}", "Lỗi");
            }
        }

        private void BtnGroup_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Group list feature coming soon!", "Info");
        }

        private void EditItem_Click(object sender, EventArgs e)
        {
            if (_currentTask == null) return;

            using (FormThemTask editForm = new FormThemTask(_currentTask.TaskId))
            {
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    RefreshTaskDetail();
                }
            }
        }

        private async void DeleteItem_Click(object sender, EventArgs e)
        {
            if (_currentTask == null) return;

            var confirm = MessageBox.Show($"Bạn có chắc chắn muốn xóa task '{_currentTask.Title}'?",
                                        "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm == DialogResult.Yes)
            {
                try
                {
                    bool success = await _taskApi.DeleteTaskAsync(_currentTask.TaskId);
                    
                    if (success)
                    {
                        // ✅ Raise TaskDeleted event trước khi close
                        TaskDeleted?.Invoke(this, EventArgs.Empty);
                        
                        MessageBox.Show("Task đã được xóa thành công!", "Thành công");
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Không thể xóa task. Vui lòng thử lại!", "Lỗi");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi xóa task: {ex.Message}", "Lỗi");
                }
            }
        }

        private async void ChangeStatusItem_Click(TimeFlow.Models.TaskStatus newStatus)
        {
            if (_currentTask == null || _statusBadge == null) return;

            try
            {
                bool success = await _taskApi.UpdateTaskStatusAsync(_currentTask.TaskId, newStatus);

                if (success)
                {
                    // Update local state
                    var oldStatus = _currentTask.Status;
                    _currentTask.Status = newStatus;

                    // Update UI
                    Color newColor = GetStatusColor(newStatus);
                    _statusBadge.Text = _currentTask.StatusText;
                    _statusBadge.BackColor = newColor;
                    _statusBadge.ForeColor = newColor == AppColors.Yellow500 ? AppColors.Gray800 : Color.White;

                    // ✅ Raise TaskUpdated event
                    TaskUpdated?.Invoke(this, new TaskUpdateEventArgs
                    {
                        TaskId = _currentTask.TaskId,
                        Status = newStatus
                    });

                    MessageBox.Show($"Trạng thái đã được đổi sang {_currentTask.StatusText}", "Thành công", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Refresh to get updated activities
                    RefreshTaskDetail();
                }
                else
                {
                    MessageBox.Show("Không thể cập nhật trạng thái. Vui lòng thử lại!", "Lỗi");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi cập nhật trạng thái: {ex.Message}", "Lỗi");
            }
        }

        private Color GetStatusColor(TimeFlow.Models.TaskStatus status)
        {
            return status switch
            {
                TimeFlow.Models.TaskStatus.Pending => AppColors.Yellow500,
                TimeFlow.Models.TaskStatus.InProgress => AppColors.Blue500,
                TimeFlow.Models.TaskStatus.Completed => AppColors.Green500,
                TimeFlow.Models.TaskStatus.Cancelled => AppColors.Gray400,
                _ => AppColors.Gray400
            };
        }

        private Color GetPriorityColor(TaskPriority priority)
        {
            return priority switch
            {
                TaskPriority.Low => AppColors.Green500,
                TaskPriority.Medium => AppColors.Orange500,
                TaskPriority.High => AppColors.Red600,
                _ => AppColors.Gray400
            };
        }

        // ✅ Progressive Rendering Methods
        private void RenderComments()
        {
            if (_currentTask == null || !_currentTask.HasComments) return;

            // Find center content panel
            var centerPanel = FindCenterContentPanel();
            if (centerPanel == null) return;

            centerPanel.SuspendLayout();

            // Remove "No comments" or "Loading comments" label if exists
            var labelToRemove = centerPanel.Controls.OfType<Label>()
                .FirstOrDefault(l => l.Text.Contains("No comments") || l.Text.Contains("Loading comments"));
            if (labelToRemove != null)
            {
                centerPanel.Controls.Remove(labelToRemove);
            }

            // Render comments (limit to first 10)
            const int INITIAL_COMMENTS = 10;
            var commentsToRender = _currentTask.Comments.Take(INITIAL_COMMENTS).ToList();

            // Find spacer index (last control)
            int spacerIndex = centerPanel.Controls.Count - 1;

            foreach (var comment in commentsToRender)
            {
                var commentControl = CreateComment(
                    comment.DisplayName,
                    comment.Content,
                    comment.TimeAgo
                );
                
                // ✅ Add control first
                centerPanel.Controls.Add(commentControl);
                // ✅ Then set index to insert before spacer
                centerPanel.Controls.SetChildIndex(commentControl, spacerIndex);
            }

            // Add "Load more" if needed
            if (_currentTask.Comments.Count > INITIAL_COMMENTS)
            {
                var loadMoreLabel = new Label
                {
                    Text = $"+ Load {_currentTask.Comments.Count - INITIAL_COMMENTS} more comments",
                    Font = new Font("Segoe UI", 10F, FontStyle.Italic),
                    ForeColor = AppColors.Blue500,
                    Cursor = Cursors.Hand,
                    AutoSize = true,
                    Margin = new Padding(0, 10, 0, 0)
                };
                loadMoreLabel.Click += (s, e) => LoadMoreComments(centerPanel, INITIAL_COMMENTS);
                
                // ✅ Add then set index
                centerPanel.Controls.Add(loadMoreLabel);
                centerPanel.Controls.SetChildIndex(loadMoreLabel, spacerIndex);
            }

            centerPanel.ResumeLayout();
        }

        private void RenderActivities()
        {
            if (_currentTask == null || !_currentTask.HasActivities) return;

            // Find right sidebar panel
            var rightSidebar = FindRightSidebarPanel();
            if (rightSidebar == null) return;

            rightSidebar.SuspendLayout();

            // Remove "No activity" or "Loading activities" label if exists
            var labelToRemove = rightSidebar.Controls.OfType<Label>()
                .FirstOrDefault(l => l.Text.Contains("No activity") || l.Text.Contains("Loading activities"));
            if (labelToRemove != null)
            {
                rightSidebar.Controls.Remove(labelToRemove);
            }

            // Render activities (limit to first 10)
            const int INITIAL_ACTIVITIES = 10;
            var activitiesToRender = _currentTask.Activities
                .OrderByDescending(a => a.CreatedAt)
                .Take(INITIAL_ACTIVITIES)
                .ToList();

            int contentWidth = 302; // sidebar width - padding
            
            // Find spacer index (last control)
            int spacerIndex = rightSidebar.Controls.Count - 1;

            foreach (var activity in activitiesToRender)
            {
                var activityControl = CreateActivityLog(
                    activity.Description,
                    activity.TimeAgo,
                    contentWidth
                );
                
                // ✅ Add control first
                rightSidebar.Controls.Add(activityControl);
                // ✅ Then set index to insert before spacer
                rightSidebar.Controls.SetChildIndex(activityControl, spacerIndex);
            }

            // Add "Show more" if needed
            if (_currentTask.Activities.Count > INITIAL_ACTIVITIES)
            {
                var showMoreLabel = new Label
                {
                    Text = $"+ {_currentTask.Activities.Count - INITIAL_ACTIVITIES} more activities",
                    Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                    ForeColor = AppColors.Gray500,
                    AutoSize = true,
                    Margin = new Padding(0, 10, 0, 0)
                };
                
                // ✅ Add then set index
                rightSidebar.Controls.Add(showMoreLabel);
                rightSidebar.Controls.SetChildIndex(showMoreLabel, spacerIndex);
            }

            rightSidebar.ResumeLayout();
        }

        private void LoadMoreComments(FlowLayoutPanel centerPanel, int alreadyLoaded)
        {
            centerPanel.SuspendLayout();

            // Remove "Load more" label
            var loadMoreLabel = centerPanel.Controls.OfType<Label>()
                .FirstOrDefault(l => l.Text.StartsWith("+ Load"));
            if (loadMoreLabel != null)
            {
                centerPanel.Controls.Remove(loadMoreLabel);
            }

            // Load next batch
            const int BATCH_SIZE = 10;
            var nextBatch = _currentTask.Comments
                .Skip(alreadyLoaded)
                .Take(BATCH_SIZE)
                .ToList();

            // Find spacer index
            int spacerIndex = centerPanel.Controls.Count - 1;

            foreach (var comment in nextBatch)
            {
                var commentControl = CreateComment(
                    comment.DisplayName,
                    comment.Content,
                    comment.TimeAgo
                );
                
                // ✅ Add then set index
                centerPanel.Controls.Add(commentControl);
                centerPanel.Controls.SetChildIndex(commentControl, spacerIndex);
            }

            // Add "Load more" again if needed
            int totalLoaded = alreadyLoaded + BATCH_SIZE;
            if (_currentTask.Comments.Count > totalLoaded)
            {
                var newLoadMoreLabel = new Label
                {
                    Text = $"+ Load {_currentTask.Comments.Count - totalLoaded} more comments",
                    Font = new Font("Segoe UI", 10F, FontStyle.Italic),
                    ForeColor = AppColors.Blue500,
                    Cursor = Cursors.Hand,
                    AutoSize = true,
                    Margin = new Padding(0, 10, 0, 0)
                };
                newLoadMoreLabel.Click += (s, e) => LoadMoreComments(centerPanel, totalLoaded);
                
                // ✅ Add then set index
                centerPanel.Controls.Add(newLoadMoreLabel);
                centerPanel.Controls.SetChildIndex(newLoadMoreLabel, spacerIndex);
            }

            centerPanel.ResumeLayout();
        }

        private FlowLayoutPanel FindCenterContentPanel()
        {
            // Navigate: Form → rootPanel → mainLayout → column[1] → scrollContainer → contentPanel
            var rootPanel = this.Controls.OfType<Panel>().FirstOrDefault();
            if (rootPanel == null) return null;

            var mainLayout = rootPanel.Controls.OfType<TableLayoutPanel>().FirstOrDefault();
            if (mainLayout == null) return null;

            var scrollContainer = mainLayout.Controls.OfType<Panel>().Skip(1).FirstOrDefault();
            if (scrollContainer == null) return null;

            return scrollContainer.Controls.OfType<FlowLayoutPanel>().FirstOrDefault();
        }

        private FlowLayoutPanel FindRightSidebarPanel()
        {
            // Navigate: Form → rootPanel → mainLayout → column[2] → ModernPanel → contentFlow
            var rootPanel = this.Controls.OfType<Panel>().FirstOrDefault();
            if (rootPanel == null) return null;

            var mainLayout = rootPanel.Controls.OfType<TableLayoutPanel>().FirstOrDefault();
            if (mainLayout == null) return null;

            var modernPanel = mainLayout.Controls.OfType<ModernPanel>().FirstOrDefault();
            if (modernPanel == null) return null;

            return modernPanel.Controls.OfType<FlowLayoutPanel>().FirstOrDefault();
        }

        private void FormTaskDetail_Load(object sender, EventArgs e)
        {
            // Form load event handler
        }
    }
}