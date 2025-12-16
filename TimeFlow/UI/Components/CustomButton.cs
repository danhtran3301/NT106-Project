using System;
using System.Drawing;
using System.Windows.Forms;

namespace TimeFlow.UI.Components
{
    /// <summary>
    /// Custom button extending ModernPanel with hover effects and border color changes
    /// </summary>
    public class CustomButton : ModernPanel
    {
        private Color _originalBackColor;
        private Color _hoverColor = AppColors.Gray200;

        private Color _originalBorderColor;
        private Color _hoverBorderColor = AppColors.MenuBorderColor;

        public CustomButton()
        {
            this.Cursor = Cursors.Hand;
            this.SetStyle(ControlStyles.Selectable, true);
            this.TabStop = true;
            this.Text = "CustomButton";
            this.TextAlign = ContentAlignment.MiddleCenter;
            _originalBackColor = this.BackColor;
            _originalBorderColor = this.BorderColor;
        }

        public Color HoverColor
        {
            get { return _hoverColor; }
            set { _hoverColor = value; }
        }

        public Color HoverBorderColor
        {
            get { return _hoverBorderColor; }
            set { _hoverBorderColor = value; }
        }

        public override Color BackColor
        {
            get => base.BackColor;
            set { base.BackColor = value; _originalBackColor = value; }
        }

        public override Color BorderColor
        {
            get => base.BorderColor;
            set { base.BorderColor = value; _originalBorderColor = value; }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            base.BackColor = _hoverColor;
            base.BorderColor = _hoverBorderColor;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            base.BackColor = _originalBackColor;
            base.BorderColor = _originalBorderColor;
        }
    }
}
