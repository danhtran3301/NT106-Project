using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;

namespace TimeFlow
{
    public partial class ChatForm : Form
    {
        // --- CẤU HÌNH MẠNG ---
        private TcpClient _client;
        private NetworkStream _stream;
        private Thread _listenThread;
        private string _myUsername;
        private string _currentReceiver = ""; // Không hardcode nữa, chờ chọn từ list
        private bool _isConnected = false;

        // Constructor chính (Được gọi từ Form Login)
        public ChatForm(TcpClient client, string myUsername)
        {
            InitializeComponent();
            _client = client;
            _stream = client.GetStream();
            _myUsername = myUsername;
            _isConnected = true;

            this.Text = $"TimeFlow Chat - {_myUsername}";
            lblChatTitle.Text = "Chọn một người để bắt đầu chat"; // Tiêu đề mặc định

            // Bắt đầu lắng nghe tin nhắn
            StartListening();
        }

        // --- HÀM 1: KẾT NỐI SERVER (Chỉ dùng khi test riêng Form này) ---
        private void ConnectToServer(string user)
        {
            try
            {
                _client = new TcpClient("127.0.0.1", 1010); // Đổi PORT cho khớp server
                _stream = _client.GetStream();
                _myUsername = user;
                _isConnected = true;

                // Gửi lệnh Login giả để Server biết mình là ai
                var loginData = new { type = "login", data = new { username = user, password = "123" } };
                byte[] data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(loginData));
                _stream.Write(data, 0, data.Length);

                StartListening();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể kết nối Server: " + ex.Message);
            }
        }

        // --- HÀM 2: LẮNG NGHE TIN NHẮN (BACKGROUND THREAD) ---
        private void StartListening()
        {
            _listenThread = new Thread(() =>
            {
                try
                {
                    byte[] buffer = new byte[8192]; // Tăng buffer để nhận list user dài
                    while (_isConnected)
                    {
                        int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0) break;

                        string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                        // Xử lý trường hợp dính nhiều gói tin (Packet Stickiness) - Nâng cao
                        // Ở mức độ cơ bản này, ta giả định mỗi lần read là 1 gói JSON chuẩn
                        ProcessServerMessage(json);
                    }
                }
                catch
                {
                    _isConnected = false;
                    this.Invoke((MethodInvoker)delegate {
                        MessageBox.Show("Mất kết nối tới Server!");
                        this.Close();
                    });
                }
            });
            _listenThread.IsBackground = true;
            _listenThread.Start();
        }

        // --- HÀM 2: XỬ LÝ GÓI TIN TỪ SERVER ---
        private void ProcessServerMessage(string json)
        {
            try
            {
                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    JsonElement root = doc.RootElement;
                    if (root.TryGetProperty("type", out JsonElement typeElem))
                    {
                        string type = typeElem.GetString();

                        // Nếu là tin nhắn chat đến
                        if (type == "receive_message")
                        {
                            string sender = root.GetProperty("sender").GetString();
                            string content = root.GetProperty("content").GetString();
                            string time = root.GetProperty("timestamp").GetString();

                            this.Invoke((MethodInvoker)delegate
                            {
                                bool isMe = (sender == _myUsername);
                                AddMessageBubble(content, isMe, time);
                            });
                        }
                    }
                }
            }
            catch { /* Bỏ qua lỗi parse JSON rác */ }
        }

            // Gửi yêu cầu lấy lịch sử
            SendJson(new { type = "get_history", target_user = targetUser });
        }

        // --- HÀM 5: GỬI TIN NHẮN ---
        private void btnSend_Click(object sender, EventArgs e)
        {
            string msg = txtMessage.Text.Trim();
            if (string.IsNullOrEmpty(msg)) return;
            if (string.IsNullOrEmpty(_currentReceiver))
            {
                MessageBox.Show("Vui lòng chọn người nhận trước!");
                return;
            }

            if (_isConnected)
            {
                // Gửi JSON đi
                SendJson(new
                {
                    type = "chat",
                    receiver = _currentReceiver,
                    content = msg
                });

                // Hiện ngay lên màn hình mình cho mượt
                AddMessageBubble(msg, true, DateTime.Now.ToString("HH:mm"));
                txtMessage.Clear();
            }
        }

        // Helper gửi JSON
        private void SendJson(object data)
        {
            try
            {
                string json = JsonSerializer.Serialize(data);
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                _stream.Write(bytes, 0, bytes.Length);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi gửi: " + ex.Message);
            }
        }

        // --- UI LOGIC: VẼ BONG BÓNG CHAT (GIỮ NGUYÊN CODE CŨ CỦA BẠN - RẤT TỐT) ---
        private void AddMessageBubble(string message, bool isMe, string time)
        {
            Panel pnlRow = new Panel();
            pnlRow.Width = flowChatMessages.ClientSize.Width - 25;
            pnlRow.BackColor = Color.Transparent;
            pnlRow.Padding = new Padding(isMe ? 80 : 10, 5, isMe ? 10 : 80, 5);

            Panel pnlBubble = new Panel();
            pnlBubble.BackColor = Color.Transparent;

            Label lblContent = new Label();
            lblContent.Text = message;
            lblContent.Font = new Font("Segoe UI", 11);
            lblContent.ForeColor = isMe ? Color.White : Color.Black;
            lblContent.AutoSize = true;
            lblContent.MaximumSize = new Size(pnlRow.Width - 120, 0);
            lblContent.Location = new Point(isMe ? 12 : 18, 10);
            lblContent.BackColor = Color.Transparent;

            Label lblTime = new Label();
            lblTime.Text = time;
            lblTime.Font = new Font("Arial", 8, FontStyle.Italic);
            lblTime.ForeColor = isMe ? Color.FromArgb(230, 230, 230) : Color.Gray;
            lblTime.AutoSize = true;
            lblTime.BackColor = Color.Transparent;

            pnlBubble.Controls.Add(lblContent);
            pnlBubble.Controls.Add(lblTime);

            Size textSize = lblContent.GetPreferredSize(new Size(pnlRow.Width - 120, 0));
            lblTime.Location = new Point(lblContent.Left, lblContent.Top + textSize.Height + 2);
            pnlBubble.Size = new Size(textSize.Width + 40, textSize.Height + 35);

            pnlBubble.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Color color = isMe ? Color.FromArgb(0, 132, 255) : Color.FromArgb(240, 242, 245);
                using (var brush = new SolidBrush(color))
                {
                    var path = DrawSmoothBubble(pnlBubble.ClientRectangle, 15, isMe);
                    g.FillPath(brush, path);
                }
            };

            pnlRow.Height = pnlBubble.Height + 10;
            pnlBubble.Dock = isMe ? DockStyle.Right : DockStyle.Left;
            pnlRow.Controls.Add(pnlBubble);

            flowChatMessages.Controls.Add(pnlRow);
            flowChatMessages.ScrollControlIntoView(pnlRow);
        }

        private GraphicsPath DrawSmoothBubble(Rectangle r, int radius, bool isMe)
        {
            // (Giữ nguyên code vẽ bong bóng của bạn)
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2; int tailSize = 8;
            r.Width -= 1; r.Height -= 1;
            if (isMe)
            {
                path.AddArc(r.X, r.Y, d, d, 180, 90);
                path.AddArc(r.Right - d - tailSize, r.Y, d, d, 270, 90);
                path.AddLine(r.Right - tailSize, r.Y + radius, r.Right - tailSize, r.Bottom - radius);
                path.AddArc(r.Right - d - tailSize, r.Bottom - d, d, d, 0, 90);
                path.AddLine(r.Right - tailSize - radius, r.Bottom, r.Right, r.Bottom);
                path.AddLine(r.Right, r.Bottom, r.Right - tailSize, r.Bottom - tailSize);
                path.AddLine(r.Right - tailSize, r.Bottom - tailSize, r.X + radius, r.Bottom);
                path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            }
            else
            {
                path.AddArc(r.X + tailSize, r.Y, d, d, 180, 90);
                path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
                path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
                path.AddLine(r.Right - radius, r.Bottom, r.X + tailSize, r.Bottom);
                path.AddLine(r.X + tailSize, r.Bottom, r.X, r.Bottom);
                path.AddLine(r.X, r.Bottom, r.X + tailSize, r.Bottom - tailSize);
                path.AddArc(r.X + tailSize, r.Bottom - d, d, d, 90, 90);
            }
            path.CloseFigure();
            return path;
        }

        private void btnClose_Click(object sender, EventArgs e) => this.Close();
        private void btnBack_Click(object sender, EventArgs e) => this.Close();
        private void btnAddFile_Click(object sender, EventArgs e) { }
        private void pnlInputBackground_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, ((Panel)sender).Width - 1, ((Panel)sender).Height - 1);
            using (GraphicsPath path = GetRoundedRectPath(rect, 20))
            using (Pen pen = new Pen(Color.LightGray, 1))
            using (Brush brush = new SolidBrush(Color.FromArgb(240, 240, 240)))
            {
                g.FillPath(brush, path);
                g.DrawPath(pen, path);
            }
        }
        private GraphicsPath GetRoundedRectPath(Rectangle bounds, int radius)
        {
            int diameter = radius * 2; Size size = new Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();
            if (radius == 0) { path.AddRectangle(bounds); return path; }
            path.AddArc(arc, 180, 90); arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90); arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90); arc.X = bounds.Left;
            path.AddArc(arc, 90, 90); path.CloseFigure(); return path;
        }
    }
}