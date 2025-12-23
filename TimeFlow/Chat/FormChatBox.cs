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

        private TcpClient _client;
        private NetworkStream _stream;
        private Thread _listenThread;
        private string _myUsername;
        private string _currentReceiver = "";
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
            if (lblChatTitle != null) lblChatTitle.Text = "Chọn một người để bắt đầu chat";

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
                        ProcessServerMessage(json);
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
                            if (root.TryGetProperty("users", out JsonElement usersElem))
                            {
                                var users = JsonSerializer.Deserialize<string[]>(usersElem.GetRawText());
                                this.Invoke((MethodInvoker)delegate { UpdateSidebar(users); });
                            }
                        }
                        else if (type == "receive_message")
                        {
                            string sender = root.GetProperty("sender").GetString();
                            string content = root.GetProperty("content").GetString();
                            string time = root.GetProperty("timestamp").GetString();

                            this.Invoke((MethodInvoker)delegate
                            {
                                if (sender == _currentReceiver || sender == _myUsername)
                                {
                                    bool isMe = (sender == _myUsername);
                                    AddMessageBubble(content, isMe, time);
                                }
                            });
                        }
                        else if (type == "history_data")
                        {
                            if (root.TryGetProperty("data", out JsonElement dataArray))
                            {
                                this.Invoke((MethodInvoker)delegate
                                {
                                    flowChatMessages.Controls.Clear();
                                    foreach (JsonElement msg in dataArray.EnumerateArray())
                                    {
                                        string sender = msg.GetProperty("SenderUsername").GetString();
                                        string content = msg.GetProperty("Content").GetString();
                                        DateTime time = msg.GetProperty("Timestamp").GetDateTime();
                                        bool isMe = (sender == _myUsername);

                                        AddMessageBubble(content, isMe, time.ToString("HH:mm"));
                                    }
                                    if (flowChatMessages.Controls.Count > 0)
                                        flowChatMessages.ScrollControlIntoView(flowChatMessages.Controls[flowChatMessages.Controls.Count - 1]);
                                });
                            }
                        }
                    }
                }
            }
            catch {  }
        }


        private void UpdateSidebar(string[] users)
        {
            if (flowSidebar == null) return;
            flowSidebar.Controls.Clear();

            foreach (var user in users)
            {
                if (user == _myUsername) continue;

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

                btn.Click += (s, e) => SwitchUser(user);
                flowSidebar.Controls.Add(btn);
            }
        }

        private void SwitchUser(string targetUser)
        {
            _currentReceiver = targetUser;
            lblChatTitle.Text = targetUser;
            flowChatMessages.Controls.Clear();

            foreach (Control c in flowSidebar.Controls)
            {
                if (c is Button b)
                {
                    bool isTarget = (b.Text == targetUser);
                    b.BackColor = isTarget ? Color.AliceBlue : Color.White;
                    b.ForeColor = isTarget ? Color.DodgerBlue : Color.Black;
                }
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            string msg = txtMessage.Text.Trim();
            if (string.IsNullOrEmpty(msg)) return;
            if (string.IsNullOrEmpty(_currentReceiver))
            {
                MessageBox.Show("Chọn người để chat trước!");
                return;
            }

            if (_isConnected)
            {
                SendJson(new { type = "chat", receiver = _currentReceiver, content = msg });
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
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi gửi: " + ex.Message);
            }
        }

        private void AddMessageBubble(string message, bool isMe, string time)
        {
            if (flowChatMessages == null) return;

            Panel pnlRow = new Panel();
            pnlRow.Width = flowChatMessages.ClientSize.Width - 25;
            pnlRow.Height = 50;
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

            Label lblTime = new Label();
            lblTime.Text = time;
            lblTime.Font = new Font("Arial", 8, FontStyle.Italic);
            lblTime.ForeColor = isMe ? Color.FromArgb(230, 230, 230) : Color.Gray;
            lblTime.AutoSize = true;

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
    }
}