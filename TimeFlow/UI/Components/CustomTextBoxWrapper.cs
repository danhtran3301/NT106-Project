using System.Drawing;
using System.Windows.Forms;

namespace TimeFlow.UI.Components
{
    /// <summary>
    /// Custom text box wrapper with rounded border extending ModernPanel
    /// </summary>
    public class CustomTextBoxWrapper : ModernPanel
    {
        private RichTextBox _textBox;

        public CustomTextBoxWrapper()
        {
            this.BorderRadius = 6;
            this.BorderThickness = 1;
            this.BorderColor = AppColors.Gray300;
            this.BackColor = Color.White;
            this.Padding = new Padding(8);

            _textBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
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
}
