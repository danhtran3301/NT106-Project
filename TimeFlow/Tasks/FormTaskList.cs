using System;
using System.Drawing;
using System.Windows.Forms;
using TimeFlow.UI.Components;

namespace TimeFlow.Tasks
{
    public partial class FormTaskList : Form
    {
        private readonly Font FontRegular = new Font("Segoe UI", 10F, FontStyle.Regular);
        private readonly Font FontBold = new Font("Segoe UI", 10F, FontStyle.Bold);
        private readonly Font FontHeaderTitle = new Font("Segoe UI", 14F, FontStyle.Bold);
        private readonly Color HeaderIconColor = AppColors.Gray600;

        public FormTaskList()
        {
            InitializeComponent();
            SetupLayout();
        }

        private void SetupLayout()
        {
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

            mainLayout.Controls.Add(CreateLeftMenu(), 0, 0);
            mainLayout.Controls.Add(CreateTaskListContent(), 1, 0);
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
            arrowButton.Click += (sender, e) => { MessageBox.Show("Quay lại..."); };
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

            var tasks = new[]
            {
                new { Name = "Design a new dashboard for the mobile app", Assignees = 3, DueDate = "Dec 15, 2025", Status = "In Progress", StatusColor = AppColors.Blue500, Priority = "High", PriorityColor = AppColors.Red500 },
                new { Name = "Database Schema (CSDL) Deadline", Assignees = 1, DueDate = "Nov 01, 2025", Status = "Pending", StatusColor = AppColors.Yellow500, Priority = "Medium", PriorityColor = AppColors.Orange500 },
                new { Name = "Philosophy Theory Preparation", Assignees = 2, DueDate = "Dec 21, 2025", Status = "Completed", StatusColor = AppColors.Green500, Priority = "Low", PriorityColor = AppColors.Green500 },
                new { Name = "Submit Q4 Report", Assignees = 1, DueDate = "Nov 30, 2025", Status = "Pending", StatusColor = AppColors.Yellow500, Priority = "High", PriorityColor = AppColors.Red500 },
                new { Name = "Team Building Event Planning", Assignees = 4, DueDate = "Dec 05, 2025", Status = "In Progress", StatusColor = AppColors.Blue500, Priority = "Medium", PriorityColor = AppColors.Orange500 },
                new { Name = "Task 6: Review design docs", Assignees = 1, DueDate = "Dec 10, 2025", Status = "In Progress", StatusColor = AppColors.Blue500, Priority = "Medium", PriorityColor = AppColors.Orange500 },
                new { Name = "Task 7: Prepare presentation slides", Assignees = 2, DueDate = "Dec 12, 2025", Status = "Pending", StatusColor = AppColors.Yellow500, Priority = "Low", PriorityColor = AppColors.Green500 },
                new { Name = "Task 8: Final API integration", Assignees = 3, DueDate = "Dec 18, 2025", Status = "In Progress", StatusColor = AppColors.Blue500, Priority = "High", PriorityColor = AppColors.Red500 },
                new { Name = "Task 9: Database backup", Assignees = 1, DueDate = "Dec 25, 2025", Status = "Completed", StatusColor = AppColors.Green500, Priority = "Low", PriorityColor = AppColors.Green500 },
                new { Name = "Task 10: Holiday Planning", Assignees = 4, DueDate = "Jan 01, 2026", Status = "Pending", StatusColor = AppColors.Yellow500, Priority = "Medium", PriorityColor = AppColors.Orange500 },
            };
            int activeTaskCount = tasks.Length;

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
            columnHeader.SizeChanged += (sender, e) => {
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

            foreach (var task in tasks)
            {
                Control taskItem = CreateTaskListItem(task.Name, task.Assignees, task.DueDate, task.Status, task.StatusColor, task.Priority, task.PriorityColor);

                taskItem.Anchor = AnchorStyles.Left | AnchorStyles.Right;
                taskItem.SizeChanged += (sender, e) => {
                    if (taskItem.Parent is FlowLayoutPanel parent)
                    {
                        taskItem.Width = parent.ClientSize.Width - parent.Padding.Left - parent.Padding.Right;
                    }
                };

                contentPanel.Controls.Add(taskItem);
            }
            return contentPanel;
        }

        private Control CreateTaskListItem(string name, int assignees, string dueDate, string status, Color statusColor, string priority, Color priorityColor)
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

            FlowLayoutPanel nameFlow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                Dock = DockStyle.Fill,
                WrapContents = false,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            nameFlow.Controls.Add(new Label { Text = name, Font = FontBold, ForeColor = AppColors.Gray800, AutoSize = true, MaximumSize = new Size(350, 0) });
            nameFlow.Controls.Add(new Label { Text = $"{assignees} assignees", Font = FontRegular, ForeColor = AppColors.Gray500, AutoSize = true, Margin = new Padding(0, 4, 0, 0) });
            taskLayout.Controls.Add(nameFlow, 0, 0);

            taskLayout.Controls.Add(new Label { Text = dueDate, Font = FontRegular, ForeColor = AppColors.Gray700, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 1, 0);

            Panel statusWrapper = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0) };
            ModernPanel statusTag = CreateTag(status, statusColor);
            statusWrapper.Controls.Add(statusTag);
            statusTag.Anchor = AnchorStyles.None;
            taskLayout.Controls.Add(statusWrapper, 2, 0);

            Panel priorityWrapper = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0) };
            ModernPanel priorityTag = CreateTag(priority, priorityColor);
            priorityWrapper.Controls.Add(priorityTag);
            priorityTag.Anchor = AnchorStyles.None;
            taskLayout.Controls.Add(priorityWrapper, 3, 0);

            taskItemPanel.Controls.Add(taskLayout);
            return taskItemPanel;
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
    }
}