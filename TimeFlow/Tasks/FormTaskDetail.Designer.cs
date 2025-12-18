using System;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using TimeFlow.UI.Components;
using TimeFlow.Models;
using TimeFlow.UI;
using TimeFlow.Services;

namespace TimeFlow.Tasks
{
    partial class FormTaskDetail
    {
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

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // FormTaskDetail
            // 
            ClientSize = new Size(1573, 778);
            Name = "FormTaskDetail";
            Load += FormTaskDetail_Load;
            ResumeLayout(false);

            // KHÔNG gọi SetupLayout ở đây - sẽ gọi sau khi load xong data
        }

        #endregion

        // UI Setup Methods
        private void SetupLayout()
        {
            this.SuspendLayout(); // ✅ Ngừng layout calculation
            
            this.Text = "Task Details";
            this.BackColor = AppColors.Gray100;
            this.WindowState = FormWindowState.Maximized;
            this.Padding = new Padding(0);
            this.MinimumSize = new Size(800, 600);

            // Clear existing controls before re-rendering
            this.Controls.Clear();

            // Check if task loaded
            if (_currentTask == null)
            {
                Label errorLabel = new Label
                {
                    Text = "Đang tải thông tin task...",
                    Font = FontTitle,
                    ForeColor = AppColors.Gray600,
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter
                };
                this.Controls.Add(errorLabel);
                this.ResumeLayout(); // ✅ Resume layout
                return;
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
            
            this.ResumeLayout(); // ✅ Calculate layout 1 lần duy nhất
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

            CustomButton arrowButton = new CustomButton
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
                Margin = new Padding(0, 18, 0, 0)
            };
            arrowButton.Click += (sender, e) => { this.Close(); };
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

            CustomButton optionsButton = new CustomButton
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

            editItem.Click += EditItem_Click;
            deleteItem.Click += DeleteItem_Click;
            CreateStatusSubMenu(statusMenu);

            bool hasPerm = UserHasPermission();
            editItem.Enabled = hasPerm;
            deleteItem.Enabled = hasPerm;

            if (!hasPerm)
            {
                editItem.Text += " (Read Only)";
            }

            rightFlow.Controls.Add(optionsButton);
            headerPanel.Controls.Add(rightFlow, 2, 0);

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

            var btnYourTask = CreateMenuButton("Your Task", AppColors.Blue500, Color.White, buttonWidth, buttonHeight);
            btnYourTask.Click += BtnYourTask_Click;
            menuPanel.Controls.Add(btnYourTask);

            var btnGroup = CreateMenuButton("Group", AppColors.Green500, Color.White, buttonWidth, buttonHeight);
            btnGroup.Click += BtnGroup_Click;
            menuPanel.Controls.Add(btnGroup);

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
                SelectionStart = DateTime.Today,
                SelectionEnd = DateTime.Today,
                ShowTodayCircle = false,
                TitleBackColor = Color.White,
                TitleForeColor = AppColors.Gray800,
                TrailingForeColor = AppColors.Gray300,
                CalendarDimensions = new Size(1, 1),
                Width = 248,
                Height = 180,
                Margin = new Padding(0, 30, 0, 0)
            };

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
                AutoSize = true,
                Padding = new Padding(32, 130, 32, 24),
                BackColor = Color.White,
            };
            scrollContainer.Controls.Add(contentPanel);

            int centerContentWidth = 800;

            // Header with title and status
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

            Label title = new Label
            {
                Text = _currentTask.Title,
                Font = FontTitle,
                ForeColor = AppColors.Gray800,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                MaximumSize = new Size(600, 0)
            };
            headerLayout.Controls.Add(title, 0, 0);

            Color statusColor = GetStatusColor(_currentTask.Status);
            
            _statusBadge = new ModernPanel
            {
                Text = _currentTask.StatusText,
                BackColor = statusColor,
                ForeColor = statusColor == AppColors.Yellow500 ? AppColors.Gray800 : Color.White,
                Font = FontBold,
                BorderRadius = 6,
                Width = 130,
                Height = 50,
                TextAlign = ContentAlignment.MiddleCenter,
                Anchor = AnchorStyles.Right,
                Margin = new Padding(10, 0, 0, 0)
            };
            headerLayout.Controls.Add(_statusBadge, 1, 0);

            contentPanel.Controls.Add(headerLayout);

            // Description
            if (!string.IsNullOrEmpty(_currentTask.Description))
            {
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
            }
            // 1. Tạo ô tìm kiếm
            CustomTextBoxWrapper txtSearchComments = new CustomTextBoxWrapper
            {
                Width = centerContentWidth,
                Height = 40,
                TextBoxText = "🔍 Search comments...",
                Margin = new Padding(0, 10, 0, 10)
            };
            txtSearchComments.Enter += (s, e) => {
                this.BeginInvoke((MethodInvoker)delegate {
                 txtSearchComments.SelectAll();
                });
            };
            // 2. Thêm sự kiện khi người dùng gõ chữ
            txtSearchComments.TextChanged += (s, e) => {

                string keyword = txtSearchComments.TextBoxText.Trim();
                if (keyword == "Search comments...") keyword = "";

                // Gọi hàm lọc dữ liệu
                FilterComments(keyword);
            };

            // 3. Thêm vào contentPanel trước phần tiêu đề Comments
            contentPanel.Controls.Add(txtSearchComments);

            // Comments section
            Label commentsTitle = new Label
            {
                Text = _currentTask.HasComments ? $"Comments ({_currentTask.Comments.Count})" : "Comments",
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
            newCommentBox.Enter += (s, e) => {
                this.BeginInvoke((MethodInvoker)delegate {
                    newCommentBox.SelectAll();
                });
            };
            CustomButton postButton = new CustomButton
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
                    MessageBox.Show("Comment feature will be implemented soon!", "Info");
                    // TODO: Implement add comment API
                }
            };
            contentPanel.Controls.Add(postButton);

            // Display existing comments
            if (_currentTask.HasComments)
            {
                foreach (var comment in _currentTask.Comments.Take(10)) // ✅ Limit initial render
                {
                    contentPanel.Controls.Add(CreateComment(
                        comment.DisplayName, 
                        comment.Content, 
                        comment.TimeAgo
                    ));
                }
                
                // Add "Load more" if needed
                if (_currentTask.Comments.Count > 10)
                {
                    Label loadMoreLabel = new Label
                    {
                        Text = $"+ Load {_currentTask.Comments.Count - 10} more comments",
                        Font = new Font("Segoe UI", 10F, FontStyle.Italic),
                        ForeColor = AppColors.Blue500,
                        Cursor = Cursors.Hand,
                        AutoSize = true,
                        Margin = new Padding(0, 10, 0, 0)
                    };
                    loadMoreLabel.Click += (s, e) => LoadMoreComments(contentPanel, 10);
                    contentPanel.Controls.Add(loadMoreLabel);
                }
            }
            else if (_isLoadingDetails)
            {
                // ✅ Show loading skeleton
                Label loadingComments = new Label
                {
                    Text = "⏳ Loading comments...",
                    Font = FontRegular,
                    ForeColor = AppColors.Gray500,
                    AutoSize = true,
                    Margin = new Padding(0, 10, 0, 0)
                };
                contentPanel.Controls.Add(loadingComments);
            }
            else
            {
                Label noComments = new Label
                {
                    Text = "No comments yet. Be the first to comment!",
                    Font = FontRegular,
                    ForeColor = AppColors.Gray500,
                    AutoSize = true,
                    Margin = new Padding(0, 10, 0, 0)
                };
                contentPanel.Controls.Add(noComments);
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

            // Group info (chỉ hiển thị nếu là group task)
            if (_currentTask.IsGroupTask && _currentTask.HasAssignees)
            {
                contentFlow.Controls.Add(new Label
                {
                    Text = $"Group Task",
                    Font = FontBold,
                    ForeColor = AppColors.Blue600,
                    AutoSize = true,
                    Margin = new Padding(0, 0, 0, 5)
                });

                string assigneeText = string.Join(", ", _currentTask.Assignees.Take(3));
                if (_currentTask.Assignees.Count > 3)
                {
                    assigneeText += $" and {_currentTask.Assignees.Count - 3} more";
                }

                contentFlow.Controls.Add(new Label
                {
                    Text = $"Assigned to: {assigneeText}",
                    Font = FontRegular,
                    AutoSize = true,
                    MaximumSize = new Size(contentWidth, 0),
                    Margin = new Padding(0, 0, 0, 15)
                });
            }

            // Details section
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

            // Assignees
            FlowLayoutPanel assigneesValue = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                AutoSize = true,
                Margin = new Padding(0)
            };
            assigneesValue.Controls.Add(new Label { 
                Text = "🧑‍💻", 
                Font = new Font("Segoe UI Emoji", 10F), 
                AutoSize = true, 
                Margin = new Padding(0, 0, 4, 0) 
            });
            
            string assigneeNames = _currentTask.HasAssignees
                ? string.Join(", ", _currentTask.Assignees.Take(2))
                : "Unassigned";
            assigneesValue.Controls.Add(new Label { 
                Text = assigneeNames, 
                Font = FontRegular, 
                ForeColor = AppColors.Gray800, 
                AutoSize = true 
            });
            
            if (_currentTask.Assignees.Count > 2)
            {
                assigneesValue.Controls.Add(new Label { 
                    Text = $"(+{_currentTask.Assignees.Count - 2})", 
                    Font = FontRegular, 
                    ForeColor = AppColors.Gray500, 
                    AutoSize = true 
                });
            }
            AddDetailRow(detailsContainer, "Assignees", assigneesValue, 0);

            // Due Date
            Label dueDate = new Label
            {
                Text = _currentTask.DueDateText,
                Font = FontRegular,
                ForeColor = AppColors.Gray800,
                AutoSize = true
            };
            AddDetailRow(detailsContainer, "Due Date", dueDate, 1);

            // Priority
            Color priorityColor = GetPriorityColor(_currentTask.Priority);
            
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
            AddDetailRow(detailsContainer, "Priority", priority, 2);

            // Progress
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
//# warn: This is probably a bug. Please check if the label should have a specific font or if it can stay as is.
                Font = FontRegular,
                ForeColor = AppColors.Gray800,
                Margin = new Padding(8, -5, 0, 0),
                AutoSize = true
            };
            progressValue.Controls.Add(progressPanel);
            progressValue.Controls.Add(progressLabel);
            AddDetailRow(detailsContainer, "Progress", progressValue, 3);

            contentFlow.Controls.Add(detailsContainer);

            // Activity section
            Label activityTitle = new Label
            {
                Text = _currentTask.HasActivities ? $"Activity ({_currentTask.Activities.Count})" : "Activity",
                Font = FontBold,
                ForeColor = AppColors.Gray800,
                AutoSize = true,
                Margin = new Padding(0, 30, 0, 24)
            };
            contentFlow.Controls.Add(activityTitle);

            // Display activities
            if (_currentTask.HasActivities)
            {
                foreach (var activity in _currentTask.Activities.OrderByDescending(a => a.CreatedAt).Take(10))
                {
                    contentFlow.Controls.Add(CreateActivityLog(
                        activity.Description, 
                        activity.TimeAgo, 
                        contentWidth
                    ));
                }

                if (_currentTask.Activities.Count > 10)
                {
                    Label moreActivities = new Label
                    {
                        Text = $"+ {_currentTask.Activities.Count - 10} more activities",
                        Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                        ForeColor = AppColors.Gray500,
                        AutoSize = true,
                        Margin = new Padding(0, 10, 0, 0)
                    };
                    contentFlow.Controls.Add(moreActivities);
                }
            }
            else if (_isLoadingDetails)
            {
                // ✅ Show loading skeleton
                Label loadingActivity = new Label
                {
                    Text = "⏳ Loading activities...",
                    Font = FontRegular,
                    ForeColor = AppColors.Gray500,
                    AutoSize = true
                };
                contentFlow.Controls.Add(loadingActivity);
            }
            else
            {
                Label noActivity = new Label
                {
                    Text = "No activity yet.",
                    Font = FontRegular,
                    ForeColor = AppColors.Gray500,
                    AutoSize = true
                };
                contentFlow.Controls.Add(noActivity);
            }

            Panel spacer = new Panel { Height = 50, Width = 1, BackColor = Color.Transparent };
            contentFlow.Controls.Add(spacer);

            return mainSidebarPanel;
        }

        // Helper Methods
        private CustomButton CreateMenuButton(string text, Color backColor, Color foreColor, int width, int height, Color? hoverColor = null)
        {
            return new CustomButton
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

        private Control CreateComment(string user, string text, string time)
        {
            FlowLayoutPanel comment = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                Width = 800,
                Margin = new Padding(0, 0, 0, 16),
                Padding = new Padding(12),
                BackColor = AppColors.Gray50,
                BorderStyle = BorderStyle.FixedSingle

            };

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
                MaximumSize = new Size(760, 0),
                AutoSize = true
            };
            comment.Controls.Add(content);

            return comment;
        }


        private Control CreateActivityLog(string activity, string time, int width)
        {
            FlowLayoutPanel logItem = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                Width = width,
                Margin = new Padding(0, 0, 0, 12)
            };

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

        private void AddDetailRow(TableLayoutPanel container, string label, Control control, int row)
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

            container.Controls.Add(lbl, 0, row);
            container.Controls.Add(control, 1, row);
        }

        private void CreateStatusSubMenu(ToolStripMenuItem statusMenu)
        {
            System.Array statusValues = System.Enum.GetValues(typeof(TimeFlow.Models.TaskStatus));
            foreach (TimeFlow.Models.TaskStatus status in statusValues)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(status.ToString());
                if (_currentTask != null && status == _currentTask.Status) 
                    item.Checked = true;
                item.Click += (sender, e) => ChangeStatusItem_Click(status);
                statusMenu.DropDownItems.Add(item);
            }
        }
    }
}
