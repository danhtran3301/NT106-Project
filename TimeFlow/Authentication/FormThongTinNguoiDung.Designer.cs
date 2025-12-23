namespace TimeFlow.Authentication
{
    partial class FormThongTinNguoiDung
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormThongTinNguoiDung));
            label1 = new Label();
            btnLogout = new Button();
            label2 = new Label();
            label3 = new Label();
            labelEmailInfo = new Label();
            labelUsernameInfo = new Label();
            btnExit = new Button();
            pictureBox1 = new PictureBox();
            pictureBox3 = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = SystemColors.Control;
            label1.Font = new Font("Segoe UI Variable Display", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(159, 35);
            label1.Name = "label1";
            label1.Size = new Size(198, 31);
            label1.TabIndex = 0;
            label1.Text = "User Information";
            // 
            // btnLogout
            // 
            btnLogout.BackColor = Color.FromArgb(255, 255, 192);
            btnLogout.Font = new Font("Segoe UI Variable Display", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnLogout.Location = new Point(58, 417);
            btnLogout.Name = "btnLogout";
            btnLogout.Size = new Size(127, 44);
            btnLogout.TabIndex = 2;
            btnLogout.Text = "Đăng xuất";
            btnLogout.UseVisualStyleBackColor = false;
            btnLogout.Click += btnLogout_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.BackColor = SystemColors.Control;
            label2.Font = new Font("Segoe UI Variable Display", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.Location = new Point(79, 105);
            label2.Name = "label2";
            label2.Size = new Size(106, 27);
            label2.TabIndex = 3;
            label2.Text = "Username";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.BackColor = SystemColors.Control;
            label3.Font = new Font("Segoe UI Variable Display", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label3.Location = new Point(79, 276);
            label3.Name = "label3";
            label3.Size = new Size(61, 27);
            label3.TabIndex = 4;
            label3.Text = "Email";
            // 
            // labelEmailInfo
            // 
            labelEmailInfo.AutoSize = true;
            labelEmailInfo.BackColor = SystemColors.Control;
            labelEmailInfo.Font = new Font("Segoe UI Variable Display Semib", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelEmailInfo.Location = new Point(113, 330);
            labelEmailInfo.Name = "labelEmailInfo";
            labelEmailInfo.Size = new Size(290, 27);
            labelEmailInfo.TabIndex = 5;
            labelEmailInfo.Text = "\"Email người dùng đã đăng ký\"";
            // 
            // labelUsernameInfo
            // 
            labelUsernameInfo.AutoSize = true;
            labelUsernameInfo.BackColor = SystemColors.Control;
            labelUsernameInfo.Font = new Font("Segoe UI Variable Display Semib", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelUsernameInfo.Location = new Point(130, 190);
            labelUsernameInfo.Name = "labelUsernameInfo";
            labelUsernameInfo.Size = new Size(273, 27);
            labelUsernameInfo.TabIndex = 6;
            labelUsernameInfo.Text = "\"Tên người dùng đã đăng ký\"";
            // 
            // btnExit
            // 
            btnExit.BackColor = Color.FromArgb(255, 255, 192);
            btnExit.Font = new Font("Segoe UI Variable Display", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnExit.Location = new Point(288, 417);
            btnExit.Name = "btnExit";
            btnExit.Size = new Size(115, 44);
            btnExit.TabIndex = 7;
            btnExit.Text = "Thoát";
            btnExit.UseVisualStyleBackColor = false;
            btnExit.Click += buttonExit_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = SystemColors.ButtonHighlight;
            pictureBox1.Location = new Point(-7, 1);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(476, 493);
            pictureBox1.TabIndex = 8;
            pictureBox1.TabStop = false;
            // 
            // pictureBox3
            // 
            pictureBox3.Image = (Image)resources.GetObject("pictureBox3.Image");
            pictureBox3.Location = new Point(-7, 1);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new Size(830, 504);
            pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox3.TabIndex = 10;
            pictureBox3.TabStop = false;
            // 
            // FormThongTinNguoiDung
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(823, 491);
            Controls.Add(btnExit);
            Controls.Add(labelUsernameInfo);
            Controls.Add(labelEmailInfo);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(btnLogout);
            Controls.Add(label1);
            Controls.Add(pictureBox1);
            Controls.Add(pictureBox3);
            Name = "FormThongTinNguoiDung";
            Text = "TimeFlow";
            Load += FormThongTinNguoiDung_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Button btnLogout;
        private Label label2;
        private Label label3;
        private Label labelEmailInfo;
        private Label labelUsernameInfo;
        private Button btnExit;
        private PictureBox pictureBox1;
        private PictureBox pictureBox2;
        private PictureBox pictureBox3;
    }
}