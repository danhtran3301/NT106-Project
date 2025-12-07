﻿
namespace BT3_LTMCB
{
    partial class FormDangNhap
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormDangNhap));
            usernameTxtbox = new TextBox();
            label1 = new Label();
            btnLogin = new Button();
            label2 = new Label();
            label3 = new Label();
            passwordTxtbox = new TextBox();
            label4 = new Label();
            btnSignup = new Button();
            linkLabel1 = new LinkLabel();
            checkBoxShowPassword = new CheckBox();
            pictureBox1 = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // usernameTxtbox
            // 
            usernameTxtbox.Font = new Font("Segoe UI", 13.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            usernameTxtbox.Location = new Point(402, 130);
            usernameTxtbox.Margin = new Padding(3, 2, 3, 2);
            usernameTxtbox.Name = "usernameTxtbox";
            usernameTxtbox.Size = new Size(346, 32);
            usernameTxtbox.TabIndex = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI Variable Display", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(487, 53);
            label1.Name = "label1";
            label1.Size = new Size(179, 32);
            label1.TabIndex = 1;
            label1.Text = "Member Login";
            label1.Click += label1_Click;
            // 
            // btnLogin
            // 
            btnLogin.BackColor = Color.Green;
            btnLogin.Font = new Font("Segoe UI Variable Display", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnLogin.ForeColor = Color.WhiteSmoke;
            btnLogin.Location = new Point(470, 289);
            btnLogin.Margin = new Padding(3, 2, 3, 2);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(214, 41);
            btnLogin.TabIndex = 2;
            btnLogin.Text = "Đăng nhập";
            btnLogin.UseVisualStyleBackColor = false;
            btnLogin.Click += buttonLogin_Click;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI Variable Display", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.Location = new Point(402, 104);
            label2.Name = "label2";
            label2.Size = new Size(101, 26);
            label2.TabIndex = 3;
            label2.Text = "Username";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI Variable Display", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label3.Location = new Point(402, 176);
            label3.Name = "label3";
            label3.Size = new Size(97, 26);
            label3.TabIndex = 4;
            label3.Text = "Password";
            // 
            // passwordTxtbox
            // 
            passwordTxtbox.AcceptsTab = true;
            passwordTxtbox.Font = new Font("Segoe UI", 13.8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            passwordTxtbox.Location = new Point(402, 208);
            passwordTxtbox.Margin = new Padding(3, 2, 3, 2);
            passwordTxtbox.Name = "passwordTxtbox";
            passwordTxtbox.PasswordChar = '*';
            passwordTxtbox.Size = new Size(346, 32);
            passwordTxtbox.TabIndex = 1;
            passwordTxtbox.TextChanged += textBox2_TextChanged;
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI Variable Display", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label4.Location = new Point(438, 349);
            label4.Name = "label4";
            label4.Size = new Size(172, 20);
            label4.TabIndex = 6;
            label4.Text = "Bạn chưa có tài khoản?";
            // 
            // btnSignup
            // 
            btnSignup.Font = new Font("Segoe UI Variable Display", 10.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            btnSignup.Location = new Point(622, 344);
            btnSignup.Margin = new Padding(3, 2, 3, 2);
            btnSignup.Name = "btnSignup";
            btnSignup.Size = new Size(90, 28);
            btnSignup.TabIndex = 7;
            btnSignup.Text = "Đăng ký";
            btnSignup.UseVisualStyleBackColor = true;
            btnSignup.Click += buttonSignup_Click;
            // 
            // linkLabel1
            // 
            linkLabel1.AutoSize = true;
            linkLabel1.Font = new Font("Segoe UI Variable Display", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            linkLabel1.Location = new Point(622, 244);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new Size(126, 21);
            linkLabel1.TabIndex = 8;
            linkLabel1.TabStop = true;
            linkLabel1.Text = "Quên mật khẩu?";
            linkLabel1.TextAlign = ContentAlignment.TopRight;
            linkLabel1.LinkClicked += linkLabel1_LinkClicked;
            // 
            // checkBoxShowPassword
            // 
            checkBoxShowPassword.AutoSize = true;
            checkBoxShowPassword.Font = new Font("Segoe UI Variable Display Semib", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            checkBoxShowPassword.Location = new Point(402, 244);
            checkBoxShowPassword.Margin = new Padding(3, 2, 3, 2);
            checkBoxShowPassword.Name = "checkBoxShowPassword";
            checkBoxShowPassword.Size = new Size(133, 25);
            checkBoxShowPassword.TabIndex = 9;
            checkBoxShowPassword.Text = "Hiện mật khẩu";
            checkBoxShowPassword.UseVisualStyleBackColor = true;
            checkBoxShowPassword.CheckedChanged += CheckBoxShowPassword_CheckedChanged;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(94, 121);
            pictureBox1.Margin = new Padding(3, 2, 3, 2);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(214, 178);
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox1.TabIndex = 10;
            pictureBox1.TabStop = false;
            // 
            // FormDangNhap
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(854, 423);
            Controls.Add(pictureBox1);
            Controls.Add(checkBoxShowPassword);
            Controls.Add(linkLabel1);
            Controls.Add(btnSignup);
            Controls.Add(label4);
            Controls.Add(passwordTxtbox);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(btnLogin);
            Controls.Add(label1);
            Controls.Add(usernameTxtbox);
            Margin = new Padding(3, 2, 3, 2);
            Name = "FormDangNhap";
            Text = "TimeFlow";
            Load += FormDangNhap_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        { 
        }

        private void label1_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion

        private TextBox usernameTxtbox;
        private Label label1;
        private Button btnLogin;
        private Label label2;
        private Label label3;
        private TextBox passwordTxtbox;
        private Label label4;
        private Button btnSignup;
        private LinkLabel linkLabel1;
        private CheckBox checkBox1;
        private CheckBox checkBoxShowPassword;
        private PictureBox pictureBox1;
    }
}