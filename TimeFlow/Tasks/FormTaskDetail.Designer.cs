using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using TimeFlow.UI.Components;
using TimeFlow.Models;
using TimeFlow.UI;
using TimeFlow.Services;
using TimeFlow.Tasks;
namespace TimeFlow.Tasks
{
    public partial class FormTaskDetail : Form
    {
        private void InitializeComponent()
        {
            SetupLayout();
            this.LayoutMdi(MdiLayout.ArrangeIcons);
        }

        private void SetupLayout()
        {
            this.Text = "Task Details";
            this.BackColor = AppColors.Gray100;
            this.WindowState = FormWindowState.Maximized;
            this.Padding = new Padding(0);
            this.MinimumSize = new Size(800, 600);

            // Set default task if none provided
            if (_currentTask == null)
            {
                _currentTask = Services.TaskManager.GetTaskById(1) ?? Services.TaskManager.GetAllTasks().FirstOrDefault();
            }

            Panel rootPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                Padding = new Padding(0)
            };
            this.Controls.Add(rootPanel);

            rootPanel.Controls.Add(CreateHeaderBar());

            TableLayoutPanel mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                ColumnStyles =
                {
                    new ColumnStyle(SizeType.Absolute, 260F),
                    new ColumnStyle(SizeType.Percent, 100F),
                    new ColumnStyle(SizeType.Absolute, 320F)
                },
                RowStyles = { new RowStyle(SizeType.Percent, 100F) },
                Padding = new Padding(0),
                Margin = new Padding(0),
                BackColor = Color.White
            };
            rootPanel.Controls.Add(mainLayout);

            mainLayout.Controls.Add(CreateLeftMenu(), 0, 0);
            mainLayout.Controls.Add(CreateCenterContent(), 1, 0);
            mainLayout.Controls.Add(CreateRightSidebar(), 2, 0);
        }

        private Control CreateHeaderBar()
        {
            TableLayoutPanel headerPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.White,
                ColumnCount = 3,
                RowCount = 1,
                ColumnStyles =
                {
                    new ColumnStyle(SizeType.Absolute, 60F),
                    new ColumnStyle(SizeType.Percent, 100F),
                    new ColumnStyle(SizeType.Absolute, 100F)
                },
                RowStyles = { new RowStyle(SizeType.Percent, 100F) },
                Padding = new Padding(16, 0, 16, 0)
            };

            FlowLayoutPanel leftFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom,
                Margin = new Padding(0),
                AutoSize = false,
                Height = 60
            };

            TimeFlow.UI.Components.CustomButton arrowButton = new TimeFlow.UI.Components.CustomButton
            {
                Text = "←",
                Font = new Font("Segoe UI Emoji", 16F),
                ForeColor = HeaderIconColor,
                BackColor = Color.Transparent,
                HoverColor = AppColors.Gray200,
                BorderRadius = 4,
                Width = 40,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0,18,0,0)
            };
            arrowButton.Click += (sender, e) => { MessageBox.Show("Quay lại trang trước..."); };
            leftFlow.Controls.Add(arrowButton);
            headerPanel.Controls.Add(leftFlow, 0, 0);

            Label titleLabel = new Label
            {
                Text = "Task Details",
                Font = FontHeaderTitle,
                ForeColor = AppColors.Gray800,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(0)
            };
            headerPanel.Controls.Add(titleLabel, 1, 0);

            FlowLayoutPanel rightFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                AutoSize = false,
                Height = 60,
                Anchor = AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom,
                Margin = new Padding(0)
            };

            TimeFlow.UI.Components.CustomButton closeButton = new TimeFlow.UI.Components.CustomButton
            {
                Text = "✕",
                Font = new Font("Segoe UI Emoji", 14F, FontStyle.Bold),
                ForeColor = HeaderIconColor,
                BackColor = Color.Transparent,
                HoverColor = AppColors.Gray200,
                BorderRadius = 4,
                Width = 40,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0,18,0,0)
            };
            closeButton.Click += (sender, e) => { this.Close(); };
            rightFlow.Controls.Add(closeButton);

            TimeFlow.UI.Components.CustomButton optionsButton = new TimeFlow.UI.Components.CustomButton
            {
                Text = "...",
                Font = new Font("Segoe UI Emoji", 14F, FontStyle.Bold),
                ForeColor = HeaderIconColor,
                BackColor = Color.Transparent,
                HoverColor = AppColors.Gray200,
                BorderRadius = 4,
                Width = 40,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 16, 0, 0)
            };
            /*rightFlow.Controls.Add(optionsButton);
            headerPanel.Controls.Add(rightFlow, 2, 0);*/
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            ToolStripMenuItem editItem = new ToolStripMenuItem("Chỉnh sửa (Edit)");
            ToolStripMenuItem deleteItem = new ToolStripMenuItem("Xóa Task (Delete)");
            ToolStripMenuItem statusMenu = new ToolStripMenuItem("Thay đổi Trạng thái (Status)");
            contextMenu.Items.Add(editItem);
            contextMenu.Items.Add(deleteItem);
            contextMenu.Items.Add(new ToolStripSeparator()); 
            contextMenu.Items.Add(statusMenu);
            optionsButton.Click += (sender, e) =>
            {
                contextMenu.Show(optionsButton, new Point(optionsButton.Width - contextMenu.Width, optionsButton.Height));
            };
            //editItem.Click += EditItem_Click;
            deleteItem.Click += DeleteItem_Click;
            CreateStatusSubMenu(statusMenu);

            rightFlow.Controls.Add(optionsButton);
            Panel separator = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 1,
                BackColor = AppColors.Gray200
            };

            Panel headerContainer = new Panel { Dock = DockStyle.Top, Height = 81, BackColor = Color.Transparent };
            headerContainer.Controls.Add(headerPanel);
            separator.Location = new Point(0, headerPanel.Height);
            headerContainer.Controls.Add(separator);

            return headerContainer;
        }

        private Control CreateLeftMenu()
        {
            FlowLayoutPanel menuPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(16, 130, 16, 16),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Margin = new Padding(0, 0, 1, 0)
            };

            int buttonWidth = 200;
            int buttonHeight = 50;

            FlowLayoutPanel accountHeader = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Width = buttonWidth,
                Margin = new Padding(0, 0, 0, 30)
            };
            accountHeader.Controls.Add(new Label
            {
                Text = "👤",
                Font = new Font("Segoe UI Emoji", 12F),
                AutoSize = true,
                Margin = new Padding(0, 0, 30, 0)
            });
            accountHeader.Controls.Add(new Label
            {
                Text = "ACCOUNT",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = AppColors.Gray700,
                AutoSize = true
            });
            var btnYourTask = CreateMenuButton("Your Task", AppColors.Blue500, Color.White, buttonWidth, buttonHeight);
            btnYourTask.Click += BtnYourTask_Click; 
            menuPanel.Controls.Add(btnYourTask);

           /* var btnGroup = CreateMenuButton("Group", AppColors.Green500, Color.White, buttonWidth, buttonHeight);
            btnGroup.Click += BtnGroup_Click; 
            menuPanel.Controls.Add(btnGroup);*/

            var btnNewTask = CreateMenuButton("New task", AppColors.Orange500, Color.White, buttonWidth, buttonHeight);
            btnNewTask.Click += BtnNewTask_Click; 
            menuPanel.Controls.Add(btnNewTask);

            Color submitColor = AppColors.Purple500;
            var btnSubmitTask = CreateMenuButton("Submit task", submitColor, Color.White, buttonWidth, buttonHeight, Color.FromArgb(200, submitColor));
            btnSubmitTask.Click += BtnSubmitTask_Click; 
            menuPanel.Controls.Add(btnSubmitTask);
            MonthCalendar monthCalendar = new MonthCalendar
            {
                BackColor = Color.White,
                ForeColor = AppColors.Gray700,
                Font = FontRegular,
                SelectionStart = new DateTime(2025, 11, 16),
                SelectionEnd = new DateTime(2025, 11, 16),
                ShowTodayCircle = false,
                TitleBackColor = Color.White,
                TitleForeColor = AppColors.Gray800,
                TrailingForeColor = AppColors.Gray300,
                CalendarDimensions = new Size(1, 1)
            };

            monthCalendar.Width = 248;
            monthCalendar.Height = 180;
            monthCalendar.Margin = new Padding(0, 30, 0, 0);

            menuPanel.Controls.Add(monthCalendar);

            return menuPanel;
        }

        private Control CreateCenterContent()
        {
            Panel scrollContainer = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,                
                BackColor = Color.White,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };

            FlowLayoutPanel contentPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = false,
                AutoSize =true,
                Padding = new Padding(32, 130, 32, 24),
                BackColor = Color.White,
            };
            scrollContainer.Controls.Add(contentPanel);

            int centerContentWidth = 800;

            TableLayoutPanel headerLayout = new TableLayoutPanel
            {
                Width = centerContentWidth,
                AutoSize = true,
                ColumnCount = 2,
                RowCount = 1,
                ColumnStyles =
                {
                    new ColumnStyle(SizeType.AutoSize),
                    new ColumnStyle(SizeType.AutoSize)
                },
                Margin = new Padding(0, 0, 0, 20),
                BackColor = Color.Transparent
            };
            headerLayout.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            headerLayout.AutoSize = true;

            Label title = new Label
            {
                Text = _currentTask.Name,
                Font = FontTitle,
                ForeColor = AppColors.Gray800,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                MaximumSize = new Size(600, 0)
            };
            headerLayout.Controls.Add(title, 0, 0);

            Color statusColor = _currentTask.Status switch
            {
                TaskState.Pending => AppColors.Yellow500,
                TaskState.InProgress => AppColors.Blue500,
                TaskState.Completed => AppColors.Green500,
                _ => AppColors.Gray400
            };

            ModernPanel status = new ModernPanel
            {
                Text = _currentTask.StatusText,
                BackColor = statusColor,
                ForeColor = statusColor == AppColors.Yellow500 ? AppColors.Gray800 : Color.White,
                Font = FontBold,
                BorderRadius = 6,
                Width = 130,
                Height = 50,
                TextAlign = ContentAlignment.MiddleCenter,
                Anchor = AnchorStyles.Right
            };
            status.Margin = new Padding(10, 0, 0, 0);
            headerLayout.Controls.Add(status, 1, 0);

            contentPanel.Controls.Add(headerLayout);

            Label description = new Label
            {
                Text = _currentTask.Description,
                Font = FontRegular,
                ForeColor = AppColors.Gray600,
                MaximumSize = new Size(centerContentWidth, 0),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 20)
            };
            contentPanel.Controls.Add(description);

            Label commentsTitle = new Label
            {
                Text = "Comments",
                Font = FontTitle,
                ForeColor = AppColors.Gray800,
                AutoSize = true,
                Margin = new Padding(0, 30, 0, 16)
            };
            contentPanel.Controls.Add(commentsTitle);

            CustomTextBoxWrapper newCommentBox = new CustomTextBoxWrapper
            {
                Width = centerContentWidth,
                Height = 100,
                Margin = new Padding(0, 0, 0, 10)
            };
            contentPanel.Controls.Add(newCommentBox);

            TimeFlow.UI.Components.CustomButton postButton = new TimeFlow.UI.Components.CustomButton
            {
                Text = "Post",
                BackColor = AppColors.Blue500,
                ForeColor = Color.White,
                HoverColor = AppColors.Blue600,
                BorderRadius = 6,
                Width = 100,
                Height = 40,
                Font = FontBold,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 0, 0, 24)
            };
            postButton.Click += (s, e) => {
                if (!string.IsNullOrWhiteSpace(newCommentBox.TextBoxText) && newCommentBox.TextBoxText != "Add a comment...")
                {
                    Services.TaskManager.AddComment(_currentTask.Id, "Current User", newCommentBox.TextBoxText);
                    newCommentBox.TextBoxText = "Add a comment...";
                    // Refresh to show new comment
                    this.Controls.Clear();
                    InitializeComponent();
                }
            };
            contentPanel.Controls.Add(postButton);

            // Display existing comments from task data
            foreach (var comment in _currentTask.Comments.OrderByDescending(c => c.CreatedDate))
            {
                contentPanel.Controls.Add(CreateComment(comment.Username, comment.Content, comment.TimeAgo));
            }

            Panel spacer = new Panel { Height = 50, Width = 1, BackColor = Color.Transparent };
            contentPanel.Controls.Add(spacer);
            return scrollContainer;
        }

        private Control CreateRightSidebar()
        {
            int sidebarWidth = 350;
            int sidebarPadding = 24;
            int contentWidth = sidebarWidth - (sidebarPadding * 2);

            ModernPanel mainSidebarPanel = new ModernPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderRadius = 0,
                BorderThickness = 0,
                Margin = new Padding(0)
            };

            FlowLayoutPanel contentFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(sidebarPadding, 130, sidebarPadding, sidebarPadding),
                BackColor = Color.Transparent
            };
            mainSidebarPanel.Controls.Add(contentFlow);

            Label detailsTitle = new Label
            {
                Text = "Details",
                Font = FontBold,
                ForeColor = AppColors.Gray800,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 16)
            };
            contentFlow.Controls.Add(detailsTitle);

            TableLayoutPanel detailsContainer = new TableLayoutPanel
            {
                ColumnCount = 2,
                RowCount = 4,
                Width = contentWidth,
                AutoSize = true,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                Margin = new Padding(0, 0, 0, 30),
                ColumnStyles =
                {
                    new ColumnStyle(SizeType.Absolute, 130F),
                    new ColumnStyle(SizeType.Percent, 100F)
                }
            };

            Action<string, Control, int> AddDetailRowToTable = (label, control, row) =>
            {
                Label lbl = new Label
                {
                    Text = label,
                    Font = FontRegular,
                    ForeColor = AppColors.Gray500,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Margin = new Padding(0, 10, 0, 10)
                };

                control.Margin = new Padding(0, 10, 0, 10);
                control.Anchor = AnchorStyles.Left | AnchorStyles.Top;

                detailsContainer.Controls.Add(lbl, 0, row);
                detailsContainer.Controls.Add(control, 1, row);
            };

            FlowLayoutPanel assigneesValue = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Margin = new Padding(0)
            };

            assigneesValue.Controls.Add(new Label { Text = "🧑‍💻", Font = new Font("Segoe UI Emoji", 10F), AutoSize = true, Margin = new Padding(0, 0, 4, 0) });
            string assigneeNames = _currentTask.Assignees.Count > 0 
                ? string.Join(", ", _currentTask.Assignees.Take(2))
                : "Unassigned";
            assigneesValue.Controls.Add(new Label { Text = assigneeNames, Font = FontRegular, ForeColor = AppColors.Gray800, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft });
            if (_currentTask.Assignees.Count > 2)
            {
                assigneesValue.Controls.Add(new Label { Text = $"(+{_currentTask.Assignees.Count - 2})", Font = FontRegular, ForeColor = AppColors.Gray500, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft });
            }

            AddDetailRowToTable("Assignees", assigneesValue, 0);

            Label dueDate = new Label
            {
                Text = _currentTask.DueDateText,
                Font = FontRegular,
                ForeColor = AppColors.Gray800,
                AutoSize = true,
                TextAlign = ContentAlignment.MiddleLeft
            };
            AddDetailRowToTable("Due Date", dueDate, 1);

            FlowLayoutPanel priorityFlow = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Margin = new Padding(0)
            };

            Color priorityColor = _currentTask.Priority switch
            {
                TaskPriorityLevel.Low => AppColors.Green500,
                TaskPriorityLevel.Medium => AppColors.Orange500,
                TaskPriorityLevel.High => AppColors.Red600,
                TaskPriorityLevel.Critical => AppColors.Red700,
                _ => AppColors.Gray400
            };

            ModernPanel priority = new ModernPanel
            {
                Text = _currentTask.PriorityText,
                BackColor = priorityColor,
                ForeColor = Color.White,
                Font = FontBold,
                BorderRadius = 6,
                Width = 100,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter
            };
            priorityFlow.Controls.Add(priority);
            AddDetailRowToTable("Priority", priorityFlow, 2);

            FlowLayoutPanel progressValue = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Margin = new Padding(0)
            };

            ModernPanel progressPanel = new ModernPanel
            {
                BackColor = AppColors.Gray200,
                BorderRadius = 4,
                Height = 8,
                Width = 150
            };
            ModernPanel progressBar = new ModernPanel
            {
                BackColor = AppColors.Blue500,
                BorderRadius = 4,
                Height = 8,
                Width = (int)(150 * (_currentTask.Progress / 100.0))
            };
            progressPanel.Controls.Add(progressBar);

            Label progressLabel = new Label
            {
                Text = $"{_currentTask.Progress}%",
                Font = FontRegular,
                ForeColor = AppColors.Gray800,
                Margin = new Padding(8, -5, 0, 0),
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = true
            };
            progressValue.Controls.Add(progressPanel);
            progressValue.Controls.Add(progressLabel);
            AddDetailRowToTable("Progress", progressValue, 3);

            contentFlow.Controls.Add(detailsContainer);

            Label activityTitle = new Label
            {
                Text = "Activity",
                Font = FontBold,
                ForeColor = AppColors.Gray800,
                AutoSize = true,
                Margin = new Padding(0, 30, 0, 24)
            };
            contentFlow.Controls.Add(activityTitle);

            int activityLogWidth = contentWidth;

            // Display activities from task data
            foreach (var activity in _currentTask.Activities.OrderByDescending(a => a.CreatedDate))
            {
                contentFlow.Controls.Add(CreateActivityLog(activity.Description, activity.TimeAgo, activityLogWidth));
            }

            Panel spacer = new Panel { Height = 50, Width = 1, BackColor = Color.Transparent };
            contentFlow.Controls.Add(spacer);

            return mainSidebarPanel;
        }
    }
}
