using System;
using System.Drawing;
using System.Windows.Forms;

namespace TimeFlow
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
            this.BackColor = ColorPalette.Gray100;
            this.WindowState = FormWindowState.Maximized;
            this.Padding = new Padding(0);
            this.MinimumSize = new Size(800, 600);

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

            CustomButton arrowButton = new CustomButton
            {
                Text = "←",
                Font = new Font("Segoe UI Emoji", 16F),
                ForeColor = HeaderIconColor,
                BackColor = Color.Transparent,
                HoverColor = ColorPalette.Gray200,
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
                ForeColor = ColorPalette.Gray800,
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

            CustomButton closeButton = new CustomButton
            {
                Text = "✕",
                Font = new Font("Segoe UI Emoji", 14F, FontStyle.Bold),
                ForeColor = HeaderIconColor,
                BackColor = Color.Transparent,
                HoverColor = ColorPalette.Gray200,
                BorderRadius = 4,
                Width = 40,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0,18,0,0)
            };
            closeButton.Click += (sender, e) => { this.Close(); };
            rightFlow.Controls.Add(closeButton);

            CustomButton optionsButton = new CustomButton
            {
                Text = "...",
                Font = new Font("Segoe UI Emoji", 14F, FontStyle.Bold),
                ForeColor = HeaderIconColor,
                BackColor = Color.Transparent,
                HoverColor = ColorPalette.Gray200,
                BorderRadius = 4,
                Width = 40,
                Height = 40,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 16, 0, 0)
            };
            rightFlow.Controls.Add(optionsButton);

            headerPanel.Controls.Add(rightFlow, 2, 0);

            Panel separator = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 1,
                BackColor = ColorPalette.Gray200
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
                ForeColor = ColorPalette.Gray700,
                AutoSize = true
            });
            menuPanel.Controls.Add(accountHeader);

            menuPanel.Controls.Add(CreateMenuButton("Your Task", ColorPalette.Blue500, Color.White, buttonWidth, buttonHeight));

            menuPanel.Controls.Add(CreateMenuButton("Group", ColorPalette.Green500, Color.White, buttonWidth, buttonHeight));

            menuPanel.Controls.Add(CreateMenuButton("New task", ColorPalette.Orange500, Color.White, buttonWidth, buttonHeight));

            Color submitColor = ColorPalette.Purple500;

            menuPanel.Controls.Add(CreateMenuButton("Submit task", submitColor, Color.White, buttonWidth, buttonHeight,
                Color.FromArgb(200, submitColor)));


            MonthCalendar monthCalendar = new MonthCalendar
            {
                BackColor = Color.White,
                ForeColor = ColorPalette.Gray700,
                Font = FontRegular,
                SelectionStart = new DateTime(2025, 11, 16),
                SelectionEnd = new DateTime(2025, 11, 16),
                ShowTodayCircle = false,
                TitleBackColor = Color.White,
                TitleForeColor = ColorPalette.Gray800,
                TrailingForeColor = ColorPalette.Gray300,
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
                Text = "Design a new dashboard for the mobile app",
                Font = FontTitle,
                ForeColor = ColorPalette.Gray800,
                AutoSize = true,
                Anchor = AnchorStyles.Left
            };
            headerLayout.Controls.Add(title, 0, 0);

            CustomPanel status = new CustomPanel
            {
                Text = "In Progress",
                BackColor = ColorPalette.Blue500,
                ForeColor = Color.White,
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
                Text = "The current dashboard design is outdated and doesn't provide a good user experience. We need to create a new design that is modern, intuitive, and visually appealing. The new design should include a clear information hierarchy, data visualizations, and easy navigation.",
                Font = FontRegular,
                ForeColor = ColorPalette.Gray600,
                MaximumSize = new Size(centerContentWidth, 0),
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 20)
            };
            contentPanel.Controls.Add(description);


            Label keyReqTitle = new Label
            {
                Text = "Key requirements:",
                Font = FontBold,
                ForeColor = ColorPalette.Gray800,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 8)
            };
            contentPanel.Controls.Add(keyReqTitle);

            string[] requirements =
            {
                "User-friendly interface with a clean layout.",
                "Interactive charts and graphs for data visualization.",
                "Customizable widgets for personalization.",
                "Responsive design for various screen sizes."
            };

            foreach (var req in requirements)
            {
                Label reqLabel = new Label
                {
                    Text = "• " + req,
                    Font = FontRegular,
                    ForeColor = ColorPalette.Gray600,
                    AutoSize = true,
                    Margin = new Padding(0, 0, 0, 4)
                };
                contentPanel.Controls.Add(reqLabel);
            }

            Label commentsTitle = new Label
            {
                Text = "Comments",
                Font = FontTitle,
                ForeColor = ColorPalette.Gray800,
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

            CustomButton postButton = new CustomButton
            {
                Text = "Post",
                BackColor = ColorPalette.Blue500,
                ForeColor = Color.White,
                HoverColor = ColorPalette.Blue600,
                BorderRadius = 6,
                Width = 100,
                Height = 40,
                Font = FontBold,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 0, 0, 24)
            };
            contentPanel.Controls.Add(postButton);

            contentPanel.Controls.Add(CreateComment("Diana", "Can we make sure the dark mode colors are consistent with the web version?", "3 hours ago"));
            contentPanel.Controls.Add(CreateComment("Charlie", "Great progress! I've attached the latest wireframes.", "1 hour ago"));


            Panel spacer = new Panel { Height = 50, Width = 1, BackColor = Color.Transparent };
            contentPanel.Controls.Add(spacer);
            return scrollContainer;
        }

        private Control CreateRightSidebar()
        {
            int sidebarWidth = 350;
            int sidebarPadding = 24;
            int contentWidth = sidebarWidth - (sidebarPadding * 2);

            CustomPanel mainSidebarPanel = new CustomPanel
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
                ForeColor = ColorPalette.Gray800,
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
                    ForeColor = ColorPalette.Gray500,
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
            assigneesValue.Controls.Add(new Label { Text = "Alice, Bob", Font = FontRegular, ForeColor = ColorPalette.Gray800, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft });
            assigneesValue.Controls.Add(new Label { Text = "(+2)", Font = FontRegular, ForeColor = ColorPalette.Gray500, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft });

            AddDetailRowToTable("Assignees", assigneesValue, 0);

            Label dueDate = new Label
            {
                Text = "Dec 15, 2025",
                Font = FontRegular,
                ForeColor = ColorPalette.Gray800,
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
            CustomPanel priority = new CustomPanel
            {
                Text = "High",
                BackColor = ColorPalette.Red600,
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

            CustomPanel progressPanel = new CustomPanel
            {
                BackColor = ColorPalette.Gray200,
                BorderRadius = 4,
                Height = 8,
                Width = 150
            };
            CustomPanel progressBar = new CustomPanel
            {
                BackColor = ColorPalette.Blue500,
                BorderRadius = 4,
                Height = 8,
                Width = (int)(150 * 0.75)
            };
            progressPanel.Controls.Add(progressBar);

            Label progressLabel = new Label
            {
                Text = "75%",
                Font = FontRegular,
                ForeColor = ColorPalette.Gray800,
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
                ForeColor = ColorPalette.Gray800,
                AutoSize = true,
                Margin = new Padding(0, 30, 0, 24)
            };
            contentFlow.Controls.Add(activityTitle);

            int activityLogWidth = contentWidth;

            contentFlow.Controls.Add(CreateActivityLog("Alice assigned this task to Bob.", "2 days ago", activityLogWidth));
            contentFlow.Controls.Add(CreateActivityLog("Charlie changed the due date to Dec 15, 2025.", "1 day ago", activityLogWidth));
            contentFlow.Controls.Add(CreateActivityLog("Diana left a comment.", "3 hours ago", activityLogWidth));

            Panel spacer = new Panel { Height = 50, Width = 1, BackColor = Color.Transparent };
            contentFlow.Controls.Add(spacer);

            return mainSidebarPanel;
        }
    }
}