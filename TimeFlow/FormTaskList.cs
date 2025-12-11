using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace TimeFlow
{
    // ====================================================================
    // 1. Color Palette 
    // ====================================================================
    public static class CustomColors
    {
        public static readonly Color Gray50 = ColorTranslator.FromHtml("#f9fafb");
        public static readonly Color Gray100 = ColorTranslator.FromHtml("#f3f4f6");
        public static readonly Color Gray200 = ColorTranslator.FromHtml("#e5e7eb");
        public static readonly Color Gray500 = ColorTranslator.FromHtml("#6b7280");
        public static readonly Color Gray800 = ColorTranslator.FromHtml("#1f2937");
        public static readonly Color Gray900 = ColorTranslator.FromHtml("#111827");
        public static readonly Color Blue500 = ColorTranslator.FromHtml("#3b82f6");
        public static readonly Color Blue600 = ColorTranslator.FromHtml("#2563eb");
        public static readonly Color Blue100 = ColorTranslator.FromHtml("#dbeafe");
        public static readonly Color Blue800 = ColorTranslator.FromHtml("#1e40af");
        public static readonly Color Yellow100 = ColorTranslator.FromHtml("#fef3c7");
        public static readonly Color Yellow800 = ColorTranslator.FromHtml("#92400e");
        public static readonly Color Green100 = ColorTranslator.FromHtml("#d1fae5");
        public static readonly Color Green800 = ColorTranslator.FromHtml("#065f46");
        public static readonly Color Red50 = ColorTranslator.FromHtml("#fef2f2");
        public static readonly Color Red600 = ColorTranslator.FromHtml("#dc2626");
        public static readonly Color Orange50 = ColorTranslator.FromHtml("#fff7ed");
        public static readonly Color Orange600 = ColorTranslator.FromHtml("#ea580c");
        public static readonly Color Green50 = ColorTranslator.FromHtml("#f0fdf4");
        public static readonly Color Green600 = ColorTranslator.FromHtml("#059669");
        public static readonly Color White = Color.White;
    }

    // ====================================================================
    // 2. Custom Controls 
    // ====================================================================

    public static class GraphicsPathHelper
    {
        public static GraphicsPath GetRoundedRect(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            float diameter = radius * 2f;
            SizeF size = new SizeF(diameter, diameter);
            RectangleF arc = new RectangleF(rect.Location, size);

            rect.Width -= 0.1f; rect.Height -= 0.1f;

            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - diameter; path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - diameter; path.AddArc(arc, 0, 90);
            arc.X = rect.Left; path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }
    }

    public class CustomRoundControl : Panel
    {
        private int _borderRadius = 0;
        public int BorderRadius
        {
            get { return _borderRadius; }
            set { _borderRadius = value; Invalidate(); }
        }
        public CustomRoundControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            if (_borderRadius > 0)
            {
                using (GraphicsPath path = GraphicsPathHelper.GetRoundedRect(new RectangleF(0, 0, this.Width, this.Height), _borderRadius))
                {
                    if (this.Parent is ScrollableControl parentScroll)
                    {
                        this.Region = new Region(path);
                    }

                    using (SolidBrush brush = new SolidBrush(this.BackColor)) { e.Graphics.FillPath(brush, path); }
                }
            }
            else
            {
                using (SolidBrush brush = new SolidBrush(this.BackColor)) { e.Graphics.FillRectangle(brush, this.ClientRectangle); }
            }
        }
    }

    public class CustomButton : CustomRoundControl
    {
        private Color _normalColor;
        private Color _hoverColor;
        private Color _currentColor;
        private string _buttonText = string.Empty;

        public Color NormalColor { get { return _normalColor; } set { _normalColor = value; _currentColor = value; Invalidate(); } }
        public Color HoverColor { get { return _hoverColor; } set { _hoverColor = value; } }
        public string ButtonText { get { return _buttonText; } set { _buttonText = value; Invalidate(); } }

        public CustomButton()
        {
            _normalColor = CustomColors.Blue500; _hoverColor = CustomColors.Blue600;
            _currentColor = _normalColor; this.Cursor = Cursors.Hand; this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            this.Text = "";
        }

        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); _currentColor = _hoverColor; Invalidate(); }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); _currentColor = _normalColor; Invalidate(); }
        protected override void OnMouseDown(MouseEventArgs e) { base.OnMouseDown(e); _currentColor = Color.FromArgb(Math.Max(0, _currentColor.R - 20), Math.Max(0, _currentColor.G - 20), Math.Max(0, _currentColor.B - 20)); Invalidate(); }
        protected override void OnMouseUp(MouseEventArgs e) { base.OnMouseUp(e); _currentColor = this.ClientRectangle.Contains(e.Location) ? _hoverColor : _normalColor; Invalidate(); }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);

            using (GraphicsPath path = GraphicsPathHelper.GetRoundedRect(new RectangleF(0, 0, this.Width, this.Height), BorderRadius))
            using (SolidBrush backBrush = new SolidBrush(_currentColor))
            {
                e.Graphics.FillPath(backBrush, path);

                if (!string.IsNullOrEmpty(_buttonText))
                {
                    TextFormatFlags flags = TextFormatFlags.VerticalCenter | TextFormatFlags.Left;
                    TextRenderer.DrawText(e.Graphics, _buttonText, this.Font, new Rectangle(15, 0, rect.Width - 15, rect.Height), this.ForeColor, flags);
                }
            }
        }
    }

    public class RoundedLabel : Label
    {
        public int BorderRadius { get; set; } = 10;

        public RoundedLabel()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
            this.TextAlign = ContentAlignment.MiddleCenter; this.AutoSize = false;
            this.Padding = new Padding(8, 2, 8, 2);
            this.Width = 80; this.Height = 24;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);

            using (GraphicsPath path = GraphicsPathHelper.GetRoundedRect(rect, BorderRadius))
            using (SolidBrush backBrush = new SolidBrush(this.BackColor))
            {
                e.Graphics.FillPath(backBrush, path);
            }

            TextRenderer.DrawText(e.Graphics, this.Text, this.Font, rect, this.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }

    // ====================================================================
    // 3. Task Row Control 
    // ====================================================================

    public class TaskRowPanel : CustomRoundControl
    {
        private Color _normalBgColor = CustomColors.White;
        private Color _hoverBgColor = CustomColors.Gray100;
        private Label lblTitle;
        private const int ROW_HEIGHT = 65;

        public TaskRowPanel(Task task)
        {
            this.Height = ROW_HEIGHT; this.Dock = DockStyle.Top; this.BackColor = _normalBgColor;
            this.Cursor = Cursors.Hand;
            this.Padding = new Padding(0);
            InitializeComponents(task);
        }

        private void InitializeComponents(Task task)
        {
            TableLayoutPanel tlp = new TableLayoutPanel();
            tlp.Dock = DockStyle.Fill; tlp.ColumnCount = 4;
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.666f));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.666f));
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.666f));
            tlp.RowCount = 1; tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            tlp.BackColor = Color.Transparent;
            tlp.Padding = new Padding(20, 0, 20, 0);

            TableLayoutPanel tlpC1 = new TableLayoutPanel()
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                ColumnCount = 1,
                RowCount = 2,
                Padding = new Padding(0)
            };
            tlpC1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            tlpC1.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            tlpC1.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));

            lblTitle = new Label
            {
                Text = task.Title,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = CustomColors.Gray900,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(0, 0, 0, 0),
                MaximumSize = new Size(0, 20)
            };
            Label lblAssignees = new Label
            {
                Text = $"{task.Assignees} assignees",
                Font = new Font("Segoe UI", 7.5f, FontStyle.Regular),
                ForeColor = CustomColors.Gray500,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(0, 0, 0, 0),
                MaximumSize = new Size(0, 15)
            };

            tlpC1.Controls.Add(lblTitle, 0, 0);
            tlpC1.Controls.Add(lblAssignees, 0, 1);

            Label lblDueDate = new Label { Text = task.DueDate, Font = new Font("Segoe UI", 9f, FontStyle.Regular), ForeColor = CustomColors.Gray500, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };

            RoundedLabel lblStatus = CreateBadge(task.Status, task.StatusBgColor, task.StatusTextColor);
            Panel pnlStatusWrapper = CreateCenterWrapper(lblStatus);

            RoundedLabel lblPriority = CreateBadge(task.Priority, task.PriorityBgColor, task.PriorityTextColor);
            Panel pnlPriorityWrapper = CreateCenterWrapper(lblPriority);

            tlp.Controls.Add(tlpC1, 0, 0);
            tlp.Controls.Add(lblDueDate, 1, 0);
            tlp.Controls.Add(pnlStatusWrapper, 2, 0); tlp.Controls.Add(pnlPriorityWrapper, 3, 0);
            this.Controls.Add(tlp);
            AddHoverAndClickPropagation(tlp);
        }

        private Panel CreateCenterWrapper(Control content)
        {
            Panel wrapper = new Panel() { Dock = DockStyle.Fill, BackColor = Color.Transparent };
            wrapper.Controls.Add(content);

            wrapper.Resize += (s, e) =>
            {
                content.Location = new Point((wrapper.Width / 2 - content.Width / 2), (wrapper.Height / 2 - content.Height / 2));
            };

            content.Location = new Point((wrapper.Width / 2 - content.Width / 2), (wrapper.Height / 2 - content.Height / 2));
            return wrapper;
        }


        private void AddHoverAndClickPropagation(Control parent)
        {
            parent.MouseEnter += (s, e) => OnMouseEnter(e); parent.MouseLeave += (s, e) => OnMouseLeave(e); parent.Click += (s, e) => OnClick(e);
            foreach (Control control in parent.Controls)
            {
                control.MouseEnter += (s, e) => OnMouseEnter(e); control.MouseLeave += (s, e) => OnMouseLeave(e); control.Click += (s, e) => OnClick(e);
                if (control.HasChildren) { AddHoverAndClickPropagation(control); }
            }
        }

        private RoundedLabel CreateBadge(string text, Color backColor, Color foreColor)
        {
            return new RoundedLabel { Text = text, BackColor = backColor, ForeColor = foreColor, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold) };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // Vẽ đường phân cách dưới
            e.Graphics.DrawLine(new Pen(CustomColors.Gray200), 0, this.Height - 1, this.Width, this.Height - 1);
            base.OnPaint(e);
        }

        protected override void OnMouseEnter(EventArgs e) { base.OnMouseEnter(e); this.BackColor = _hoverBgColor; lblTitle.ForeColor = CustomColors.Blue600; }
        protected override void OnMouseLeave(EventArgs e) { base.OnMouseLeave(e); this.BackColor = _normalBgColor; lblTitle.ForeColor = CustomColors.Gray900; }
    }

    // ====================================================================
    // 4. Data Model 
    // ====================================================================

    public class Task
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Assignees { get; set; }
        public string DueDate { get; set; }
        public string Status { get; set; }
        public Color StatusBgColor { get; set; }
        public Color StatusTextColor { get; set; }
        public string Priority { get; set; }
        public Color PriorityBgColor { get; set; }
        public Color PriorityTextColor { get; set; }
    }

    // ====================================================================
    // 5. Main Form: FormTaskList 
    // ====================================================================

    public partial class FormTaskList : Form
    {
        private List<Task> tasks;
        private Panel pnlSidebar;
        private Panel pnlMainContent;
        private FlowLayoutPanel flpTaskList;

        public FormTaskList()
        {
            InitializeData();
            InitializeForm();
            InitializeLayout();
            SetupSidebar();
            SetupTaskList();
        }

        private void InitializeData()
        {
            tasks = new List<Task>
            {
                new Task { Id = 1, Title = "Design a new dashboard for the mobile app", Assignees = 3, DueDate = "Dec 15, 2025", Status = "In Progress", StatusBgColor = CustomColors.Blue100, StatusTextColor = CustomColors.Blue800, Priority = "High", PriorityBgColor = CustomColors.Red50, PriorityTextColor = CustomColors.Red600 },
                new Task { Id = 2, Title = "Database Schema (CSDL) Deadline", Assignees = 1, DueDate = "Nov 01, 2025", Status = "Pending", StatusBgColor = CustomColors.Yellow100, StatusTextColor = CustomColors.Yellow800, Priority = "Medium", PriorityBgColor = CustomColors.Orange50, PriorityTextColor = CustomColors.Orange600 },
                new Task { Id = 3, Title = "Philosophy Theory Preparation", Assignees = 2, DueDate = "Dec 21, 2025", Status = "Completed", StatusBgColor = CustomColors.Green100, StatusTextColor = CustomColors.Green800, Priority = "Low", PriorityBgColor = CustomColors.Green50, PriorityTextColor = CustomColors.Green600 },
                new Task { Id = 4, Title = "Submit Q4 Report", Assignees = 1, DueDate = "Nov 30, 2025", Status = "Pending", StatusBgColor = CustomColors.Yellow100, StatusTextColor = CustomColors.Yellow800, Priority = "High", PriorityBgColor = CustomColors.Red50, PriorityTextColor = CustomColors.Red600 },
                new Task { Id = 5, Title = "Team Building Event Planning", Assignees = 4, DueDate = "Dec 05, 2025", Status = "In Progress", StatusBgColor = CustomColors.Blue100, StatusTextColor = CustomColors.Blue800, Priority = "Medium", PriorityBgColor = CustomColors.Orange50, PriorityTextColor = CustomColors.Orange600 }
            };
        }

        private void InitializeForm()
        {
            this.Text = "My Tasks"; this.WindowState = FormWindowState.Maximized;
            this.BackColor = CustomColors.Gray50; this.Font = new Font("Segoe UI", 9f);
            this.FormBorderStyle = FormBorderStyle.None;

            Panel pnlTitleBar = new Panel() { Dock = DockStyle.Top, Height = 45, BackColor = CustomColors.White };

            Label lblClose = new Label() { Text = "✕", Font = new Font("Arial", 12f, FontStyle.Regular), Dock = DockStyle.Right, AutoSize = true, Cursor = Cursors.Hand, Padding = new Padding(15, 12, 15, 0), ForeColor = CustomColors.Gray500 };
            lblClose.Click += (s, e) => this.Close();

            FlowLayoutPanel pnlBack = new FlowLayoutPanel()
            {
                Dock = DockStyle.Left,
                AutoSize = true,
                Cursor = Cursors.Default,
                Margin = new Padding(15, 0, 0, 0),
                BackColor = CustomColors.White
            };

            Label lblArrow = new Label()
            {
                Text = "←",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 12, 5, 0), 
                ForeColor = CustomColors.Gray900,
                Cursor = Cursors.Hand
            };

            Label lblTitle = new Label()
            {
                Text = "My Tasks",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                AutoSize = true,
                Margin = new Padding(0, 12, 0, 0),
                ForeColor = CustomColors.Gray900
            };

            pnlBack.Controls.Add(lblArrow);
            pnlBack.Controls.Add(lblTitle);

            lblArrow.Click += (s, e) => MessageBox.Show("'Quay lại' được click!", "Thông báo");

            Action<Color> setColors = (color) =>
            {
                lblArrow.ForeColor = color;
                lblTitle.ForeColor = color;
            };

            lblArrow.MouseEnter += (s, e) => setColors(CustomColors.Blue500);
            lblArrow.MouseLeave += (s, e) => setColors(CustomColors.Gray900);

            lblTitle.MouseEnter += (s, e) => setColors(CustomColors.Blue500);
            lblTitle.MouseLeave += (s, e) => setColors(CustomColors.Gray900);

            
            pnlTitleBar.Controls.Add(lblClose); 
            pnlTitleBar.Controls.Add(pnlBack);  
            this.Controls.Add(pnlTitleBar);
        }

        private void InitializeLayout()
        {
            TableLayoutPanel tlpMain = new TableLayoutPanel();
            tlpMain.Dock = DockStyle.Fill; tlpMain.ColumnCount = 2; tlpMain.RowCount = 1;
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280));
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            tlpMain.BackColor = Color.Transparent;

            pnlSidebar = new Panel() { Dock = DockStyle.Fill, BackColor = CustomColors.White, Padding = new Padding(20) };

            Panel pnlMainContentWrapper = new Panel()
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = CustomColors.Gray50,
                Padding = new Padding(25, 20, 25, 20)
            };

            pnlMainContent = new Panel() { BackColor = CustomColors.Gray50, Dock = DockStyle.Top, Margin = new Padding(0) };
            pnlMainContentWrapper.Controls.Add(pnlMainContent);

            pnlMainContentWrapper.Resize += (s, e) =>
            {
                int newWidth = pnlMainContentWrapper.ClientSize.Width - pnlMainContentWrapper.Padding.Left - pnlMainContentWrapper.Padding.Right;
                pnlMainContent.Width = newWidth;

                if (pnlMainContent.Controls.Count > 0 && pnlMainContent.Controls[0] is CustomRoundControl pnlCard)
                {
                    pnlCard.Width = newWidth;
                }
            };


            tlpMain.Controls.Add(pnlSidebar, 0, 0);
            tlpMain.Controls.Add(pnlMainContentWrapper, 1, 0);

            this.Controls.Add(tlpMain);
        }

        private void SetupSidebar()
        {
            FlowLayoutPanel flpSidebar = new FlowLayoutPanel()
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.TopDown,
                AutoSize = true,
                WrapContents = false,
                BackColor = CustomColors.White
            };
            flpSidebar.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            flpSidebar.Controls.Add(new Panel() { Height = 50, BackColor = Color.Transparent, Width = 240 });


            FlowLayoutPanel pnlAccount = new FlowLayoutPanel() { FlowDirection = FlowDirection.LeftToRight, AutoSize = true, Margin = new Padding(0, 0, 0, 15) };
            Label lblIcon = new Label() { Text = "👤", Font = new Font("Segoe UI Symbol", 10f, FontStyle.Regular), ForeColor = CustomColors.Gray500, AutoSize = true, Margin = new Padding(0, 0, 5, 0) };
            Label lblAccountTitle = new Label() { Text = "ACCOUNT", Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = CustomColors.Gray500, AutoSize = true, Margin = new Padding(0, 1, 0, 0) };
            pnlAccount.Controls.Add(lblIcon); pnlAccount.Controls.Add(lblAccountTitle);
            flpSidebar.Controls.Add(pnlAccount);


            CustomButton btnYourTask = CreateSidebarButton("Your Task", CustomColors.Blue500, CustomColors.Blue600);
            CustomButton btnGroup = CreateSidebarButton("Group", ColorTranslator.FromHtml("#34d399"), ColorTranslator.FromHtml("#10b981"));
            CustomButton btnNewTask = CreateSidebarButton("New task", ColorTranslator.FromHtml("#ff9f40"), ColorTranslator.FromHtml("#e58c35"));
            CustomButton btnSubmitTask = CreateSidebarButton("Submit task", ColorTranslator.FromHtml("#9364cc"), ColorTranslator.FromHtml("#7c4dbe"));

            flpSidebar.Controls.Add(btnYourTask); flpSidebar.Controls.Add(btnGroup);
            flpSidebar.Controls.Add(btnNewTask); flpSidebar.Controls.Add(btnSubmitTask);

            flpSidebar.Controls.Add(new Panel() { Height = 25, BackColor = Color.Transparent, Width = 240 });

            // Calendar
            Label lblMonth = new Label() { Text = "November 2025", Font = new Font("Segoe UI", 9f, FontStyle.Bold), AutoSize = true, ForeColor = CustomColors.Gray800, Margin = new Padding(0, 0, 0, 5) };
            flpSidebar.Controls.Add(lblMonth);

            MonthCalendar monthCalendar = new MonthCalendar
            {
                CalendarDimensions = new Size(1, 1),
                BackColor = CustomColors.White,
                TitleBackColor = CustomColors.Blue500,
                TitleForeColor = CustomColors.White,
                TrailingForeColor = CustomColors.Gray500,
                Font = new Font("Segoe UI", 9f),
                SelectionStart = new DateTime(2025, 11, 16),
                SelectionEnd = new DateTime(2025, 11, 16),
            };

            monthCalendar.Width = 240;
            monthCalendar.Height = 160;
            monthCalendar.Margin = new Padding(0, 0, 0, 0);

            flpSidebar.Controls.Add(monthCalendar);
            pnlSidebar.Controls.Add(flpSidebar);
        }

        private CustomButton CreateSidebarButton(string text, Color normalColor, Color hoverColor)
        {
            string icon = "";
            switch (text)
            {
                case "Your Task": icon = "📋"; break;
                case "Group": icon = "👥"; break;
                case "New task": icon = "➕"; break;
                case "Submit task": icon = "📤"; break;
            }

            return new CustomButton
            {
                ButtonText = $"{icon}  {text}",
                Width = 240,
                Height = 40,
                Margin = new Padding(0, 0, 0, 10),
                BorderRadius = 8,
                NormalColor = normalColor,
                HoverColor = hoverColor,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.White
            };
        }

        private void SetupTaskList()
        {
            CustomRoundControl pnlCard = new CustomRoundControl()
            {
                BackColor = CustomColors.White,
                BorderRadius = 10,
                Dock = DockStyle.Top
            };
            pnlCard.Width = pnlMainContent.ClientSize.Width;
            pnlCard.Margin = new Padding(0);

            List<Task> allTasks = new List<Task>(tasks);
            for (int i = 0; i < 15; i++)
            {
                allTasks.Add(new Task { Id = i + 6, Title = $"More Tasks {i + 1}", Assignees = 2, DueDate = $"Jan {i + 1}, 2026", Status = "Pending", StatusBgColor = CustomColors.Yellow100, StatusTextColor = CustomColors.Yellow800, Priority = "Medium", PriorityBgColor = CustomColors.Orange50, PriorityTextColor = CustomColors.Orange600 });
            }

            int activeTasksCount = allTasks.Count(t => t.Status != "Completed");

            Panel pnlHeader = new Panel()
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = CustomColors.White,
                Padding = new Padding(20, 20, 20, 0)
            };

            Label lblYourTasks = new Label()
            {
                Text = "Your Tasks",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = CustomColors.Gray800,
                Dock = DockStyle.Left,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = true, 
            };

            Label lblActiveTasks = new Label()
            {
                Text = $"{activeTasksCount} active tasks",
                Font = new Font("Segoe UI", 9f, FontStyle.Regular),
                ForeColor = CustomColors.Gray500,
                Dock = DockStyle.Right,
                TextAlign = ContentAlignment.MiddleRight,
                AutoSize = true 
            };

            pnlHeader.Controls.Add(lblActiveTasks); pnlHeader.Controls.Add(lblYourTasks);

            Panel pnlDivider1 = new Panel() { Dock = DockStyle.Top, Height = 1, BackColor = CustomColors.Gray200 };

            TableLayoutPanel tlpTableHead = new TableLayoutPanel()
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = CustomColors.White,
                Padding = new Padding(20, 0, 20, 0),
                Margin = new Padding(0)
            };
            tlpTableHead.ColumnCount = 4;
            tlpTableHead.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            tlpTableHead.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.666f));
            tlpTableHead.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.666f));
            tlpTableHead.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 16.666f));
            tlpTableHead.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            string[] headers = { "TASK NAME", "DUE DATE", "STATUS", "PRIORITY" };
            ContentAlignment[] alignments = { ContentAlignment.MiddleLeft, ContentAlignment.MiddleCenter, ContentAlignment.MiddleCenter, ContentAlignment.MiddleCenter };

            for (int i = 0; i < headers.Length; i++) { tlpTableHead.Controls.Add(new Label() { Text = headers[i], Dock = DockStyle.Fill, TextAlign = alignments[i], Font = new Font("Segoe UI", 8f, FontStyle.Bold), ForeColor = CustomColors.Gray500 }, i, 0); }

            flpTaskList = new FlowLayoutPanel()
            {
                Dock = DockStyle.Top,
                AutoScroll = false,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = CustomColors.White,
                Padding = new Padding(0, 3, 0, 0)
            };

            foreach (var task in allTasks)
            {
                TaskRowPanel row = new TaskRowPanel(task);
                row.Width = flpTaskList.ClientSize.Width;
                flpTaskList.Controls.Add(row);
            }

            Panel pnlSpacer = new Panel() { Height = 20, BackColor = Color.White, Width = flpTaskList.ClientSize.Width };
            flpTaskList.Controls.Add(pnlSpacer);

            int totalTaskRowsHeight = allTasks.Count * 65;
            flpTaskList.Height = totalTaskRowsHeight + pnlSpacer.Height;

            pnlCard.Controls.Clear();
            pnlCard.Controls.Add(flpTaskList);
            pnlCard.Controls.Add(tlpTableHead);
            pnlCard.Controls.Add(pnlDivider1);
            pnlCard.Controls.Add(pnlHeader);

            pnlCard.Height = pnlHeader.Height + pnlDivider1.Height + tlpTableHead.Height + flpTaskList.Height;
            pnlMainContent.Controls.Add(pnlCard);

            pnlMainContent.Height = pnlCard.Height + 50;

            pnlCard.Resize += (s, e) => {
                flpTaskList.Width = pnlCard.ClientSize.Width;
                foreach (Control c in flpTaskList.Controls) { c.Width = flpTaskList.ClientSize.Width; }
            };
        }
    }
}