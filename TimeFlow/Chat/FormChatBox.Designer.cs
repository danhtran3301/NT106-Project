namespace TimeFlow
{
    partial class FormChatBox
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            tableLayoutMain = new TableLayoutPanel();
            pnlLeftHeader = new Panel();
            lblHeaderTitle = new Label();
            btnBack = new Button();
            pnlRightHeader = new Panel();
            lblChatTitle = new Label();
            btnClose = new Button();
            flowSidebar = new FlowLayoutPanel();
            pnlChatContainer = new Panel();
            flowChatMessages = new FlowLayoutPanel();
            pnlInput = new Panel();
            pnlInputBackground = new Panel();
            txtMessage = new TextBox();
            btnSend = new Button();
            btnAddFile = new Button();
            tableLayoutMain.SuspendLayout();
            pnlLeftHeader.SuspendLayout();
            pnlRightHeader.SuspendLayout();
            pnlChatContainer.SuspendLayout();
            pnlInput.SuspendLayout();
            pnlInputBackground.SuspendLayout();
            SuspendLayout();
            // 
            // tableLayoutMain
            // 
            tableLayoutMain.BackColor = Color.White;
            tableLayoutMain.ColumnCount = 2;
            tableLayoutMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            tableLayoutMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            tableLayoutMain.Controls.Add(pnlLeftHeader, 0, 0);
            tableLayoutMain.Controls.Add(pnlRightHeader, 1, 0);
            tableLayoutMain.Controls.Add(flowSidebar, 0, 1);
            tableLayoutMain.Controls.Add(pnlChatContainer, 1, 1);
            tableLayoutMain.Dock = DockStyle.Fill;
            tableLayoutMain.Location = new Point(0, 0);
            tableLayoutMain.Margin = new Padding(4, 5, 4, 5);
            tableLayoutMain.Name = "tableLayoutMain";
            tableLayoutMain.RowCount = 2;
            tableLayoutMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 92F));
            tableLayoutMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tableLayoutMain.Size = new Size(1333, 923);
            tableLayoutMain.TabIndex = 0;
            // 
            // pnlLeftHeader
            // 
            pnlLeftHeader.BackColor = Color.WhiteSmoke;
            pnlLeftHeader.BorderStyle = BorderStyle.FixedSingle;
            pnlLeftHeader.Controls.Add(lblHeaderTitle);
            pnlLeftHeader.Controls.Add(btnBack);
            pnlLeftHeader.Dock = DockStyle.Fill;
            pnlLeftHeader.Location = new Point(0, 0);
            pnlLeftHeader.Margin = new Padding(0);
            pnlLeftHeader.Name = "pnlLeftHeader";
            pnlLeftHeader.Size = new Size(399, 92);
            pnlLeftHeader.TabIndex = 0;
            // 
            // lblHeaderTitle
            // 
            lblHeaderTitle.Dock = DockStyle.Fill;
            lblHeaderTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblHeaderTitle.Location = new Point(53, 0);
            lblHeaderTitle.Margin = new Padding(4, 0, 4, 0);
            lblHeaderTitle.Name = "lblHeaderTitle";
            lblHeaderTitle.Padding = new Padding(13, 0, 0, 0);
            lblHeaderTitle.Size = new Size(344, 90);
            lblHeaderTitle.TabIndex = 1;
            lblHeaderTitle.Text = "Contacts";
            lblHeaderTitle.TextAlign = ContentAlignment.MiddleLeft;
            lblHeaderTitle.Click += lblHeaderTitle_Click;
            // 
            // btnBack
            // 
            btnBack.Cursor = Cursors.Hand;
            btnBack.Dock = DockStyle.Left;
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.FlatStyle = FlatStyle.Flat;
            btnBack.Font = new Font("Arial", 12F, FontStyle.Bold);
            btnBack.Location = new Point(0, 0);
            btnBack.Margin = new Padding(4, 5, 4, 5);
            btnBack.Name = "btnBack";
            btnBack.Size = new Size(53, 90);
            btnBack.TabIndex = 0;
            btnBack.Text = "←";
            btnBack.UseVisualStyleBackColor = true;
            btnBack.Click += btnBack_Click;
            // 
            // pnlRightHeader
            // 
            pnlRightHeader.BackColor = Color.White;
            pnlRightHeader.BorderStyle = BorderStyle.FixedSingle;
            pnlRightHeader.Controls.Add(lblChatTitle);
            pnlRightHeader.Controls.Add(btnClose);
            pnlRightHeader.Dock = DockStyle.Fill;
            pnlRightHeader.Location = new Point(399, 0);
            pnlRightHeader.Margin = new Padding(0);
            pnlRightHeader.Name = "pnlRightHeader";
            pnlRightHeader.Size = new Size(934, 92);
            pnlRightHeader.TabIndex = 1;
            // 
            // lblChatTitle
            // 
            lblChatTitle.Dock = DockStyle.Fill;
            lblChatTitle.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblChatTitle.Location = new Point(0, 0);
            lblChatTitle.Margin = new Padding(4, 0, 4, 0);
            lblChatTitle.Name = "lblChatTitle";
            lblChatTitle.Padding = new Padding(20, 0, 0, 0);
            lblChatTitle.Size = new Size(879, 90);
            lblChatTitle.TabIndex = 1;
            lblChatTitle.Text = "User B";
            lblChatTitle.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // btnClose
            // 
            btnClose.Cursor = Cursors.Hand;
            btnClose.Dock = DockStyle.Right;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.FlatAppearance.MouseOverBackColor = Color.MistyRose;
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.Font = new Font("Arial", 10F, FontStyle.Bold);
            btnClose.ForeColor = Color.DimGray;
            btnClose.Location = new Point(879, 0);
            btnClose.Margin = new Padding(4, 5, 4, 5);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(53, 90);
            btnClose.TabIndex = 0;
            btnClose.Text = "X";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // flowSidebar
            // 
            flowSidebar.AutoScroll = true;
            flowSidebar.BackColor = Color.White;
            flowSidebar.BorderStyle = BorderStyle.FixedSingle;
            flowSidebar.Dock = DockStyle.Fill;
            flowSidebar.FlowDirection = FlowDirection.TopDown;
            flowSidebar.Location = new Point(0, 92);
            flowSidebar.Margin = new Padding(0);
            flowSidebar.Name = "flowSidebar";
            flowSidebar.Size = new Size(399, 831);
            flowSidebar.TabIndex = 2;
            flowSidebar.WrapContents = false;
            // 
            // pnlChatContainer
            // 
            pnlChatContainer.BackColor = Color.WhiteSmoke;
            pnlChatContainer.Controls.Add(flowChatMessages);
            pnlChatContainer.Controls.Add(pnlInput);
            pnlChatContainer.Dock = DockStyle.Fill;
            pnlChatContainer.Location = new Point(399, 92);
            pnlChatContainer.Margin = new Padding(0);
            pnlChatContainer.Name = "pnlChatContainer";
            pnlChatContainer.Size = new Size(934, 831);
            pnlChatContainer.TabIndex = 3;
            // 
            // flowChatMessages
            // 
            flowChatMessages.AutoScroll = true;
            flowChatMessages.BackColor = Color.White;
            flowChatMessages.Dock = DockStyle.Fill;
            flowChatMessages.FlowDirection = FlowDirection.TopDown;
            flowChatMessages.Location = new Point(0, 0);
            flowChatMessages.Margin = new Padding(4, 5, 4, 5);
            flowChatMessages.Name = "flowChatMessages";
            flowChatMessages.Padding = new Padding(13, 15, 13, 15);
            flowChatMessages.Size = new Size(934, 723);
            flowChatMessages.TabIndex = 1;
            flowChatMessages.WrapContents = false;
            // 
            // pnlInput
            // 
            pnlInput.BackColor = Color.White;
            pnlInput.Controls.Add(pnlInputBackground);
            pnlInput.Controls.Add(btnSend);
            pnlInput.Controls.Add(btnAddFile);
            pnlInput.Dock = DockStyle.Bottom;
            pnlInput.Location = new Point(0, 723);
            pnlInput.Margin = new Padding(4, 5, 4, 5);
            pnlInput.Name = "pnlInput";
            pnlInput.Padding = new Padding(13, 15, 13, 15);
            pnlInput.Size = new Size(934, 108);
            pnlInput.TabIndex = 0;
            // 
            // pnlInputBackground
            // 
            pnlInputBackground.Controls.Add(txtMessage);
            pnlInputBackground.Dock = DockStyle.Fill;
            pnlInputBackground.Location = new Point(66, 15);
            pnlInputBackground.Margin = new Padding(4, 5, 4, 5);
            pnlInputBackground.Name = "pnlInputBackground";
            pnlInputBackground.Padding = new Padding(20, 18, 20, 15);
            pnlInputBackground.Size = new Size(788, 78);
            pnlInputBackground.TabIndex = 2;
            pnlInputBackground.Paint += pnlInputBackground_Paint;
            // 
            // txtMessage
            // 
            txtMessage.BackColor = Color.FromArgb(240, 240, 240);
            txtMessage.BorderStyle = BorderStyle.None;
            txtMessage.Dock = DockStyle.Fill;
            txtMessage.Font = new Font("Segoe UI", 11F);
            txtMessage.Location = new Point(20, 18);
            txtMessage.Margin = new Padding(4, 5, 4, 5);
            txtMessage.Multiline = true;
            txtMessage.Name = "txtMessage";
            txtMessage.Size = new Size(748, 45);
            txtMessage.TabIndex = 0;
            // 
            // btnSend
            // 
            btnSend.Cursor = Cursors.Hand;
            btnSend.Dock = DockStyle.Right;
            btnSend.FlatAppearance.BorderSize = 0;
            btnSend.FlatStyle = FlatStyle.Flat;
            btnSend.Font = new Font("Arial", 16F, FontStyle.Bold);
            btnSend.ForeColor = Color.DodgerBlue;
            btnSend.Location = new Point(854, 15);
            btnSend.Margin = new Padding(4, 5, 4, 5);
            btnSend.Name = "btnSend";
            btnSend.Size = new Size(67, 78);
            btnSend.TabIndex = 1;
            btnSend.Text = "➤";
            btnSend.UseVisualStyleBackColor = true;
            btnSend.Click += btnSend_Click;
            // 
            // btnAddFile
            // 
            btnAddFile.Cursor = Cursors.Hand;
            btnAddFile.Dock = DockStyle.Left;
            btnAddFile.FlatAppearance.BorderSize = 0;
            btnAddFile.FlatStyle = FlatStyle.Flat;
            btnAddFile.Font = new Font("Arial", 16F, FontStyle.Bold);
            btnAddFile.ForeColor = Color.DodgerBlue;
            btnAddFile.Location = new Point(13, 15);
            btnAddFile.Margin = new Padding(4, 5, 4, 5);
            btnAddFile.Name = "btnAddFile";
            btnAddFile.Size = new Size(53, 78);
            btnAddFile.TabIndex = 0;
            btnAddFile.Text = "+";
            btnAddFile.UseVisualStyleBackColor = true;
            btnAddFile.Click += btnAddFile_Click;
            // 
            // ChatForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1333, 923);
            Controls.Add(tableLayoutMain);
            Margin = new Padding(4, 5, 4, 5);
            Name = "ChatForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Chat Application";
            tableLayoutMain.ResumeLayout(false);
            pnlLeftHeader.ResumeLayout(false);
            pnlRightHeader.ResumeLayout(false);
            pnlChatContainer.ResumeLayout(false);
            pnlInput.ResumeLayout(false);
            pnlInputBackground.ResumeLayout(false);
            pnlInputBackground.PerformLayout();
            ResumeLayout(false);

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