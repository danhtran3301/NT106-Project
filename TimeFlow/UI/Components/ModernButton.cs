using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace TimeFlow.UI.Components
{
    /// <summary>
    /// Modern flat button with rounded corners and hover effect
    /// </summary>
    public class ModernButton : Button
    {
        public int BorderRadius { get; set; } = 4;
        public Color BorderColor { get; set; } = Color.Transparent;
        public Color HoverColor { get; set; } = Color.Empty;

        private Color _originalBackColor;

        public ModernButton()
        {
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderSize = 0;
            this.Cursor = Cursors.Hand;
            this.DoubleBuffered = true;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _originalBackColor = this.BackColor;
            if (HoverColor != Color.Empty) 
                this.BackColor = HoverColor;
            else 
                this.BackColor = ControlPaint.Light(this.BackColor);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            this.BackColor = _originalBackColor;
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            RectangleF rect = new RectangleF(0, 0, this.Width - 1, this.Height - 1);
            GraphicsPath path = new GraphicsPath();
            float r = BorderRadius;

            path.AddArc(rect.X, rect.Y, r, r, 180, 90);
            path.AddArc(rect.Width - r, rect.Y, r, r, 270, 90);
            path.AddArc(rect.Width - r, rect.Height - r, r, r, 0, 90);
            path.AddArc(rect.X, rect.Height - r, r, r, 90, 90);
            path.CloseFigure();

            this.Region = new Region(path);

            using (SolidBrush brush = new SolidBrush(this.BackColor))
            {
                pevent.Graphics.FillPath(brush, path);
            }

            if (BorderColor != Color.Transparent)
            {
                using (Pen pen = new Pen(BorderColor, 1))
                {
                    pevent.Graphics.DrawPath(pen, path);
                }
            }

            // Draw Text center
            TextRenderer.DrawText(pevent.Graphics, this.Text, this.Font, this.ClientRectangle, 
                this.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }
}
