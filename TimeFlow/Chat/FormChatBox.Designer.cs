using System;
using System.Drawing;
using System.Windows.Forms;

namespace TimeFlow.Chat
{
    partial class ChatForm
    {
        private System.ComponentModel.IContainer components = null;

        private TableLayoutPanel tableLayoutMain;
        private Panel pnlLeftHeader, pnlRightHeader, pnlChatContainer, pnlInput;
        private Button btnBack, btnClose, btnAddFile, btnSend;
        private Label lblHeaderTitle, lblChatTitle;
        private FlowLayoutPanel flowSidebar, flowChatMessages;

        // Control nhập liệu mới
        private Panel pnlInputBackground; // Panel dùng để vẽ viền bo tròn
        private TextBox txtMessage;

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
            this.Text = "Chat Application";
            this.StartPosition = FormStartPosition.CenterScreen;

            // 1. Trả về Form chuẩn của Windows (Có thanh tiêu đề, nút tắt/mở chuẩn)
            this.FormBorderStyle = FormBorderStyle.Sizable;

            // --- Main Layout ---
            this.tableLayoutMain = new TableLayoutPanel();
            this.tableLayoutMain.Dock = DockStyle.Fill;
            this.tableLayoutMain.ColumnCount = 2;
            this.tableLayoutMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F)); // Sidebar nhỏ hơn chút cho đẹp
            this.tableLayoutMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            this.tableLayoutMain.RowCount = 2;
            this.tableLayoutMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));
            this.tableLayoutMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            // --- Header Trái ---
            this.pnlLeftHeader = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(144, 238, 144), Padding = new Padding(5) };
            this.btnBack = new Button { Text = "←", Dock = DockStyle.Left, Width = 40, FlatStyle = FlatStyle.Flat, Font = new Font("Arial", 12, FontStyle.Bold), BackColor = Color.Transparent };
            this.btnBack.FlatAppearance.BorderSize = 0;
            this.btnBack.Click += new EventHandler(this.btnBack_Click);
            this.lblHeaderTitle = new Label { Text = "Group", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 12, FontStyle.Bold), Padding = new Padding(10, 0, 0, 0) };
            this.pnlLeftHeader.Controls.AddRange(new Control[] { this.lblHeaderTitle, this.btnBack });

            // --- Header Phải ---
            this.pnlRightHeader = new Panel { Dock = DockStyle.Fill, BackColor = Color.White, Padding = new Padding(5) };
            this.btnClose = new Button { Text = "X", Dock = DockStyle.Right, Width = 40, FlatStyle = FlatStyle.Flat, Font = new Font("Arial", 10, FontStyle.Bold) };
            this.btnClose.Click += new EventHandler(this.btnClose_Click);
            this.lblChatTitle = new Label { Text = "Group 1", Dock = DockStyle.Left, Width = 200, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 11, FontStyle.Bold) };
            this.pnlRightHeader.Controls.AddRange(new Control[] { this.lblChatTitle, this.btnClose });

            // --- Sidebar List ---
            this.flowSidebar = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

            // --- Chat Container ---
            this.pnlChatContainer = new Panel { Dock = DockStyle.Fill, BackColor = Color.WhiteSmoke };

            // Khu vực tin nhắn (FlowLayoutPanel)
            this.flowChatMessages = new FlowLayoutPanel();
            this.flowChatMessages.Dock = DockStyle.Fill;
            this.flowChatMessages.AutoScroll = true;
            this.flowChatMessages.FlowDirection = FlowDirection.TopDown; // Xếp dọc
            this.flowChatMessages.WrapContents = false;
            this.flowChatMessages.Padding = new Padding(20);

            // --- Input Area (Đã chỉnh sửa để bo tròn) ---
            this.pnlInput = new Panel { Dock = DockStyle.Bottom, Height = 70, BackColor = Color.White, Padding = new Padding(10) };

            this.btnAddFile = new Button { Text = "+", Dock = DockStyle.Left, Width = 40, FlatStyle = FlatStyle.Flat, Font = new Font("Arial", 16, FontStyle.Bold), Cursor = Cursors.Hand };
            this.btnAddFile.FlatAppearance.BorderSize = 0;
            this.btnAddFile.Click += new EventHandler(this.btnAddFile_Click);

            this.btnSend = new Button { Text = "➤", Dock = DockStyle.Right, Width = 50, FlatStyle = FlatStyle.Flat, Font = new Font("Arial", 14), ForeColor = Color.DodgerBlue, Cursor = Cursors.Hand, BackColor = Color.White };
            this.btnSend.FlatAppearance.BorderSize = 0;
            this.btnSend.Click += new EventHandler(this.btnSend_Click);

            // Wrapper bo tròn cho Textbox
            this.pnlInputBackground = new Panel();
            this.pnlInputBackground.Dock = DockStyle.Fill;
            this.pnlInputBackground.Padding = new Padding(15, 10, 15, 10); // Padding để text không sát viền
            this.pnlInputBackground.BackColor = Color.Transparent; // Để vẽ lại trong Logic

            this.txtMessage = new TextBox();
            this.txtMessage.Dock = DockStyle.Fill;
            this.txtMessage.BorderStyle = BorderStyle.None; // Bỏ viền gốc vuông vức
            this.txtMessage.Font = new Font("Segoe UI", 11);
            this.txtMessage.Multiline = true;
            this.txtMessage.BackColor = Color.FromArgb(240, 240, 240); // Màu nền trùng màu vẽ panel

            this.pnlInputBackground.Controls.Add(this.txtMessage);

            this.pnlInput.Controls.Add(this.pnlInputBackground);
            this.pnlInput.Controls.Add(this.btnSend);
            this.pnlInput.Controls.Add(this.btnAddFile);

            this.pnlChatContainer.Controls.Add(this.flowChatMessages);
            this.pnlChatContainer.Controls.Add(this.pnlInput);

            // Add to Layout
            this.tableLayoutMain.Controls.Add(this.pnlLeftHeader, 0, 0);
            this.tableLayoutMain.Controls.Add(this.pnlRightHeader, 1, 0);
            this.tableLayoutMain.Controls.Add(this.flowSidebar, 0, 1);
            this.tableLayoutMain.Controls.Add(this.pnlChatContainer, 1, 1);

            this.Controls.Add(this.tableLayoutMain);

            // Hook sự kiện vẽ cho input
            this.pnlInputBackground.Paint += new PaintEventHandler(this.pnlInputBackground_Paint);

            GenerateDummyData();
        }
    }
}