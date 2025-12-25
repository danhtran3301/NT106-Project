using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Windows.Forms;
using TimeFlow.Models;
using TimeFlow.Services;
using TimeFlow.Configuration;

namespace TimeFlow
{
    // Class hứng dữ liệu nhóm từ Server
    public class GroupDto
    {
        public int groupId { get; set; }
        public string groupName { get; set; }
        public string description { get; set; }
    }

    public partial class FormChatBox : Form
    {
        // --- CẤU HÌNH MẠNG ---
        private TcpClient _client;
        private NetworkStream _stream;
        private Thread _listenThread;
        private bool _isConnected = false;

        // Thông tin User
        private string _myUsername;

        // Trạng thái Chat hiện tại
        private int? _currentGroupId = null; // ID nhóm đang chọn
        private string _currentReceiver = ""; // Tên người/nhóm nhận
        private bool _isGroupChat = false;    // Cờ đánh dấu đang chat nhóm

        // --- CONSTRUCTOR MẶC ĐỊNH ---
        public FormChatBox()
        {
            InitializeComponent();
            _myUsername = SessionManager.Username ?? "Guest";
            ConnectToServer();
        }

        // --- CONSTRUCTOR VỚI GROUP ID (Mở chat cho group cụ thể) ---
        public FormChatBox(int groupId, string groupName) : this()
        {
            // Set ngay group đang chat
            _currentGroupId = groupId;
            _currentReceiver = groupName;
            _isGroupChat = true;
            
            // Cập nhật UI
            lblChatTitle.Text = $"💬 {groupName}";
            this.Text = $"TimeFlow Chat - {groupName}";
            
            // Load lịch sử chat cho group này sau khi connected
            if (_isConnected)
            {
                LoadGroupChatHistory(groupId);
            }
        }

        private void ConnectToServer()
        {
            try
            {
                _client = new TcpClient();
                _client.Connect(ServerConfig.Host, ServerConfig.Port);
                _stream = _client.GetStream();
                _isConnected = true;

                // ✅ SỬA: Dùng autologin với token từ SessionManager thay vì login với password hardcoded
                if (!string.IsNullOrEmpty(SessionManager.Token))
                {
                    var autoLoginPacket = new { type = "autologin", token = SessionManager.Token };
                    string json = JsonSerializer.Serialize(autoLoginPacket);
                    SendString(json);
                }
                else
                {
                    // Fallback: Nếu không có token, thông báo lỗi
                    MessageBox.Show("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.", "Lỗi", 
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    this.Close();
                    return;
                }

                // 2. Gửi yêu cầu lấy danh sách nhóm (Đợi 0.5s để server xử lý login xong)
                Thread.Sleep(500);
                var getGroupsPacket = new { type = "get_my_groups", token = SessionManager.Token };
                SendString(JsonSerializer.Serialize(getGroupsPacket));

                // Bắt đầu luồng lắng nghe tin nhắn
                StartListening();

                if (!_isGroupChat)
                {
                    this.Text = $"TimeFlow Chat - Logged in as: {_myUsername}";
                }
                AppendSystemMessage($"Connected to server as {_myUsername}");
                
                // ✅ Load lịch sử chat nếu đã chọn group
                if (_currentGroupId.HasValue)
                {
                    LoadGroupChatHistory(_currentGroupId.Value);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể kết nối tới Chat Server: " + ex.Message);
                this.Text = "TimeFlow Chat - Disconnected";
            }
        }

        // ✅ MỚI: Load lịch sử chat cho group
        private void LoadGroupChatHistory(int groupId)
        {
            try
            {
                var request = new 
                { 
                    type = "get_group_chat_history", 
                    token = SessionManager.Token,
                    groupId = groupId 
                };
                SendString(JsonSerializer.Serialize(request));
            }
            catch (Exception ex)
            {
                AppendSystemMessage($"Failed to load chat history: {ex.Message}");
            }
        }

        // --- XỬ LÝ GỬI NHẬN ---

        private void SendString(string data)
        {
            if (!_isConnected) return;
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(data);
                _stream.Write(bytes, 0, bytes.Length);
            }
            catch { _isConnected = false; }
        }

        private void StartListening()
        {
            _listenThread = new Thread(() =>
            {
                byte[] buffer = new byte[40960]; // Tăng buffer để nhận list nhóm lớn
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
                // Xử lý gói tin JSON từ server
                using (JsonDocument doc = JsonDocument.Parse(json))
                {
                    JsonElement root = doc.RootElement;
                    if (root.TryGetProperty("type", out JsonElement typeElem))
                    {
                        string type = typeElem.GetString();

                        // ✅ CASE: Autologin response
                        if (root.TryGetProperty("status", out JsonElement statusElem))
                        {
                            string status = statusElem.GetString();
                            if (status == "autologin_success")
                            {
                                this.Invoke((MethodInvoker)delegate {
                                    AppendSystemMessage("✓ Authenticated successfully");
                                });
                                return;
                            }
                            else if (status == "autologin_fail")
                            {
                                this.Invoke((MethodInvoker)delegate {
                                    AppendSystemMessage("⚠ Authentication failed. Please re-login.");
                                });
                                return;
                            }
                        }

                        // CASE 1: Nhận danh sách nhóm
                        if (type == "my_groups_list")
                        {
                            if (root.GetProperty("status").GetString() == "success")
                            {
                                var dataStr = root.GetProperty("data").ToString();
                                var groups = JsonSerializer.Deserialize<List<GroupDto>>(dataStr);

                                // Vẽ lên giao diện (Thread safe)
                                this.Invoke((MethodInvoker)delegate {
                                    RenderGroupsToSidebar(groups);
                                    
                                    // Nếu đã có group được chọn sẵn, highlight nó
                                    if (_currentGroupId.HasValue)
                                    {
                                        HighlightSelectedGroup(_currentGroupId.Value);
                                    }
                                });
                            }
                        }

                        // CASE 2: Nhận tin nhắn chat nhóm
                        if (type == "receive_group_message")
                        {
                            string sender = root.GetProperty("sender").GetString();
                            string content = root.GetProperty("content").GetString();
                            int groupId = root.GetProperty("groupId").GetInt32();

                            // Chỉ hiện tin nhắn nếu đúng group đang chat
                            if (_currentGroupId.HasValue && groupId == _currentGroupId.Value)
                            {
                                bool isMe = sender == _myUsername;
                                this.Invoke((MethodInvoker)delegate {
                                    AddMessageBubble(content, isMe ? "Me" : sender, isMe);
                                });
                            }
                        }

                        // CASE 3: Nhận tin nhắn chat 1-1
                        if (type == "receive_message")
                        {
                            string sender = root.GetProperty("sender").GetString();
                            string content = root.GetProperty("content").GetString();

                            // Chat 1-1: hiện nếu đúng người đang chat
                            if (!_isGroupChat && sender == _currentReceiver)
                            {
                                this.Invoke((MethodInvoker)delegate {
                                    AddMessageBubble(content, sender, false);
                                });
                            }
                        }
                        
                        // ✅ CASE 4: Nhận lịch sử chat nhóm
                        if (type == "group_chat_history")
                        {
                            if (root.GetProperty("status").GetString() == "success")
                            {
                                int groupId = root.GetProperty("groupId").GetInt32();
                                
                                // Chỉ render nếu đúng group đang xem
                                if (_currentGroupId.HasValue && groupId == _currentGroupId.Value)
                                {
                                    this.Invoke((MethodInvoker)delegate {
                                        flowChatMessages.Controls.Clear();
                                        
                                        if (root.TryGetProperty("messages", out JsonElement messagesElem))
                                        {
                                            foreach (var msg in messagesElem.EnumerateArray())
                                            {
                                                string sender = msg.GetProperty("sender").GetString();
                                                string content = msg.GetProperty("content").GetString();
                                                bool isMe = sender == _myUsername;
                                                AddMessageBubble(content, isMe ? "Me" : sender, isMe);
                                            }
                                        }
                                        
                                        if (flowChatMessages.Controls.Count == 0)
                                        {
                                            AppendSystemMessage("No messages yet. Start the conversation!");
                                        }
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch { }
        }

        // --- UI LOGIC: VẼ DANH SÁCH NHÓM ---

        private void RenderGroupsToSidebar(List<GroupDto> groups)
        {
            flowSidebar.Controls.Clear();

            foreach (var group in groups)
            {
                // Panel chứa 1 item group
                Panel pnlItem = new Panel();
                pnlItem.Size = new Size(flowSidebar.Width - 25, 70);
                pnlItem.BackColor = Color.White;
                pnlItem.Cursor = Cursors.Hand;
                pnlItem.Margin = new Padding(10, 5, 10, 5);
                pnlItem.Tag = group.groupId; // Lưu groupId để highlight sau

                // Avatar chữ cái đầu
                Label lblAvatar = new Label();
                lblAvatar.Text = group.groupName.Substring(0, 1).ToUpper();
                lblAvatar.Size = new Size(45, 45);
                lblAvatar.Location = new Point(10, 12);
                lblAvatar.TextAlign = ContentAlignment.MiddleCenter;
                lblAvatar.BackColor = Color.DodgerBlue;
                lblAvatar.ForeColor = Color.White;
                lblAvatar.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                // Bo tròn avatar
                GraphicsPath path = new GraphicsPath();
                path.AddEllipse(0, 0, 45, 45);
                lblAvatar.Region = new Region(path);

                // Tên nhóm
                Label lblName = new Label();
                lblName.Text = group.groupName;
                lblName.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                lblName.Location = new Point(65, 15);
                lblName.AutoSize = true;

                // Mô tả ngắn
                Label lblDesc = new Label();
                lblDesc.Text = group.description ?? "Group Chat";
                lblDesc.Font = new Font("Segoe UI", 8, FontStyle.Regular);
                lblDesc.ForeColor = Color.Gray;
                lblDesc.Location = new Point(65, 40);
                lblDesc.AutoSize = true;

                // Sự kiện Click chọn nhóm
                EventHandler clickEvent = (s, e) =>
                {
                    // Reset màu các item khác
                    foreach (Control c in flowSidebar.Controls) c.BackColor = Color.White;
                    pnlItem.BackColor = Color.FromArgb(230, 240, 255); // Highlight màu xanh nhạt

                    // Cập nhật trạng thái
                    _currentGroupId = group.groupId;
                    _currentReceiver = group.groupName;
                    _isGroupChat = true;

                    lblChatTitle.Text = $"💬 {group.groupName}";
                    flowChatMessages.Controls.Clear(); // Xóa chat cũ
                    
                    // ✅ Load lịch sử chat từ server
                    LoadGroupChatHistory(group.groupId);
                    
                    AppendSystemMessage($"Now chatting in: {group.groupName}");
                };

                pnlItem.Click += clickEvent;
                lblAvatar.Click += clickEvent;
                lblName.Click += clickEvent;
                lblDesc.Click += clickEvent;

                pnlItem.Controls.Add(lblAvatar);
                pnlItem.Controls.Add(lblName);
                pnlItem.Controls.Add(lblDesc);

                flowSidebar.Controls.Add(pnlItem);
            }
        }

        private void HighlightSelectedGroup(int groupId)
        {
            foreach (Control c in flowSidebar.Controls)
            {
                if (c is Panel pnl && pnl.Tag != null && (int)pnl.Tag == groupId)
                {
                    pnl.BackColor = Color.FromArgb(230, 240, 255);
                }
                else
                {
                    c.BackColor = Color.White;
                }
            }
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

            // Kiểm tra xem đã chọn ai để chat chưa
            if (string.IsNullOrEmpty(_currentReceiver) && _currentGroupId == null)
            {
                MessageBox.Show("Vui lòng chọn một nhóm hoặc user để chat!");
                return;
            }

            try
            {
                object packet;

                if (_isGroupChat && _currentGroupId != null)
                {
                    packet = new
                    {
                        type = "chat",
                        receiver = _currentGroupId.ToString(), 
                        isGroup = true,
                        content = content
                    };
                }
                else
                {
                    packet = new
                    {
                        type = "chat",
                        receiver = string.IsNullOrEmpty(_currentReceiver) ? "UserB" : _currentReceiver,
                        content = content
                    };
                }

                SendString(JsonSerializer.Serialize(packet));
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
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendSystemMessage), msg);
                return;
            }
            Label lbl = new Label();
            lbl.Text = msg;
            lbl.ForeColor = Color.Gray;
            lbl.Font = new Font("Segoe UI", 8, FontStyle.Italic);
            lbl.AutoSize = true;
            lbl.Padding = new Padding(0, 5, 0, 5);
            lbl.Dock = DockStyle.Top;
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
            lblContent.BackColor = Color.Transparent;

            // Time
            Label lblTime = new Label();
            lblTime.Text = DateTime.Now.ToString("HH:mm");
            lblTime.Font = new Font("Arial", 8, FontStyle.Italic);
            lblTime.ForeColor = isMe ? Color.FromArgb(220, 220, 220) : Color.Gray;
            lblTime.AutoSize = true;
            lblTime.BackColor = Color.Transparent;

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
        private void btnCreateGroup_Click(object sender, EventArgs e) { MessageBox.Show("Tính năng tạo nhóm đang phát triển!"); }
        
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _isConnected = false;
            try { _stream?.Close(); } catch { }
            try { _client?.Close(); } catch { }
            base.OnFormClosing(e);
        }
    }
}