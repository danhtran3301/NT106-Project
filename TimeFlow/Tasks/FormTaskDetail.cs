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
        private readonly Font FontRegular = new Font("Segoe UI", 10F, FontStyle.Regular);
        private readonly Font FontBold = new Font("Segoe UI", 10F, FontStyle.Bold);
        private readonly Font FontTitle = new Font("Segoe UI", 16F, FontStyle.Bold);
        private readonly Font FontHeaderTitle = new Font("Segoe UI", 12F, FontStyle.Bold);
        private readonly Color HeaderIconColor = AppColors.Gray600;
        private readonly TaskApiClient _taskApi;

        private TaskDetailViewModel _currentTask;
        private TaskItem _basicTaskData; 
        private ModernPanel _statusBadge;
        private int _taskId;
        private bool _isLoadingDetails = false;

        private CustomButton _btnEditTask;
        private CustomButton _btnChangeStatus;
        private CustomButton _btnDeleteTask;
        private CustomButton _btnSubmitTask;

        // Event de nhan thong bao tu cha
        public event EventHandler<TaskUpdateEventArgs> TaskUpdated;
        public event EventHandler TaskDeleted;

        // Constructors
        public FormTaskDetail()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            _taskApi = new TaskApiClient();
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.DoubleBuffer |
                          ControlStyles.UserPaint |
                          ControlStyles.AllPaintingInWmPaint, true);

            this.UpdateStyles();
            
            this.Activated += (s, e) => UpdateButtonStates();
        }

        public FormTaskDetail(TaskItem basicTask) : this()
        {
            _basicTaskData = basicTask;
            _taskId = basicTask.TaskId;
            _currentTask = ConvertToBasicViewModel(basicTask);
            SetupLayout();
            LoadFullDetailsAsync(basicTask.TaskId);
        }

        public FormTaskDetail(int taskId) : this()
        {
            _taskId = taskId;
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
            
            // Load du lieu bat dong bo
            LoadTaskDetailAsync(taskId);
        }

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
                Comments = new List<CommentViewModel>(),
                Activities = new List<ActivityViewModel>()
            };
        }

        private async void LoadFullDetailsAsync(int taskId)
        {
            try
            {
                _isLoadingDetails = true;
                
                var fullDetails = await _taskApi.GetTaskDetailFullAsync(taskId);

                if (fullDetails != null)
                {
                    //  Update voi full data
                    _currentTask = fullDetails;
                    
                    //Progressive rendering
                    this.Invoke((MethodInvoker)delegate
                    {
                        // Update assignee info neu la group task
                        if (_currentTask.IsGroupTask)
                        {
                            UpdateAssigneeInfo();
                        }
                        
                        // Render comments neu co
                        if (_currentTask.HasComments)
                        {
                            RenderComments();
                        }
                        
                        // Render activities neu co
                        if (_currentTask.HasActivities)
                        {
                            RenderActivities();
                        }
                        
                        // Update button states sau khi load xong
                        UpdateButtonStates();
                        
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

        private async Task LoadTaskDetailAsync(int taskId)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;

                _currentTask = await _taskApi.GetTaskDetailFullAsync(taskId);

                if (_currentTask == null)
                {
                    MessageBox.Show("Không tìm thấy thông tin công việc!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    return;
                }

                // Setup UI voi du lieu da nap 
                this.Invoke((MethodInvoker)delegate
                {
                    SetupLayout();
                    
                    //Update button states sau khi setup xong
                    UpdateButtonStates();
                    
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
        private void FilterComments(string keyword)
        {
            if (_currentTask == null) return;
            var centerPanel = FindCenterContentPanel();
            if (centerPanel == null) return;
            centerPanel.SuspendLayout(); 
            var controlsToRemove = centerPanel.Controls.OfType<FlowLayoutPanel>()
                .Where(c => c.Tag != null && c.Tag.ToString() == "CommentItem").ToList();

            foreach (var ctrl in controlsToRemove) centerPanel.Controls.Remove(ctrl);

            // dung LINQ de loc du lieu tu bo nho
            var filteredList = _currentTask.Comments
                .Where(c => string.IsNullOrEmpty(keyword) ||
                            c.Content.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                            c.DisplayName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToList();
            int spacerIndex = centerPanel.Controls.Count - 1;
            foreach (var comment in filteredList)
            {
                var commentControl = CreateComment(comment.DisplayName, comment.Content, comment.TimeAgo);
                commentControl.Tag = "CommentItem"; 

                centerPanel.Controls.Add(commentControl);
                centerPanel.Controls.SetChildIndex(commentControl, spacerIndex);
            }

            centerPanel.ResumeLayout();
        }

        private bool UserHasPermission()
        {
            if (_currentTask == null) return false;
            int currentUserId = SessionManager.UserId ?? 0;
            
            if (currentUserId == 0) return false;
            return _currentTask.CreatedBy == currentUserId;
        }

        private void RefreshTaskDetail()
        {
            if (_taskId > 0)
            {
                LoadTaskDetailAsync(_taskId);
            }
        }

        private void UpdateButtonStates()
        {
            if (_currentTask == null) return;

            bool isCompleted = _currentTask.Status == TimeFlow.Models.TaskStatus.Completed;
            bool hasPerm = UserHasPermission();

            if (_btnEditTask != null)
            {
                _btnEditTask.Enabled = hasPerm && !isCompleted;
                _btnEditTask.BackColor = AppColors.Blue500;
                _btnEditTask.Text = isCompleted ? "✏️ Edit (Done)" : "✏️ Edit Task";
            }

            if (_btnChangeStatus != null)
            {
                _btnChangeStatus.Enabled = !isCompleted;
                _btnChangeStatus.BackColor = AppColors.Orange500; 
            }

            if (_btnSubmitTask != null)
            {
                _btnSubmitTask.Enabled = !isCompleted;
                _btnSubmitTask.BackColor = AppColors.Purple500;
                _btnSubmitTask.Text = isCompleted ? "✓ Finished" : "Submit task";
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
            
            //kiem tra task da completed chua
            if (_currentTask.Status == TimeFlow.Models.TaskStatus.Completed)
            {
                MessageBox.Show("Task này đã được nộp rồi!", "Thông báo", 
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // xac nhan submit
            var confirmResult = MessageBox.Show(
                "Bạn có chắc chắn muốn nộp task này?\n\n" +
                "⚠️ Lưu ý: Sau khi nộp, task sẽ chuyển sang trạng thái HOÀN THÀNH và không thể chỉnh sửa nữa. " +
                "Bạn chỉ có thể xem thông tin.",
                "Xác nhận nộp Task",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmResult != DialogResult.Yes)
            {
                return;
            }

            try
            {
                this.Cursor = Cursors.WaitCursor;
                
                bool success = await _taskApi.UpdateTaskStatusAsync(
                    _currentTask.TaskId, 
                    TimeFlow.Models.TaskStatus.Completed
                );
                
                if (success)
                {
                    // Raise event de refresh form cha
                    TaskUpdated?.Invoke(this, new TaskUpdateEventArgs
                    {
                        TaskId = _currentTask.TaskId,
                        Status = TimeFlow.Models.TaskStatus.Completed
                    });
                    
                    MessageBox.Show(
                        "✓ Task đã được nộp thành công!\n\n" +
                        "Task hiện đã chuyển sang trạng thái HOÀN THÀNH.\n" +
                        "Bạn không thể chỉnh sửa task này nữa, chỉ có thể xem thông tin.",
                        "Nộp Task Thành Công",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    
                    // Refresh de cap nhat UI 
                    RefreshTaskDetail();
                }
                else
                {
                    MessageBox.Show("Không thể nộp task. Vui lòng thử lại!", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi nộp task: {ex.Message}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void BtnGroup_Click(object sender, EventArgs e)
        {
            // mo form group tasks
            FormGroupTaskList groupTasksForm = new FormGroupTaskList();
            groupTasksForm.Show();
        }

        private async void EditItem_Click(object sender, EventArgs e)
        {
            if (_currentTask == null) return;

            using (FormThemTask editForm = new FormThemTask(_currentTask.TaskId))
            {
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    await LoadTaskDetailAsync(_taskId);

                    TaskUpdated?.Invoke(this, new TaskUpdateEventArgs
                    {
                        TaskId = _currentTask.TaskId,
                        Title = _currentTask.Title,
                        Status = _currentTask.Status,
                        Priority = _currentTask.Priority,
                        DueDate = _currentTask.DueDate
                    });

                    UpdateButtonStates();
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
                        //Raise TaskDeleted  truoc khi close
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
            if (_currentTask.Status == newStatus)
            {
                MessageBox.Show($"Task đã ở trạng thái {_currentTask.StatusText} rồi!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                this.Cursor = Cursors.WaitCursor;
                
                bool success = await _taskApi.UpdateTaskStatusAsync(_currentTask.TaskId, newStatus);

                if (success)
                {
                    // cap nhat local state
                    var oldStatus = _currentTask.Status;
                    _currentTask.Status = newStatus;

                    // cap nhat UI
                    Color newColor = GetStatusColor(newStatus);
                    _statusBadge.Text = _currentTask.StatusText;
                    _statusBadge.BackColor = newColor;
                    _statusBadge.ForeColor = newColor == AppColors.Yellow500 ? AppColors.Gray800 : Color.White;
                    _statusBadge.Text = _currentTask.StatusText;
                    _statusBadge.Invalidate();
                    //cap nhat trang thai button 
                    UpdateButtonStates();

                    //Raise TaskUpdated 
                    TaskUpdated?.Invoke(this, new TaskUpdateEventArgs
                    {
                        TaskId = _currentTask.TaskId,
                        Status = newStatus
                    });

                    MessageBox.Show($"Trạng thái đã được đổi sang {_currentTask.StatusText}", "Thành công", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            finally
            {
                this.Cursor = Cursors.Default;
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

        private void RenderComments()
        {
            if (_currentTask == null || !_currentTask.HasComments) return;
            var centerPanel = FindCenterContentPanel();
            if (centerPanel == null) return;

            centerPanel.SuspendLayout();
            var labelToRemove = centerPanel.Controls.OfType<Label>()
                .FirstOrDefault(l => l.Text.Contains("No comments") || l.Text.Contains("Loading comments"));
            if (labelToRemove != null)
            {
                centerPanel.Controls.Remove(labelToRemove);
            }

            const int INITIAL_COMMENTS = 10;
            var commentsToRender = _currentTask.Comments.Take(INITIAL_COMMENTS).ToList();
            int spacerIndex = centerPanel.Controls.Count - 1;

            foreach (var comment in commentsToRender)
            {
                var commentControl = CreateComment(
                    comment.DisplayName,
                    comment.Content,
                    comment.TimeAgo
                );
                centerPanel.Controls.Add(commentControl);
                centerPanel.Controls.SetChildIndex(commentControl, spacerIndex);
            }
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
                centerPanel.Controls.Add(loadMoreLabel);
                centerPanel.Controls.SetChildIndex(loadMoreLabel, spacerIndex);
            }

            centerPanel.ResumeLayout();
        }

        private void RenderActivities()
        {
            if (_currentTask == null || !_currentTask.HasActivities) return;
            var rightSidebar = FindRightSidebarPanel();
            if (rightSidebar == null) return;

            rightSidebar.SuspendLayout();
            var labelToRemove = rightSidebar.Controls.OfType<Label>()
                .FirstOrDefault(l => l.Text.Contains("No activity") || l.Text.Contains("Loading activities"));
            if (labelToRemove != null)
            {
                rightSidebar.Controls.Remove(labelToRemove);
            }
            const int INITIAL_ACTIVITIES = 10;
            var activitiesToRender = _currentTask.Activities
                .OrderByDescending(a => a.CreatedAt)
                .Take(INITIAL_ACTIVITIES)
                .ToList();

            int contentWidth = 302; 
            int spacerIndex = rightSidebar.Controls.Count - 1;

            foreach (var activity in activitiesToRender)
            {
                var activityControl = CreateActivityLog(
                    activity.Description,
                    activity.TimeAgo,
                    contentWidth
                );
                
                rightSidebar.Controls.Add(activityControl);
                rightSidebar.Controls.SetChildIndex(activityControl, spacerIndex);
            }
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
                rightSidebar.Controls.Add(showMoreLabel);
                rightSidebar.Controls.SetChildIndex(showMoreLabel, spacerIndex);
            }

            rightSidebar.ResumeLayout();
        }

        private void LoadMoreComments(FlowLayoutPanel centerPanel, int alreadyLoaded)
        {
            centerPanel.SuspendLayout();
            var loadMoreLabel = centerPanel.Controls.OfType<Label>()
                .FirstOrDefault(l => l.Text.StartsWith("+ Load"));
            if (loadMoreLabel != null)
            {
                centerPanel.Controls.Remove(loadMoreLabel);
            }
            const int BATCH_SIZE = 10;
            var nextBatch = _currentTask.Comments
                .Skip(alreadyLoaded)
                .Take(BATCH_SIZE)
                .ToList();
            int spacerIndex = centerPanel.Controls.Count - 1;

            foreach (var comment in nextBatch)
            {
                var commentControl = CreateComment(
                    comment.DisplayName,
                    comment.Content,
                    comment.TimeAgo
                );           
                centerPanel.Controls.Add(commentControl);
                centerPanel.Controls.SetChildIndex(commentControl, spacerIndex);
            }
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
                
                centerPanel.Controls.Add(newLoadMoreLabel);
                centerPanel.Controls.SetChildIndex(newLoadMoreLabel, spacerIndex);
            }

            centerPanel.ResumeLayout();
        }

        private FlowLayoutPanel FindCenterContentPanel()
        {
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
            var rootPanel = this.Controls.OfType<Panel>().FirstOrDefault();
            if (rootPanel == null) return null;

            var mainLayout = rootPanel.Controls.OfType<TableLayoutPanel>().FirstOrDefault();
            if (mainLayout == null) return null;

            var modernPanel = mainLayout.Controls.OfType<ModernPanel>().FirstOrDefault();
            if (modernPanel == null) return null;

            return modernPanel.Controls.OfType<FlowLayoutPanel>().FirstOrDefault();
        }

        private void UpdateAssigneeInfo()
        {
            if (_currentTask == null || !_currentTask.IsGroupTask) return;

            var rightSidebar = FindRightSidebarPanel();
            if (rightSidebar == null) return;
            var assigneeLabels = rightSidebar.Controls.OfType<Label>()
                .Where(l => l.Text != null && l.Text.StartsWith("Assigned to:"))
                .ToList();

            if (assigneeLabels.Count > 0)
            {
                var assigneeLabel = assigneeLabels[0];
                string assigneeText;
                if (_currentTask.HasAssignees)
                {
                    assigneeText = string.Join(", ", _currentTask.Assignees.Take(3));
                    if (_currentTask.Assignees.Count > 3)
                    {
                        assigneeText += $" and {_currentTask.Assignees.Count - 3} more";
                    }
                    assigneeLabel.Text = $"Assigned to: {assigneeText}";
                    assigneeLabel.ForeColor = AppColors.Gray800;
                }
                else
                {
                    assigneeLabel.Text = "Assigned to: ⚠ Unassigned";
                    assigneeLabel.ForeColor = AppColors.Red500;
                }
            }
            var detailsContainer = rightSidebar.Controls.OfType<TableLayoutPanel>()
                .FirstOrDefault(t => t.ColumnCount == 2 && t.RowCount >= 1);
            
            if (detailsContainer != null)
            {
                if (detailsContainer.Controls.Count > 1)
                {
                    var assigneesPanel = detailsContainer.Controls[1] as FlowLayoutPanel;
                    if (assigneesPanel != null && assigneesPanel.Controls.Count >= 2)
                    {
                        var assigneeNameLabel = assigneesPanel.Controls[1] as Label;
                        if (assigneeNameLabel != null)
                        {
                            if (_currentTask.HasAssignees)
                            {
                                string assigneeNames = string.Join(", ", _currentTask.Assignees.Take(2));
                                assigneeNameLabel.Text = assigneeNames;
                                assigneeNameLabel.ForeColor = AppColors.Gray800;
                                if (_currentTask.Assignees.Count > 2)
                                {
                                    var countLabel = assigneesPanel.Controls.OfType<Label>()
                                        .FirstOrDefault(l => l.Text != null && l.Text.StartsWith("(+"));
                                    if (countLabel != null)
                                    {
                                        countLabel.Text = $"(+{_currentTask.Assignees.Count - 2})";
                                    }
                                    else
                                    {
                                        assigneesPanel.Controls.Add(new Label
                                        {
                                            Text = $"(+{_currentTask.Assignees.Count - 2})",
                                            Font = FontRegular,
                                            ForeColor = AppColors.Gray500,
                                            AutoSize = true
                                        });
                                    }
                                }
                                else
                                {
                                    var countLabel = assigneesPanel.Controls.OfType<Label>()
                                        .FirstOrDefault(l => l.Text != null && l.Text.StartsWith("(+"));
                                    if (countLabel != null)
                                    {
                                        assigneesPanel.Controls.Remove(countLabel);
                                    }
                                }
                            }
                            else
                            {
                                assigneeNameLabel.Text = "Unassigned";
                                assigneeNameLabel.ForeColor = AppColors.Gray800;
                                var countLabel = assigneesPanel.Controls.OfType<Label>()
                                    .FirstOrDefault(l => l.Text != null && l.Text.StartsWith("(+"));
                                if (countLabel != null)
                                {
                                    assigneesPanel.Controls.Remove(countLabel);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void FormTaskDetail_Load(object sender, EventArgs e)
        {
        }
    }
}