namespace TimeFlow
{
    partial class ChatForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.tableLayoutMain = new System.Windows.Forms.TableLayoutPanel();
            this.pnlLeftHeader = new System.Windows.Forms.Panel();
            this.lblHeaderTitle = new System.Windows.Forms.Label();
            this.btnBack = new System.Windows.Forms.Button();
            this.pnlRightHeader = new System.Windows.Forms.Panel();
            this.lblChatTitle = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.flowSidebar = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlChatContainer = new System.Windows.Forms.Panel();
            this.flowChatMessages = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlInput = new System.Windows.Forms.Panel();
            this.pnlInputBackground = new System.Windows.Forms.Panel();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.btnAddFile = new System.Windows.Forms.Button();
            this.tableLayoutMain.SuspendLayout();
            this.pnlLeftHeader.SuspendLayout();
            this.pnlRightHeader.SuspendLayout();
            this.pnlChatContainer.SuspendLayout();
            this.pnlInput.SuspendLayout();
            this.pnlInputBackground.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutMain
            // 
            this.tableLayoutMain.BackColor = System.Drawing.Color.White;
            this.tableLayoutMain.ColumnCount = 2;
            this.tableLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutMain.Controls.Add(this.pnlLeftHeader, 0, 0);
            this.tableLayoutMain.Controls.Add(this.pnlRightHeader, 1, 0);
            this.tableLayoutMain.Controls.Add(this.flowSidebar, 0, 1);
            this.tableLayoutMain.Controls.Add(this.pnlChatContainer, 1, 1);
            this.tableLayoutMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutMain.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutMain.Name = "tableLayoutMain";
            this.tableLayoutMain.RowCount = 2;
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutMain.Size = new System.Drawing.Size(1000, 600);
            this.tableLayoutMain.TabIndex = 0;
            // 
            // pnlLeftHeader
            // 
            this.pnlLeftHeader.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlLeftHeader.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlLeftHeader.Controls.Add(this.lblHeaderTitle);
            this.pnlLeftHeader.Controls.Add(this.btnBack);
            this.pnlLeftHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLeftHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlLeftHeader.Margin = new System.Windows.Forms.Padding(0);
            this.pnlLeftHeader.Name = "pnlLeftHeader";
            this.pnlLeftHeader.Size = new System.Drawing.Size(300, 60);
            this.pnlLeftHeader.TabIndex = 0;
            // 
            // lblHeaderTitle
            // 
            this.lblHeaderTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHeaderTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblHeaderTitle.Location = new System.Drawing.Point(40, 0);
            this.lblHeaderTitle.Name = "lblHeaderTitle";
            this.lblHeaderTitle.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.lblHeaderTitle.Size = new System.Drawing.Size(258, 58);
            this.lblHeaderTitle.TabIndex = 1;
            this.lblHeaderTitle.Text = "Tin nhắn & Nhóm";
            this.lblHeaderTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnBack
            // 
            this.btnBack.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBack.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnBack.FlatAppearance.BorderSize = 0;
            this.btnBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBack.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
            this.btnBack.Location = new System.Drawing.Point(0, 0);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(40, 58);
            this.btnBack.TabIndex = 0;
            this.btnBack.Text = "←";
            this.btnBack.UseVisualStyleBackColor = true;
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
            // 
            // pnlRightHeader
            // 
            this.pnlRightHeader.BackColor = System.Drawing.Color.White;
            this.pnlRightHeader.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlRightHeader.Controls.Add(this.lblChatTitle);
            this.pnlRightHeader.Controls.Add(this.btnClose);
            this.pnlRightHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRightHeader.Location = new System.Drawing.Point(300, 0);
            this.pnlRightHeader.Margin = new System.Windows.Forms.Padding(0);
            this.pnlRightHeader.Name = "pnlRightHeader";
            this.pnlRightHeader.Size = new System.Drawing.Size(700, 60);
            this.pnlRightHeader.TabIndex = 1;
            // 
            // lblChatTitle
            // 
            this.lblChatTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblChatTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblChatTitle.Location = new System.Drawing.Point(0, 0);
            this.lblChatTitle.Name = "lblChatTitle";
            this.lblChatTitle.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.lblChatTitle.Size = new System.Drawing.Size(658, 58);
            this.lblChatTitle.TabIndex = 1;
            this.lblChatTitle.Text = "Chọn hội thoại...";
            this.lblChatTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnClose
            // 
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClose.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.btnClose.ForeColor = System.Drawing.Color.DimGray;
            this.btnClose.Location = new System.Drawing.Point(658, 0);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(40, 58);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "X";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // flowSidebar
            // 
            this.flowSidebar.AutoScroll = true;
            this.flowSidebar.BackColor = System.Drawing.Color.White;
            this.flowSidebar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowSidebar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowSidebar.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowSidebar.Location = new System.Drawing.Point(0, 60);
            this.flowSidebar.Margin = new System.Windows.Forms.Padding(0);
            this.flowSidebar.Name = "flowSidebar";
            this.flowSidebar.Size = new System.Drawing.Size(300, 540);
            this.flowSidebar.TabIndex = 2;
            this.flowSidebar.WrapContents = false;
            // 
            // pnlChatContainer
            // 
            this.pnlChatContainer.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlChatContainer.Controls.Add(this.flowChatMessages);
            this.pnlChatContainer.Controls.Add(this.pnlInput);
            this.pnlChatContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlChatContainer.Location = new System.Drawing.Point(300, 60);
            this.pnlChatContainer.Margin = new System.Windows.Forms.Padding(0);
            this.pnlChatContainer.Name = "pnlChatContainer";
            this.pnlChatContainer.Size = new System.Drawing.Size(700, 540);
            this.pnlChatContainer.TabIndex = 3;
            // 
            // flowChatMessages
            // 
            this.flowChatMessages.AutoScroll = true;
            this.flowChatMessages.BackColor = System.Drawing.Color.White;
            this.flowChatMessages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowChatMessages.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowChatMessages.Location = new System.Drawing.Point(0, 0);
            this.flowChatMessages.Name = "flowChatMessages";
            this.flowChatMessages.Padding = new System.Windows.Forms.Padding(10);
            this.flowChatMessages.Size = new System.Drawing.Size(700, 470);
            this.flowChatMessages.TabIndex = 1;
            this.flowChatMessages.WrapContents = false;
            // 
            // pnlInput
            // 
            this.pnlInput.BackColor = System.Drawing.Color.White;
            this.pnlInput.Controls.Add(this.pnlInputBackground);
            this.pnlInput.Controls.Add(this.btnSend);
            this.pnlInput.Controls.Add(this.btnAddFile);
            this.pnlInput.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlInput.Location = new System.Drawing.Point(0, 470);
            this.pnlInput.Name = "pnlInput";
            this.pnlInput.Padding = new System.Windows.Forms.Padding(10);
            this.pnlInput.Size = new System.Drawing.Size(700, 70);
            this.pnlInput.TabIndex = 0;
            // 
            // pnlInputBackground
            // 
            this.pnlInputBackground.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.pnlInputBackground.Controls.Add(this.txtMessage);
            this.pnlInputBackground.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlInputBackground.Location = new System.Drawing.Point(50, 10);
            this.pnlInputBackground.Name = "pnlInputBackground";
            this.pnlInputBackground.Padding = new System.Windows.Forms.Padding(15, 12, 15, 10);
            this.pnlInputBackground.Size = new System.Drawing.Size(590, 50);
            this.pnlInputBackground.TabIndex = 2;
            this.pnlInputBackground.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlInputBackground_Paint);
            // 
            // txtMessage
            // 
            this.txtMessage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.txtMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMessage.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtMessage.Location = new System.Drawing.Point(15, 12);
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(560, 20);
            this.txtMessage.TabIndex = 0;
            // 
            // btnSend
            // 
            this.btnSend.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSend.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnSend.FlatAppearance.BorderSize = 0;
            this.btnSend.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSend.Font = new System.Drawing.Font("Arial", 16F, System.Drawing.FontStyle.Bold);
            this.btnSend.ForeColor = System.Drawing.Color.DodgerBlue;
            this.btnSend.Location = new System.Drawing.Point(640, 10);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(50, 50);
            this.btnSend.TabIndex = 1;
            this.btnSend.Text = "➤";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // btnAddFile
            // 
            this.btnAddFile.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddFile.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnAddFile.FlatAppearance.BorderSize = 0;
            this.btnAddFile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddFile.Font = new System.Drawing.Font("Arial", 16F, System.Drawing.FontStyle.Bold);
            this.btnAddFile.ForeColor = System.Drawing.Color.DodgerBlue;
            this.btnAddFile.Location = new System.Drawing.Point(10, 10);
            this.btnAddFile.Name = "btnAddFile";
            this.btnAddFile.Size = new System.Drawing.Size(40, 50);
            this.btnAddFile.TabIndex = 0;
            this.btnAddFile.Text = "+";
            this.btnAddFile.UseVisualStyleBackColor = true;
            // 
            // ChatForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 600);
            this.Controls.Add(this.tableLayoutMain);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "ChatForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TimeFlow Chat";
            this.tableLayoutMain.ResumeLayout(false);
            this.pnlLeftHeader.ResumeLayout(false);
            this.pnlRightHeader.ResumeLayout(false);
            this.pnlChatContainer.ResumeLayout(false);
            this.pnlInput.ResumeLayout(false);
            this.pnlInputBackground.ResumeLayout(false);
            this.pnlInputBackground.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutMain;
        private System.Windows.Forms.Panel pnlLeftHeader;
        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.Label lblHeaderTitle;
        private System.Windows.Forms.Panel pnlRightHeader;
        private System.Windows.Forms.Label lblChatTitle;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.FlowLayoutPanel flowSidebar;
        private System.Windows.Forms.Panel pnlChatContainer;
        private System.Windows.Forms.FlowLayoutPanel flowChatMessages;
        private System.Windows.Forms.Panel pnlInput;
        private System.Windows.Forms.Button btnAddFile;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Panel pnlInputBackground;
        private System.Windows.Forms.TextBox txtMessage;
    }
}