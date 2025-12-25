using System;
using System.Drawing;
using System.Windows.Forms;

namespace TimeFlow
{
    partial class FormChatBox
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.tableLayoutMain = new System.Windows.Forms.TableLayoutPanel();
            this.pnlLeftHeader = new System.Windows.Forms.Panel();
            this.btnCreateGroup = new System.Windows.Forms.Button();
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

            // tableLayoutMain
            this.tableLayoutMain.BackColor = System.Drawing.Color.White;
            this.tableLayoutMain.ColumnCount = 2;
            this.tableLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 300F));
            this.tableLayoutMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutMain.Controls.Add(this.pnlLeftHeader, 0, 0);
            this.tableLayoutMain.Controls.Add(this.pnlRightHeader, 1, 0);
            this.tableLayoutMain.Controls.Add(this.flowSidebar, 0, 1);
            this.tableLayoutMain.Controls.Add(this.pnlChatContainer, 1, 1);
            this.tableLayoutMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutMain.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutMain.Name = "tableLayoutMain";
            this.tableLayoutMain.RowCount = 2;
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 70F));
            this.tableLayoutMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutMain.Size = new System.Drawing.Size(1000, 600);
            this.tableLayoutMain.TabIndex = 0;

            // pnlLeftHeader
            this.pnlLeftHeader.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlLeftHeader.Controls.Add(this.btnCreateGroup);
            this.pnlLeftHeader.Controls.Add(this.lblHeaderTitle);
            this.pnlLeftHeader.Controls.Add(this.btnBack);
            this.pnlLeftHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLeftHeader.BorderStyle = BorderStyle.FixedSingle;
            this.pnlLeftHeader.Margin = new Padding(0);

            // btnBack
            this.btnBack.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBack.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnBack.FlatAppearance.BorderSize = 0;
            this.btnBack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBack.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.btnBack.Size = new System.Drawing.Size(40, 68);
            this.btnBack.Text = "←";
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);

            // btnCreateGroup
            this.btnCreateGroup.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCreateGroup.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnCreateGroup.FlatAppearance.BorderSize = 0;
            this.btnCreateGroup.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCreateGroup.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.btnCreateGroup.ForeColor = System.Drawing.Color.DodgerBlue;
            this.btnCreateGroup.Size = new System.Drawing.Size(40, 68);
            this.btnCreateGroup.Text = "+";
            this.btnCreateGroup.Click += new System.EventHandler(this.btnCreateGroup_Click);

            // lblHeaderTitle
            this.lblHeaderTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblHeaderTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblHeaderTitle.Location = new System.Drawing.Point(40, 0);
            this.lblHeaderTitle.Text = "Nhóm & Tin nhắn";
            this.lblHeaderTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // pnlRightHeader
            this.pnlRightHeader.BackColor = System.Drawing.Color.White;
            this.pnlRightHeader.Controls.Add(this.lblChatTitle);
            this.pnlRightHeader.Controls.Add(this.btnClose);
            this.pnlRightHeader.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlRightHeader.BorderStyle = BorderStyle.FixedSingle;
            this.pnlRightHeader.Margin = new Padding(0);

            // lblChatTitle
            this.lblChatTitle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblChatTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblChatTitle.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.lblChatTitle.Text = "Chat Room";
            this.lblChatTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

            // btnClose
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClose.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnClose.ForeColor = System.Drawing.Color.DimGray;
            this.btnClose.Size = new System.Drawing.Size(48, 68);
            this.btnClose.Text = "X";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);

            // flowSidebar
            this.flowSidebar.AutoScroll = true;
            this.flowSidebar.BackColor = System.Drawing.Color.White;
            this.flowSidebar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flowSidebar.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowSidebar.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowSidebar.WrapContents = false;
            this.flowSidebar.Margin = new Padding(0);

            // pnlChatContainer
            this.pnlChatContainer.BackColor = System.Drawing.Color.WhiteSmoke;
            this.pnlChatContainer.Controls.Add(this.flowChatMessages);
            this.pnlChatContainer.Controls.Add(this.pnlInput);
            this.pnlChatContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlChatContainer.Margin = new System.Windows.Forms.Padding(0);

            // flowChatMessages
            this.flowChatMessages.AutoScroll = true;
            this.flowChatMessages.BackColor = System.Drawing.Color.White;
            this.flowChatMessages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowChatMessages.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowChatMessages.Padding = new System.Windows.Forms.Padding(10);
            this.flowChatMessages.WrapContents = false;

            // pnlInput
            this.pnlInput.BackColor = System.Drawing.Color.White;
            this.pnlInput.Controls.Add(this.pnlInputBackground);
            this.pnlInput.Controls.Add(this.btnSend);
            this.pnlInput.Controls.Add(this.btnAddFile);
            this.pnlInput.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlInput.Padding = new System.Windows.Forms.Padding(10);
            this.pnlInput.Size = new System.Drawing.Size(700, 70);

            // pnlInputBackground
            this.pnlInputBackground.Controls.Add(this.txtMessage);
            this.pnlInputBackground.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlInputBackground.Padding = new System.Windows.Forms.Padding(10, 12, 10, 10);
            this.pnlInputBackground.Paint += new System.Windows.Forms.PaintEventHandler(this.pnlInputBackground_Paint);

            // txtMessage
            this.txtMessage.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);
            this.txtMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMessage.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.txtMessage.Multiline = true;
            this.txtMessage.Size = new System.Drawing.Size(570, 28);

            // btnSend
            this.btnSend.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSend.Dock = System.Windows.Forms.DockStyle.Right;
            this.btnSend.FlatAppearance.BorderSize = 0;
            this.btnSend.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSend.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.btnSend.ForeColor = System.Drawing.Color.DodgerBlue;
            this.btnSend.Size = new System.Drawing.Size(50, 50);
            this.btnSend.Text = "➤";
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);

            // btnAddFile
            this.btnAddFile.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddFile.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnAddFile.FlatAppearance.BorderSize = 0;
            this.btnAddFile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddFile.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.btnAddFile.ForeColor = System.Drawing.Color.DodgerBlue;
            this.btnAddFile.Size = new System.Drawing.Size(40, 50);
            this.btnAddFile.Text = "+";
            this.btnAddFile.Click += new System.EventHandler(this.btnAddFile_Click);

            // FormChatBox
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 600);
            this.Controls.Add(this.tableLayoutMain);
            this.Name = "FormChatBox";
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
        private System.Windows.Forms.Button btnCreateGroup;
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