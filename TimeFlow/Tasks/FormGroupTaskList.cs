using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks; // Thêm dòng này
using TimeFlow.UI.Components;
using TimeFlow.Models;
using TimeFlow.Services;
// using TimeFlow.Forms; <--- XÓA DÒNG NÀY (Vì ChatForm nằm trong TimeFlow namespace gốc)
using TimeFlow; // <--- THÊM DÒNG NÀY để nhận diện ChatForm và GlobalState

namespace TimeFlow.Tasks
{
    public partial class FormGroupTaskList : Form
    {
        // --- Styles & Constants ---
        private readonly Font FontRegular = new Font("Segoe UI", 10F, FontStyle.Regular);
        private readonly Font FontBold = new Font("Segoe UI", 10F, FontStyle.Bold);
        private readonly Font FontHeaderTitle = new Font("Segoe UI", 14F, FontStyle.Bold);
        private readonly Color HeaderIconColor = AppColors.Gray600;
        private const int INITIAL_TASKS_TO_RENDER = 20;

        // --- API Clients ---
        private readonly TaskApiClient _taskApi;
        private readonly GroupApiClient _groupApi;

        // --- Data ---
        private List<TaskItem> _currentTasks;
        private int _tasksRendered = 0;
        private int? _selectedGroupId;
        private string _groupName = "All Groups";

        // --- UI References ---
        private CustomFlowLayoutPanel _contentPanel;
        private FlowLayoutPanel _groupsContainer;
        private Label _headerTitleLabel;

        public FormGroupTaskList(int? groupId = null, string groupName = "All Groups")
        {
            InitializeComponent();

            _taskApi = new TaskApiClient();
            _groupApi = new GroupApiClient();
            _currentTasks = new List<TaskItem>();
            _selectedGroupId = groupId;
            _groupName = groupName;

            SetupCustomLayout();
        }

        private void SetupCustomLayout()
        {
            this.SuspendLayout();
            this.BackColor = AppColors.Gray100;
            this.Padding = new Padding(0);

            Panel rootPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(0)
            };
            this.Controls.Add(rootPanel);

            Control headerBar = CreateHeaderBar();
            headerBar.Dock = DockStyle.Top;
            rootPanel.Controls.Add(headerBar);

            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowStyles = { new RowStyle(SizeType.Percent, 100F) },
                ColumnStyles =
                {
                    new ColumnStyle(SizeType.Absolute, 300F),
                    new ColumnStyle(SizeType.Percent, 100F)
                },
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                Padding = new Padding(0),
                Margin = new Padding(0),
                BackColor = Color.Transparent
            };
            rootPanel.Controls.Add(mainLayout);

            Control leftMenu = CreateLeftMenu();
            mainLayout.Controls.Add(leftMenu, 0, 0);

            mainLayout.Controls.Add(CreateTaskListContent(), 1, 0);

            this.ResumeLayout();

            LoadUserGroupsAsync();
        }

        private Control CreateHeaderBar()
        {
            Panel headerWrapper = new Panel
            {
                Dock = DockStyle.Top,
                Height = 61,
                BackColor = Color.White,
                Margin = new Padding(0)
            };

            TableLayoutPanel headerTable = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.White,
                ColumnCount = 4,
                ColumnStyles =
                {
                    new ColumnStyle(SizeType.AutoSize),
                    new ColumnStyle(SizeType.Percent, 100F),
                    new ColumnStyle(SizeType.AutoSize),
                    new ColumnStyle(SizeType.AutoSize)
                },
                RowCount = 1,
                Padding = new Padding(16, 10, 16, 10)
            };

            FlowLayoutPanel leftContainer = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom,
                Margin = new Padding(0)
            };

            CustomButton arrowButton = new CustomButton
            {
                Text = "←",
                Font = new Font("Segoe UI Emoji", 16F),
                ForeColor = HeaderIconColor,
                BackColor = Color.White,
                HoverColor = AppColors.Gray200,
                BorderRadius = 4,
                Width = 40,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter
            };
            arrowButton.Click += (sender, e) => { this.Close(); };
            leftContainer.Controls.Add(arrowButton);

            _headerTitleLabel = new Label
            {
                Text = $"👥 {_groupName}",
                Font = FontHeaderTitle,
                ForeColor = AppColors.Gray800,
                AutoSize = true,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(8, 0, 0, 0)
            };
            leftContainer.Controls.Add(_headerTitleLabel);
            headerTable.Controls.Add(leftContainer, 0, 0);

            // Chat Button
            CustomButton chatButton = new CustomButton
            {
                Text = "💬",
                Font = new Font("Segoe UI Emoji", 14F),
                ForeColor = AppColors.Blue600,
                BackColor = Color.AliceBlue, // Đảm bảo Color.AliceBlue (System.Drawing) hoặc AppColors.AliceBlue
                HoverColor = AppColors.Blue100, // Đã fix trong AppColors.cs
                BorderRadius = 8,
                Width = 40,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 0, 10, 0),
                Cursor = Cursors.Hand
            };
            chatButton.Click += BtnChat_Click;
            headerTable.Controls.Add(chatButton, 2, 0);

            // Close Button
            CustomButton closeButton = new CustomButton
            {
                Text = "✕",
                Font = new Font("Segoe UI Emoji", 14F, FontStyle.Bold),
                ForeColor = HeaderIconColor,
                BackColor = Color.White,
                HoverColor = AppColors.Gray200,
                BorderRadius = 4,
                Width = 40,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter
            };
            closeButton.Click += (sender, e) => { this.Close(); };
            headerTable.Controls.Add(closeButton, 3, 0);

            Panel separator = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = AppColors.Gray200 };

            headerTable.Dock = DockStyle.Fill;
            headerWrapper.Controls.Add(headerTable);
            headerWrapper.Controls.Add(separator);

            return headerWrapper;
        }

        private Control CreateLeftMenu()
        {
            Panel menuWrapper = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(0)
            };

            FlowLayoutPanel menuPanel = new CustomFlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(24, 20, 24, 16),
                BackColor = Color.White
            };

            Panel separator = new Panel { Dock = DockStyle.Right, Width = 1, BackColor = AppColors.Gray200 };

            menuWrapper.Controls.Add(menuPanel);
            menuWrapper.Controls.Add(separator);

            Label groupsTitle = new Label
            {
                Text = "YOUR GROUPS",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = AppColors.Gray700,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 16)
            };
            menuPanel.Controls.Add(groupsTitle);

            _groupsContainer = new FlowLayoutPanel
            {
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Width = 252,
                Margin = new Padding(0, 0, 0, 20)
            };
            menuPanel.Controls.Add(_groupsContainer);

            var btnAll = CreateMenuButton("All Groups", AppColors.Blue500, Color.White, 40, AppColors.Blue600);
            btnAll.Click += (s, e) => SwitchGroup(null, "All Groups");
            _groupsContainer.Controls.Add(btnAll);

            Label filterTitle = new Label
            {
                Text = "FILTER",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = AppColors.Gray700,
                AutoSize = true,
                Margin = new Padding(0, 10, 0, 16)
            };
            menuPanel.Controls.Add(filterTitle);

            menuPanel.Controls.Add(CreateMenuButton("All Tasks", Color.White, AppColors.Gray700, 40, AppColors.Gray100, 1, AppColors.Gray300));
            menuPanel.Controls.Add(CreateMenuButton("Assigned", Color.White, AppColors.Gray700, 40, AppColors.Gray100, 1, AppColors.Gray300));
            menuPanel.Controls.Add(CreateMenuButton("Unassigned", Color.White, AppColors.Gray700, 40, AppColors.Gray100, 1, AppColors.Gray300));

            return menuWrapper;
        }

        private Control CreateTaskListContent()
        {
            CustomFlowLayoutPanel contentPanel = new CustomFlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(32, 20, 32, 24),
                BackColor = AppColors.Gray100,
            };

            LoadTasksAsync(contentPanel);
            return contentPanel;
        }

        // --- EVENTS ---

        private void BtnChat_Click(object sender, EventArgs e)
        {
            try
            {
                // Kiểm tra GlobalState đã được định nghĩa chưa
                if (GlobalState.Client != null && GlobalState.CurrentUser != null)
                {
                    ChatForm chatForm = new ChatForm(GlobalState.Client, GlobalState.CurrentUser.Username);
                    chatForm.Show();
                }
                else
                {
                    MessageBox.Show("Chưa kết nối tới máy chủ chat hoặc chưa đăng nhập!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể mở cửa sổ chat: " + ex.Message);
            }
        }

        private async void LoadUserGroupsAsync()
        {
            try
            {
                // Xóa các button cũ (trừ nút All Groups đầu tiên)
                while (_groupsContainer.Controls.Count > 1)
                {
                    _groupsContainer.Controls.RemoveAt(1);
                }

                // --- GỌI API THẬT ---
                // Đảm bảo CurrentUser đã có UserId (được set lúc Login)
                int currentUserId = GlobalState.CurrentUser?.UserId ?? 0;

                if (currentUserId == 0)
                {
                    MessageBox.Show("Không tìm thấy thông tin người dùng. Vui lòng đăng nhập lại.");
                    return;
                }

                var groups = await _groupApi.GetGroupsByUserIdAsync(currentUserId);

                if (groups == null || groups.Count == 0)
                {
                    // Không có group nào hoặc lỗi kết nối
                    return;
                }
                // --------------------

                foreach (var group in groups)
                {
                    var btn = CreateMenuButton(
                        group.GroupName,
                        Color.White,
                        AppColors.Gray700,
                        40,
                        AppColors.Gray100,
                        0
                    );

                    // Highlight nếu đang chọn group này
                    if (_selectedGroupId == group.GroupId)
                    {
                        btn.BackColor = AppColors.Blue100;
                        btn.ForeColor = AppColors.Blue600;
                    }

                    // Sự kiện Click: Chuyển Group
                    btn.Click += (s, e) => SwitchGroup(group.GroupId, group.GroupName);
                    _groupsContainer.Controls.Add(btn);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading groups: " + ex.Message);
                MessageBox.Show("Không thể tải danh sách nhóm.");
            }
        }

        private void SwitchGroup(int? groupId, string groupName)
        {
            _selectedGroupId = groupId;
            _groupName = groupName;

            if (_headerTitleLabel != null) _headerTitleLabel.Text = $"👥 {_groupName}";

            foreach (Control c in _groupsContainer.Controls)
            {
                if (c is CustomButton btn)
                {
                    if (btn.Text == groupName)
                    {
                        btn.BackColor = AppColors.Blue500;
                        btn.ForeColor = Color.White;
                    }
                    else
                    {
                        btn.BackColor = Color.White;
                        btn.ForeColor = AppColors.Gray700;
                    }
                }
            }

            _contentPanel.Controls.Clear();
            LoadTasksAsync(_contentPanel);
        }

        private async void LoadTasksAsync(CustomFlowLayoutPanel contentPanel)
        {
            try
            {
                _contentPanel = contentPanel;

                Label loadingLabel = new Label
                {
                    Text = "⏳ Loading group tasks...",
                    Font = FontRegular,
                    ForeColor = AppColors.Gray600,
                    AutoSize = true,
                    Margin = new Padding(0, 20, 0, 0)
                };
                contentPanel.Controls.Add(loadingLabel);

                var allTasks = await _taskApi.GetTasksAsync();

                _currentTasks = allTasks.Where(t => t.IsGroupTask).ToList();

                if (_selectedGroupId.HasValue)
                {
                    _currentTasks = _currentTasks.Where(t => t.GroupTask?.GroupId == _selectedGroupId.Value).ToList();
                }

                contentPanel.Controls.Remove(loadingLabel);
                RenderTaskList(contentPanel, _currentTasks);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load group tasks: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RenderTaskList(CustomFlowLayoutPanel contentPanel, List<TaskItem> tasks)
        {
            contentPanel.SuspendLayout();

            int assignedCount = tasks.Count(t => t.GroupTask?.AssignedTo != null);
            int unassignedCount = tasks.Count(t => t.GroupTask?.AssignedTo == null);

            // Header Layout
            TableLayoutPanel headerLayout = new TableLayoutPanel
            {
                ColumnCount = 2,
                ColumnStyles = { new ColumnStyle(SizeType.Percent, 100F), new ColumnStyle(SizeType.Absolute, 180F) },
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 24),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            headerLayout.SizeChanged += (s, e) => { if (headerLayout.Parent is FlowLayoutPanel p) headerLayout.Width = p.ClientSize.Width - p.Padding.Left - p.Padding.Right; };

            Label title = new Label
            {
                Text = _selectedGroupId.HasValue ? _groupName : "All Group Tasks",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = AppColors.Gray800,
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom
            };
            headerLayout.Controls.Add(title, 0, 0);

            Label stats = new Label
            {
                Text = $"{assignedCount} assigned | {unassignedCount} unassigned",
                Font = FontRegular,
                ForeColor = AppColors.Gray600,
                AutoSize = true,
                Anchor = AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom,
                TextAlign = ContentAlignment.BottomRight,
            };
            headerLayout.Controls.Add(stats, 1, 0);
            contentPanel.Controls.Add(headerLayout);

            // Column Headers
            TableLayoutPanel columnHeader = new TableLayoutPanel
            {
                ColumnCount = 5,
                ColumnStyles =
                {
                    new ColumnStyle(SizeType.Percent, 40F),
                    new ColumnStyle(SizeType.Percent, 20F),
                    new ColumnStyle(SizeType.Percent, 15F),
                    new ColumnStyle(SizeType.Percent, 12.5F),
                    new ColumnStyle(SizeType.Percent, 12.5F)
                },
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 10),
                BackColor = Color.Transparent,
                Padding = new Padding(12, 0, 12, 0),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            columnHeader.SizeChanged += (s, e) => { if (columnHeader.Parent is FlowLayoutPanel p) columnHeader.Width = p.ClientSize.Width - p.Padding.Left - p.Padding.Right; };

            void AddHeaderLabel(string text, int col)
            {
                columnHeader.Controls.Add(new Label { Text = text, Font = FontBold, ForeColor = AppColors.Gray500, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, col, 0);
            }

            AddHeaderLabel("TASK NAME", 0);
            AddHeaderLabel("ASSIGNEE", 1);
            AddHeaderLabel("DUE DATE", 2);
            AddHeaderLabel("STATUS", 3);
            AddHeaderLabel("PRIORITY", 4);
            contentPanel.Controls.Add(columnHeader);

            // Render Tasks (Virtual Scrolling)
            _tasksRendered = 0;
            int tasksToRender = Math.Min(INITIAL_TASKS_TO_RENDER, tasks.Count);
            RenderTaskBatch(contentPanel, tasks, 0, tasksToRender);

            if (tasks.Count > INITIAL_TASKS_TO_RENDER) AddLoadMoreButton(contentPanel, tasks);

            contentPanel.ResumeLayout();
        }

        private void RenderTaskBatch(CustomFlowLayoutPanel contentPanel, List<TaskItem> tasks, int startIndex, int count)
        {
            var tasksToRender = tasks.Skip(startIndex).Take(count).ToList();
            foreach (var task in tasksToRender)
            {
                Control taskItem = CreateGroupTaskItem(task);
                taskItem.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                taskItem.SizeChanged += (s, e) => { if (taskItem.Parent is FlowLayoutPanel p) taskItem.Width = p.ClientSize.Width - p.Padding.Left - p.Padding.Right; };
                contentPanel.Controls.Add(taskItem);
                _tasksRendered++;
            }
        }

        private Control CreateGroupTaskItem(TaskItem task)
        {
            ModernPanel taskItemPanel = new ModernPanel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.White,
                BorderRadius = 12,
                BorderThickness = 1,
                BorderColor = AppColors.Gray200,
                Margin = new Padding(0, 0, 0, 12),
                Cursor = Cursors.Hand,
            };
            taskItemPanel.Click += (s, e) => OpenTaskDetail(task);

            TableLayoutPanel taskLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 5,
                ColumnStyles = { new ColumnStyle(SizeType.Percent, 40F), new ColumnStyle(SizeType.Percent, 20F), new ColumnStyle(SizeType.Percent, 15F), new ColumnStyle(SizeType.Percent, 12.5F), new ColumnStyle(SizeType.Percent, 12.5F) },
                RowCount = 1,
                Padding = new Padding(16, 8, 16, 8),
                Margin = new Padding(0),
                BackColor = Color.Transparent,
            };
            taskLayout.Click += (s, e) => OpenTaskDetail(task);

            // 1. Name
            var nameLbl = new Label { Text = task.Title, Font = FontBold, ForeColor = AppColors.Gray800, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            nameLbl.Click += (s, e) => OpenTaskDetail(task);
            taskLayout.Controls.Add(nameLbl, 0, 0);

            // 2. Assignee
            string assigneeText = task.GroupTask?.AssignedTo != null ? "Assigned" : "⚠ Unassigned";
            var assigneeLbl = new Label { Text = assigneeText, Font = FontRegular, ForeColor = task.GroupTask?.AssignedTo != null ? AppColors.Gray700 : AppColors.Red500, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            assigneeLbl.Click += (s, e) => OpenTaskDetail(task);
            taskLayout.Controls.Add(assigneeLbl, 1, 0);

            // 3. Due Date
            var dueLbl = new Label { Text = task.DueDate.HasValue ? task.DueDate.Value.ToString("MMM dd, yyyy") : "No due date", Font = FontRegular, ForeColor = AppColors.Gray700, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            dueLbl.Click += (s, e) => OpenTaskDetail(task);
            taskLayout.Controls.Add(dueLbl, 2, 0);

            // 4. Status
            Panel statusWrapper = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0) };
            statusWrapper.Click += (s, e) => OpenTaskDetail(task);
            var statusTag = CreateTag(task.StatusText, GetStatusColor(task.Status));
            statusTag.Click += (s, e) => OpenTaskDetail(task);
            statusWrapper.Controls.Add(statusTag); statusTag.Anchor = AnchorStyles.None;
            taskLayout.Controls.Add(statusWrapper, 3, 0);

            // 5. Priority
            Panel priorityWrapper = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0) };
            priorityWrapper.Click += (s, e) => OpenTaskDetail(task);
            var priorityTag = CreateTag(task.PriorityText, GetPriorityColor(task.Priority));
            priorityTag.Click += (s, e) => OpenTaskDetail(task);
            priorityWrapper.Controls.Add(priorityTag); priorityTag.Anchor = AnchorStyles.None;
            taskLayout.Controls.Add(priorityWrapper, 4, 0);

            taskItemPanel.Controls.Add(taskLayout);
            return taskItemPanel;
        }

        private void OpenTaskDetail(TaskItem task)
        {
            FormTaskDetail detailForm = new FormTaskDetail(task);

            // Xử lý sự kiện update/delete để refresh list
            detailForm.TaskUpdated += (s, e) =>
            {
                var t = _currentTasks.FirstOrDefault(x => x.TaskId == e.TaskId);
                if (t != null) t.Status = e.Status;
                RefreshTaskList();
            };

            detailForm.TaskDeleted += (s, e) =>
            {
                var t = _currentTasks.FirstOrDefault(x => x.TaskId == task.TaskId);
                if (t != null) _currentTasks.Remove(t);
                RefreshTaskList();
            };

            detailForm.Show();
        }

        // Định nghĩa hàm RefreshTaskList để sửa lỗi "does not exist"
        private void RefreshTaskList()
        {
            if (_contentPanel == null) return;

            _contentPanel.SuspendLayout();

            var controlsToRemove = _contentPanel.Controls
                .OfType<Control>()
                .Where(c => c is ModernPanel || c is CustomButton || (c is Label && c.Text.Contains("All")))
                .ToList();

            foreach (var control in controlsToRemove)
            {
                _contentPanel.Controls.Remove(control);
            }

            _tasksRendered = 0;
            int tasksToRender = Math.Min(INITIAL_TASKS_TO_RENDER, _currentTasks.Count);
            RenderTaskBatch(_contentPanel, _currentTasks, 0, tasksToRender);

            if (_currentTasks.Count > INITIAL_TASKS_TO_RENDER)
            {
                AddLoadMoreButton(_contentPanel, _currentTasks);
            }

            _contentPanel.ResumeLayout();
        }

        private void AddLoadMoreButton(CustomFlowLayoutPanel contentPanel, List<TaskItem> tasks)
        {
            var btn = new CustomButton
            {
                Text = $"⬇ Load {Math.Min(20, tasks.Count - _tasksRendered)} more tasks",
                BackColor = AppColors.Blue500,
                ForeColor = Color.White,
                HoverColor = AppColors.Blue600,
                BorderRadius = 8,
                Width = 300,
                Height = 50,
                Font = FontBold,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 20, 0, 20),
                Anchor = AnchorStyles.None
            };
            btn.Click += (s, e) =>
            {
                contentPanel.SuspendLayout(); contentPanel.Controls.Remove(btn);
                RenderTaskBatch(contentPanel, tasks, _tasksRendered, Math.Min(20, tasks.Count - _tasksRendered));
                if (_tasksRendered < tasks.Count) AddLoadMoreButton(contentPanel, tasks);
                else contentPanel.Controls.Add(new Label { Text = $"✓ All {tasks.Count} tasks loaded", Font = FontRegular, ForeColor = AppColors.Gray500, AutoSize = true, Margin = new Padding(0, 20, 0, 20) });
                contentPanel.ResumeLayout();
            };
            contentPanel.Controls.Add(btn);
        }

        // --- HELPERS ---
        private CustomButton CreateMenuButton(string text, Color backColor, Color foreColor, int height, Color? hoverColor = null, int borderThickness = 0, Color? borderColor = null)
        {
            var btn = new CustomButton
            {
                Text = text,
                BackColor = backColor,
                ForeColor = foreColor,
                HoverColor = hoverColor ?? AppColors.Blue600,
                BorderRadius = 8,
                Width = 252,
                Height = height,
                Font = FontBold,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 0, 0, 12),
                BorderThickness = borderThickness,
                BorderColor = borderColor ?? Color.Transparent,
                Cursor = Cursors.Hand
            };
            if (borderColor.HasValue) btn.HoverBorderColor = borderColor.Value;
            return btn;
        }

        private Color GetStatusColor(TimeFlow.Models.TaskStatus status) => status switch
        {
            TimeFlow.Models.TaskStatus.Pending => AppColors.Yellow500,
            TimeFlow.Models.TaskStatus.InProgress => AppColors.Blue500,
            TimeFlow.Models.TaskStatus.Completed => AppColors.Green500,
            _ => AppColors.Gray400
        };

        private Color GetPriorityColor(TaskPriority priority) => priority switch
        {
            TaskPriority.Low => AppColors.Green500,
            TaskPriority.Medium => AppColors.Orange500,
            TaskPriority.High => AppColors.Red500,
            _ => AppColors.Gray400
        };

        private ModernPanel CreateTag(string text, Color backColor)
        {
            return new ModernPanel
            {
                Text = text,
                BackColor = backColor,
                ForeColor = (backColor == AppColors.Yellow500 || backColor == AppColors.Green500) ? AppColors.Gray800 : Color.White,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                BorderRadius = 4,
                Height = 24,
                TextAlign = ContentAlignment.MiddleCenter,
                Anchor = AnchorStyles.None,
                AutoSize = true,
                Padding = new Padding(8, 2, 8, 2),
                Margin = new Padding(0)
            };
        }

        private void FormGroupTaskList_Load(object sender, EventArgs e) { }
    }
}