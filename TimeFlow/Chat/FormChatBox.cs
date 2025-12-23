using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Windows.Forms;

namespace TimeFlow
{
    public partial class FormChatBox : Form
    {
        // --- CẤU HÌNH MẠNG ---
        private TcpClient _client;
        private NetworkStream _stream;
        private Thread _listenThread;
        private bool _isConnected = false;

        // Thông tin User (Vì không truyền vào nên ta sẽ tự tạo hoặc lấy mặc định)
        private string _myUsername;

        // Cấu hình Server mặc định
        private const string SERVER_IP = "127.0.0.1";
        private const int SERVER_PORT = 5000;

        // Quản lý Chat Group
        // Key: GroupId (dạng string "g_1"), Value: Nội dung chat
        private Dictionary<string, List<string>> _chatHistory = new Dictionary<string, List<string>>();

        // --- CONSTRUCTOR KHÔNG THAM SỐ (ĐỂ MATCH VỚI GROUP TASK LIST) ---
        public FormChatBox()
        {
            InitializeComponent();

            // Tự động tạo Username ngẫu nhiên để test (Vì không nhận được từ form cha)
            // Trong thực tế, bạn có thể lấy từ biến toàn cục: Program.CurrentUser.Username
            _myUsername = "User_" + new Random().Next(100, 999);

            // Tự động kết nối tới Server
            ConnectToServer();
        }

        private void ConnectToServer()
        {
            try
            {
                _client = new TcpClient();
                _client.Connect(SERVER_IP, SERVER_PORT);
                _stream = _client.GetStream();
                _isConnected = true;

                // Gửi gói tin Login để xác thực với Server
                var loginPacket = new { type = "login", username = _myUsername, password = "password" };
                string json = JsonSerializer.Serialize(loginPacket);
                SendString(json);

                // Bắt đầu luồng lắng nghe tin nhắn
                StartListening();

                this.Text = $"TimeFlow Chat - Logged in as: {_myUsername}";
                AppendSystemMessage($"Connected to server as {_myUsername}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể kết nối tới Chat Server: " + ex.Message);
                this.Text = "TimeFlow Chat - Disconnected";
            }
        }

        // --- XỬ LÝ GỬI NHẬN ---

        private void SendString(string data)
        {
            if (!_isConnected) return;
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            _stream.Write(bytes, 0, bytes.Length);
        }

        private void StartListening()
        {
            _listenThread = new Thread(() =>
            {
                byte[] buffer = new byte[4096];
                while (_isConnected)
                {
                    try
                    {
                        int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0) break; // Ngắt kết nối

                        string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        ProcessIncomingMessage(json);
                    }
                    catch
                    {
                        _isConnected = false;
                        break;
                    }
                }
            });
            _listenThread.IsBackground = true;
            _listenThread.Start();
        }

        private void ProcessIncomingMessage(string json)
        {
            try
            {
                // Xử lý gói tin JSON từ server (chat, group_chat, v.v.)
                // Ở đây demo hiển thị mọi tin nhắn nhận được
                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    JsonElement root = doc.RootElement;
                    if (root.TryGetProperty("type", out JsonElement typeElem))
                    {
                        string type = typeElem.GetString();

                        // Xử lý tin nhắn chat
                        if (type == "receive_message" || type == "receive_group_message")
                        {
                            string sender = root.GetProperty("sender").GetString();
                            string content = root.GetProperty("content").GetString();

                            // Update UI (Invoke vì đang ở thread khác)
                            this.Invoke((MethodInvoker)delegate {
                                AddMessageBubble(content, sender, false);
                            });
                        }
                    }
                }
            }
            catch { /* Bỏ qua lỗi parse JSON cục bộ */ }
        }

        // --- UI EVENTS ---

        private void btnSend_Click(object sender, EventArgs e)
        {
            string content = txtMessage.Text.Trim();
            if (string.IsNullOrEmpty(content)) return;

            if (!_isConnected)
            {
                MessageBox.Show("Mất kết nối server!");
                return;
            }

            // Gửi tin nhắn (Mặc định chat broadcast hoặc chat group tùy logic server)
            // Ở đây giả lập gửi tin nhắn Chat thông thường
            var packet = new
            {
                type = "chat",
                receiver = "All", // Hoặc ID nhóm
                content = content
            };

            try
            {
                SendString(JsonSerializer.Serialize(packet));

                // Hiển thị lên giao diện của mình
                AddMessageBubble(content, "Me", true);
                txtMessage.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi gửi tin: " + ex.Message);
            }
        }

        private void btnClose_Click(object sender, EventArgs e) => this.Close();
        private void btnBack_Click(object sender, EventArgs e) => this.Close();

        // Sự kiện vẽ khung input cho đẹp
        private void pnlInputBackground_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Rectangle rect = new Rectangle(0, 0, pnlInputBackground.Width - 1, pnlInputBackground.Height - 1);
            using (GraphicsPath path = GetRoundedPath(rect, 20))
            using (Pen pen = new Pen(Color.LightGray))
            {
                g.DrawPath(pen, path);
            }
        }

        // --- HELPERS UI ---

        private void AppendSystemMessage(string msg)
        {
            // Hiển thị thông báo hệ thống nhỏ
            Label lbl = new Label();
            lbl.Text = msg;
            lbl.ForeColor = Color.Gray;
            lbl.Font = new Font("Segoe UI", 8, FontStyle.Italic);
            lbl.AutoSize = true;
            lbl.Padding = new Padding(0, 5, 0, 5);
            lbl.Dock = DockStyle.Top; // Add vào flow

            // Hacky way to add to FlowLayout at bottom
            flowChatMessages.Controls.Add(lbl);
            flowChatMessages.ScrollControlIntoView(lbl);
        }

        private void AddMessageBubble(string message, string senderName, bool isMe)
        {
            Panel pnlRow = new Panel();
            pnlRow.Width = flowChatMessages.ClientSize.Width - 25;
            pnlRow.BackColor = Color.Transparent;
            pnlRow.Padding = new Padding(isMe ? 80 : 10, 5, isMe ? 10 : 80, 5);

            Panel pnlBubble = new Panel();
            pnlBubble.BackColor = Color.Transparent;

            // Sender Name
            int nameHeight = 0;
            if (!isMe)
            {
                Label lblSender = new Label();
                lblSender.Text = senderName;
                lblSender.Font = new Font("Segoe UI", 8, FontStyle.Bold);
                lblSender.ForeColor = Color.DimGray;
                lblSender.AutoSize = true;
                lblSender.Location = new Point(15, 2);
                pnlBubble.Controls.Add(lblSender);
                nameHeight = 15;
            }

            // Content
            Label lblContent = new Label();
            lblContent.Text = message;
            lblContent.Font = new Font("Segoe UI", 11);
            lblContent.ForeColor = isMe ? Color.White : Color.Black;
            lblContent.AutoSize = true;
            lblContent.MaximumSize = new Size(pnlRow.Width - 140, 0);
            lblContent.Location = new Point(isMe ? 12 : 18, 5 + nameHeight);

            // Time
            Label lblTime = new Label();
            lblTime.Text = DateTime.Now.ToString("HH:mm");
            lblTime.Font = new Font("Arial", 8, FontStyle.Italic);
            lblTime.ForeColor = isMe ? Color.FromArgb(220, 220, 220) : Color.Gray;
            lblTime.AutoSize = true;

            pnlBubble.Controls.Add(lblContent);
            pnlBubble.Controls.Add(lblTime);

            // Calculate Size
            Size textSize = lblContent.GetPreferredSize(new Size(pnlRow.Width - 140, 0));
            lblTime.Location = new Point(lblContent.Left, lblContent.Top + textSize.Height + 2);
            pnlBubble.Size = new Size(textSize.Width + 40, textSize.Height + 30 + nameHeight);

            // Paint Event
            pnlBubble.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                Color color = isMe ? Color.FromArgb(0, 132, 255) : Color.FromArgb(240, 242, 245);
                using (var brush = new SolidBrush(color))
                {
                    Rectangle r = new Rectangle(0, nameHeight, pnlBubble.Width, pnlBubble.Height - nameHeight);
                    var path = GetRoundedPath(r, 15);
                    g.FillPath(brush, path);
                }
            };

            pnlRow.Height = pnlBubble.Height + 10;
            pnlBubble.Dock = isMe ? DockStyle.Right : DockStyle.Left;
            pnlRow.Controls.Add(pnlBubble);

            flowChatMessages.Controls.Add(pnlRow);
            flowChatMessages.ScrollControlIntoView(pnlRow);
        }

        private GraphicsPath GetRoundedPath(Rectangle r, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int d = radius * 2;
            r.Width -= 1; r.Height -= 1;
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void btnAddFile_Click(object sender, EventArgs e) { }
        private void btnCreateGroup_Click(object sender, EventArgs e) { MessageBox.Show("Tính năng tạo nhóm"); }
    }
}