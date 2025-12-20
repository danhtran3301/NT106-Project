using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TimeFlow.UI.Components
{
    /// <summary>
    /// Panel with rounded corners and customizable border
    /// </summary>
    public class ModernPanel : Panel
    {
        private ContentAlignment _textAlign = ContentAlignment.MiddleCenter;
        public ContentAlignment TextAlign
        {
            get { return _textAlign; }
            set { _textAlign = value; Invalidate(); }
        }

        private int _borderRadius = 8;
        public int BorderRadius
        {
            get { return _borderRadius; }
            set { _borderRadius = value; this.Invalidate(); }
        }

        private int _borderThickness = 0;
        public int BorderThickness
        {
            get { return _borderThickness; }
            set { _borderThickness = value; this.Invalidate(); }
        }

        private Color _borderColor = Color.Transparent;
        public virtual Color BorderColor
        {
            get { return _borderColor; }
            set { _borderColor = value; this.Invalidate(); }
        }

        public override Color BackColor
        {
            get => base.BackColor;
            set
            {
                base.BackColor = value;
                this.Invalidate();
            }
        }

        public ModernPanel()
        {
            this.SetStyle(ControlStyles.UserPaint |
                   ControlStyles.AllPaintingInWmPaint |
                   ControlStyles.DoubleBuffer |
                   ControlStyles.OptimizedDoubleBuffer |
                   ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.White;
            this.DoubleBuffered = true;
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

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // Don't paint background - we'll handle it in OnPaint
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle fillRect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            GraphicsPath path = GetRoundedPath(fillRect, _borderRadius);

            e.Graphics.SetClip(path);

            using (SolidBrush brush = new SolidBrush(base.BackColor))
            {
                e.Graphics.FillPath(brush, path);
            }

            base.OnPaint(e);

            e.Graphics.ResetClip();

            if (_borderThickness > 0)
            {
                GraphicsPath borderPath = GetRoundedPath(new Rectangle(0, 0, this.Width - 1, this.Height - 1), _borderRadius);
                using (Pen pen = new Pen(_borderColor, _borderThickness))
                {
                    pen.Alignment = PenAlignment.Center;
                    e.Graphics.DrawPath(pen, borderPath);
                }
            }

            if (!string.IsNullOrEmpty(this.Text))
            {
                TextFormatFlags flags = GetTextFormatFlags(_textAlign);
                Rectangle textRect = this.ClientRectangle;
                TextRenderer.DrawText(e.Graphics, this.Text, this.Font, textRect, this.ForeColor, flags);
            }
        }

        private GraphicsPath GetRoundedPath(RectangleF rect, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            int diameter = (int)(radius * 2);
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle((int)rect.Location.X, (int)rect.Location.Y, diameter, diameter);

            path.AddArc(arc, 180, 90);
            arc.X = (int)(rect.Right - diameter);
            path.AddArc(arc, 270, 90);
            arc.Y = (int)(rect.Bottom - diameter);
            path.AddArc(arc, 0, 90);
            arc.X = (int)rect.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }
    }
}