using System;
using System.Drawing;
using System.Windows.Forms;

namespace Exercise3
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent(); // Hàm này gọi từ file Designer

            LoadMockData();

            // Fix lỗi layout khi resize
            flowGroups.SizeChanged += (s, e) => {
                foreach (Control c in flowGroups.Controls) c.Width = flowGroups.ClientSize.Width;
            };
            flowMessages.SizeChanged += (s, e) => {
                foreach (Control c in flowMessages.Controls) c.Width = flowMessages.ClientSize.Width - 60;
            };
        }

        private void LoadMockData()
        {
            // Sidebar List
            for (int i = 0; i < 10; i++)
            {
                Panel groupItem = CreateGroupItem(i == 0);
                flowGroups.Controls.Add(groupItem);
            }
            flowGroups.Controls.Add(new Panel { Height = 50, Width = 10, BackColor = Color.Transparent });

            // Chat Content
            AddMessage("Received", Theme.BubbleTeal, false);
            AddMessage("Sent", Theme.BubblePurple, true);
            AddMessage("Received", Theme.BubbleOrange, false);
            AddMessage("Sent", Theme.BubblePurple, true);
            AddMessage("Received", Theme.BubbleTeal, false);

            flowMessages.Controls.Add(new Panel { Height = 80, Width = 10, BackColor = Color.Transparent });
        }

        private Panel CreateGroupItem(bool isSelected)
        {
            Panel item = new Panel();
            item.Height = 60;
            item.Padding = new Padding(15, 10, 15, 10);
            item.BackColor = isSelected ? Theme.BlueSelected : Color.White;
            item.Cursor = Cursors.Hand;

            RoundedPanel avatar = new RoundedPanel
            {
                Size = new Size(40, 40),
                CornerRadius = 20,
                FillColor = Theme.PurpleAvatar,
                Dock = DockStyle.Left
            };

            Label lblName = new Label
            {
                Text = "Group 1",
                Font = isSelected ? Theme.FontBold : Theme.FontRegular,
                ForeColor = Theme.TextDark,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Padding = new Padding(15, 0, 0, 0)
            };

            item.Controls.Add(lblName);
            item.Controls.Add(avatar);

            item.MouseEnter += (s, e) => { if (!isSelected) item.BackColor = Theme.BgGray50; };
            item.MouseLeave += (s, e) => { if (!isSelected) item.BackColor = Color.White; };

            item.Paint += (s, e) => {
                using (Pen p = new Pen(Theme.BorderGray)) e.Graphics.DrawLine(p, 0, item.Height - 1, item.Width, item.Height - 1);
            };

            return item;
        }

        private void AddMessage(string text, Color color, bool isRight)
        {
            var msg = new MessageBubble(color, isRight);
            flowMessages.Controls.Add(msg);
        }
    }
}