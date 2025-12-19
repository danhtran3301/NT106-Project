using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using TimeFlow.UI.Components;
using TimeFlow.Models;
using TimeFlow.Services;
using System.Collections.Generic;

namespace TimeFlow.Tasks
{
    public partial class FormTaskList : Form
    {
        private readonly Font FontRegular = new Font("Segoe UI", 10F, FontStyle.Regular);
        private readonly Font FontBold = new Font("Segoe UI", 10F, FontStyle.Bold);
        private readonly Font FontHeaderTitle = new Font("Segoe UI", 14F, FontStyle.Bold);
        private readonly Color HeaderIconColor = AppColors.Gray600;
        private readonly TaskApiClient _taskApi;
        private List<TaskItem> _currentTasks;

        private Control _cachedLeftMenu;
        private Control _cachedHeaderBar;
        
        private const int INITIAL_TASKS_TO_RENDER = 20;
        private int _tasksRendered = 0;
        private CustomFlowLayoutPanel _contentPanel;

        public FormTaskList()
        {
            InitializeComponent();
            _taskApi = new TaskApiClient();
            _currentTasks = new List<TaskItem>();
            SetupLayout();
        }

        private void SetupLayout()
        {
            this.SuspendLayout(); 
            
            this.Text = "My Tasks";
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

            Control headerBar = _cachedHeaderBar ?? CreateHeaderBar();
            if (_cachedHeaderBar == null)
                _cachedHeaderBar = headerBar;
            
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

            Control leftMenu = _cachedLeftMenu ?? CreateLeftMenu();
            if (_cachedLeftMenu == null)
                _cachedLeftMenu = leftMenu;
            
            mainLayout.Controls.Add(leftMenu, 0, 0);
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
            TimeFlow.UI.Components.CustomButton arrowButton = new TimeFlow.UI.Components.CustomButton
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
            arrowButton.Click += (sender, e) =>
            {
                FormGiaoDien home = Application.OpenForms.OfType<FormGiaoDien>().FirstOrDefault();

                if (home != null)
                {
                    home.Show(); 
                    home.BringToFront(); 
                }
                this.Close();
            };
            leftContainer.Controls.Add(arrowButton);

            Label titleLabel = new Label
            {
                Text = "My Tasks",
                Font = FontHeaderTitle,
                ForeColor = AppColors.Gray800,
                AutoSize = true,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(8, 0, 0, 0)
            };
            leftContainer.Controls.Add(titleLabel);
            headerTable.Controls.Add(leftContainer, 0, 0);

            TimeFlow.UI.Components.CustomButton closeButton = new TimeFlow.UI.Components.CustomButton
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
            headerTable.Controls.Add(closeButton, 2, 0);

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
                Padding = new Padding(24, 20, 24, 16),
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

            menuPanel.Controls.Add(CreateMenuHeader("ACCOUNT", "👤", new Padding(0, 0, 0, 16)));

            Label projectsTitle = new Label
            {
                Text = "PROJECTS",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = AppColors.Gray700,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 16)
            };
            menuPanel.Controls.Add(projectsTitle);

            menuPanel.Controls.Add(CreateMenuButton("Your Task", AppColors.Blue500, Color.White, buttonHeight, AppColors.Blue600, 1, AppColors.MenuBorderColor));
            menuPanel.Controls.Add(CreateMenuButton("Group", AppColors.Green500, Color.White, buttonHeight, AppColors.Green600, 1, AppColors.MenuBorderColor));
            menuPanel.Controls.Add(CreateMenuButton("New task", AppColors.Orange500, Color.White, buttonHeight, AppColors.Orange600, 1, AppColors.MenuBorderColor));
            menuPanel.Controls.Add(CreateMenuButton("Submit task", AppColors.Purple500, Color.White, buttonHeight, AppColors.Purple600, 1, AppColors.MenuBorderColor));

            Label calendarTitle = new Label
            {
                Text = "Calendar",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = AppColors.Gray800,
                AutoSize = true,
                Margin = new Padding(0, 30, 0, 8)
            };
            menuPanel.Controls.Add(calendarTitle);
            Control customCalendar = CreateCustomCalendarControl(new DateTime(2025, 11, 16), DateTime.Today);
            menuPanel.Controls.Add(customCalendar);

            return menuWrapper;
        }

        private Control CreateMenuHeader(string text, string icon, Padding margin)
        {
            FlowLayoutPanel header = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Margin = margin
            };
            header.Controls.Add(new Label
            {
                Text = icon,
                Font = new Font("Segoe UI Emoji", 12F),
                AutoSize = true,
                Margin = new Padding(0, 0, 4, 0)
            });
            header.Controls.Add(new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = AppColors.Gray700,
                AutoSize = true
            });
            return header;
        }

        private TimeFlow.UI.Components.CustomButton CreateMenuButton(string text, Color backColor, Color foreColor, int height, Color? hoverColor = null, int borderThickness = 0, Color? borderColor = null)
        {
            var button = new TimeFlow.UI.Components.CustomButton
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

        private Control CreateCustomCalendarControl(DateTime selectionDate, DateTime today)
        {
            MonthCalendar monthCalendar = new MonthCalendar
            {
                BackColor = Color.White,
                ForeColor = AppColors.Gray700,
                Font = FontRegular,
                SelectionStart = selectionDate,
                SelectionEnd = selectionDate,
                ShowTodayCircle = false,
                TitleBackColor = Color.White,
                TitleForeColor = AppColors.Gray800,
                TrailingForeColor = AppColors.Gray300,
                CalendarDimensions = new Size(1, 1),
                TodayDate = today
            };

            monthCalendar.Width = 252;
            monthCalendar.Height = 180;
            monthCalendar.Margin = new Padding(0);

            return monthCalendar;
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

        private async void LoadTasksAsync(CustomFlowLayoutPanel contentPanel)
        {
            try
            {
                _contentPanel = contentPanel; 

                Label loadingLabel = new Label
                {
                    Text = "⏳ Loading tasks...",
                    Font = FontRegular,
                    ForeColor = AppColors.Gray600,
                    AutoSize = true,
                    Margin = new Padding(0, 20, 0, 0)
                };
                contentPanel.Controls.Add(loadingLabel);

                var allTasks = await _taskApi.GetTasksAsync();

                _currentTasks = allTasks.Where(t => !t.IsGroupTask).ToList();
                contentPanel.Controls.Remove(loadingLabel);

                RenderTaskList(contentPanel, _currentTasks);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load tasks: {ex.Message}", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            
            int activeTaskCount = tasks.Count(t => t.Status != TimeFlow.Models.TaskStatus.Completed);

            TableLayoutPanel headerLayout = new TableLayoutPanel
            {
                ColumnCount = 2,
                ColumnStyles =
                {
                    new ColumnStyle(SizeType.Percent, 100F),
                    new ColumnStyle(SizeType.Absolute, 120F)
                },
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 24),
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

            Label title = new Label
            {
                Text = "Your Tasks",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = AppColors.Gray800,
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom
            };
            headerLayout.Controls.Add(title, 0, 0);

            Label activeTasks = new Label
            {
                Text = $"{activeTaskCount} active tasks",
                Font = FontRegular,
                ForeColor = AppColors.Gray600,
                AutoSize = true,
                Anchor = AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom,
                TextAlign = ContentAlignment.BottomRight,
            };
            headerLayout.Controls.Add(activeTasks, 1, 0);
            contentPanel.Controls.Add(headerLayout);

            TableLayoutPanel columnHeader = new TableLayoutPanel
            {
                ColumnCount = 4,
                ColumnStyles =
                {
                    new ColumnStyle(SizeType.Percent, 50F),
                    new ColumnStyle(SizeType.Percent, 20F),
                    new ColumnStyle(SizeType.Percent, 15F),
                    new ColumnStyle(SizeType.Percent, 15F)
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
            AddHeaderLabel("DUE DATE", 1);
            AddHeaderLabel("STATUS", 2);
            AddHeaderLabel("PRIORITY", 3);
            contentPanel.Controls.Add(columnHeader);

            int tasksToRender = Math.Min(INITIAL_TASKS_TO_RENDER, tasks.Count);
            _tasksRendered = 0;
            
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
                Color statusColor = GetStatusColor(task.Status);
                Color priorityColor = GetPriorityColor(task.Priority);

                string dueDateText = task.DueDate.HasValue ? task.DueDate.Value.ToString("MMM dd, yyyy") : "No due date";
                int assigneeCount = 0; 

                Control taskItem = CreateTaskListItem(
                    task.TaskId,
                    task.Title,
                    assigneeCount,
                    dueDateText,
                    task.StatusText,
                    statusColor,
                    task.PriorityText,
                    priorityColor
                );

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

        private Control CreateTaskListItem(int taskId, string name, int assignees, string dueDate, string status, Color statusColor, string priority, Color priorityColor)
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

            taskItemPanel.Click += (sender, e) => OpenTaskDetail(taskId);

            TableLayoutPanel taskLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                ColumnStyles =
                {
                    new ColumnStyle(SizeType.Percent, 50F),
                    new ColumnStyle(SizeType.Percent, 20F),
                    new ColumnStyle(SizeType.Percent, 15F),
                    new ColumnStyle(SizeType.Percent, 15F)
                },
                RowCount = 1,
                Padding = new Padding(16, 8, 16, 8),
                Margin = new Padding(0),
                BackColor = Color.Transparent,
            };
            taskLayout.Click += (sender, e) => OpenTaskDetail(taskId);

            FlowLayoutPanel nameFlow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                Dock = DockStyle.Fill,
                WrapContents = false,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            nameFlow.Click += (sender, e) => OpenTaskDetail(taskId);

            Label nameLabel = new Label { Text = name, Font = FontBold, ForeColor = AppColors.Gray800, AutoSize = true, MaximumSize = new Size(350, 0) };
            nameLabel.Click += (sender, e) => OpenTaskDetail(taskId);
            nameFlow.Controls.Add(nameLabel);

            var task = _currentTasks.FirstOrDefault(t => t.TaskId == taskId);
            string assigneeText = GetAssigneeText(task);
            
            Label assigneeLabel = new Label { Text = assigneeText, Font = FontRegular, ForeColor = AppColors.Gray500, AutoSize = true, Margin = new Padding(0, 4, 0, 0) };
            assigneeLabel.Click += (sender, e) => OpenTaskDetail(taskId);
            nameFlow.Controls.Add(assigneeLabel);

            taskLayout.Controls.Add(nameFlow, 0, 0);

            Label dueDateLabel = new Label { Text = dueDate, Font = FontRegular, ForeColor = AppColors.Gray700, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft };
            dueDateLabel.Click += (sender, e) => OpenTaskDetail(taskId);
            taskLayout.Controls.Add(dueDateLabel, 1, 0);

            Panel statusWrapper = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0) };
            statusWrapper.Click += (sender, e) => OpenTaskDetail(taskId);
            ModernPanel statusTag = CreateTag(status, statusColor);
            statusTag.Click += (sender, e) => OpenTaskDetail(taskId);
            statusWrapper.Controls.Add(statusTag);
            statusTag.Anchor = AnchorStyles.None;
            taskLayout.Controls.Add(statusWrapper, 2, 0);

            Panel priorityWrapper = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0) };
            priorityWrapper.Click += (sender, e) => OpenTaskDetail(taskId);
            ModernPanel priorityTag = CreateTag(priority, priorityColor);
            priorityTag.Click += (sender, e) => OpenTaskDetail(taskId);
            priorityWrapper.Controls.Add(priorityTag);
            priorityTag.Anchor = AnchorStyles.None;
            taskLayout.Controls.Add(priorityWrapper, 3, 0);

            taskItemPanel.Controls.Add(taskLayout);
            return taskItemPanel;
        }

        private string GetAssigneeText(TaskItem task)
        {
            if (task == null) return "Unknown";
            
            if (task.IsGroupTask)
            {
                if (task.GroupTask?.AssignedTo != null)
                {
                    return "1 assignee";
                }
                return "⚠ Unassigned";
            }
            
            return "You (owner)";
        }

        private void OpenTaskDetail(int taskId)
        {
            var task = _currentTasks.FirstOrDefault(t => t.TaskId == taskId);
            if (task != null)
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
                    var taskToRemove = _currentTasks.FirstOrDefault(t => t.TaskId == taskId);
                    if (taskToRemove != null)
                    {
                        _currentTasks.Remove(taskToRemove);
                    }
                    
                    RefreshTaskList();
                };
                
                detailForm.FormClosed += (s, e) => this.Show();
                this.Hide();
                detailForm.Show();
            }
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

            int activeTaskCount = _currentTasks.Count(t => t.Status != TimeFlow.Models.TaskStatus.Completed);
            var activeLabel = _contentPanel.Controls.OfType<TableLayoutPanel>().FirstOrDefault()
                ?.Controls.OfType<Label>().FirstOrDefault(l => l.Text.Contains("active"));
            
            if (activeLabel != null)
            {
                activeLabel.Text = $"{activeTaskCount} active tasks";
            }
            
            _contentPanel.ResumeLayout();
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

        private void FormTaskList_Load(object sender, EventArgs e)
        {

        }
    }
}