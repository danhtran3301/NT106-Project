//using System.Drawing;
//using System.Windows.Forms;
//using System.Drawing.Drawing2D;
//using System;
//using System.Collections.Generic;

//public static class ColorPalette
//{
//    public static readonly Color Gray50 = ColorTranslator.FromHtml("#F9FAFB");
//    public static readonly Color Gray100 = ColorTranslator.FromHtml("#F3F4F6");
//    public static readonly Color Gray200 = ColorTranslator.FromHtml("#E5E7EB");
//    public static readonly Color Gray300 = ColorTranslator.FromHtml("#D1D5DB");
//    public static readonly Color Gray400 = ColorTranslator.FromHtml("#9CA3AF");
//    public static readonly Color Gray500 = ColorTranslator.FromHtml("#6B7280");
//    public static readonly Color Gray600 = ColorTranslator.FromHtml("#4B5563");
//    public static readonly Color Gray700 = ColorTranslator.FromHtml("#374151");
//    public static readonly Color Gray800 = ColorTranslator.FromHtml("#1F2937");

//    public static readonly Color Blue500 = ColorTranslator.FromHtml("#3B82F6");
//    public static readonly Color Blue600 = ColorTranslator.FromHtml("#2563EB");
//    public static readonly Color Green500 = ColorTranslator.FromHtml("#10B981");
//    public static readonly Color Green600 = ColorTranslator.FromHtml("#059669");
//    public static readonly Color Red500 = ColorTranslator.FromHtml("#EF4444");
//    public static readonly Color Red600 = ColorTranslator.FromHtml("#DC2626");
//    public static readonly Color Red700 = ColorTranslator.FromHtml("#B91C1C");
//    public static readonly Color Yellow500 = ColorTranslator.FromHtml("#F59E0B");
//    public static readonly Color Orange500 = ColorTranslator.FromHtml("#F97316");
//    public static readonly Color Orange600 = ColorTranslator.FromHtml("#EA580C");
//    public static readonly Color Purple500 = ColorTranslator.FromHtml("#8B5CF6");
//    public static readonly Color Purple600 = ColorTranslator.FromHtml("#7C3AED");

//    public static readonly Color MenuBorderColor = Color.Black;
//}

//public class CustomFlowLayoutPanel : FlowLayoutPanel
//{
//    public CustomFlowLayoutPanel()
//    {
//        this.DoubleBuffered = true;
//        this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
//    }
//}


//public class CustomPanel : Panel
//{
//    private ContentAlignment _textAlign = ContentAlignment.MiddleCenter;
//    public ContentAlignment TextAlign
//    {
//        get { return _textAlign; }
//        set { _textAlign = value; Invalidate(); }
//    }

//    private int _borderRadius = 8;
//    public int BorderRadius
//    {
//        get { return _borderRadius; }
//        set { _borderRadius = value; this.Invalidate(); }
//    }

//    private int _borderThickness = 0;
//    public int BorderThickness
//    {
//        get { return _borderThickness; }
//        set { _borderThickness = value; this.Invalidate(); }
//    }

//    private Color _borderColor = Color.Transparent;

//    public virtual Color BorderColor
//    {
//        get { return _borderColor; }
//        set { _borderColor = value; this.Invalidate(); }
//    }

//    public override Color BackColor
//    {
//        get => base.BackColor;
//        set
//        {
//            base.BackColor = value;
//            this.Invalidate();
//        }
//    }


//    public CustomPanel()
//    {
//        this.SetStyle(ControlStyles.UserPaint |
//                      ControlStyles.AllPaintingInWmPaint |
//                      ControlStyles.DoubleBuffer |
//                      ControlStyles.OptimizedDoubleBuffer |
//                      ControlStyles.ResizeRedraw, true);
//        this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
//        this.BackColor = Color.White;
//    }

//    private TextFormatFlags GetTextFormatFlags(ContentAlignment alignment)
//    {
//        TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.WordBreak;

//        if (alignment.ToString().Contains("Top")) flags |= TextFormatFlags.Top;
//        else if (alignment.ToString().Contains("Bottom")) flags |= TextFormatFlags.Bottom;
//        else flags |= TextFormatFlags.VerticalCenter;

//        if (alignment.ToString().Contains("Left")) flags |= TextFormatFlags.Left;
//        else if (alignment.ToString().Contains("Right")) flags |= TextFormatFlags.Right;
//        else flags |= TextFormatFlags.HorizontalCenter;

//        return flags;
//    }

//    protected override void OnPaintBackground(PaintEventArgs e)
//    {
//    }

//    protected override void OnPaint(PaintEventArgs e)
//    {
//        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

//        Rectangle fillRect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
//        GraphicsPath path = GetRoundedRect(fillRect, _borderRadius);

//        e.Graphics.SetClip(path);

//        using (SolidBrush brush = new SolidBrush(base.BackColor))
//        {
//            e.Graphics.FillPath(brush, path);
//        }

//        base.OnPaint(e);

//        e.Graphics.ResetClip();

//        if (_borderThickness > 0)
//        {
//            GraphicsPath borderPath = GetRoundedRect(new Rectangle(0, 0, this.Width - 1, this.Height - 1), _borderRadius);
//            using (Pen pen = new Pen(_borderColor, _borderThickness))
//            {
//                pen.Alignment = PenAlignment.Center;
//                e.Graphics.DrawPath(pen, borderPath);
//            }
//        }

//        if (!string.IsNullOrEmpty(this.Text))
//        {
//            TextFormatFlags flags = GetTextFormatFlags(_textAlign);
//            Rectangle textRect = this.ClientRectangle;
//            TextRenderer.DrawText(e.Graphics, this.Text, this.Font, textRect, this.ForeColor, flags);
//        }
//    }

//    private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
//    {
//        GraphicsPath path = new GraphicsPath();
//        if (radius <= 0)
//        {
//            path.AddRectangle(rect);
//            return path;
//        }

//        int diameter = radius * 2;
//        Size size = new Size(diameter, diameter);
//        Rectangle arc = new Rectangle(rect.Location, size);

//        path.AddArc(arc, 180, 90);
//        arc.X = rect.Right - diameter;
//        path.AddArc(arc, 270, 90);
//        arc.Y = rect.Bottom - diameter;
//        path.AddArc(arc, 0, 90);
//        arc.X = rect.Left;
//        path.AddArc(arc, 90, 90);

//        path.CloseFigure();
//        return path;
//    }
//}

//public class CustomButton : CustomPanel
//{
//    private Color _originalBackColor;
//    private Color _hoverColor = ColorPalette.Blue600;

//    private Color _originalBorderColor;
//    private Color _hoverBorderColor = ColorPalette.MenuBorderColor;

//    public CustomButton()
//    {
//        this.Cursor = Cursors.Hand;
//        this.SetStyle(ControlStyles.Selectable, true);
//        this.TabStop = true;
//        this.Text = "CustomButton";
//        this.TextAlign = ContentAlignment.MiddleCenter;
//        _originalBackColor = this.BackColor;
//        _originalBorderColor = this.BorderColor;
//    }
//    public Color HoverColor
//    {
//        get { return _hoverColor; }
//        set { _hoverColor = value; }
//    }
//    public Color HoverBorderColor
//    {
//        get { return _hoverBorderColor; }
//        set { _hoverBorderColor = value; }
//    }

//    public override Color BackColor
//    {
//        get => base.BackColor;
//        set { base.BackColor = value; _originalBackColor = value; }
//    }

//    public override Color BorderColor
//    {
//        get => base.BorderColor;
//        set { base.BorderColor = value; _originalBorderColor = value; }
//    }

//    protected override void OnMouseEnter(EventArgs e)
//    {
//        base.OnMouseEnter(e);
//        this.BackColor = _hoverColor;
//        this.BorderColor = _hoverBorderColor;
//    }

//    protected override void OnMouseLeave(EventArgs e)
//    {
//        base.OnMouseLeave(e);
//        this.BackColor = _originalBackColor;
//        this.BorderColor = _originalBorderColor;
//    }
//}

//public class CustomTextBoxWrapper : CustomPanel
//{
//    private RichTextBox _textBox;

//    public CustomTextBoxWrapper()
//    {
//        this.BorderRadius = 6;
//        this.BorderThickness = 1;
//        this.BorderColor = ColorPalette.Gray300;
//        this.BackColor = Color.White;
//        this.Padding = new Padding(8);

//        _textBox = new RichTextBox
//        {
//            Dock = DockStyle.Fill,
//            BorderStyle = BorderStyle.None,
//            Font = new Font("Segoe UI", 10F, FontStyle.Regular),
//            BackColor = Color.White,
//            Text = "Add a comment..."
//        };

//        this.Controls.Add(_textBox);
//    }

//    public string TextBoxText
//    {
//        get { return _textBox.Text; }
//        set { _textBox.Text = value; }
//    }
//}
//public class TaskList : Form
//{
//    private readonly Font FontRegular = new Font("Segoe UI", 10F, FontStyle.Regular);
//    private readonly Font FontBold = new Font("Segoe UI", 10F, FontStyle.Bold);
//    private readonly Font FontHeaderTitle = new Font("Segoe UI", 14F, FontStyle.Bold);
//    private readonly Color HeaderIconColor = ColorPalette.Gray600;

//    public TaskList()
//    {
//        SetupLayout();
//    }

//    private void SetupLayout()
//    {
//        this.Text = "My Tasks";
//        this.BackColor = ColorPalette.Gray100;
//        this.WindowState = FormWindowState.Maximized;
//        this.Padding = new Padding(0);
//        this.MinimumSize = new Size(1024, 600);

//        Panel rootPanel = new Panel
//        {
//            Dock = DockStyle.Fill,
//            BackColor = Color.White,
//            Padding = new Padding(0)
//        };
//        this.Controls.Add(rootPanel);

//        Control headerBar = CreateHeaderBar();
//        headerBar.Dock = DockStyle.Top;
//        rootPanel.Controls.Add(headerBar);

//        TableLayoutPanel mainLayout = new TableLayoutPanel
//        {
//            Dock = DockStyle.Fill,
//            ColumnCount = 2,
//            RowStyles = { new RowStyle(SizeType.Percent, 100F) },
//            ColumnStyles =
//            {
//                new ColumnStyle(SizeType.Absolute, 300F),
//                new ColumnStyle(SizeType.Percent, 100F)
//            },
//            CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
//            Padding = new Padding(0),
//            Margin = new Padding(0),
//            BackColor = Color.Transparent
//        };
//        rootPanel.Controls.Add(mainLayout);

//        mainLayout.Controls.Add(CreateLeftMenu(), 0, 0);
//        mainLayout.Controls.Add(CreateTaskListContent(), 1, 0);
//    }

//    private Control CreateHeaderBar()
//    {
//        Panel headerWrapper = new Panel
//        {
//            Dock = DockStyle.Top,
//            Height = 61,
//            BackColor = Color.White,
//            Margin = new Padding(0)
//        };

//        TableLayoutPanel headerTable = new TableLayoutPanel
//        {
//            Dock = DockStyle.Top,
//            Height = 60,
//            BackColor = Color.White,
//            ColumnCount = 3,
//            ColumnStyles =
//            {
//                new ColumnStyle(SizeType.AutoSize),
//                new ColumnStyle(SizeType.Percent, 100F),
//                new ColumnStyle(SizeType.AutoSize)
//            },
//            RowCount = 1,
//            RowStyles = { new RowStyle(SizeType.Percent, 100F) },
//            Padding = new Padding(16, 10, 16, 10)
//        };

//        FlowLayoutPanel leftContainer = new FlowLayoutPanel
//        {
//            FlowDirection = FlowDirection.LeftToRight,
//            AutoSize = true,
//            Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom,
//            Margin = new Padding(0)
//        };
//        CustomButton arrowButton = new CustomButton
//        {
//            Text = "←",
//            Font = new Font("Segoe UI Emoji", 16F),
//            ForeColor = HeaderIconColor,
//            BackColor = Color.White,
//            HoverColor = ColorPalette.Gray200,
//            BorderRadius = 4,
//            Width = 40,
//            Height = 40,
//            TextAlign = ContentAlignment.MiddleCenter,
//            Margin = new Padding(0)
//        };
//        arrowButton.Click += (sender, e) => { MessageBox.Show("Quay lại..."); };
//        leftContainer.Controls.Add(arrowButton);

//        Label titleLabel = new Label
//        {
//            Text = "My Tasks",
//            Font = FontHeaderTitle,
//            ForeColor = ColorPalette.Gray800,
//            AutoSize = true,
//            Dock = DockStyle.Fill,
//            TextAlign = ContentAlignment.MiddleLeft,
//            Margin = new Padding(8, 0, 0, 0)
//        };
//        leftContainer.Controls.Add(titleLabel);
//        headerTable.Controls.Add(leftContainer, 0, 0);

//        CustomButton closeButton = new CustomButton
//        {
//            Text = "✕",
//            Font = new Font("Segoe UI Emoji", 14F, FontStyle.Bold),
//            ForeColor = HeaderIconColor,
//            BackColor = Color.White,
//            HoverColor = ColorPalette.Gray200,
//            BorderRadius = 4,
//            Width = 40,
//            Height = 40,
//            TextAlign = ContentAlignment.MiddleCenter,
//            Margin = new Padding(0)
//        };
//        closeButton.Click += (sender, e) => { this.Close(); };
//        headerTable.Controls.Add(closeButton, 2, 0);

//        Panel separator = new Panel
//        {
//            Dock = DockStyle.Bottom,
//            Height = 1,
//            BackColor = ColorPalette.Gray200
//        };

//        headerTable.Dock = DockStyle.Fill;
//        headerWrapper.Controls.Add(headerTable);
//        headerWrapper.Controls.Add(separator);

//        return headerWrapper;
//    }

//    private Control CreateLeftMenu()
//    {
//        Panel menuWrapper = new Panel
//        {
//            Dock = DockStyle.Fill,
//            BackColor = Color.White,
//            Padding = new Padding(0),
//            Margin = new Padding(0)
//        };
//        FlowLayoutPanel menuPanel = new CustomFlowLayoutPanel
//        {
//            Dock = DockStyle.Fill,
//            FlowDirection = FlowDirection.TopDown,
//            WrapContents = false,
//            AutoScroll = true,
//            Padding = new Padding(24, 20, 24, 16),
//            BackColor = Color.White,
//            Margin = new Padding(0)
//        };
//        Panel separator = new Panel
//        {
//            Dock = DockStyle.Right,
//            Width = 1,
//            BackColor = ColorPalette.Gray200
//        };
//        menuWrapper.Controls.Add(menuPanel);
//        menuWrapper.Controls.Add(separator);

//        int buttonHeight = 40;

//        menuPanel.Controls.Add(CreateMenuHeader("ACCOUNT", "👤", new Padding(0, 0, 0, 16)));

//        Label projectsTitle = new Label
//        {
//            Text = "PROJECTS",
//            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
//            ForeColor = ColorPalette.Gray700,
//            AutoSize = true,
//            Margin = new Padding(0, 0, 0, 16)
//        };
//        menuPanel.Controls.Add(projectsTitle);

//        menuPanel.Controls.Add(CreateMenuButton("Your Task", ColorPalette.Blue500, Color.White, buttonHeight, ColorPalette.Blue600, 1, ColorPalette.MenuBorderColor));
//        menuPanel.Controls.Add(CreateMenuButton("Group", ColorPalette.Green500, Color.White, buttonHeight, ColorPalette.Green600, 1, ColorPalette.MenuBorderColor));
//        menuPanel.Controls.Add(CreateMenuButton("New task", ColorPalette.Orange500, Color.White, buttonHeight, ColorPalette.Orange600, 1, ColorPalette.MenuBorderColor));
//        menuPanel.Controls.Add(CreateMenuButton("Submit task", ColorPalette.Purple500, Color.White, buttonHeight, ColorPalette.Purple600, 1, ColorPalette.MenuBorderColor));

//        Label calendarTitle = new Label
//        {
//            Text = "Calendar",
//            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
//            ForeColor = ColorPalette.Gray800,
//            AutoSize = true,
//            Margin = new Padding(0, 30, 0, 8)
//        };
//        menuPanel.Controls.Add(calendarTitle);
//        Control customCalendar = CreateCustomCalendarControl(new DateTime(2025, 11, 16), DateTime.Today);
//        menuPanel.Controls.Add(customCalendar);

//        return menuWrapper;
//    }

//    private Control CreateMenuHeader(string text, string icon, Padding margin)
//    {
//        FlowLayoutPanel header = new FlowLayoutPanel
//        {
//            FlowDirection = FlowDirection.LeftToRight,
//            AutoSize = true,
//            Margin = margin
//        };
//        header.Controls.Add(new Label
//        {
//            Text = icon,
//            Font = new Font("Segoe UI Emoji", 12F),
//            AutoSize = true,
//            Margin = new Padding(0, 0, 4, 0)
//        });
//        header.Controls.Add(new Label
//        {
//            Text = text,
//            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
//            ForeColor = ColorPalette.Gray700,
//            AutoSize = true
//        });
//        return header;
//    }

//    private CustomButton CreateMenuButton(string text, Color backColor, Color foreColor, int height, Color? hoverColor = null, int borderThickness = 0, Color? borderColor = null)
//    {
//        return new CustomButton
//        {
//            Text = text,
//            BackColor = backColor,
//            ForeColor = foreColor,
//            HoverColor = hoverColor ?? ColorPalette.Blue600,
//            BorderRadius = 8,
//            Width = 252,
//            Height = height,
//            Font = FontBold,
//            TextAlign = ContentAlignment.MiddleCenter,
//            Margin = new Padding(0, 0, 0, 12),
//            BorderThickness = borderThickness,
//            BorderColor = borderColor ?? Color.Transparent
//        };
//    }

//    private Control CreateCustomCalendarControl(DateTime selectionDate, DateTime today)
//    {
//        MonthCalendar monthCalendar = new MonthCalendar
//        {
//            BackColor = Color.White,
//            ForeColor = ColorPalette.Gray700,
//            Font = FontRegular,
//            SelectionStart = selectionDate,
//            SelectionEnd = selectionDate,
//            ShowTodayCircle = false,
//            TitleBackColor = Color.White,
//            TitleForeColor = ColorPalette.Gray800,
//            TrailingForeColor = ColorPalette.Gray300,
//            CalendarDimensions = new Size(1, 1),
//            TodayDate = today
//        };

//        monthCalendar.Width = 252;
//        monthCalendar.Height = 180;
//        monthCalendar.Margin = new Padding(0);

//        return monthCalendar;
//    }

//    private Control CreateTaskListContent()
//    {
//        CustomFlowLayoutPanel contentPanel = new CustomFlowLayoutPanel
//        {
//            Dock = DockStyle.Fill,
//            FlowDirection = FlowDirection.TopDown,
//            WrapContents = false,
//            AutoScroll = true,
//            Padding = new Padding(32, 20, 32, 24),
//            BackColor = ColorPalette.Gray100,
//        };

//        var tasks = new[]
//        {
//            new { Name = "Design a new dashboard for the mobile app", Assignees = 3, DueDate = "Dec 15, 2025", Status = "In Progress", StatusColor = ColorPalette.Blue500, Priority = "High", PriorityColor = ColorPalette.Red500 },
//            new { Name = "Database Schema (CSDL) Deadline", Assignees = 1, DueDate = "Nov 01, 2025", Status = "Pending", StatusColor = ColorPalette.Yellow500, Priority = "Medium", PriorityColor = ColorPalette.Orange500 },
//            new { Name = "Philosophy Theory Preparation", Assignees = 2, DueDate = "Dec 21, 2025", Status = "Completed", StatusColor = ColorPalette.Green500, Priority = "Low", PriorityColor = ColorPalette.Green500 },
//            new { Name = "Submit Q4 Report", Assignees = 1, DueDate = "Nov 30, 2025", Status = "Pending", StatusColor = ColorPalette.Yellow500, Priority = "High", PriorityColor = ColorPalette.Red500 },
//            new { Name = "Team Building Event Planning", Assignees = 4, DueDate = "Dec 05, 2025", Status = "In Progress", StatusColor = ColorPalette.Blue500, Priority = "Medium", PriorityColor = ColorPalette.Orange500 },
//            new { Name = "Task 6: Review design docs", Assignees = 1, DueDate = "Dec 10, 2025", Status = "In Progress", StatusColor = ColorPalette.Blue500, Priority = "Medium", PriorityColor = ColorPalette.Orange500 },
//            new { Name = "Task 7: Prepare presentation slides", Assignees = 2, DueDate = "Dec 12, 2025", Status = "Pending", StatusColor = ColorPalette.Yellow500, Priority = "Low", PriorityColor = ColorPalette.Green500 },
//            new { Name = "Task 8: Final API integration", Assignees = 3, DueDate = "Dec 18, 2025", Status = "In Progress", StatusColor = ColorPalette.Blue500, Priority = "High", PriorityColor = ColorPalette.Red500 },
//            new { Name = "Task 9: Database backup", Assignees = 1, DueDate = "Dec 25, 2025", Status = "Completed", StatusColor = ColorPalette.Green500, Priority = "Low", PriorityColor = ColorPalette.Green500 },
//            new { Name = "Task 10: Holiday Planning", Assignees = 4, DueDate = "Jan 01, 2026", Status = "Pending", StatusColor = ColorPalette.Yellow500, Priority = "Medium", PriorityColor = ColorPalette.Orange500 },
//        };
//        int activeTaskCount = tasks.Length;

//        TableLayoutPanel headerLayout = new TableLayoutPanel
//        {
//            ColumnCount = 2,
//            ColumnStyles =
//            {
//                new ColumnStyle(SizeType.Percent, 100F),
//                new ColumnStyle(SizeType.Absolute, 120F)
//            },
//            RowCount = 1,
//            Margin = new Padding(0, 0, 0, 24),
//            BackColor = Color.Transparent,
//            Anchor = AnchorStyles.Left | AnchorStyles.Right
//        };
//        headerLayout.SizeChanged += (sender, e) =>
//        {
//            if (headerLayout.Parent is FlowLayoutPanel parent)
//            {
//                headerLayout.Width = parent.ClientSize.Width - parent.Padding.Left - parent.Padding.Right;
//            }
//        };

//        Label title = new Label
//        {
//            Text = "Your Tasks",
//            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
//            ForeColor = ColorPalette.Gray800,
//            AutoSize = true,
//            Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom
//        };
//        headerLayout.Controls.Add(title, 0, 0);

//        Label activeTasks = new Label
//        {
//            Text = $"{activeTaskCount} active tasks",
//            Font = FontRegular,
//            ForeColor = ColorPalette.Gray600,
//            AutoSize = true,
//            Anchor = AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom,
//            TextAlign = ContentAlignment.BottomRight,
//        };
//        headerLayout.Controls.Add(activeTasks, 1, 0);
//        contentPanel.Controls.Add(headerLayout);

//        TableLayoutPanel columnHeader = new TableLayoutPanel
//        {
//            ColumnCount = 4,
//            ColumnStyles =
//            {
//                new ColumnStyle(SizeType.Percent, 50F),
//                new ColumnStyle(SizeType.Percent, 20F),
//                new ColumnStyle(SizeType.Percent, 15F),
//                new ColumnStyle(SizeType.Percent, 15F)
//            },
//            RowCount = 1,
//            Margin = new Padding(0, 0, 0, 10),
//            BackColor = Color.Transparent,
//            Padding = new Padding(12, 0, 12, 0),
//            Anchor = AnchorStyles.Left | AnchorStyles.Right
//        };
//        columnHeader.SizeChanged += (sender, e) => {
//            if (columnHeader.Parent is FlowLayoutPanel parent)
//            {
//                columnHeader.Width = parent.ClientSize.Width - parent.Padding.Left - parent.Padding.Right;
//            }
//        };
//        Action<string, int> AddHeaderLabel = (text, col) =>
//        {
//            Label lbl = new Label
//            {
//                Text = text,
//                Font = FontBold,
//                ForeColor = ColorPalette.Gray500,
//                Dock = DockStyle.Fill,
//                TextAlign = ContentAlignment.MiddleLeft,
//            };
//            columnHeader.Controls.Add(lbl, col, 0);
//        };
//        AddHeaderLabel("TASK NAME", 0);
//        AddHeaderLabel("DUE DATE", 1);
//        AddHeaderLabel("STATUS", 2);
//        AddHeaderLabel("PRIORITY", 3);
//        contentPanel.Controls.Add(columnHeader);

//        foreach (var task in tasks)
//        {
//            Control taskItem = CreateTaskListItem(task.Name, task.Assignees, task.DueDate, task.Status, task.StatusColor, task.Priority, task.PriorityColor);

//            taskItem.Anchor = AnchorStyles.Left | AnchorStyles.Right;
//            taskItem.SizeChanged += (sender, e) => {
//                if (taskItem.Parent is FlowLayoutPanel parent)
//                {
//                    taskItem.Width = parent.ClientSize.Width - parent.Padding.Left - parent.Padding.Right;
//                }
//            };

//            contentPanel.Controls.Add(taskItem);
//        }
//        return contentPanel;
//    }

//    private Control CreateTaskListItem(string name, int assignees, string dueDate, string status, Color statusColor, string priority, Color priorityColor)
//    {
//        CustomPanel taskItemPanel = new CustomPanel
//        {
//            Dock = DockStyle.Top,
//            Height = 80,
//            BackColor = Color.White,
//            BorderRadius = 12,
//            BorderThickness = 1,
//            BorderColor = ColorPalette.Gray200,
//            Margin = new Padding(0, 0, 0, 12),
//            Cursor = Cursors.Hand,
//        };

//        TableLayoutPanel taskLayout = new TableLayoutPanel
//        {
//            Dock = DockStyle.Fill,
//            ColumnCount = 4,
//            ColumnStyles =
//            {
//                new ColumnStyle(SizeType.Percent, 50F),
//                new ColumnStyle(SizeType.Percent, 20F),
//                new ColumnStyle(SizeType.Percent, 15F),
//                new ColumnStyle(SizeType.Percent, 15F)
//            },
//            RowCount = 1,
//            Padding = new Padding(16, 8, 16, 8),
//            Margin = new Padding(0),
//            BackColor = Color.Transparent,
//        };

//        FlowLayoutPanel nameFlow = new FlowLayoutPanel
//        {
//            FlowDirection = FlowDirection.TopDown,
//            Dock = DockStyle.Fill,
//            WrapContents = false,
//            Padding = new Padding(0),
//            Margin = new Padding(0)
//        };
//        nameFlow.Controls.Add(new Label { Text = name, Font = FontBold, ForeColor = ColorPalette.Gray800, AutoSize = true, MaximumSize = new Size(350, 0) });
//        nameFlow.Controls.Add(new Label { Text = $"{assignees} assignees", Font = FontRegular, ForeColor = ColorPalette.Gray500, AutoSize = true, Margin = new Padding(0, 4, 0, 0) });
//        taskLayout.Controls.Add(nameFlow, 0, 0);

//        taskLayout.Controls.Add(new Label { Text = dueDate, Font = FontRegular, ForeColor = ColorPalette.Gray700, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 1, 0);

//        Panel statusWrapper = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0) };
//        CustomPanel statusTag = CreateTag(status, statusColor);
//        statusWrapper.Controls.Add(statusTag);
//        statusTag.Anchor = AnchorStyles.None;
//        taskLayout.Controls.Add(statusWrapper, 2, 0);

//        Panel priorityWrapper = new Panel { Dock = DockStyle.Fill, Margin = new Padding(0) };
//        CustomPanel priorityTag = CreateTag(priority, priorityColor);
//        priorityWrapper.Controls.Add(priorityTag);
//        priorityTag.Anchor = AnchorStyles.None;
//        taskLayout.Controls.Add(priorityWrapper, 3, 0);

//        taskItemPanel.Controls.Add(taskLayout);
//        return taskItemPanel;
//    }

//    private CustomPanel CreateTag(string text, Color backColor)
//    {
//        Color tagForeColor = Color.White;
//        if (backColor == ColorPalette.Yellow500 || backColor == ColorPalette.Green500)
//        {
//            tagForeColor = ColorPalette.Gray800;
//        }

//        return new CustomPanel
//        {
//            Text = text,
//            BackColor = backColor,
//            ForeColor = tagForeColor,
//            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),

//            BorderRadius = 4,
//            Height = 24,

//            TextAlign = ContentAlignment.MiddleCenter,
//            Anchor = AnchorStyles.None,

//            AutoSize = true,
//            Padding = new Padding(8, 2, 8, 2),
//            Margin = new Padding(0)
//        };
//    }
//}