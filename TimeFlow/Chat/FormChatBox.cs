using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;

namespace TimeFlow
{
    public partial class ChatForm : Form
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private Thread _listenThread;
        private string _myUsername;
        private string _currentReceiver = "";
        private int? _currentGroupId = null; 
        private bool _isConnected = false;

        public ChatForm()
        {
            InitializeComponent();
        }

        public ChatForm(TcpClient client, string myUsername)
        {
            InitializeComponent();
            _client = client;
            _stream = client.GetStream();
            _myUsername = myUsername;
            _isConnected = true;

            this.Text = $"TimeFlow Chat - {_myUsername}";
            if (lblChatTitle != null) lblChatTitle.Text = "Chọn một người hoặc nhóm để bắt đầu";

            StartListening();
        }

        private void StartListening()
        {
            _listenThread = new Thread(() =>
            {
                try
                {
                    byte[] buffer = new byte[8192];
                    while (_isConnected && _client != null && _client.Connected)
                    {
                        int bytesRead = _stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0) break;

                        string json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        this.Invoke((MethodInvoker)delegate {
                            ProcessServerMessage(json);
                        });
                    }
                }
                catch
                {
                    _isConnected = false;
                }
            });
            _listenThread.IsBackground = true;
            _listenThread.Start();
        }

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
                            var users = JsonSerializer.Deserialize<string[]>(root.GetProperty("users").GetRawText());
                            UpdateSidebar(users, new List<TimeFlow.Models.Group>());
                        }
                        else if (type == "receive_message")
                        {
                            string sender = root.GetProperty("sender").GetString();
                            string content = root.GetProperty("content").GetString();
                            string time = root.GetProperty("timestamp").GetString();

                            if (sender == _currentReceiver || sender == _myUsername)
                            {
                                AddMessageBubble(content, sender == _myUsername, time);
                            }
                        }
                        else if (type == "receive_group_message")
                        {
                            int groupId = root.GetProperty("groupId").GetInt32();
                            string sender = root.GetProperty("sender").GetString();
                            string content = root.GetProperty("content").GetString();
                            string time = root.GetProperty("timestamp").GetString();

                            if (_currentGroupId == groupId)
                            {
                                AddMessageBubble(content, sender == _myUsername, time);
                            }
                        }
                        else if (type == "history_data")
                        {
                            flowChatMessages.Controls.Clear();
                            foreach (JsonElement msg in root.GetProperty("data").EnumerateArray())
                            {
                                string sender = msg.GetProperty("SenderUsername").GetString();
                                string content = msg.GetProperty("Content").GetString();
                                string time = msg.GetProperty("Timestamp").GetDateTime().ToString("HH:mm");
                                AddMessageBubble(content, sender == _myUsername, time);
                            }
                            ScrollToBottom();
                        }
                        else if (type == "group_history_response")
                        {
                            flowChatMessages.Controls.Clear();
                            foreach (JsonElement msg in root.GetProperty("messages").EnumerateArray())
                            {
                                string sender = msg.GetProperty("Sender").GetString();
                                string content = msg.GetProperty("Content").GetString();
                                string time = msg.GetProperty("Time").GetDateTime().ToString("HH:mm");
                                AddMessageBubble(content, sender == _myUsername, time);
                            }
                            ScrollToBottom();
                        }
                    }
                }
            }
            catch { }
        }

        // ================== GIAO DIỆN SIDEBAR ==================
        public void UpdateSidebar(string[] onlineUsers, List<TimeFlow.Models.Group> myGroups)
        {
            if (flowSidebar == null) return;
            flowSidebar.Controls.Clear();

            // Hiển thị các Nhóm
            foreach (var group in myGroups)
            {
                Button btnGroup = CreateSidebarButton($"[G] {group.GroupName}", Color.LightGreen);
                btnGroup.Click += (s, e) => {
                    _currentGroupId = group.GroupId;
                    _currentReceiver = "";
                    lblChatTitle.Text = $"Group: {group.GroupName}";
                    HighlightActiveButton(btnGroup);
                    flowChatMessages.Controls.Clear();
                    SendJson(new { type = "get_group_history", groupId = group.GroupId });
                };
                flowSidebar.Controls.Add(btnGroup);
            }

            // Hiển thị các User
            foreach (var user in onlineUsers)
            {
                if (user == _myUsername) continue;
                Button btnUser = CreateSidebarButton(user, Color.AliceBlue);
                btnUser.Click += (s, e) => {
                    _currentGroupId = null;
                    _currentReceiver = user;
                    lblChatTitle.Text = user;
                    HighlightActiveButton(btnUser);
                    flowChatMessages.Controls.Clear();
                    SendJson(new { type = "get_history", target_user = user });
                };
                flowSidebar.Controls.Add(btnUser);
            }
        }

        private Button CreateSidebarButton(string text, Color backColor)
        {
            return new Button
            {
                Text = text,
                Width = flowSidebar.Width - 25,
                Height = 50,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
        }

        private void HighlightActiveButton(Button activeBtn)
        {
            foreach (Control c in flowSidebar.Controls)
                if (c is Button b) { b.BackColor = Color.White; b.ForeColor = Color.Black; }
            activeBtn.BackColor = Color.DodgerBlue;
            activeBtn.ForeColor = Color.White;
        }

        // ================== GỬI TIN NHẮN ==================
        private void btnSend_Click(object sender, EventArgs e)
        {
            string msg = txtMessage.Text.Trim();
            if (string.IsNullOrEmpty(msg)) return;

            if (_currentGroupId == null && string.IsNullOrEmpty(_currentReceiver))
            {
                MessageBox.Show("Hãy chọn một người hoặc nhóm để chat!");
                return;
            }

            if (_isConnected)
            {
                object packet;
                if (_currentGroupId != null)
                    packet = new { type = "chat", groupId = _currentGroupId, content = msg };
                else
                    packet = new { type = "chat", receiver = _currentReceiver, content = msg };

                SendJson(packet);
                AddMessageBubble(msg, true, DateTime.Now.ToString("HH:mm"));
                txtMessage.Clear();
            }
        }

        private void SendJson(object data)
        {
            try
            {
                string json = JsonSerializer.Serialize(data);
                byte[] bytes = Encoding.UTF8.GetBytes(json);
                _stream.Write(bytes, 0, bytes.Length);
            }
            catch (Exception ex) { MessageBox.Show("Lỗi gửi: " + ex.Message); }
        }

        // ================== HIỂN THỊ BONG BÓNG CHAT ==================
        private void AddMessageBubble(string message, bool isMe, string time)
        {
            if (flowChatMessages == null) return;

            Panel pnlRow = new Panel
            {
                Width = flowChatMessages.ClientSize.Width - 25,
                BackColor = Color.Transparent,
                Padding = new Padding(isMe ? 80 : 10, 5, isMe ? 10 : 80, 5)
            };

            Panel pnlBubble = new Panel { BackColor = Color.Transparent };
            Label lblContent = new Label
            {
                Text = message,
                Font = new Font("Segoe UI", 11),
                ForeColor = isMe ? Color.White : Color.Black,
                AutoSize = true,
                MaximumSize = new Size(pnlRow.Width - 120, 0),
                Location = new Point(isMe ? 12 : 18, 10)
            };

            Label lblTime = new Label
            {
                Text = time,
                Font = new Font("Arial", 8, FontStyle.Italic),
                ForeColor = isMe ? Color.FromArgb(230, 230, 230) : Color.Gray,
                AutoSize = true
            };

            pnlBubble.Controls.Add(lblContent);
            pnlBubble.Controls.Add(lblTime);

            Size textSize = lblContent.GetPreferredSize(new Size(pnlRow.Width - 120, 0));
            lblTime.Location = new Point(lblContent.Left, lblContent.Top + textSize.Height + 2);
            pnlBubble.Size = new Size(textSize.Width + 40, textSize.Height + 35);

            pnlBubble.Paint += (s, e) => {
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
            ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            if (flowChatMessages.Controls.Count > 0)
                flowChatMessages.ScrollControlIntoView(flowChatMessages.Controls[flowChatMessages.Controls.Count - 1]);
        }

        // ================== ĐỒ HỌA & TIỆN ÍCH ==================
        private GraphicsPath DrawSmoothBubble(Rectangle r, int radius, bool isMe)
        {
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

        private void btnClose_Click(object sender, EventArgs e) => this.Close();
        private void btnBack_Click(object sender, EventArgs e) => this.Close();
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
    }
}