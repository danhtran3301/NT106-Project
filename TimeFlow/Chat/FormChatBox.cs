using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Windows.Forms;

namespace TimeFlow
{
    public partial class ChatForm : Form
    {
        // --- CẤU HÌNH MẠNG ---
        private TcpClient _client;
        private NetworkStream _stream;
        private Thread _listenThread;
        private string _myUsername;
        private string _currentReceiver = "UserB"; // Mặc định chat với UserB (sẽ đổi khi click list user)
        private bool _isConnected = false;

        // Constructor mặc định (Dùng cho testing)
        public ChatForm()
        {
            InitializeComponent();
            // Tự động kết nối giả lập nếu chạy form này trực tiếp
            ConnectToServer("UserA");
        }

        // Constructor chính (Được gọi từ Form Login sau khi đăng nhập thành công)
        public ChatForm(TcpClient client, string myUsername)
        {
            InitializeComponent();
            _client = client;
            _stream = client.GetStream();
            _myUsername = myUsername;
            _isConnected = true;

            this.Text = $"Chat App - {_myUsername}";

            // Bắt đầu lắng nghe tin nhắn
            StartListening();
        }

        // Hàm xử lý gói tin danh sách user từ Server
        private void UpdateSidebar(string[] users)
        {
            // Xóa danh sách cũ
            flowSidebar.Controls.Clear();

            foreach (var user in users)
            {
                if (user == _myUsername) continue; // Không hiện chính mình

                Button btn = new Button();
                btn.Text = user;
                btn.Width = flowSidebar.Width - 25;
                btn.Height = 50;
                btn.TextAlign = ContentAlignment.MiddleLeft;
                btn.Padding = new Padding(20, 0, 0, 0);
                btn.FlatStyle = FlatStyle.Flat;
                btn.FlatAppearance.BorderSize = 0;
                btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                btn.Cursor = Cursors.Hand;

                // Kiểm tra xem nút này có phải người đang chat không để đổi màu
                if (user == _currentReceiver)
                {
                    btn.BackColor = Color.AliceBlue;
                    btn.ForeColor = Color.DodgerBlue;
                }
                else
                {
                    btn.BackColor = Color.White;
                    btn.ForeColor = Color.Black;
                }

                // SỰ KIỆN QUAN TRỌNG: CLICK ĐỂ CHỌN NGƯỜI CHAT
                btn.Click += (s, e) => SwitchChatUser(user);

                flowSidebar.Controls.Add(btn);
            }
        }

        // Hàm đổi người chat
        private void SwitchChatUser(string targetUser)
        {
            _currentReceiver = targetUser;
            lblChatTitle.Text = targetUser; // Cập nhật tên trên Header

            // Highlight lại sidebar để biết đang chọn ai
            foreach (Control c in flowSidebar.Controls)
            {
                if (c is Button btn)
                {
                    if (btn.Text == targetUser)
                    {
                        btn.BackColor = Color.AliceBlue;
                        btn.ForeColor = Color.DodgerBlue;
                    }
                    else
                    {
                        btn.BackColor = Color.White;
                        btn.ForeColor = Color.Black;
                    }
                }
            }

            // Quan trọng: Xóa chat cũ của người trước
            flowChatMessages.Controls.Clear();

            // TODO: Ở đây bạn sẽ gửi lệnh lên Server để lấy lịch sử chat cũ
            // Ví dụ: SendJson(new { type = "get_history", with_user = targetUser });
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
                    byte[] buffer = new byte[4096];
                    while (_isConnected)
                    {
                        int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0) break;

                        string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        ProcessServerMessage(json);
                    }
                }
                catch
                {
                    // Ngắt kết nối hoặc lỗi mạng
                    _isConnected = false;
                }
            });
            _listenThread.IsBackground = true;
            _listenThread.Start();
        }

        // Xử lý gói tin JSON nhận được
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
                        if (type == "user_list")
                        {
                            // Lấy mảng users từ JSON
                            var users = root.GetProperty("users").Deserialize<string[]>();

                            // Cập nhật UI (Sidebar)
                            this.Invoke((MethodInvoker)delegate
                            {
                                UpdateSidebar(users);
                            });
                        }
                        // Nếu là tin nhắn chat đến
                        else if (type == "receive_message")
                        {
                            string sender = root.GetProperty("sender").GetString();
                            string content = root.GetProperty("content").GetString();
                            string time = root.GetProperty("timestamp").GetString();

                            // Update UI phải dùng Invoke
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
        // --- HÀM 3: GỬI TIN NHẮN ---
        private void btnSend_Click(object sender, EventArgs e)
        {
            string msg = txtMessage.Text.Trim();
            if (string.IsNullOrEmpty(msg)) return;

            if (_isConnected)
            {
                try
                {
                    // 1. Tạo JSON gửi đi
                    var packet = new
                    {
                        type = "chat",
                        receiver = _currentReceiver, // Người nhận hiện tại
                        content = msg
                    };

                    string jsonToSend = JsonSerializer.Serialize(packet);
                    byte[] bytes = Encoding.UTF8.GetBytes(jsonToSend);
                    _stream.Write(bytes, 0, bytes.Length);

                    // 2. Hiển thị ngay lên màn hình của mình
                    AddMessageBubble(msg, true, DateTime.Now.ToString("HH:mm"));
                    txtMessage.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi gửi tin: " + ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Mất kết nối tới Server!");
            }
        }

        // --- UI LOGIC: VẼ BONG BÓNG CHAT (ĐÃ TỐI ƯU) ---
        private void AddMessageBubble(string message, bool isMe, string time)
        {
            // 1. Panel chứa 1 dòng tin nhắn
            Panel pnlRow = new Panel();
            pnlRow.Width = flowChatMessages.ClientSize.Width - 25;
            pnlRow.BackColor = Color.Transparent;
            pnlRow.Padding = new Padding(isMe ? 80 : 10, 5, isMe ? 10 : 80, 5); // Padding sâu hơn để tin nhắn ngắn lại

            // 2. Bong bóng màu
            Panel pnlBubble = new Panel();
            pnlBubble.BackColor = Color.Transparent;

            // 3. Label Nội dung
            Label lblContent = new Label();
            lblContent.Text = message;
            lblContent.Font = new Font("Segoe UI", 11);
            lblContent.ForeColor = isMe ? Color.White : Color.Black;
            lblContent.AutoSize = true;
            lblContent.MaximumSize = new Size(pnlRow.Width - 120, 0); // Giới hạn chiều rộng text
            lblContent.Location = new Point(isMe ? 12 : 18, 10); // Cách lề để chừa chỗ cho đuôi
            lblContent.BackColor = Color.Transparent;

            // 4. Label Thời gian (Mới thêm)
            Label lblTime = new Label();
            lblTime.Text = time;
            lblTime.Font = new Font("Arial", 8, FontStyle.Italic);
            lblTime.ForeColor = isMe ? Color.FromArgb(230, 230, 230) : Color.Gray;
            lblTime.AutoSize = true;
            lblTime.BackColor = Color.Transparent;

            // Add control để tính size
            pnlBubble.Controls.Add(lblContent);
            pnlBubble.Controls.Add(lblTime);

            // 5. Tính toán kích thước dynamic
            Size textSize = lblContent.GetPreferredSize(new Size(pnlRow.Width - 120, 0));

            // Set vị trí thời gian nằm dưới text
            lblTime.Location = new Point(lblContent.Left, lblContent.Top + textSize.Height + 2);

            pnlBubble.Size = new Size(textSize.Width + 40, textSize.Height + 35); // +35 cho padding và timestamp

            // 6. Sự kiện vẽ
            pnlBubble.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Màu sắc: Xanh Messenger (Mình) - Xám (Bạn)
                Color color = isMe ? Color.FromArgb(0, 132, 255) : Color.FromArgb(240, 242, 245);
                using (var brush = new SolidBrush(color))
                {
                    var path = DrawSmoothBubble(pnlBubble.ClientRectangle, 15, isMe);
                    g.FillPath(brush, path);
                }
            };

            // Layout
            pnlRow.Height = pnlBubble.Height + 10;
            pnlBubble.Dock = isMe ? DockStyle.Right : DockStyle.Left;
            pnlRow.Controls.Add(pnlBubble);

            flowChatMessages.Controls.Add(pnlRow);

            // Auto-scroll xuống cuối
            flowChatMessages.ControlAdded += (s, e) => flowChatMessages.ScrollControlIntoView(pnlRow);
            flowChatMessages.ScrollControlIntoView(pnlRow);
        }

        // --- LOGIC VẼ HÌNH DÁNG (GIỮ NGUYÊN NHƯNG CHỈNH RADIUS) ---
        private GraphicsPath DrawSmoothBubble(Rectangle r, int radius, bool isMe)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            int tailSize = 8;
            r.Width -= 1; r.Height -= 1;

            if (isMe)
            {
                path.AddArc(r.X, r.Y, d, d, 180, 90);
                path.AddArc(r.Right - d - tailSize, r.Y, d, d, 270, 90);
                path.AddLine(r.Right - tailSize, r.Y + radius, r.Right - tailSize, r.Bottom - radius);
                path.AddArc(r.Right - d - tailSize, r.Bottom - d, d, d, 0, 90);
                // Đuôi phải
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
                // Đuôi trái
                path.AddLine(r.X + tailSize, r.Bottom, r.X, r.Bottom);
                path.AddLine(r.X, r.Bottom, r.X + tailSize, r.Bottom - tailSize);
                path.AddArc(r.X + tailSize, r.Bottom - d, d, d, 90, 90);
            }
            path.CloseFigure();
            return path;
        }

        // Các event handler phụ
        private void btnClose_Click(object sender, EventArgs e) => this.Close();
        private void btnBack_Click(object sender, EventArgs e) => this.Close();
        private void btnAddFile_Click(object sender, EventArgs e) { /* Tính năng gửi file */ }

        // Vẽ ô nhập liệu
        private void pnlInputBackground_Paint(object sender, PaintEventArgs e)
        {
            // Code vẽ bo tròn ô input (giữ nguyên)
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
    }
}