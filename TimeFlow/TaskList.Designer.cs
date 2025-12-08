namespace TimeFlow
{
    partial class TaskList : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Panel rootPanel;
        private System.Windows.Forms.TableLayoutPanel mainLayout;
        private System.Windows.Forms.Control headerBar;
        private System.Windows.Forms.Control leftMenu;
        private System.Windows.Forms.Control taskListContent;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            // Khai báo lại các Font để InitializeComponent không bị lỗi nếu chúng là private trong TaskList.cs
            System.Drawing.Font FontRegular = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);
            System.Drawing.Font FontBold = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            System.Drawing.Font FontHeaderTitle = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            System.Drawing.Color HeaderIconColor = ColorPalette.Gray600;

            this.rootPanel = new System.Windows.Forms.Panel();
            this.headerBar = this.CreateHeaderBar();
            this.mainLayout = new System.Windows.Forms.TableLayoutPanel();
            this.leftMenu = this.CreateLeftMenu();
            this.taskListContent = this.CreateTaskListContent();

            this.SuspendLayout();
            // 
            // TaskList
            // 
            this.Text = "My Tasks";
            this.BackColor = ColorPalette.Gray100;
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Padding = new System.Windows.Forms.Padding(0);
            this.MinimumSize = new System.Drawing.Size(1024, 600);

            // 
            // rootPanel
            // 
            this.rootPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rootPanel.BackColor = System.Drawing.Color.White;
            this.rootPanel.Padding = new System.Windows.Forms.Padding(0);

            // 
            // headerBar
            // 
            this.headerBar.Dock = System.Windows.Forms.DockStyle.Top;

            // 
            // mainLayout
            // 
            this.mainLayout.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainLayout.ColumnCount = 2;
            this.mainLayout.RowCount = 1;
            this.mainLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 300F));
            this.mainLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainLayout.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.None;
            this.mainLayout.Padding = new System.Windows.Forms.Padding(0);
            this.mainLayout.Margin = new System.Windows.Forms.Padding(0);
            this.mainLayout.BackColor = System.Drawing.Color.Transparent;

            // Thêm Controls vào rootPanel và mainLayout
            this.mainLayout.Controls.Add(this.leftMenu, 0, 0);
            this.mainLayout.Controls.Add(this.taskListContent, 1, 0);

            this.rootPanel.Controls.Add(this.mainLayout);
            this.rootPanel.Controls.Add(this.headerBar);

            this.Controls.Add(this.rootPanel);

            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private System.Windows.Forms.Control CreateHeaderBar()
        {
            System.Windows.Forms.Panel headerWrapper = new System.Windows.Forms.Panel
            {
                Dock = System.Windows.Forms.DockStyle.Top,
                Height = 61,
                BackColor = System.Drawing.Color.White,
                Margin = new System.Windows.Forms.Padding(0)
            };

            System.Windows.Forms.TableLayoutPanel headerTable = new System.Windows.Forms.TableLayoutPanel
            {
                Dock = System.Windows.Forms.DockStyle.Top,
                Height = 60,
                BackColor = System.Drawing.Color.White,
                ColumnCount = 3,
                ColumnStyles =
                {
                    new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize),
                    new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F),
                    new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.AutoSize)
                },
                RowCount = 1,
                RowStyles = { new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F) },
                Padding = new System.Windows.Forms.Padding(16, 10, 16, 10)
            };

            System.Windows.Forms.FlowLayoutPanel leftContainer = new System.Windows.Forms.FlowLayoutPanel
            {
                FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight,
                AutoSize = true,
                Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom,
                Margin = new System.Windows.Forms.Padding(0)
            };
            CustomButton arrowButton = new CustomButton
            {
                Text = "←",
                Font = new System.Drawing.Font("Segoe UI Emoji", 16F),
                ForeColor = ColorPalette.Gray600,
                BackColor = System.Drawing.Color.White,
                HoverColor = ColorPalette.Gray200,
                BorderRadius = 4,
                Width = 40,
                Height = 40,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Margin = new System.Windows.Forms.Padding(0)
            };
            arrowButton.Click += (sender, e) => { System.Windows.Forms.MessageBox.Show("Quay lại..."); };
            leftContainer.Controls.Add(arrowButton);

            System.Windows.Forms.Label titleLabel = new System.Windows.Forms.Label
            {
                Text = "My Tasks",
                Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold),
                ForeColor = ColorPalette.Gray800,
                AutoSize = true,
                Dock = System.Windows.Forms.DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                Margin = new System.Windows.Forms.Padding(8, 0, 0, 0)
            };
            leftContainer.Controls.Add(titleLabel);
            headerTable.Controls.Add(leftContainer, 0, 0);

            CustomButton closeButton = new CustomButton
            {
                Text = "✕",
                Font = new System.Drawing.Font("Segoe UI Emoji", 14F, System.Drawing.FontStyle.Bold),
                ForeColor = ColorPalette.Gray600,
                BackColor = System.Drawing.Color.White,
                HoverColor = ColorPalette.Gray200,
                BorderRadius = 4,
                Width = 40,
                Height = 40,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Margin = new System.Windows.Forms.Padding(0)
            };
            closeButton.Click += (sender, e) => { this.Close(); };
            headerTable.Controls.Add(closeButton, 2, 0);

            System.Windows.Forms.Panel separator = new System.Windows.Forms.Panel
            {
                Dock = System.Windows.Forms.DockStyle.Bottom,
                Height = 1,
                BackColor = ColorPalette.Gray200
            };

            headerTable.Dock = System.Windows.Forms.DockStyle.Fill;
            headerWrapper.Controls.Add(headerTable);
            headerWrapper.Controls.Add(separator);

            return headerWrapper;
        }

        private System.Windows.Forms.Control CreateLeftMenu()
        {
            System.Drawing.Font FontRegular = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);
            System.Drawing.Font FontBold = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);

            System.Windows.Forms.Panel menuWrapper = new System.Windows.Forms.Panel
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                BackColor = System.Drawing.Color.White,
                Padding = new System.Windows.Forms.Padding(0),
                Margin = new System.Windows.Forms.Padding(0)
            };
            CustomFlowLayoutPanel menuPanel = new CustomFlowLayoutPanel
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                FlowDirection = System.Windows.Forms.FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new System.Windows.Forms.Padding(24, 20, 24, 16),
                BackColor = System.Drawing.Color.White,
                Margin = new System.Windows.Forms.Padding(0)
            };
            System.Windows.Forms.Panel separator = new System.Windows.Forms.Panel
            {
                Dock = System.Windows.Forms.DockStyle.Right,
                Width = 1,
                BackColor = ColorPalette.Gray200
            };
            menuWrapper.Controls.Add(menuPanel);
            menuWrapper.Controls.Add(separator);

            int buttonHeight = 40;

            menuPanel.Controls.Add(CreateMenuHeader("ACCOUNT", "👤", new System.Windows.Forms.Padding(0, 0, 0, 16)));

            System.Windows.Forms.Label projectsTitle = new System.Windows.Forms.Label
            {
                Text = "PROJECTS",
                Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold),
                ForeColor = ColorPalette.Gray700,
                AutoSize = true,
                Margin = new System.Windows.Forms.Padding(0, 0, 0, 16)
            };
            menuPanel.Controls.Add(projectsTitle);

            menuPanel.Controls.Add(CreateMenuButton("Your Task", ColorPalette.Blue500, System.Drawing.Color.White, buttonHeight, ColorPalette.Blue600, 1, ColorPalette.MenuBorderColor));
            menuPanel.Controls.Add(CreateMenuButton("Group", ColorPalette.Green500, System.Drawing.Color.White, buttonHeight, ColorPalette.Green600, 1, ColorPalette.MenuBorderColor));
            menuPanel.Controls.Add(CreateMenuButton("New task", ColorPalette.Orange500, System.Drawing.Color.White, buttonHeight, ColorPalette.Orange600, 1, ColorPalette.MenuBorderColor));
            menuPanel.Controls.Add(CreateMenuButton("Submit task", ColorPalette.Purple500, System.Drawing.Color.White, buttonHeight, ColorPalette.Purple600, 1, ColorPalette.MenuBorderColor));

            System.Windows.Forms.Label calendarTitle = new System.Windows.Forms.Label
            {
                Text = "Calendar",
                Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold),
                ForeColor = ColorPalette.Gray800,
                AutoSize = true,
                Margin = new System.Windows.Forms.Padding(0, 30, 0, 8)
            };
            menuPanel.Controls.Add(calendarTitle);
            System.Windows.Forms.Control customCalendar = CreateCustomCalendarControl(new System.DateTime(2025, 11, 16), System.DateTime.Today);
            menuPanel.Controls.Add(customCalendar);

            return menuWrapper;
        }

        private System.Windows.Forms.Control CreateMenuHeader(string text, string icon, System.Windows.Forms.Padding margin)
        {
            System.Windows.Forms.FlowLayoutPanel header = new System.Windows.Forms.FlowLayoutPanel
            {
                FlowDirection = System.Windows.Forms.FlowDirection.LeftToRight,
                AutoSize = true,
                Margin = margin
            };
            header.Controls.Add(new System.Windows.Forms.Label
            {
                Text = icon,
                Font = new System.Drawing.Font("Segoe UI Emoji", 12F),
                AutoSize = true,
                Margin = new System.Windows.Forms.Padding(0, 0, 4, 0)
            });
            header.Controls.Add(new System.Windows.Forms.Label
            {
                Text = text,
                Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold),
                ForeColor = ColorPalette.Gray700,
                AutoSize = true
            });
            return header;
        }

        private CustomButton CreateMenuButton(string text, System.Drawing.Color backColor, System.Drawing.Color foreColor, int height, System.Drawing.Color? hoverColor = null, int borderThickness = 0, System.Drawing.Color? borderColor = null)
        {
            System.Drawing.Font FontBold = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            return new CustomButton
            {
                Text = text,
                BackColor = backColor,
                ForeColor = foreColor,
                HoverColor = hoverColor ?? ColorPalette.Blue600,
                BorderRadius = 8,
                Width = 252,
                Height = height,
                Font = FontBold,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Margin = new System.Windows.Forms.Padding(0, 0, 0, 12),
                BorderThickness = borderThickness,
                BorderColor = borderColor ?? System.Drawing.Color.Transparent
            };
        }

        private System.Windows.Forms.Control CreateCustomCalendarControl(System.DateTime selectionDate, System.DateTime today)
        {
            System.Drawing.Font FontRegular = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);
            System.Windows.Forms.MonthCalendar monthCalendar = new System.Windows.Forms.MonthCalendar
            {
                BackColor = System.Drawing.Color.White,
                ForeColor = ColorPalette.Gray700,
                Font = FontRegular,
                SelectionStart = selectionDate,
                SelectionEnd = selectionDate,
                ShowTodayCircle = false,
                TitleBackColor = System.Drawing.Color.White,
                TitleForeColor = ColorPalette.Gray800,
                TrailingForeColor = ColorPalette.Gray300,
                CalendarDimensions = new System.Drawing.Size(1, 1),
                TodayDate = today
            };

            monthCalendar.Width = 252;
            monthCalendar.Height = 180;
            monthCalendar.Margin = new System.Windows.Forms.Padding(0);

            return monthCalendar;
        }

        private System.Windows.Forms.Control CreateTaskListContent()
        {
            System.Drawing.Font FontRegular = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);
            System.Drawing.Font FontBold = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);

            CustomFlowLayoutPanel contentPanel = new CustomFlowLayoutPanel
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                FlowDirection = System.Windows.Forms.FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new System.Windows.Forms.Padding(32, 20, 32, 24),
                BackColor = ColorPalette.Gray100,
            };

            var tasks = new[]
            {
                new { Name = "Design a new dashboard for the mobile app", Assignees = 3, DueDate = "Dec 15, 2025", Status = "In Progress", StatusColor = ColorPalette.Blue500, Priority = "High", PriorityColor = ColorPalette.Red500 },
                new { Name = "Database Schema (CSDL) Deadline", Assignees = 1, DueDate = "Nov 01, 2025", Status = "Pending", StatusColor = ColorPalette.Yellow500, Priority = "Medium", PriorityColor = ColorPalette.Orange500 },
                new { Name = "Philosophy Theory Preparation", Assignees = 2, DueDate = "Dec 21, 2025", Status = "Completed", StatusColor = ColorPalette.Green500, Priority = "Low", PriorityColor = ColorPalette.Green500 },
                new { Name = "Submit Q4 Report", Assignees = 1, DueDate = "Nov 30, 2025", Status = "Pending", StatusColor = ColorPalette.Yellow500, Priority = "High", PriorityColor = ColorPalette.Red500 },
                new { Name = "Team Building Event Planning", Assignees = 4, DueDate = "Dec 05, 2025", Status = "In Progress", StatusColor = ColorPalette.Blue500, Priority = "Medium", PriorityColor = ColorPalette.Orange500 },
                new { Name = "Task 6: Review design docs", Assignees = 1, DueDate = "Dec 10, 2025", Status = "In Progress", StatusColor = ColorPalette.Blue500, Priority = "Medium", PriorityColor = ColorPalette.Orange500 },
                new { Name = "Task 7: Prepare presentation slides", Assignees = 2, DueDate = "Dec 12, 2025", Status = "Pending", StatusColor = ColorPalette.Yellow500, Priority = "Low", PriorityColor = ColorPalette.Green500 },
                new { Name = "Task 8: Final API integration", Assignees = 3, DueDate = "Dec 18, 2025", Status = "In Progress", StatusColor = ColorPalette.Blue500, Priority = "High", PriorityColor = ColorPalette.Red500 },
                new { Name = "Task 9: Database backup", Assignees = 1, DueDate = "Dec 25, 2025", Status = "Completed", StatusColor = ColorPalette.Green500, Priority = "Low", PriorityColor = ColorPalette.Green500 },
                new { Name = "Task 10: Holiday Planning", Assignees = 4, DueDate = "Jan 01, 2026", Status = "Pending", StatusColor = ColorPalette.Yellow500, Priority = "Medium", PriorityColor = ColorPalette.Orange500 },
            };
            int activeTaskCount = tasks.Length;

            System.Windows.Forms.TableLayoutPanel headerLayout = new System.Windows.Forms.TableLayoutPanel
            {
                ColumnCount = 2,
                ColumnStyles =
                {
                    new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F),
                    new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F)
                },
                RowCount = 1,
                Margin = new System.Windows.Forms.Padding(0, 0, 0, 24),
                BackColor = System.Drawing.Color.Transparent,
                Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right
            };
            headerLayout.SizeChanged += (sender, e) =>
            {
                if (headerLayout.Parent is FlowLayoutPanel parent)
                {
                    headerLayout.Width = parent.ClientSize.Width - parent.Padding.Left - parent.Padding.Right;
                }
            };

            System.Windows.Forms.Label title = new System.Windows.Forms.Label
            {
                Text = "Your Tasks",
                Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold),
                ForeColor = ColorPalette.Gray800,
                AutoSize = true,
                Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom
            };
            headerLayout.Controls.Add(title, 0, 0);

            System.Windows.Forms.Label activeTasks = new System.Windows.Forms.Label
            {
                Text = $"{activeTaskCount} active tasks",
                Font = FontRegular,
                ForeColor = ColorPalette.Gray600,
                AutoSize = true,
                Anchor = System.Windows.Forms.AnchorStyles.Right | System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom,
                TextAlign = System.Drawing.ContentAlignment.BottomRight,
            };
            headerLayout.Controls.Add(activeTasks, 1, 0);
            contentPanel.Controls.Add(headerLayout);

            System.Windows.Forms.TableLayoutPanel columnHeader = new System.Windows.Forms.TableLayoutPanel
            {
                ColumnCount = 4,
                ColumnStyles =
                {
                    new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F),
                    new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F),
                    new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F),
                    new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F)
                },
                RowCount = 1,
                Margin = new System.Windows.Forms.Padding(0, 0, 0, 10),
                BackColor = System.Drawing.Color.Transparent,
                Padding = new System.Windows.Forms.Padding(12, 0, 12, 0),
                Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right
            };
            columnHeader.SizeChanged += (sender, e) => {
                if (columnHeader.Parent is FlowLayoutPanel parent)
                {
                    columnHeader.Width = parent.ClientSize.Width - parent.Padding.Left - parent.Padding.Right;
                }
            };
            System.Action<string, int> AddHeaderLabel = (text, col) =>
            {
                System.Windows.Forms.Label lbl = new System.Windows.Forms.Label
                {
                    Text = text,
                    Font = FontBold,
                    ForeColor = ColorPalette.Gray500,
                    Dock = System.Windows.Forms.DockStyle.Fill,
                    TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
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
                System.Windows.Forms.Control taskItem = CreateTaskListItem(task.Name, task.Assignees, task.DueDate, task.Status, task.StatusColor, task.Priority, task.PriorityColor);

                taskItem.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
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

        private System.Windows.Forms.Control CreateTaskListItem(string name, int assignees, string dueDate, string status, System.Drawing.Color statusColor, string priority, System.Drawing.Color priorityColor)
        {
            System.Drawing.Font FontRegular = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);
            System.Drawing.Font FontBold = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);

            CustomPanel taskItemPanel = new CustomPanel
            {
                Dock = System.Windows.Forms.DockStyle.Top,
                Height = 80,
                BackColor = System.Drawing.Color.White,
                BorderRadius = 12,
                BorderThickness = 1,
                BorderColor = ColorPalette.Gray200,
                Margin = new System.Windows.Forms.Padding(0, 0, 0, 12),
                Cursor = System.Windows.Forms.Cursors.Hand,
            };

            System.Windows.Forms.TableLayoutPanel taskLayout = new System.Windows.Forms.TableLayoutPanel
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                ColumnCount = 4,
                ColumnStyles =
                {
                    new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F),
                    new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F),
                    new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F),
                    new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 15F)
                },
                RowCount = 1,
                Padding = new System.Windows.Forms.Padding(16, 8, 16, 8),
                Margin = new System.Windows.Forms.Padding(0),
                BackColor = System.Drawing.Color.Transparent,
            };

            System.Windows.Forms.FlowLayoutPanel nameFlow = new System.Windows.Forms.FlowLayoutPanel
            {
                FlowDirection = System.Windows.Forms.FlowDirection.TopDown,
                Dock = System.Windows.Forms.DockStyle.Fill,
                WrapContents = false,
                Padding = new System.Windows.Forms.Padding(0),
                Margin = new System.Windows.Forms.Padding(0)
            };
            nameFlow.Controls.Add(new System.Windows.Forms.Label { Text = name, Font = FontBold, ForeColor = ColorPalette.Gray800, AutoSize = true, MaximumSize = new System.Drawing.Size(350, 0) });
            nameFlow.Controls.Add(new System.Windows.Forms.Label { Text = $"{assignees} assignees", Font = FontRegular, ForeColor = ColorPalette.Gray500, AutoSize = true, Margin = new System.Windows.Forms.Padding(0, 4, 0, 0) });
            taskLayout.Controls.Add(nameFlow, 0, 0);

            taskLayout.Controls.Add(new System.Windows.Forms.Label { Text = dueDate, Font = FontRegular, ForeColor = ColorPalette.Gray700, Dock = System.Windows.Forms.DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft }, 1, 0);

            System.Windows.Forms.Panel statusWrapper = new System.Windows.Forms.Panel { Dock = System.Windows.Forms.DockStyle.Fill, Margin = new System.Windows.Forms.Padding(0) };
            CustomPanel statusTag = CreateTag(status, statusColor);
            statusWrapper.Controls.Add(statusTag);
            statusTag.Anchor = System.Windows.Forms.AnchorStyles.None;
            taskLayout.Controls.Add(statusWrapper, 2, 0);

            System.Windows.Forms.Panel priorityWrapper = new System.Windows.Forms.Panel { Dock = System.Windows.Forms.DockStyle.Fill, Margin = new System.Windows.Forms.Padding(0) };
            CustomPanel priorityTag = CreateTag(priority, priorityColor);
            priorityWrapper.Controls.Add(priorityTag);
            priorityTag.Anchor = System.Windows.Forms.AnchorStyles.None;
            taskLayout.Controls.Add(priorityWrapper, 3, 0);

            taskItemPanel.Controls.Add(taskLayout);
            return taskItemPanel;
        }

        private CustomPanel CreateTag(string text, System.Drawing.Color backColor)
        {
            System.Drawing.Color tagForeColor = System.Drawing.Color.White;
            if (backColor == ColorPalette.Yellow500 || backColor == ColorPalette.Green500)
            {
                tagForeColor = ColorPalette.Gray800;
            }

            return new CustomPanel
            {
                Text = text,
                BackColor = backColor,
                ForeColor = tagForeColor,
                Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold),

                BorderRadius = 4,
                Height = 24,

                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Anchor = System.Windows.Forms.AnchorStyles.None,

                AutoSize = true,
                Padding = new System.Windows.Forms.Padding(8, 2, 8, 2),
                Margin = new System.Windows.Forms.Padding(0)
            };
        }

        #endregion
    }
}