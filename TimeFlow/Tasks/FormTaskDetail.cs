using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TimeFlow
{

    public static class ColorPalette
    {
        public static readonly Color Gray50 = ColorTranslator.FromHtml("#F9FAFB");
        public static readonly Color Gray100 = ColorTranslator.FromHtml("#F3F4F6");
        public static readonly Color Gray200 = ColorTranslator.FromHtml("#E5E7EB");
        public static readonly Color Gray300 = ColorTranslator.FromHtml("#D1D5DB");
        public static readonly Color Gray400 = ColorTranslator.FromHtml("#9CA3AF");
        public static readonly Color Gray500 = ColorTranslator.FromHtml("#6B7280");
        public static readonly Color Gray600 = ColorTranslator.FromHtml("#4B5563");
        public static readonly Color Gray700 = ColorTranslator.FromHtml("#374151");
        public static readonly Color Gray800 = ColorTranslator.FromHtml("#1F2937");
        public static readonly Color Blue500 = ColorTranslator.FromHtml("#3B82F6");
        public static readonly Color Blue600 = ColorTranslator.FromHtml("#2563EB");
        public static readonly Color Green500 = ColorTranslator.FromHtml("#10B981");
        public static readonly Color Green600 = ColorTranslator.FromHtml("#059669");
        public static readonly Color Red500 = ColorTranslator.FromHtml("#EF4444");
        public static readonly Color Red600 = ColorTranslator.FromHtml("#DC2626");
        public static readonly Color Red700 = ColorTranslator.FromHtml("#B91C1C");
        public static readonly Color Yellow500 = ColorTranslator.FromHtml("#F59E0B");
        public static readonly Color Orange500 = ColorTranslator.FromHtml("#F97316");
        public static readonly Color Purple500 = ColorTranslator.FromHtml("#8B5CF6");
    }


    public class CustomPanel : Panel
    {
        private ContentAlignment _textAlign = ContentAlignment.MiddleCenter;
        public ContentAlignment TextAlign
        {
            get { return _textAlign; }
            set { if (_textAlign != value) { _textAlign = value; Invalidate(); } }
        }

        private int _borderRadius = 8;
        public int BorderRadius
        {
            get { return _borderRadius; }
            set
            {
                if (_borderRadius != value)
                {
                    _borderRadius = value;
                    UpdateRegion();
                }
            }
        }


        private int _borderThickness = 0;
        public int BorderThickness
        {
            get { return _borderThickness; }
            set { if (_borderThickness != value) { _borderThickness = value; this.Invalidate(); } }
        }

        private Color _borderColor = Color.Transparent;
        public Color BorderColor
        {
            get { return _borderColor; }
            set { if (_borderColor != value) { _borderColor = value; this.Invalidate(); } }
        }

        public CustomPanel()
        {
            this.SetStyle(ControlStyles.DoubleBuffer |
                          ControlStyles.UserPaint |
                          ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
        }

        private TextFormatFlags GetTextFormatFlags(ContentAlignment alignment)
        {
            TextFormatFlags flags = TextFormatFlags.NoPadding | TextFormatFlags.WordBreak;

            if (alignment.ToString().Contains("Top")) flags |= TextFormatFlags.Top;
            else if (alignment.ToString().Contains("Bottom")) flags |= TextFormatFlags.Bottom;
            else flags |= TextFormatFlags.VerticalCenter;

            if (alignment.ToString().Contains("Left")) flags |= TextFormatFlags.Left;
            else if (alignment.ToString().Contains("Right")) flags |= TextFormatFlags.Right;
            else flags |= TextFormatFlags.HorizontalCenter;
            return flags;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            GraphicsPath path = GetRoundedRect(rect, _borderRadius);

            using (SolidBrush brush = new SolidBrush(this.BackColor))
            {
                e.Graphics.FillPath(brush, path);
            }

            if (_borderThickness > 0)
            {
                using (Pen pen = new Pen(_borderColor, _borderThickness))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }

            if (!string.IsNullOrEmpty(this.Text))
            {
                TextFormatFlags flags = GetTextFormatFlags(_textAlign);
                Rectangle textRect = this.ClientRectangle;
                TextRenderer.DrawText(e.Graphics, this.Text, this.Font, textRect, this.ForeColor, flags);
            }

        }

        private void UpdateRegion()
        {
            if (this.Width > 0 && this.Height > 0)
            {
                Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);
                GraphicsPath path = GetRoundedRect(rect, _borderRadius);
                this.Region = new Region(path);
                this.Invalidate();
            }
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            UpdateRegion();
        }

        private GraphicsPath GetRoundedRect(Rectangle rect, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(rect.Location, size);

            path.AddArc(arc, 180, 90);
            arc.X = rect.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = rect.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = rect.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }
    }



    public class CustomButton : CustomPanel
    {
        private Color _originalBackColor;
        private Color _hoverColor = ColorPalette.Gray200;
        private bool _isMouseDown = false;

        public CustomButton()
        {
            this.Cursor = Cursors.Hand;
            this.SetStyle(ControlStyles.Selectable, true);
            this.TabStop = true;
            this.Text = "CustomButton";
            this.TextAlign = ContentAlignment.MiddleCenter;
            _originalBackColor = this.BackColor;
        }

        public Color HoverColor
        {
            get { return _hoverColor; }
            set { _hoverColor = value; }
        }

        public override Color BackColor
        {
            get => base.BackColor;
            set { base.BackColor = value; _originalBackColor = value; }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                _isMouseDown = true;
                this.BackColor = ColorPalette.Gray300;
                this.Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Left)
            {
                _isMouseDown = false;

                if (this.ClientRectangle.Contains(this.PointToClient(Cursor.Position)))
                {
                    this.BackColor = _hoverColor;
                }
                else
                {
                    this.BackColor = _originalBackColor;
                }
                this.Invalidate();
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            if (!_isMouseDown)
            {
                this.BackColor = _hoverColor;
                this.Invalidate();
            }
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            this.BackColor = _originalBackColor;
            this.Invalidate();
        }
    }

    public class CustomTextBoxWrapper : CustomPanel
    {
        private RichTextBox _textBox;

        public CustomTextBoxWrapper()
        {
            this.BorderRadius = 6;
            this.BorderThickness = 1;
            this.BorderColor = ColorPalette.Gray300;
            this.BackColor = Color.White;
            this.Padding = new Padding(8);

            _textBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                AutoSize = false,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                BackColor = Color.White,
                Text = "Add a comment..."
            };

            this.Controls.Add(_textBox);
        }

        public string TextBoxText
        {
            get { return _textBox.Text; }
            set { _textBox.Text = value; }
        }
    }


    public partial class FormTaskDetail : Form
    {
        private readonly Font FontRegular = new Font("Segoe UI", 10F, FontStyle.Regular);
        private readonly Font FontBold = new Font("Segoe UI", 10F, FontStyle.Bold);
        private readonly Font FontTitle = new Font("Segoe UI", 16F, FontStyle.Bold);
        private readonly Font FontHeaderTitle = new Font("Segoe UI", 12F, FontStyle.Bold);
        private readonly Color HeaderIconColor = ColorPalette.Gray600;


        public FormTaskDetail()
        {
            InitializeComponent();
            this.SetStyle(ControlStyles.DoubleBuffer |
                          ControlStyles.UserPaint |
                          ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();
            
            // Override click handlers cho navigation buttons
            OverrideNavigationButtons();
        }

        /// <summary>
        /// Override các button navigation để implement single-window pattern
        /// </summary>
        private void OverrideNavigationButtons()
        {
            // Tìm arrowButton và closeButton trong controls
            var arrowButton = FindControlByText("←");
            var closeButton = FindControlByText("✕");

            if (arrowButton != null)
            {
                // Remove old handler và add new
                arrowButton.Click -= null;
                arrowButton.Click += (s, e) => { this.Close(); }; // Close để quay về FormGiaoDien
            }

            if (closeButton != null)
            {
                closeButton.Click -= null;
                closeButton.Click += (s, e) => { this.Close(); };
            }
        }

        /// <summary>
        /// Helper để tìm control theo text
        /// </summary>
        private Control FindControlByText(string text)
        {
            foreach (Control ctrl in this.Controls)
            {
                var found = FindControlByTextRecursive(ctrl, text);
                if (found != null) return found;
            }
            return null;
        }

        private Control FindControlByTextRecursive(Control parent, string text)
        {
            if (parent.Text == text) return parent;

            foreach (Control child in parent.Controls)
            {
                if (child.Text == text) return child;
                var found = FindControlByTextRecursive(child, text);
                if (found != null) return found;
            }
            return null;
        }

        private CustomButton CreateMenuButton(string text, Color backColor, Color foreColor, int width, int height, Color? hoverColor)
        {
            return new CustomButton
            {
                Text = text,
                BackColor = backColor,
                ForeColor = foreColor,
                HoverColor = hoverColor ?? ColorPalette.Blue600,
                BorderRadius = 8,
                Width = width,
                Height = height,
                Font = FontBold,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0, 0, 0, 12)
            };
        }
        private CustomButton CreateMenuButton(string text, Color backColor, Color foreColor, int width, int height)
        {
            return CreateMenuButton(text, backColor, foreColor, width, height, null);
        }

        private Control CreateComment(string user, string text, string time)
        {
            FlowLayoutPanel comment = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = false,
                Width = 800,
                Margin = new Padding(0, 0, 0, 16),
                Padding = new Padding(0, 0, 0, 8),
                BackColor = Color.Transparent,
                BorderStyle = BorderStyle.FixedSingle
            };
            comment.AutoSize = true;
            comment.WrapContents = false;


            FlowLayoutPanel header = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.LeftToRight,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 4)
            };
            header.Controls.Add(new Label { Text = user, Font = FontBold, ForeColor = ColorPalette.Gray800, AutoSize = true, Margin = new Padding(0, 0, 8, 0) });
            header.Controls.Add(new Label { Text = time, Font = FontRegular, ForeColor = ColorPalette.Gray500, AutoSize = true });
            comment.Controls.Add(header);

            Label content = new Label
            {
                Text = text,
                Font = FontRegular,
                ForeColor = ColorPalette.Gray600,
                MaximumSize = new Size(comment.Width, 0),
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
                AutoSize = false,
                Width = width,
                Margin = new Padding(0, 0, 0, 12)
            };
            logItem.AutoSize = true;


            Label lblActivity = new Label
            {
                Text = activity,
                Font = FontRegular,
                ForeColor = ColorPalette.Gray600,
                MaximumSize = new Size(width, 0),
                AutoSize = true
            };
            Label lblTime = new Label
            {
                Text = time,
                Font = new Font("Segoe UI", 8F, FontStyle.Regular),
                ForeColor = ColorPalette.Gray400,
                AutoSize = true
            };

            logItem.Controls.Add(lblActivity);
            logItem.Controls.Add(lblTime);
            return logItem;
        }
    }
}