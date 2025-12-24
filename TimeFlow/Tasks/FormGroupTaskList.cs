using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TimeFlow.Models;
using TimeFlow.Services;
using TimeFlow.UI.Components;

namespace TimeFlow.Tasks
{
    /// <summary>
    /// Form hiển thị danh sách group tasks
    /// Kế thừa từ FormTaskList và override các methods cần thiết
    /// </summary>
    public partial class FormGroupTaskList : Form
    {
        private readonly Font FontRegular = new Font("Segoe UI", 10F, FontStyle.Regular);
        private readonly Font FontBold = new Font("Segoe UI", 10F, FontStyle.Bold);
        private readonly Font FontHeaderTitle = new Font("Segoe UI", 14F, FontStyle.Bold);
        private readonly Color HeaderIconColor = AppColors.Gray600;
        private readonly TaskApiClient _taskApi;
        private List<TaskItem> _currentTasks;
        
        // ✅ Virtual scrolling
        private const int INITIAL_TASKS_TO_RENDER = 20;
        private int _tasksRendered = 0;
        private CustomFlowLayoutPanel _contentPanel;
        
        // ✅ Group-specific properties
        private int? _selectedGroupId;
        private string _groupName = "All Groups";
        
        // ✅ Danh sách groups và left menu panel
        private List<Group> _groups = new List<Group>();
        private FlowLayoutPanel _groupsMenuPanel;

        public FormGroupTaskList(int? groupId = null, string groupName = "All Groups")
        {
            InitializeComponent();
            _taskApi = new TaskApiClient();
            _currentTasks = new List<TaskItem>();
            _selectedGroupId = groupId;
            _groupName = groupName;
            SetupLayout();
        }

        private void SetupLayout()
        {
            this.SuspendLayout();
            
            this.Text = _groupName;
            this.BackColor = AppColors.Gray100;
            this.WindowState = FormWindowState.Maximized;
            this.Padding = new Padding(0);
            this.MinimumSize = new Size(1024, 600);

            Panel rootPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(0)
            };
            this.Controls.Add(rootPanel);

            // Header bar
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

            // Left menu với group list
            Control leftMenu = CreateLeftMenu();
            mainLayout.Controls.Add(leftMenu, 0, 0);
            
            // Task list content
            mainLayout.Controls.Add(CreateTaskListContent(), 1, 0);
            
            this.ResumeLayout();
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
                ColumnCount = 3,
                ColumnStyles =
                {
                    new ColumnStyle(SizeType.AutoSize),
                    new ColumnStyle(SizeType.Percent, 100F),
                    new ColumnStyle(SizeType.AutoSize)
                },
                RowCount = 1,
                RowStyles = { new RowStyle(SizeType.Percent, 100F) },
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
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0)
            };
            arrowButton.Click += (sender, e) => { this.Close(); };
            leftContainer.Controls.Add(arrowButton);

            Label titleLabel = new Label
            {
                Text = $"👥 {_groupName}",
                Font = FontHeaderTitle,
                ForeColor = AppColors.Gray800,
                AutoSize = true,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(8, 0, 0, 0)
            };
            leftContainer.Controls.Add(titleLabel);
            headerTable.Controls.Add(leftContainer, 0, 0);

            FlowLayoutPanel rightContainer = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight, // ✅ Đổi sang LeftToRight để dễ kiểm soát
                AutoSize = true,
                WrapContents = false,
                Anchor = AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom,
                Margin = new Padding(0)
            };

            // ✅ Nút Group Chat với text rõ ràng
            CustomButton chatButton = new CustomButton
            {
                Text = "💬 Group Chat",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = AppColors.Blue500,
                BackColor = Color.White,
                HoverColor = AppColors.Blue50,
                BorderRadius = 4,
                Width = 130,
                Height = 36,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 0, 10, 0),
                BorderThickness = 1,
                BorderColor = AppColors.Blue500
            };
            chatButton.Click += BtnChat_Click;
            rightContainer.Controls.Add(chatButton);

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
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0)
            };
            closeButton.Click += (sender, e) => { this.Close(); };
            rightContainer.Controls.Add(closeButton);

            headerTable.Controls.Add(rightContainer, 2, 0);

            Panel separator = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 1,
                BackColor = AppColors.Gray200
            };

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
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            
            FlowLayoutPanel menuPanel = new CustomFlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(24, 70, 24, 16),
                BackColor = Color.White,
                Margin = new Padding(0)
            };
            
            Panel separator = new Panel
            {
                Dock = DockStyle.Right,
                Width = 1,
                BackColor = AppColors.Gray200
            };
            
            menuWrapper.Controls.Add(menuPanel);
            menuWrapper.Controls.Add(separator);

            int buttonHeight = 40;

            // Groups section header
            TableLayoutPanel groupsHeader = new TableLayoutPanel
            {
                ColumnCount = 2,
                ColumnStyles =
                {
                    new ColumnStyle(SizeType.Percent, 100F),
                    new ColumnStyle(SizeType.Absolute, 36F)
                },
                RowCount = 1,
                Width = 252,
                Height = 36,
                Margin = new Padding(0, 0, 0, 12)
            };

            Label groupsTitle = new Label
            {
                Text = "YOUR GROUPS",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = AppColors.Gray700,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            groupsHeader.Controls.Add(groupsTitle, 0, 0);

            CustomButton btnAddGroup = new CustomButton
            {
                Text = "+",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = AppColors.Blue500,
                BackColor = Color.White,
                HoverColor = AppColors.Blue50,
                BorderRadius = 4,
                Width = 36,
                Height = 36,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand,
                BorderThickness = 1,
                BorderColor = AppColors.Blue500
            };
            btnAddGroup.Click += BtnAddGroup_Click;
            groupsHeader.Controls.Add(btnAddGroup, 1, 0);

            menuPanel.Controls.Add(groupsHeader);

            // Container for groups (will be populated by LoadGroupsAsync)
            _groupsMenuPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Width = 252,
                AutoSize = true,
                Margin = new Padding(0)
            };
            menuPanel.Controls.Add(_groupsMenuPanel);

            // Load groups from server
            LoadGroupsAsync();

            // Filter section
            Label filterTitle = new Label
            {
                Text = "FILTER",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = AppColors.Gray700,
                AutoSize = true,
                Margin = new Padding(0, 24, 0, 12)
            };
            menuPanel.Controls.Add(filterTitle);

            menuPanel.Controls.Add(CreateMenuButton("All Tasks", Color.White, AppColors.Gray700, buttonHeight, AppColors.Gray100, 1, AppColors.Gray300));
            menuPanel.Controls.Add(CreateMenuButton("Assigned", Color.White, AppColors.Gray700, buttonHeight, AppColors.Gray100, 1, AppColors.Gray300));
            menuPanel.Controls.Add(CreateMenuButton("Unassigned", Color.White, AppColors.Gray700, buttonHeight, AppColors.Gray100, 1, AppColors.Gray300));

            return menuWrapper;
        }

        private async void LoadGroupsAsync()
        {
            try
            {
                if (_groupsMenuPanel == null) return;
                
                _groupsMenuPanel.SuspendLayout();
                _groupsMenuPanel.Controls.Clear();

                // Show loading
                Label loadingLabel = new Label
                {
                    Text = "⏳ Loading...",
                    Font = FontRegular,
                    ForeColor = AppColors.Gray500,
                    AutoSize = true,
                    Margin = new Padding(0, 8, 0, 8)
                };
                _groupsMenuPanel.Controls.Add(loadingLabel);
                _groupsMenuPanel.ResumeLayout(true);

                _groups = await _taskApi.GetMyGroupsAsync();

                _groupsMenuPanel.SuspendLayout();
                _groupsMenuPanel.Controls.Remove(loadingLabel);

                // All Groups button
                var allGroupsBtn = CreateMenuButton(
                    "📁 All Groups",
                    !_selectedGroupId.HasValue ? AppColors.Blue500 : Color.White,
                    !_selectedGroupId.HasValue ? Color.White : AppColors.Gray700,
                    40,
                    !_selectedGroupId.HasValue ? AppColors.Blue600 : AppColors.Gray100
                );
                allGroupsBtn.Click += (s, e) => OnGroupSelected(null, "All Groups");
                _groupsMenuPanel.Controls.Add(allGroupsBtn);

                foreach (var group in _groups)
                {
                    bool isSelected = _selectedGroupId.HasValue && _selectedGroupId.Value == group.GroupId;
                    var btn = CreateMenuButton(
                        $"👥 {group.GroupName}",
                        isSelected ? AppColors.Blue500 : Color.White,
                        isSelected ? Color.White : AppColors.Gray700,
                        40,
                        isSelected ? AppColors.Blue600 : AppColors.Gray100
                    );
                    
                    int capturedGroupId = group.GroupId;
                    string capturedGroupName = group.GroupName;
                    btn.Click += (s, e) => OnGroupSelected(capturedGroupId, capturedGroupName);
                    _groupsMenuPanel.Controls.Add(btn);
                }

                _groupsMenuPanel.ResumeLayout(true);
            }
            catch (Exception ex)
            {
                _groupsMenuPanel.SuspendLayout();
                _groupsMenuPanel.Controls.Clear();
                
                Label errorLabel = new Label
                {
                    Text = "Failed to load groups",
                    Font = FontRegular,
                    ForeColor = AppColors.Red500,
                    AutoSize = true,
                    Margin = new Padding(0, 8, 0, 8)
                };
                _groupsMenuPanel.Controls.Add(errorLabel);
                _groupsMenuPanel.ResumeLayout();
                
                MessageBox.Show($"Failed to load groups: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnGroupSelected(int? groupId, string groupName)
        {
            if (_selectedGroupId == groupId) return;
            
            _selectedGroupId = groupId;
            _groupName = groupName;
            
            // Refresh groups menu
            LoadGroupsAsync();
            
            // Refresh header title
            this.Text = $"👥 {groupName}";
            
            // Reload tasks
            if (_contentPanel != null)
            {
                _contentPanel.SuspendLayout();
                _contentPanel.Controls.Clear();
                _contentPanel.ResumeLayout(false);
                LoadTasksAsync(_contentPanel);
            }
        }

        private CustomButton CreateMenuButton(string text, Color backColor, Color foreColor, int height, Color? hoverColor = null, int borderThickness = 0, Color? borderColor = null)
        {
            var button = new CustomButton
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
                BorderColor = borderColor ?? Color.Transparent
            };

            if (borderColor.HasValue)
            {
                button.HoverBorderColor = borderColor.Value;
            }

            return button;
        }

        private void BtnAddGroup_Click(object sender, EventArgs e)
        {
            using (var createGroupForm = new FormCreateGroup())
            {
                createGroupForm.GroupCreated += (s, args) =>
                {
                    // Reload groups list
                    LoadGroupsAsync();
                    
                    // Auto-select the new group
                    _selectedGroupId = args.GroupId;
                    _groupName = args.GroupName;
                    this.Text = $"👥 {args.GroupName}";
                    
                    if (_contentPanel != null)
                    {
                        _contentPanel.SuspendLayout();
                        _contentPanel.Controls.Clear();
                        _contentPanel.ResumeLayout(false);
                        LoadTasksAsync(_contentPanel);
                    }
                };
                
                createGroupForm.ShowDialog(this);
            }
        }

        private Control CreateTaskListContent()
        {
            CustomFlowLayoutPanel contentPanel = new CustomFlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(32, 70, 32, 24), // ✅ Thêm padding-top 70px để tránh bị header che
                BackColor = AppColors.Gray100,
            };

            // Load tasks from server
            LoadTasksAsync(contentPanel);

            return contentPanel;
        }

        private async void LoadTasksAsync(CustomFlowLayoutPanel contentPanel)
        {
            try
            {
                _contentPanel = contentPanel;
                
                // Show loading indicator
                Label loadingLabel = new Label
                {
                    Text = "⏳ Loading group tasks...",
                    Font = FontRegular,
                    ForeColor = AppColors.Gray600,
                    AutoSize = true,
                    Margin = new Padding(0, 20, 0, 0)
                };
                contentPanel.Controls.Add(loadingLabel);

                // Fetch tasks - filter group tasks only
                var allTasks = await _taskApi.GetTasksAsync();
                _currentTasks = allTasks.Where(t => t.IsGroupTask).ToList();

                // Filter by selected group if specified
                if (_selectedGroupId.HasValue)
                {
                    _currentTasks = _currentTasks.Where(t => t.GroupTask?.GroupId == _selectedGroupId.Value).ToList();
                }

                // Remove loading label
                contentPanel.Controls.Remove(loadingLabel);

                // Render tasks
                RenderTaskList(contentPanel, _currentTasks);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load group tasks: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                // Show error label
                Label errorLabel = new Label
                {
                    Text = "Failed to load tasks. Please check server connection.",
                    Font = FontRegular,
                    ForeColor = AppColors.Red500,
                    AutoSize = true,
                    Margin = new Padding(0, 20, 0, 0)
                };
                contentPanel.Controls.Add(errorLabel);
            }
        }

        private void RenderTaskList(CustomFlowLayoutPanel contentPanel, List<TaskItem> tasks)
        {
            contentPanel.SuspendLayout();
            
            // Count tasks by assignment status
            int assignedCount = tasks.Count(t => t.GroupTask?.AssignedTo != null);
            int unassignedCount = tasks.Count(t => t.GroupTask?.AssignedTo == null);

            // ✅ Header với title và action buttons
            TableLayoutPanel headerLayout = new TableLayoutPanel
            {
                ColumnCount = 2,
                ColumnStyles =
                {
                    new ColumnStyle(SizeType.Percent, 100F),
                    new ColumnStyle(SizeType.AutoSize)
                },
                RowCount = 1,
                Height = 50,
                Margin = new Padding(0, 0, 0, 16),
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            headerLayout.SizeChanged += (sender, e) =>
            {
                if (headerLayout.Parent is FlowLayoutPanel parent)
                {
                    headerLayout.Width = parent.ClientSize.Width - parent.Padding.Left - parent.Padding.Right;
                }
            };

            // Left: Title và stats
            FlowLayoutPanel leftPanel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                Dock = DockStyle.Fill
            };
            
            Label title = new Label
            {
                Text = _selectedGroupId.HasValue ? $"📋 {_groupName}" : "📋 All Group Tasks",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = AppColors.Gray800,
                AutoSize = true
            };
            leftPanel.Controls.Add(title);

            Label stats = new Label
            {
                Text = $"{assignedCount} assigned | {unassignedCount} unassigned",
                Font = FontRegular,
                ForeColor = AppColors.Gray500,
                AutoSize = true
            };
            leftPanel.Controls.Add(stats);
            headerLayout.Controls.Add(leftPanel, 0, 0);

            // Right: Action buttons - chỉ hiện khi đã chọn group cụ thể
            if (_selectedGroupId.HasValue)
            {
                FlowLayoutPanel actionPanel = new FlowLayoutPanel
                {
                    FlowDirection = FlowDirection.LeftToRight,
                    AutoSize = true,
                    WrapContents = false, // ✅ Không wrap xuống dòng
                    Anchor = AnchorStyles.Right | AnchorStyles.Top,
                    Margin = new Padding(0),
                    Height = 40
                };

                // Add Member button
                CustomButton btnAddMember = new CustomButton
                {
                    Text = "👤 Add Member",
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    BackColor = AppColors.Green500,
                    ForeColor = Color.White,
                    HoverColor = AppColors.Green600,
                    BorderRadius = 6,
                    Width = 130,
                    Height = 36,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Margin = new Padding(0, 0, 10, 0) // ✅ Margin phải để cách nút kế tiếp
                };
                btnAddMember.Click += BtnAddMember_Click;
                actionPanel.Controls.Add(btnAddMember);

                // Add Task button
                CustomButton btnAddTask = new CustomButton
                {
                    Text = "📝 Add Task",
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    BackColor = AppColors.Blue500,
                    ForeColor = Color.White,
                    HoverColor = AppColors.Blue600,
                    BorderRadius = 6,
                    Width = 110,
                    Height = 36,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Margin = new Padding(0)
                };
                btnAddTask.Click += BtnCreateTask_Click;
                actionPanel.Controls.Add(btnAddTask);

                headerLayout.Controls.Add(actionPanel, 1, 0);
            }

            contentPanel.Controls.Add(headerLayout);

            // Column headers
            TableLayoutPanel columnHeader = new TableLayoutPanel
            {
                ColumnCount = 5,
                ColumnStyles =
                {
                    new ColumnStyle(SizeType.Percent, 40F),  // Task name
                    new ColumnStyle(SizeType.Percent, 20F),  // Assignee
                    new ColumnStyle(SizeType.Percent, 15F),  // Due date
                    new ColumnStyle(SizeType.Percent, 12.5F), // Status
                    new ColumnStyle(SizeType.Percent, 12.5F)  // Priority
                },
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 10),
                BackColor = Color.Transparent,
                Padding = new Padding(12, 0, 12, 0),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            columnHeader.SizeChanged += (sender, e) =>
            {
                if (columnHeader.Parent is FlowLayoutPanel parent)
                {
                    columnHeader.Width = parent.ClientSize.Width - parent.Padding.Left - parent.Padding.Right;
                }
            };
            
            Action<string, int> AddHeaderLabel = (text, col) =>
            {
                Label lbl = new Label
                {
                    Text = text,
                    Font = FontBold,
                    ForeColor = AppColors.Gray500,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                };
                columnHeader.Controls.Add(lbl, col, 0);
            };
            
            AddHeaderLabel("TASK NAME", 0);
            AddHeaderLabel("ASSIGNEE", 1);
            AddHeaderLabel("DUE DATE", 2);
            AddHeaderLabel("STATUS", 3);
            AddHeaderLabel("PRIORITY", 4);
            contentPanel.Controls.Add(columnHeader);

            // ✅ Nếu không có tasks, hiện empty state
            if (tasks.Count == 0)
            {
                Panel emptyState = new Panel
                {
                    Height = 200,
                    BackColor = Color.Transparent,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right
                };
                emptyState.SizeChanged += (sender, e) =>
                {
                    if (emptyState.Parent is FlowLayoutPanel parent)
                    {
                        emptyState.Width = parent.ClientSize.Width - parent.Padding.Left - parent.Padding.Right;
                    }
                };

                Label emptyLabel = new Label
                {
                    Text = _selectedGroupId.HasValue 
                        ? "📭 No tasks in this group yet.\nClick 'Add Task' to create the first one!"
                        : "📭 No group tasks found.\nSelect a group from the left menu.",
                    Font = new Font("Segoe UI", 12F),
                    ForeColor = AppColors.Gray500,
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill
                };
                emptyState.Controls.Add(emptyLabel);
                contentPanel.Controls.Add(emptyState);
                
                contentPanel.ResumeLayout();
                return;
            }

            // Render tasks
            _tasksRendered = 0;
            int tasksToRender = Math.Min(INITIAL_TASKS_TO_RENDER, tasks.Count);
            RenderTaskBatch(contentPanel, tasks, 0, tasksToRender);
            
            if (tasks.Count > INITIAL_TASKS_TO_RENDER)
            {
                AddLoadMoreButton(contentPanel, tasks);
            }
            
            contentPanel.ResumeLayout();
        }

        private void RenderTaskBatch(CustomFlowLayoutPanel contentPanel, List<TaskItem> tasks, int startIndex, int count)
        {
            var tasksToRender = tasks.Skip(startIndex).Take(count).ToList();
            
            foreach (var task in tasksToRender)
            {
                Control taskItem = CreateGroupTaskItem(task);
                taskItem.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                taskItem.SizeChanged += (sender, e) =>
                {
                    if (taskItem.Parent is FlowLayoutPanel parent)
                    {
                        taskItem.Width = parent.ClientSize.Width - parent.Padding.Left - parent.Padding.Right;
                    }
                };
                contentPanel.Controls.Add(taskItem);
                _tasksRendered++;
            }
        }

        private void BtnChat_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedGroupId.HasValue)
                {
                    // Mở chat cho group đang chọn
                    FormChatBox chatForm = new FormChatBox(_selectedGroupId.Value, _groupName);
                    chatForm.Show();
                }
                else
                {
                    // Chưa chọn group - mở chat chung
                    MessageBox.Show("Please select a group first to open group chat!", "No Group Selected", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to open chat: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCreateTask_Click(object sender, EventArgs e)
        {
            if (!_selectedGroupId.HasValue)
            {
                MessageBox.Show("Please select a group first!", "No Group Selected", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // ✅ Mở form tạo Group Task đầy đủ
            using (var createTaskForm = new FormCreateGroupTask(_selectedGroupId.Value, _groupName))
            {
                createTaskForm.TaskCreated += (s, args) =>
                {
                    // Reload task list sau khi tạo task thành công
                    if (_contentPanel != null)
                    {
                        _contentPanel.SuspendLayout();
                        _contentPanel.Controls.Clear();
                        _contentPanel.ResumeLayout(false);
                        LoadTasksAsync(_contentPanel);
                    }
                };
                
                createTaskForm.ShowDialog(this);
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

            taskItemPanel.Click += (sender, e) => OpenTaskDetail(task);

            TableLayoutPanel taskLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
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
                Padding = new Padding(16, 8, 16, 8),
                Margin = new Padding(0),
                BackColor = Color.Transparent,
            };
            taskLayout.Click += (sender, e) => OpenTaskDetail(task);

            // Task name
            Label nameLabel = new Label 
            { 
                Text = task.Title, 
                Font = FontBold, 
                ForeColor = AppColors.Gray800, 
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            nameLabel.Click += (sender, e) => OpenTaskDetail(task);
            taskLayout.Controls.Add(nameLabel, 0, 0);

            // Assignee
            string assigneeText = task.GroupTask?.AssignedTo != null ? "Assigned" : "⚠ Unassigned";
            Color assigneeColor = task.GroupTask?.AssignedTo != null ? AppColors.Gray700 : AppColors.Red500;
            
            Label assigneeLabel = new Label 
            { 
                Text = assigneeText, 
                Font = FontRegular, 
                ForeColor = assigneeColor,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            assigneeLabel.Click += (sender, e) => OpenTaskDetail(task);
            taskLayout.Controls.Add(assigneeLabel, 1, 0);

            // Due date
            string dueDateText = task.DueDate.HasValue ? task.DueDate.Value.ToString("MMM dd, yyyy") : "No due date";
            Label dueDateLabel = new Label 
            { 
                Text = dueDateText, 
                Font = FontRegular, 
                ForeColor = AppColors.Gray700, 
                Dock = DockStyle.Fill, 
                TextAlign = ContentAlignment.MiddleLeft 
            };
            dueDateLabel.Click += (sender, e) => OpenTaskDetail(task);
            taskLayout.Controls.Add(dueDateLabel, 2, 0);

            // Status tag
            Color statusColor = GetStatusColor(task.Status);
            Panel statusWrapper = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0) };
            statusWrapper.Click += (sender, e) => OpenTaskDetail(task);
            ModernPanel statusTag = CreateTag(task.StatusText, statusColor);
            statusTag.Click += (sender, e) => OpenTaskDetail(task);
            statusWrapper.Controls.Add(statusTag);
            statusTag.Anchor = AnchorStyles.None;
            taskLayout.Controls.Add(statusWrapper, 3, 0);

            // Priority tag
            Color priorityColor = GetPriorityColor(task.Priority);
            Panel priorityWrapper = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0) };
            priorityWrapper.Click += (sender, e) => OpenTaskDetail(task);
            ModernPanel priorityTag = CreateTag(task.PriorityText, priorityColor);
            priorityTag.Click += (sender, e) => OpenTaskDetail(task);
            priorityWrapper.Controls.Add(priorityTag);
            priorityTag.Anchor = AnchorStyles.None;
            taskLayout.Controls.Add(priorityWrapper, 4, 0);

            taskItemPanel.Controls.Add(taskLayout);
            return taskItemPanel;
        }

        // --- SỰ KIỆN: THÊM THÀNH VIÊN ---
        private async void BtnAddMember_Click(object sender, EventArgs e)
        {
            if (!_selectedGroupId.HasValue)
            {
                MessageBox.Show("Please select a group first from the left menu!", "No Group Selected", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string username = ShowInputDialog("Add Member", "Enter username to invite:");
            if (string.IsNullOrWhiteSpace(username)) return;

            try
            {
                // Show progress indicator
                this.Cursor = Cursors.WaitCursor;

                // ✅ Dùng await thay vì .Wait() để không block UI thread
                bool success = await _taskApi.AddGroupMemberAsync(_selectedGroupId.Value, username);

                if (success)
                {
                    MessageBox.Show($"User '{username}' added to group '{_groupName}' successfully!", "Success", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Failed to add member. Please check the username and try again.", "Error", 
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                string errorMsg = ex.Message;
                
                // Extract inner exception message if available
                if (ex.InnerException != null)
                {
                    errorMsg = ex.InnerException.Message;
                }

                MessageBox.Show($"Error adding member: {errorMsg}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void OpenTaskDetail(TaskItem task)
        {
            FormTaskDetail detailForm = new FormTaskDetail(task);
            
            detailForm.TaskUpdated += (s, e) =>
            {
                var taskToUpdate = _currentTasks.FirstOrDefault(t => t.TaskId == e.TaskId);
                if (taskToUpdate != null)
                {
                    taskToUpdate.Status = e.Status;
                }
                RefreshTaskList();
            };
            
            detailForm.TaskDeleted += (s, e) =>
            {
                var taskToRemove = _currentTasks.FirstOrDefault(t => t.TaskId == task.TaskId);
                if (taskToRemove != null)
                {
                    _currentTasks.Remove(taskToRemove);
                }
                RefreshTaskList();
            };
            
            detailForm.Show();
        }

        private void RefreshTaskList()
        {
            if (_contentPanel == null) return;
            
            _contentPanel.SuspendLayout();
            
            var controlsToRemove = _contentPanel.Controls
                .OfType<Control>()
                .Where(c => c is ModernPanel || c is CustomButton || c is Label && c.Text.Contains("All"))
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
            var loadMoreButton = new CustomButton
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

            loadMoreButton.Click += (s, e) =>
            {
                contentPanel.SuspendLayout();
                contentPanel.Controls.Remove(loadMoreButton);
                
                int nextBatchSize = Math.Min(20, tasks.Count - _tasksRendered);
                RenderTaskBatch(contentPanel, tasks, _tasksRendered, nextBatchSize);
                
                if (_tasksRendered < tasks.Count)
                {
                    AddLoadMoreButton(contentPanel, tasks);
                }
                else
                {
                    Label allLoadedLabel = new Label
                    {
                        Text = $"✓ All {tasks.Count} tasks loaded",
                        Font = FontRegular,
                        ForeColor = AppColors.Gray500,
                        AutoSize = true,
                        Margin = new Padding(0, 20, 0, 20)
                    };
                    contentPanel.Controls.Add(allLoadedLabel);
                }
                
                contentPanel.ResumeLayout();
            };

            contentPanel.Controls.Add(loadMoreButton);
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
                TaskPriority.High => AppColors.Red500,
                _ => AppColors.Gray400
            };
        }

        private ModernPanel CreateTag(string text, Color backColor)
        {
            Color tagForeColor = Color.White;
            if (backColor == AppColors.Yellow500 || backColor == AppColors.Green500)
            {
                tagForeColor = AppColors.Gray800;
            }

            return new ModernPanel
            {
                Text = text,
                BackColor = backColor,
                ForeColor = tagForeColor,
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

        private void FormGroupTaskList_Load(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// Hiển thị dialog nhập text đơn giản
        /// </summary>
        private string ShowInputDialog(string title, string prompt)
        {
            Form promptForm = new Form()
            {
                Width = 400,
                Height = 180,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = title,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.White
            };

            Label textLabel = new Label() 
            { 
                Left = 20, 
                Top = 20, 
                Text = prompt, 
                AutoSize = true, 
                Font = new Font("Segoe UI", 10) 
            };
            
            TextBox textBox = new TextBox() 
            { 
                Left = 20, 
                Top = 50, 
                Width = 340, 
                Font = new Font("Segoe UI", 10) 
            };
            
            Button confirmation = new Button() 
            { 
                Text = "OK", 
                Left = 240, 
                Width = 100, 
                Top = 90, 
                DialogResult = DialogResult.OK, 
                BackColor = AppColors.Blue500, 
                ForeColor = Color.White, 
                FlatStyle = FlatStyle.Flat 
            };

            promptForm.Controls.Add(textLabel);
            promptForm.Controls.Add(textBox);
            promptForm.Controls.Add(confirmation);
            promptForm.AcceptButton = confirmation;

            return promptForm.ShowDialog() == DialogResult.OK ? textBox.Text.Trim() : "";
        }
    }
}
