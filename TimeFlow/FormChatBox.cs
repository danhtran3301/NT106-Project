using System;
using System.Drawing;
using System.Drawing.Drawing2D; // Thư viện vẽ đồ họa
using System.Windows.Forms;

namespace BT3_LTMCB
{
    public partial class ChatForm : Form
    {
        public ChatForm()
        {
            InitializeComponent();
        }

        // Tạo dữ liệu giả lập
        private void GenerateDummyData()
        {
            // Sidebar: Tạo danh sách Group chat
            for (int i = 1; i <= 8; i++)
            {
                Button btn = new Button();
                btn.Text = $"   Group {i}";
                btn.Size = new Size(flowSidebar.Width - 5, 60); // Trừ 5px để không hiện scroll ngang
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.TextAlign = ContentAlignment.MiddleLeft;
                // Tạo icon tròn giả avatar
                btn.Image = CreateCircle(Color.FromArgb(200, 162, 255), 35);
                btn.TextImageRelation = TextImageRelation.ImageBeforeText;

                // Highlight item đầu tiên
                btn.BackColor = (i == 1) ? Color.FromArgb(230, 240, 255) : Color.White;
                btn.Padding = new Padding(10, 0, 0, 0);

                // Sự kiện click chuyển group
                btn.Click += (s, e) => { lblChatTitle.Text = ((Button)s).Text.Trim(); };

                flowSidebar.Controls.Add(btn);
            }

            // Chat: Tạo tin nhắn mẫu
            AddMessageBubble("Xin chào! Đây là giao diện bong bóng chat đã sửa lỗi.", false);
            AddMessageBubble("Tuyệt vời, các góc nhọn và đường viền đã liền mạch.", true);
            AddMessageBubble("Thử một tin nhắn dài hơn để xem khả năng xuống dòng tự động của bong bóng chat có hoạt động tốt hay không nhé.", false);
        }

        // --- Các sự kiện Click ---
        private void btnClose_Click(object sender, EventArgs e) => this.Close();
        private void btnBack_Click(object sender, EventArgs e) => MessageBox.Show("Chức năng Quay lại");

        private void btnAddFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
                AddMessageBubble($"[File] {System.IO.Path.GetFileName(ofd.FileName)}", true);
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtMessage.Text))
            {
                AddMessageBubble(txtMessage.Text, true);
                txtMessage.Clear();
            }
        }

        // --- Sự kiện vẽ trang trí cho ô nhập liệu (Bo tròn) ---
        private void pnlInputBackground_Paint(object sender, PaintEventArgs e)
        {
            Panel pnl = sender as Panel;
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle rect = new Rectangle(0, 0, pnl.Width - 1, pnl.Height - 1);
            int radius = 20; // Độ bo góc

            using (GraphicsPath path = GetRoundedRectPath(rect, radius))
            using (Pen pen = new Pen(Color.LightGray, 1))
            using (Brush brush = new SolidBrush(Color.FromArgb(240, 240, 240)))
            {
                g.FillPath(brush, path);
                g.DrawPath(pen, path);
            }
        }

        // --- HÀM 1: Tạo Bubble Chat (Logic chính) ---
        private void AddMessageBubble(string message, bool isMe)
        {
            // 1. Tạo Panel dòng (Row) chứa tin nhắn
            Panel pnlRow = new Panel();
            pnlRow.Width = flowChatMessages.ClientSize.Width - 25; // Trừ thanh cuộn
            pnlRow.BackColor = Color.Transparent;
            // Padding để đẩy tin nhắn sang trái/phải
            pnlRow.Padding = new Padding(isMe ? 100 : 10, 5, isMe ? 10 : 100, 5);

            // 2. Tạo Panel bong bóng
            Panel pnlBubble = new Panel();
            pnlBubble.BackColor = Color.Transparent;

            // 3. Tạo Label nội dung
            Label lblContent = new Label();
            lblContent.Text = message;
            lblContent.AutoSize = true;
            lblContent.MaximumSize = new Size(pnlRow.Width - 140, 0); // Giới hạn chiều rộng để xuống dòng
            lblContent.Font = new Font("Segoe UI", 11);
            lblContent.ForeColor = isMe ? Color.White : Color.Black;
            lblContent.BackColor = Color.Transparent;

            // Canh chỉnh vị trí text để tránh đè lên cái đuôi nhọn
            int tailGap = 12;
            lblContent.Location = new Point(isMe ? 12 : 12 + tailGap, 12);

            pnlBubble.Controls.Add(lblContent);

            // 4. Tính toán kích thước bong bóng theo text
            Size textSize = lblContent.GetPreferredSize(new Size(pnlRow.Width - 140, 0));
            pnlBubble.Size = new Size(textSize.Width + 30 + tailGap, textSize.Height + 25);

            // 5. Gán sự kiện vẽ hình dáng (Quan trọng)
            pnlBubble.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;

                // Màu sắc: Tím (Mình) - Xám (Bạn)
                Color color = isMe ? Color.FromArgb(160, 130, 255) : Color.FromArgb(225, 225, 225);

                using (var brush = new SolidBrush(color))
                {
                    // Gọi hàm vẽ hình dáng đã sửa lỗi
                    var path = DrawSmoothBubble(pnlBubble.ClientRectangle, 12, isMe);
                    g.FillPath(brush, path);
                }
            };

            // 6. Thêm vào giao diện
            pnlRow.Height = pnlBubble.Height + 10;
            if (isMe) pnlBubble.Dock = DockStyle.Right;
            else pnlBubble.Dock = DockStyle.Left;

            pnlRow.Controls.Add(pnlBubble);
            flowChatMessages.Controls.Add(pnlRow);
            flowChatMessages.ScrollControlIntoView(pnlRow);
        }

        // --- HÀM 2: Vẽ hình dáng Bubble (Đã fix lỗi hở nét) ---
        private GraphicsPath DrawSmoothBubble(Rectangle r, int radius, bool isMe)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            int tailSize = 10;

            // Thu nhỏ vùng vẽ 1px để không bị răng cưa ở mép
            r.Width -= 1;
            r.Height -= 1;

            if (isMe) // Bubble bên Phải (Của mình)
            {
                int bodyWidth = r.Width - tailSize; // Thân ko bao gồm đuôi

                path.AddArc(r.X, r.Y, d, d, 180, 90); // Góc trên trái
                path.AddArc(r.X + bodyWidth - d, r.Y, d, d, 270, 90); // Góc trên phải

                // Cạnh phải đi xuống
                path.AddLine(r.X + bodyWidth, r.Y + radius, r.X + bodyWidth, r.Bottom - radius - tailSize);

                // Vẽ đuôi nhọn
                path.AddLine(r.X + bodyWidth, r.Bottom - radius - tailSize, r.Right, r.Bottom);
                path.AddLine(r.Right, r.Bottom, r.X + bodyWidth - radius, r.Bottom);

                // Góc dưới trái
                path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            }
            else // Bubble bên Trái (Của người khác) - Đã fix lỗi
            {
                int startX = r.X + tailSize; // Điểm bắt đầu thân (chừa chỗ cho đuôi)

                path.AddArc(startX, r.Y, d, d, 180, 90); // Góc trên trái
                path.AddArc(r.Right - d, r.Y, d, d, 270, 90); // Góc trên phải
                path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90); // Góc dưới phải

                // Đáy thân
                path.AddLine(r.Right - radius, r.Bottom, startX + radius, r.Bottom);

                // Vẽ đuôi nhọn bên trái
                path.AddLine(startX + radius, r.Bottom, r.X, r.Bottom);
                path.AddLine(r.X, r.Bottom, startX, r.Bottom - radius - tailSize);
            }

            path.CloseFigure(); // Khép kín hình để tô màu không bị lỗi
            return path;
        }

        // Helper: Vẽ hình chữ nhật bo tròn (cho ô Input)
        private GraphicsPath GetRoundedRectPath(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0) { path.AddRectangle(bounds); return path; }

            path.AddArc(arc, 180, 90);
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();
            return path;
        }

        // Helper: Tạo ảnh tròn (Avatar)
        private Bitmap CreateCircle(Color c, int s)
        {
            Bitmap b = new Bitmap(s, s);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                using (Brush br = new SolidBrush(c)) g.FillEllipse(br, 0, 0, s - 1, s - 1);
            }
            return b;
        }
    }
}