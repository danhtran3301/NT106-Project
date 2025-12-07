//using System;
//using System.Drawing;
//using System.Drawing.Drawing2D;
//using System.Globalization;
//using System.Windows.Forms;

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
//    public static readonly Color Purple500 = ColorTranslator.FromHtml("#8B5CF6");
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
//    public Color BorderColor
//    {
//        get { return _borderColor; }
//        set { _borderColor = value; this.Invalidate(); }
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

//    protected override void OnPaint(PaintEventArgs e)
//    {
//        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

//        Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
//        GraphicsPath path = GetRoundedRect(rect, _borderRadius);

//        using (SolidBrush brush = new SolidBrush(this.BackColor))
//        {
//            e.Graphics.FillPath(brush, path);
//        }

//        if (_borderThickness > 0)
//        {
//            using (Pen pen = new Pen(_borderColor, _borderThickness))
//            {
//                e.Graphics.DrawPath(pen, path);
//            }
//        }

//        this.Region = new Region(path);

//        if (!string.IsNullOrEmpty(this.Text))
//        {
//            TextFormatFlags flags = GetTextFormatFlags(_textAlign);
//            Rectangle textRect = this.ClientRectangle;
//            TextRenderer.DrawText(e.Graphics, this.Text, this.Font, textRect, this.ForeColor, flags);
//        }

//        base.OnPaint(e);
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

///// <summary>
///// Custom Button with support for BorderRadius and Hover effect.
///// Inherits Text drawing from CustomPanel.
///// </summary>
//public class CustomButton : CustomPanel
//{
//    private Color _originalBackColor;
//    private Color _hoverColor = ColorPalette.Blue600;

//    public CustomButton()
//    {
//        this.Cursor = Cursors.Hand;
//        this.SetStyle(ControlStyles.Selectable, true);
//        this.TabStop = true;
//        this.Text = "CustomButton";
//        this.TextAlign = ContentAlignment.MiddleCenter;
//        _originalBackColor = this.BackColor;
//    }

//    public Color HoverColor
//    {
//        get { return _hoverColor; }
//        set { _hoverColor = value; }
//    }

//    public override Color BackColor
//    {
//        get => base.BackColor;
//        set { base.BackColor = value; _originalBackColor = value; }
//    }

//    protected override void OnMouseEnter(EventArgs e)
//    {
//        base.OnMouseEnter(e);
//        this.BackColor = _hoverColor;
//    }

//    protected override void OnMouseLeave(EventArgs e)
//    {
//        base.OnMouseLeave(e);
//        this.BackColor = _originalBackColor;
//    }
//}

///// <summary>
///// A wrapper to style a standard TextBox/RichTextBox (for Comments).
///// </summary>
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


//public class FormTaskDetail : Form
//{
//    private readonly Font FontRegular = new Font("Segoe UI", 10F, FontStyle.Regular);
//    private readonly Font FontBold = new Font("Segoe UI", 10F, FontStyle.Bold);
//    private readonly Font FontTitle = new Font("Segoe UI", 16F, FontStyle.Bold);
//    private readonly Font FontHeaderTitle = new Font("Segoe UI", 12F, FontStyle.Bold);
//    private readonly Color HeaderIconColor = ColorPalette.Gray600;

//    public FormTaskDetail()
//    {
//        SetupLayout();
//    }

//    private Control CreateHeaderBar()
//    {
//        TableLayoutPanel headerPanel = new TableLayoutPanel
//        {
//            Dock = DockStyle.Top,
//            Height = 80, 
//            BackColor = Color.White,
//            ColumnCount = 3,
//            RowCount = 1,
//            ColumnStyles =
//        {
//            new ColumnStyle(SizeType.Absolute, 60F),
//            new ColumnStyle(SizeType.Percent, 100F),
//            new ColumnStyle(SizeType.Absolute, 100F)
//        },
//            RowStyles = { new RowStyle(SizeType.Percent, 100F) },
//            Padding = new Padding(16, 0, 16, 0)
//        };

//        FlowLayoutPanel leftFlow = new FlowLayoutPanel
//        {
//            Dock = DockStyle.Fill,
//            FlowDirection = FlowDirection.LeftToRight,
//            WrapContents = false,
//            Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom,
//            Margin = new Padding(0),
//            AutoSize = false,
//            Height = 60
//        };

//        CustomButton arrowButton = new CustomButton
//        {
//            Text = "←",
//            Font = new Font("Segoe UI Emoji", 16F),
//            ForeColor = HeaderIconColor,
//            BackColor = Color.Transparent,
//            HoverColor = ColorPalette.Gray200,
//            BorderRadius = 4,
//            Width = 40,
//            Height = 40,
//            TextAlign = ContentAlignment.MiddleLeft,
//            Margin = new Padding(0) 
//        };
//        arrowButton.Click += (sender, e) => { MessageBox.Show("Quay lại trang trước..."); };
//        leftFlow.Controls.Add(arrowButton);
//        headerPanel.Controls.Add(leftFlow, 0, 0);
//        Label titleLabel = new Label
//        {
//            Text = "Task Details",
//            Font = FontHeaderTitle,
//            ForeColor = ColorPalette.Gray800,
//            AutoSize = false,
//            Dock = DockStyle.Fill,
//            TextAlign = ContentAlignment.MiddleLeft,
//            Margin = new Padding(0)
//        };
//        leftFlow.Controls.Add(titleLabel);

//        headerPanel.Controls.Add(leftFlow, 0, 0);
//        headerPanel.Controls.Add(titleLabel, 1, 0);


//        FlowLayoutPanel rightFlow = new FlowLayoutPanel
//        {
//            Dock = DockStyle.Fill,
//            FlowDirection = FlowDirection.RightToLeft,
//            WrapContents = false,
//            AutoSize = false,
//            Height = 60,
//            Anchor = AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom,
//            Margin = new Padding(0)
//        };
//        int buttonMarginTop = 40;
//        int buttonWidthHeight = 40;
//        CustomButton closeButton = new CustomButton
//        {
//            Text = "✕",
//            Font = new Font("Segoe UI Emoji", 14F, FontStyle.Bold),
//            ForeColor = HeaderIconColor,
//            BackColor = Color.Transparent,
//            HoverColor = ColorPalette.Gray200,
//            BorderRadius = 4,
//            Width = 40,
//            Height = 40,
//            TextAlign = ContentAlignment.MiddleLeft,
//            Margin = new Padding( 0) 
//        };
//        closeButton.Click += (sender, e) => { this.Close(); };
//        rightFlow.Controls.Add(closeButton);

//        CustomButton optionsButton = new CustomButton
//        {
//            Text = "...",
//            Font = new Font("Segoe UI Emoji", 14F, FontStyle.Bold),
//            ForeColor = HeaderIconColor,
//            BackColor = Color.Transparent,
//            HoverColor = ColorPalette.Gray200,
//            BorderRadius = 4,
//            Width = 40,
//            Height = 40,
//            TextAlign = ContentAlignment.MiddleLeft,
//            Margin = new Padding(0) 
//        };
//        rightFlow.Controls.Add(optionsButton);

//        headerPanel.Controls.Add(rightFlow, 2, 0);

//        Panel separator = new Panel
//        {
//            Dock = DockStyle.Bottom,
//            Height = 1,
//            BackColor = ColorPalette.Gray200
//        };
//        headerPanel.Controls.Add(separator);


//        return headerPanel;
//    }

//    private void SetupLayout()
//    {
//        this.Text = "Task Details";
//        this.BackColor = ColorPalette.Gray100;
//        this.WindowState = FormWindowState.Maximized;
//        this.Padding = new Padding(0);
//        this.MinimumSize = new Size(800, 600);

//        Panel rootPanel = new Panel
//        {
//            Dock = DockStyle.Fill,
//            BackColor = Color.White,
//            Padding = new Padding(0)
//        };
//        this.Controls.Add(rootPanel);

//        rootPanel.Controls.Add(CreateHeaderBar());

//        TableLayoutPanel mainLayout = new TableLayoutPanel
//        {
//            Dock = DockStyle.Fill, 
//            ColumnCount = 3,
//            RowCount = 1,
//            ColumnStyles =
//            {
//                new ColumnStyle(SizeType.Absolute, 480F),
//                new ColumnStyle(SizeType.Percent, 100F),
//                new ColumnStyle(SizeType.Absolute, 480F)
//            },
//            RowStyles = { new RowStyle(SizeType.Percent, 100F) },
//            Padding = new Padding(0),
//            Margin = new Padding(0),
//            BackColor = Color.White
//        };
//        rootPanel.Controls.Add(mainLayout); // Thêm vào rootPanel

//        mainLayout.Controls.Add(CreateLeftMenu(), 0, 0);
//        mainLayout.Controls.Add(CreateCenterContent(), 1, 0);
//        mainLayout.Controls.Add(CreateRightSidebar(), 2, 0);

//        this.LayoutMdi(MdiLayout.ArrangeIcons);
//    }



//    private Control CreateLeftMenu()
//    {
//        FlowLayoutPanel menuPanel = new FlowLayoutPanel
//        {
//            Dock = DockStyle.Fill,
//            FlowDirection = FlowDirection.TopDown,
//            WrapContents = false,
//            AutoScroll = false,
//            Padding = new Padding(16, 130, 16, 16),
//            BackColor = Color.White,
//            BorderStyle = BorderStyle.None,
//            Margin = new Padding(0, 0, 1, 0)
//        };


//        int buttonWidth = 400;
//        int buttonHeight = 60; 

//        FlowLayoutPanel accountHeader = new FlowLayoutPanel
//        {
//            FlowDirection = FlowDirection.LeftToRight,
//            AutoSize = true,
//            Width = buttonWidth,
//            Margin = new Padding(0, 0, 0, 30)
//        };
//        accountHeader.Controls.Add(new Label
//        {
//            Text = "👤",
//            Font = new Font("Segoe UI Emoji", 12F),
//            AutoSize = true,
//            Margin = new Padding(0, 0, 30, 0)
//        });
//        accountHeader.Controls.Add(new Label
//        {
//            Text = "ACCOUNT",
//            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
//            ForeColor = ColorPalette.Gray700,
//            AutoSize = true
//        });
//        menuPanel.Controls.Add(accountHeader);


//        menuPanel.Controls.Add(CreateMenuButton("Your Task", ColorPalette.Blue500, Color.White, buttonWidth, buttonHeight));

//        menuPanel.Controls.Add(CreateMenuButton("Group", ColorPalette.Green500, Color.White, buttonWidth, buttonHeight));

//        menuPanel.Controls.Add(CreateMenuButton("New task", ColorPalette.Orange500, Color.White, buttonWidth, buttonHeight));
        
//        Color submitColor = ColorPalette.Purple500;

//        menuPanel.Controls.Add(CreateMenuButton("Submit task", submitColor, Color.White, buttonWidth, buttonHeight,
//            Color.FromArgb(200, submitColor)));


//        MonthCalendar monthCalendar = new MonthCalendar
//        {
//            BackColor = Color.White,
//            ForeColor = ColorPalette.Gray700,
//            Font = FontRegular,

//            SelectionStart = new DateTime(2025, 11, 16),
//            SelectionEnd = new DateTime(2025, 11, 16),


//            ShowTodayCircle = false,
//            TitleBackColor = Color.White,
//            TitleForeColor = ColorPalette.Gray800,
//            TrailingForeColor = ColorPalette.Gray300,

//            CalendarDimensions = new Size(1, 1)
//        };

//        monthCalendar.Width = 248;
//        monthCalendar.Height = 180;
//        monthCalendar.Margin = new Padding(0, 30, 0, 0);

//        menuPanel.Controls.Add(monthCalendar);

//        return menuPanel;
//    }
//    private CustomButton CreateMenuButton(string text, Color backColor, Color foreColor, int width, int height, Color? hoverColor)
//    {
//        return new CustomButton
//        {
//            Text = text,
//            BackColor = backColor,
//            ForeColor = foreColor,

//            HoverColor = hoverColor ?? ColorPalette.Blue600,
//            BorderRadius = 8,
//            Width = width,
//            Height = height,
//            Font = FontBold,
//            TextAlign = ContentAlignment.MiddleCenter,
//            Margin = new Padding(0, 0, 0, 12)
//        };
//        }
//    private CustomButton CreateMenuButton(string text, Color backColor, Color foreColor, int width, int height)
//    {
//        return CreateMenuButton(text, backColor, foreColor, width, height, null);
//    }

//    private Control CreateCenterContent()
//    {
//        FlowLayoutPanel contentPanel = new FlowLayoutPanel
//        {
//            Dock = DockStyle.Fill,
//            FlowDirection = FlowDirection.TopDown,
//            WrapContents = false,
//            AutoScroll = true,
//            Padding = new Padding(32, 130, 32, 24),
//            BackColor = Color.White,
//        };

//        TableLayoutPanel headerLayout = new TableLayoutPanel
//        {
//            Width = 800,
//            AutoSize = true,
//            ColumnCount = 2,
//            RowCount = 1,
//            ColumnStyles =
//            {
//                new ColumnStyle(SizeType.Percent, 100F),
//                new ColumnStyle(SizeType.AutoSize)
//            },
//            Margin = new Padding(0, 0, 0, 20),
//            BackColor = Color.Transparent
//        };

//        Label title = new Label
//        {
//            Text = "Design a new dashboard for the mobile app",
//            Font = FontTitle,
//            ForeColor = ColorPalette.Gray800,
//            AutoSize = true,
//            Anchor = AnchorStyles.Left
//        };
//        headerLayout.Controls.Add(title, 0, 0);

//        CustomPanel status = new CustomPanel
//        {
//            Text = "In Progress",
//            BackColor = ColorPalette.Blue500,
//            ForeColor = Color.White,
//            Font = FontBold,
//            BorderRadius = 6,
//            Width = 150, 
//            Height = 50, 
//            TextAlign = ContentAlignment.MiddleCenter,
//            Anchor = AnchorStyles.Right
//        };
//        headerLayout.Controls.Add(status, 1, 0);

//        contentPanel.Controls.Add(headerLayout);

//        Label description = new Label
//        {
//            Text = "The current dashboard design is outdated and doesn't provide a good user experience. We need to create a new design that is modern, intuitive, and visually appealing. The new design should include a clear information hierarchy, data visualizations, and easy navigation.",
//            Font = FontRegular,
//            ForeColor = ColorPalette.Gray600,
//            MaximumSize = new Size(800, 0),
//            AutoSize = true,
//            Margin = new Padding(0, 0, 0, 20)
//        };
//        contentPanel.Controls.Add(description);


//        Label keyReqTitle = new Label
//        {
//            Text = "Key requirements:",
//            Font = FontBold,
//            ForeColor = ColorPalette.Gray800,
//            AutoSize = true,
//            Margin = new Padding(0, 0, 0, 8)
//        };
//        contentPanel.Controls.Add(keyReqTitle);

//        string[] requirements =
//        {
//            "User-friendly interface with a clean layout.",
//            "Interactive charts and graphs for data visualization.",
//            "Customizable widgets for personalization.",
//            "Responsive design for various screen sizes."
//        };

//        foreach (var req in requirements)
//        {
//            Label reqLabel = new Label
//            {
//                Text = "• " + req,
//                Font = FontRegular,
//                ForeColor = ColorPalette.Gray600,
//                AutoSize = true,
//                Margin = new Padding(0, 0, 0, 4)
//            };
//            contentPanel.Controls.Add(reqLabel);
//        }

//        Label commentsTitle = new Label
//        {
//            Text = "Comments",
//            Font = FontTitle,
//            ForeColor = ColorPalette.Gray800,
//            AutoSize = true,
//            Margin = new Padding(0, 30, 0, 16)
//        };
//        contentPanel.Controls.Add(commentsTitle);

//        CustomTextBoxWrapper newCommentBox = new CustomTextBoxWrapper
//        {
//            Width = 800,
//            Height = 100,
//            Margin = new Padding(0, 0, 0, 10)
//        };
//        contentPanel.Controls.Add(newCommentBox);

//        CustomButton postButton = new CustomButton
//        {
//            Text = "Post",
//            BackColor = ColorPalette.Blue500,
//            ForeColor = Color.White,
//            HoverColor = ColorPalette.Blue600,
//            BorderRadius = 6,
//            Width = 100,
//            Height = 40,
//            Font = FontBold,
//            TextAlign = ContentAlignment.MiddleCenter,
//            Margin = new Padding(0, 0, 0, 24)
//        };
//        contentPanel.Controls.Add(postButton);

//        contentPanel.Controls.Add(CreateComment("Diana", "Can we make sure the dark mode colors are consistent with the web version?", "3 hours ago"));
//        contentPanel.Controls.Add(CreateComment("Charlie", "Great progress! I've attached the latest wireframes.", "1 hour ago"));


//        Panel spacer = new Panel { Height = 50, Width = 1, BackColor = Color.Transparent };
//        contentPanel.Controls.Add(spacer);

//        return contentPanel;
//    }

//    private Control CreateDetailRowControl(string label, Control control, int rowWidth)
//    {
//        FlowLayoutPanel row = new FlowLayoutPanel
//        {
//            FlowDirection = FlowDirection.LeftToRight,
//            WrapContents = false,
//            AutoSize = true,
//            Width = rowWidth,
//            Margin = new Padding(0, 0, 0, 28)
//        };
//        Label lbl = new Label
//        {
//            Text = label,
//            Font = FontRegular,
//            ForeColor = ColorPalette.Gray500,
//            Width = 150,
//            TextAlign = ContentAlignment.MiddleLeft,
//            Margin = new Padding(0, 0, 8, 0)
//        };
//        control.Margin = new Padding(0, 3, 0, 0);

//        row.Controls.Add(lbl);
//        row.Controls.Add(control);
//        return row;
//    }

//    private Control CreateRightSidebar()
//    {
//        int sidebarWidth = 350;
//        int sidebarPadding = 24;
//        int contentWidth = sidebarWidth - (sidebarPadding * 2); 

//        CustomPanel mainSidebarPanel = new CustomPanel
//        {
//            Dock = DockStyle.Fill,
//            BackColor = Color.White,
//            BorderRadius = 0,
//            BorderThickness = 0,
//            Margin = new Padding(0)
//        };

//        FlowLayoutPanel contentFlow = new FlowLayoutPanel
//        {
//            Dock = DockStyle.Fill,
//            FlowDirection = FlowDirection.TopDown,
//            WrapContents = false,
//            AutoScroll = true,
//            Padding = new Padding(sidebarPadding, 130, sidebarPadding, sidebarPadding), 
//            BackColor = Color.Transparent
//        };
//        mainSidebarPanel.Controls.Add(contentFlow);


//        Label detailsTitle = new Label
//        {
//            Text = "Details",
//            Font = FontBold,
//            ForeColor = ColorPalette.Gray800,
//            AutoSize = true,
//            Margin = new Padding(0, 0, 0, 16)
//        };
//        contentFlow.Controls.Add(detailsTitle);

//        TableLayoutPanel detailsContainer = new TableLayoutPanel
//        {
//            ColumnCount = 2,
//            RowCount = 4,
//            Width = contentWidth,
//            AutoSize = true,
//            CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
//            Margin = new Padding(0, 0, 0, 30),
//            ColumnStyles =
//            {
//                new ColumnStyle(SizeType.Absolute, 130F),
//                new ColumnStyle(SizeType.Percent, 100F)
//            }
//        };

//        Action<string, Control, int> AddDetailRowToTable = (label, control, row) =>
//        {
//            Label lbl = new Label
//            {
//                Text = label,
//                Font = FontRegular,
//                ForeColor = ColorPalette.Gray500,
//                Dock = DockStyle.Fill,
//                TextAlign = ContentAlignment.MiddleLeft,
//                Margin = new Padding(0, 10, 0, 10) 
//            };

//            control.Margin = new Padding(0, 10, 0, 10); 
//            control.Anchor = AnchorStyles.Left | AnchorStyles.Top;

//            detailsContainer.Controls.Add(lbl, 0, row);
//            detailsContainer.Controls.Add(control, 1, row);
//        };


       
//        FlowLayoutPanel assigneesValue = new FlowLayoutPanel
//        {
//            FlowDirection = FlowDirection.LeftToRight,
//            WrapContents = false,
//            AutoSize = true,
//            Margin = new Padding(0)
//        };
       
//        assigneesValue.Controls.Add(new Label { Text = "🧑‍💻", Font = new Font("Segoe UI Emoji", 10F), AutoSize = true, Margin = new Padding(0, 0, 4, 0) });
//        assigneesValue.Controls.Add(new Label { Text = "Alice, Bob", Font = FontRegular, ForeColor = ColorPalette.Gray800, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft });
//        assigneesValue.Controls.Add(new Label { Text = "(+2)", Font = FontRegular, ForeColor = ColorPalette.Gray500, AutoSize = true, TextAlign = ContentAlignment.MiddleLeft });

//        AddDetailRowToTable("Assignees", assigneesValue, 0);

        
//        Label dueDate = new Label
//        {
//            Text = "Dec 15, 2025",
//            Font = FontRegular,
//            ForeColor = ColorPalette.Gray800,
//            AutoSize = true,
//            TextAlign = ContentAlignment.MiddleLeft
//        };
//        AddDetailRowToTable("Due Date", dueDate, 1);

        
//        FlowLayoutPanel priorityFlow = new FlowLayoutPanel
//        {
//            FlowDirection = FlowDirection.LeftToRight,
//            AutoSize = true,
//            Margin = new Padding(0)
//        };
//        CustomPanel priority = new CustomPanel
//        {
//            Text = "High",
//            BackColor = ColorPalette.Red600,
//            ForeColor = Color.White,
//            Font = FontBold,
//            BorderRadius = 6,
//            Width = 100,
//            Height = 40,
//            TextAlign = ContentAlignment.MiddleCenter
//        };
//        priorityFlow.Controls.Add(priority);
//        AddDetailRowToTable("Priority", priorityFlow, 2);

//        FlowLayoutPanel progressValue = new FlowLayoutPanel
//        {
//            FlowDirection = FlowDirection.LeftToRight,
//            WrapContents = false,
//            AutoSize = true,
//            Margin = new Padding(0)
//        };

//        CustomPanel progressPanel = new CustomPanel
//        {
//            BackColor = ColorPalette.Gray200,
//            BorderRadius = 4,
//            Height = 8,
//            Width = 150
//        };
//        CustomPanel progressBar = new CustomPanel
//        {
//            BackColor = ColorPalette.Blue500,
//            BorderRadius = 4,
//            Height = 8,
//            Width = (int)(150 * 0.75)
//        };
//        progressPanel.Controls.Add(progressBar);

//        Label progressLabel = new Label
//        {
//            Text = "75%",
//            Font = FontRegular,
//            ForeColor = ColorPalette.Gray800,
//            Margin = new Padding(8, -5, 0, 0), 
//            TextAlign = ContentAlignment.MiddleLeft,
//            AutoSize = true
//        };
//        progressValue.Controls.Add(progressPanel);
//        progressValue.Controls.Add(progressLabel);
//        AddDetailRowToTable("Progress", progressValue, 3);

//        contentFlow.Controls.Add(detailsContainer);



//        Label activityTitle = new Label
//        {
//            Text = "Activity",
//            Font = FontBold,
//            ForeColor = ColorPalette.Gray800,
//            AutoSize = true,
//            Margin = new Padding(0, 30, 0, 24)
//        };
//        contentFlow.Controls.Add(activityTitle);

//        int activityLogWidth = contentWidth;

//        contentFlow.Controls.Add(CreateActivityLog("Alice assigned this task to Bob.", "2 days ago", activityLogWidth));
//        contentFlow.Controls.Add(CreateActivityLog("Charlie changed the due date to Dec 15, 2025.", "1 day ago", activityLogWidth));
//        contentFlow.Controls.Add(CreateActivityLog("Diana left a comment.", "3 hours ago", activityLogWidth));

//        Panel spacer = new Panel { Height = 50, Width = 1, BackColor = Color.Transparent };
//        contentFlow.Controls.Add(spacer);

//        return mainSidebarPanel;
//    }

//    private Control CreateComment(string user, string text, string time)
//    {
//        FlowLayoutPanel comment = new FlowLayoutPanel
//        {
//            FlowDirection = FlowDirection.TopDown,
//            WrapContents = false,
//            AutoSize = true,
//            Width = 800,
//            Margin = new Padding(0, 0, 0, 16),
//            Padding = new Padding(0, 0, 0, 8),
//            BackColor = Color.Transparent,
//            BorderStyle = BorderStyle.FixedSingle
//        };

//        FlowLayoutPanel header = new FlowLayoutPanel
//        {
//            FlowDirection = FlowDirection.LeftToRight,
//            AutoSize = true,
//            Margin = new Padding(0, 0, 0, 4)
//        };
//        header.Controls.Add(new Label { Text = user, Font = FontBold, ForeColor = ColorPalette.Gray800, AutoSize = true, Margin = new Padding(0, 0, 8, 0) });
//        header.Controls.Add(new Label { Text = time, Font = FontRegular, ForeColor = ColorPalette.Gray500, AutoSize = true });
//        comment.Controls.Add(header);

//        Label content = new Label
//        {
//            Text = text,
//            Font = FontRegular,
//            ForeColor = ColorPalette.Gray600,
//            MaximumSize = new Size(800, 0),
//            AutoSize = true
//        };
//        comment.Controls.Add(content);

//        return comment;
//    }

//    private Control CreateActivityLog(string activity, string time, int width)
//    {
//        FlowLayoutPanel logItem = new FlowLayoutPanel
//        {
//            FlowDirection = FlowDirection.TopDown,
//            WrapContents = false,
//            AutoSize = true,
//            Width = width,
//            Margin = new Padding(0, 0, 0, 12)
//        };

//        Label lblActivity = new Label
//        {
//            Text = activity,
//            Font = FontRegular,
//            ForeColor = ColorPalette.Gray600,
//            MaximumSize = new Size(width, 0),
//            AutoSize = true
//        };
//        Label lblTime = new Label
//        {
//            Text = time,
//            Font = new Font("Segoe UI", 8F, FontStyle.Regular),
//            ForeColor = ColorPalette.Gray400,
//            AutoSize = true
//        };

//        logItem.Controls.Add(lblActivity);
//        logItem.Controls.Add(lblTime);
//        return logItem;
//    }
//}