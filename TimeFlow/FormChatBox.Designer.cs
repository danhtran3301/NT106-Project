using System.Drawing;
using System.Windows.Forms;

namespace BT3_LTMCB
{
    partial class ChatForm
    {
        private System.ComponentModel.IContainer components = null;
        private SplitContainer splitContainerMain;

        // Left Side Controls
        private Panel pnlLeftHeader;
        private Label lblLeftTitle;
        private Button btnBack;
        private FlowLayoutPanel flowLeftGroups;

        // Right Side Controls
        private Panel pnlRightHeader;
        private Label lblRightTitle;
        private Panel pnlInputArea;
        private TextBox txtMessage;
        private Button btnSend;
        private Button btnAddFile;
        private FlowLayoutPanel flowChatMessages; // Nơi chứa tin nhắn

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 600);
            this.Text = "Group Chat Dashboard";

            // 1. Split Container
            this.splitContainerMain = new SplitContainer();
            this.splitContainerMain.Dock = DockStyle.Fill;
            this.splitContainerMain.SplitterDistance = 200; // Sidebar width
            this.splitContainerMain.FixedPanel = FixedPanel.Panel1;
            this.splitContainerMain.IsSplitterFixed = true;

            // --- LEFT SIDEBAR ---
            // Header
            this.pnlLeftHeader = new Panel();
            this.pnlLeftHeader.Dock = DockStyle.Top;
            this.pnlLeftHeader.Height = 60;
            this.pnlLeftHeader.BackColor = ColorTranslator.FromHtml("#9ae6b4"); // Màu xanh lá nhạt

            this.btnBack = new Button();
            this.btnBack.Text = "←";
            this.btnBack.FlatStyle = FlatStyle.Flat;
            this.btnBack.FlatAppearance.BorderSize = 0;
            this.btnBack.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            this.btnBack.Location = new Point(10, 15);
            this.btnBack.Size = new Size(40, 30);

            this.lblLeftTitle = new Label();
            this.lblLeftTitle.Text = "Group";
            this.lblLeftTitle.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            this.lblLeftTitle.Location = new Point(60, 20);
            this.lblLeftTitle.AutoSize = true;

            this.pnlLeftHeader.Controls.Add(this.btnBack);
            this.pnlLeftHeader.Controls.Add(this.lblLeftTitle);

            // Group List
            this.flowLeftGroups = new FlowLayoutPanel();
            this.flowLeftGroups.Dock = DockStyle.Fill;
            this.flowLeftGroups.AutoScroll = true;
            this.flowLeftGroups.BackColor = Color.White;
            this.flowLeftGroups.FlowDirection = FlowDirection.TopDown;
            this.flowLeftGroups.WrapContents = false;

            // --- RIGHT CHAT AREA ---
            // Header
            this.pnlRightHeader = new Panel();
            this.pnlRightHeader.Dock = DockStyle.Top;
            this.pnlRightHeader.Height = 60;
            this.pnlRightHeader.BackColor = Color.White;
            this.pnlRightHeader.Padding = new Padding(20, 0, 20, 0);
            // Kẻ đường line dưới header
            this.pnlRightHeader.Paint += (s, e) => {
                e.Graphics.DrawLine(Pens.LightGray, 0, pnlRightHeader.Height - 1, pnlRightHeader.Width, pnlRightHeader.Height - 1);
            };

            this.lblRightTitle = new Label();
            this.lblRightTitle.Text = "Group 1"; // Mock title
            this.lblRightTitle.BackColor = Color.White;
            this.lblRightTitle.BorderStyle = BorderStyle.FixedSingle;
            this.lblRightTitle.Padding = new Padding(10, 5, 10, 5);
            this.lblRightTitle.AutoSize = true;
            this.lblRightTitle.Location = new Point(60, 15);

            this.pnlRightHeader.Controls.Add(this.lblRightTitle);

            // Input Area
            this.pnlInputArea = new Panel();
            this.pnlInputArea.Dock = DockStyle.Bottom;
            this.pnlInputArea.Height = 60;
            this.pnlInputArea.BackColor = Color.White;
            this.pnlInputArea.Padding = new Padding(10);
            this.pnlInputArea.Paint += (s, e) => {
                e.Graphics.DrawLine(Pens.LightGray, 0, 0, pnlInputArea.Width, 0);
            };

            this.btnAddFile = new Button();
            this.btnAddFile.Text = "+";
            this.btnAddFile.Font = new Font("Segoe UI", 16);
            this.btnAddFile.FlatStyle = FlatStyle.Flat;
            this.btnAddFile.FlatAppearance.BorderSize = 0;
            this.btnAddFile.Size = new Size(40, 40);
            this.btnAddFile.Dock = DockStyle.Left;

            this.btnSend = new Button();
            this.btnSend.Text = "➤";
            this.btnSend.FlatStyle = FlatStyle.Flat;
            this.btnSend.Dock = DockStyle.Right;
            this.btnSend.Size = new Size(50, 40);

            this.txtMessage = new TextBox();
            this.txtMessage.BorderStyle = BorderStyle.None;
            this.txtMessage.Font = new Font("Segoe UI", 11);
            this.txtMessage.Multiline = true;
            this.txtMessage.Dock = DockStyle.Fill;
            this.txtMessage.Text = "Type message...";

            // Container cho Textbox để tạo border
            Panel txtContainer = new Panel();
            txtContainer.BorderStyle = BorderStyle.FixedSingle;
            txtContainer.Padding = new Padding(5);
            txtContainer.Dock = DockStyle.Fill;
            txtContainer.Controls.Add(txtMessage);

            this.pnlInputArea.Controls.Add(txtContainer);
            this.pnlInputArea.Controls.Add(this.btnAddFile);
            this.pnlInputArea.Controls.Add(this.btnSend);

            // Chat Messages Area
            this.flowChatMessages = new FlowLayoutPanel();
            this.flowChatMessages.Dock = DockStyle.Fill;
            this.flowChatMessages.AutoScroll = true;
            this.flowChatMessages.BackColor = Color.White; // Nền trắng
            this.flowChatMessages.FlowDirection = FlowDirection.TopDown;
            this.flowChatMessages.WrapContents = false;
            this.flowChatMessages.Padding = new Padding(20);
            this.flowChatMessages.SizeChanged += (s, e) => {
                foreach (Control c in flowChatMessages.Controls) c.Width = flowChatMessages.ClientSize.Width - 40;
            };

            // Layout Assembly
            this.splitContainerMain.Panel1.Controls.Add(this.flowLeftGroups);
            this.splitContainerMain.Panel1.Controls.Add(this.pnlLeftHeader);
            this.splitContainerMain.Panel2.Controls.Add(this.flowChatMessages);
            this.splitContainerMain.Panel2.Controls.Add(this.pnlInputArea);
            this.splitContainerMain.Panel2.Controls.Add(this.pnlRightHeader);

            this.Controls.Add(this.splitContainerMain);
        }
    }
}