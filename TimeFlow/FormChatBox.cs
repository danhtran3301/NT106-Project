using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BT3_LTMCB
{
    public partial class ChatForm : Form
    {
        public ChatForm()
        {
            InitializeComponent();
            LoadMockData();
        }

        private void LoadMockData()
        {
            // 1. Tạo danh sách nhóm bên trái
            for (int i = 0; i < 8; i++)
            {
                AddGroupItem("Group Chat", i == 0); // Chọn item đầu tiên
            }

            // 2. Tạo tin nhắn giả lập (Màu sắc lấy từ file React)
            // #8eeed8 (Teal), #a78bfa (Purple), #fbbf24 (Orange)
            AddMessage("Message Line 1\nMessage Line 2", false, ColorTranslator.FromHtml("#8eeed8"));
            AddMessage("Hello there!", true, ColorTranslator.FromHtml("#a78bfa"));
            AddMessage("Warning info line 1\nLine 2\nLine 3", false, ColorTranslator.FromHtml("#fbbf24"));
            AddMessage("I understand.", true, ColorTranslator.FromHtml("#a78bfa"));
        }

        // Tạo item trong Sidebar
        private void AddGroupItem(string name, bool isSelected)
        {
            Panel pnlItem = new Panel();
            pnlItem.Size = new Size(180, 60);
            pnlItem.BackColor = isSelected ? Color.AliceBlue : Color.White;
            pnlItem.Cursor = Cursors.Hand;
            pnlItem.Click += (s, e) => { pnlItem.BackColor = Color.AliceBlue; };

            // Avatar tròn
            Panel pnlAvatar = new Panel();
            pnlAvatar.Size = new Size(40, 40);
            pnlAvatar.Location = new Point(10, 10);
            GraphicsPath gp = new GraphicsPath();
            gp.AddEllipse(0, 0, 40, 40);
            pnlAvatar.Region = new Region(gp);
            pnlAvatar.BackColor = ColorTranslator.FromHtml("#d8b4fe"); // Tím nhạt

            Label lblName = new Label();
            lblName.Text = name;
            lblName.Location = new Point(60, 20);
            lblName.AutoSize = true;
            lblName.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            pnlItem.Controls.Add(pnlAvatar);
            pnlItem.Controls.Add(lblName);
            flowLeftGroups.Controls.Add(pnlItem);
        }

        // Tạo bong bóng chat
        // Thay thế phương thức AddMessage cũ
        private void AddMessage(string text, bool isSentByMe, Color bgColor)
        {
            // 1. Tính toán kích thước text để resize bong bóng
            Size constrains = new Size(400, 5000); // Max width 400
            Size textSize = TextRenderer.MeasureText(text, new Font("Segoe UI", 10), constrains, TextFormatFlags.WordBreak);

            // Chiều cao bong bóng = chiều cao text + padding trên dưới
            int bubbleHeight = textSize.Height + 30;
            int bubbleWidth = Math.Max(textSize.Width + 40, 150); // Min width 150

            // 2. Container cho dòng chat
            Panel pnlRow = new Panel();
            pnlRow.Height = bubbleHeight + 10; // Thêm margin bottom
            pnlRow.Width = flowChatMessages.ClientSize.Width - 30; // Trừ scrollbar
            pnlRow.Padding = new Padding(0);
            pnlRow.Margin = new Padding(0, 0, 0, 10);
            pnlRow.BackColor = Color.Transparent;

            // 3. Bong bóng chat
            BubblePanel bubble = new BubblePanel();
            bubble.Text = text;
            bubble.Font = new Font("Segoe UI", 10);
            bubble.BubbleColor = bgColor;
            bubble.IsSender = isSentByMe;
            bubble.Size = new Size(bubbleWidth, bubbleHeight);

            // 4. Định vị
            if (isSentByMe)
            {
                bubble.Location = new Point(pnlRow.Width - bubble.Width - 5, 0);
                bubble.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            }
            else
            {
                bubble.Location = new Point(5, 0);
                bubble.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            }

            pnlRow.Controls.Add(bubble);
            flowChatMessages.Controls.Add(pnlRow);
            flowChatMessages.ScrollControlIntoView(pnlRow);
        }
    }

    // Class vẽ bong bóng chat tùy chỉnh
    // Thêm vào cuối file ChatForm.cs hoặc file logic

public class BubblePanel : Panel
    {
        public Color BubbleColor { get; set; } = Color.LightGray;
        public bool IsSender { get; set; } = false;

        public BubblePanel()
        {
            this.DoubleBuffered = true;
            this.ResizeRedraw = true;
            this.BackColor = Color.Transparent; // Quan trọng để nền trong suốt
            this.Padding = new Padding(15, 10, 15, 10); // Căn lề cho text không chạm viền

            // Tối ưu render
            this.SetStyle(ControlStyles.UserPaint |
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.SupportsTransparentBackColor, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // 1. Tạo hình dáng bo tròn
            var path = GetBubblePath(this.ClientRectangle);

            // 2. Cắt control theo hình dáng này (Loại bỏ góc đen)
            this.Region = new Region(path);

            // 3. Vẽ màu nền
            using (SolidBrush brush = new SolidBrush(BubbleColor))
            {
                e.Graphics.FillPath(brush, path);
            }

            // 4. Vẽ Text (Vẽ đè lên nền)
            TextRenderer.DrawText(e.Graphics, this.Text, this.Font,
                new Rectangle(Padding.Left, Padding.Top, Width - Padding.Horizontal, Height - Padding.Vertical),
                Color.Black,
                TextFormatFlags.WordBreak | TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

            // (Tùy chọn) Vẽ đường line trang trí mờ nhạt nếu muốn giống mẫu
            // Chỉ vẽ khi không có text hoặc text ngắn để không che chữ
            if (string.IsNullOrWhiteSpace(this.Text) || this.Text.Length < 5)
            {
                using (Pen linePen = new Pen(Color.FromArgb(50, 255, 255, 255), 3))
                {
                    e.Graphics.DrawLine(linePen, 20, Height - 15, Width - 40, Height - 15);
                }
            }
        }

        private GraphicsPath GetBubblePath(Rectangle rect)
        {
            GraphicsPath path = new GraphicsPath();
            int radius = 15;
            rect.Width -= 1; rect.Height -= 1; // Trừ 1 pixel để tránh bị cắt viền

            if (IsSender)
            {
                // Góc nhọn bên phải dưới
                path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
                path.AddLine(rect.Right, rect.Bottom - radius, rect.Right, rect.Bottom); // Đường thẳng xuống góc
                path.AddLine(rect.Right, rect.Bottom, rect.Right - radius, rect.Bottom); // Đường ngang sang trái
                path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
            }
            else
            {
                // Góc nhọn bên trái dưới
                path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
                path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
                path.AddLine(rect.X + radius, rect.Bottom, rect.X, rect.Bottom); // Đường ngang về góc trái
                path.AddLine(rect.X, rect.Bottom, rect.X, rect.Bottom - radius); // Đường lên
            }
            path.CloseFigure();
            return path;
        }
    }
}

