using System.Drawing.Drawing2D;
using TimeFlow.UI.Components;

namespace TimeFlow.UI
{
    partial class FormSettings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        private ModernButton btnLogout;
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
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Text = "FormSettings";

            Panel headerPanel = new Panel();
            headerPanel.Dock = DockStyle.Top;
            headerPanel.Height = 70;
            headerPanel.BackColor = AppColors.White;
            headerPanel.Padding = new Padding(20);

            // Border dưới header (mô phỏng bằng Panel nhỏ)
            Panel headerBorder = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = AppColors.Gray200 };
            headerPanel.Controls.Add(headerBorder);

            // Back Button
            ModernButton btnBack = new ModernButton()
            {
                Text = "←",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                Size = new Size(40, 40),
                Location = new Point(15, 15),
                BackColor = Color.Transparent,
                ForeColor = AppColors.Gray500,
                BorderRadius = 20,
                HoverColor = AppColors.Gray100
            };

            // Title
            Label lblTitle = new Label
            {
                Text = "Application Settings",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = AppColors.Gray800,
                AutoSize = true,
                Location = new Point(70, 22)
            };

            // Search Bar (Giả lập)
            ModernPanel pnlSearch = new ModernPanel
            {
                Size = new Size(300, 40),
                Location = new Point(1100 - 340, 15), // Dùng size gốc để tính vị trí ban đầu
                BackColor = AppColors.Gray50,
                BorderColor = AppColors.Gray300,
                BorderRadius = 8,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            Label lblSearchIcon = new Label { Text = "🔍", Location = new Point(10, 8), AutoSize = true, ForeColor = AppColors.Gray400, BackColor = Color.Transparent };
            TextBox txtSearch = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Location = new Point(35, 10),
                Width = 250,
                BackColor = AppColors.Gray50,
                Text = "Search settings...",
                ForeColor = AppColors.Gray500
            };
            pnlSearch.Controls.Add(lblSearchIcon);
            pnlSearch.Controls.Add(txtSearch);

            headerPanel.Controls.Add(btnBack);
            headerPanel.Controls.Add(lblTitle);
            headerPanel.Controls.Add(pnlSearch);

            // --- SIDEBAR ---
            Panel sidebarPanel = new Panel();
            sidebarPanel.Dock = DockStyle.Left;
            sidebarPanel.Width = 260;
            sidebarPanel.BackColor = AppColors.White;
            sidebarPanel.Padding = new Padding(10);

            // Border phải sidebar
            Panel sidebarBorder = new Panel { Dock = DockStyle.Right, Width = 1, BackColor = AppColors.Gray200 };
            sidebarPanel.Controls.Add(sidebarBorder);

            // Sidebar Items
            string[] menuItems = { "👤 Account", "🔔 Notifications", "🛡️ Privacy", "⚙️ General", "🎨 Appearance", "🔑 API & Integrations" };
            int topOffset = 20;
            foreach (var item in menuItems)
            {
                bool isActive = item.Contains("Account");
                ModernButton btnMenu = new ModernButton
                {
                    Text = "     " + item, // Padding text giả
                    Size = new Size(230, 45),
                    Location = new Point(10, topOffset),
                    BackColor = isActive ? AppColors.Blue50 : AppColors.White,
                    ForeColor = isActive ? AppColors.Blue600 : AppColors.Gray500,
                    BorderRadius = 8,
                    HoverColor = AppColors.Gray50,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font("Segoe UI", 10, isActive ? FontStyle.Bold : FontStyle.Regular)
                };
                sidebarPanel.Controls.Add(btnMenu);
                topOffset += 50;
            }

              btnLogout = new ModernButton
            {
                Text = "      🚪 Log Out", 
                Size = new Size(230, 45),
        
                Location = new Point(10, topOffset + 20),
                BackColor = AppColors.White,
                ForeColor = AppColors.Red600, 
                BorderRadius = 8,
                HoverColor = AppColors.Red50, 
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnLogout.Click += BtnLogout_Click;

            sidebarPanel.Controls.Add(btnLogout);

            // --- MAIN CONTENT AREA ---
            FlowLayoutPanel contentPanel = new FlowLayoutPanel();
            contentPanel.Dock = DockStyle.Fill;
            contentPanel.AutoScroll = true;

            contentPanel.Padding = new Padding(40, 40, 40, 100);

            contentPanel.BackColor = AppColors.Gray50;
            contentPanel.FlowDirection = FlowDirection.TopDown;
            contentPanel.WrapContents = false;

            // SECTION 1: ACCOUNT HEADER
            Label lblSectionTitle = new Label
            {
                Text = "Account",
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                ForeColor = AppColors.Gray800,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 20)
            };
            contentPanel.Controls.Add(lblSectionTitle);

            // CARD 1: PROFILE INFO
            ModernPanel cardProfile = CreateCard("Profile Information", "Update your personal details here.", 300);

            // Avatar Area
            Panel pnlAvatar = new Panel { Size = new Size(600, 100), Location = new Point(20, 80), BackColor = Color.Transparent };
            ModernPanel avatarCircle = new ModernPanel { Size = new Size(80, 80), BorderRadius = 40, BackColor = AppColors.Gray200 }; // Placeholder avatar
            Label lblAvatarText = new Label { Text = "AD", Font = new Font("Segoe UI", 20, FontStyle.Bold), ForeColor = AppColors.Gray500, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
            avatarCircle.Controls.Add(lblAvatarText);

            ModernButton btnChange = new ModernButton { Text = "Change", BackColor = AppColors.Blue500, ForeColor = Color.White, Size = new Size(80, 35), Location = new Point(100, 25), BorderRadius = 6 };
            ModernButton btnRemove = new ModernButton { Text = "Remove", BackColor = AppColors.White, ForeColor = AppColors.Gray700, BorderColor = AppColors.Gray300, Size = new Size(80, 35), Location = new Point(190, 25), BorderRadius = 6 };

            pnlAvatar.Controls.Add(avatarCircle);
            pnlAvatar.Controls.Add(btnChange);
            pnlAvatar.Controls.Add(btnRemove);
            cardProfile.Controls.Add(pnlAvatar);

            // Inputs
            cardProfile.Controls.Add(CreateLabeledInput("Full Name", "Alex Doe", 20, 190));
            cardProfile.Controls.Add(CreateLabeledInput("Email Address", "alex.doe@example.com", 320, 190)); // Side by side simulation requires logic, here simple absolute

            contentPanel.Controls.Add(cardProfile);

            // CARD 2: PASSWORD
            ModernPanel cardPassword = CreateCard("Change Password", "For your security, we recommend using a strong password.", 200);
            cardPassword.Controls.Add(CreateLabeledInput("Current Password", "********", 20, 80, true));
            cardPassword.Controls.Add(CreateLabeledInput("New Password", "", 320, 80, true));
            contentPanel.Controls.Add(cardPassword);

            // CARD 3: DELETE ACCOUNT
            ModernPanel cardDelete = new ModernPanel
            {
                Size = new Size(700, 150),
                Margin = new Padding(0, 0, 0, 30),
                BackColor = AppColors.Red50,
                BorderColor = AppColors.Red200,
                BorderRadius = 12
            };
            Label lblDelTitle = new Label { Text = "Delete Account", Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = AppColors.Red600, Location = new Point(20, 20), AutoSize = true };
            Label lblDelDesc = new Label { Text = "Permanently delete your account and all of your content. This action is not reversible.", ForeColor = AppColors.Red600, Location = new Point(20, 50), Size = new Size(600, 40) };
            ModernButton btnDelete = new ModernButton { Text = "Delete My Account", BackColor = AppColors.Red600, ForeColor = Color.White, Size = new Size(150, 35), Location = new Point(20, 95), BorderRadius = 6 };

            cardDelete.Controls.Add(lblDelTitle);
            cardDelete.Controls.Add(lblDelDesc);
            cardDelete.Controls.Add(btnDelete);
            contentPanel.Controls.Add(cardDelete);

            // Footer Buttons
            Panel pnlFooter = new Panel { Size = new Size(700, 50), BackColor = Color.Transparent };
            ModernButton btnSave = new ModernButton { Text = "Save Changes", BackColor = AppColors.Blue500, ForeColor = Color.White, Size = new Size(120, 35), Location = new Point(580, 0), BorderRadius = 6 };
            ModernButton btnCancel = new ModernButton { Text = "Cancel", BackColor = AppColors.White, ForeColor = AppColors.Gray700, BorderColor = AppColors.Gray300, Size = new Size(80, 35), Location = new Point(490, 0), BorderRadius = 6 };
            pnlFooter.Controls.Add(btnSave);
            pnlFooter.Controls.Add(btnCancel);
            contentPanel.Controls.Add(pnlFooter);

            // Spacer to ensure padding at the bottom
            Panel spacer = new Panel { Size = new Size(100, 100), BackColor = Color.Transparent, Margin = new Padding(0) };
            contentPanel.Controls.Add(spacer);

            // Add Main Panels
            this.Controls.Add(contentPanel);
            this.Controls.Add(sidebarPanel);
            this.Controls.Add(headerPanel);

        }

        // Helper to create the white cards with customizable height
        private ModernPanel CreateCard(string title, string subtitle, int height)
        {
            ModernPanel card = new ModernPanel
            {
                Size = new Size(700, height), // Dynamic height
                BackColor = AppColors.White,
                BorderColor = AppColors.Gray200,
                BorderRadius = 12,
                Margin = new Padding(0, 0, 0, 30)
            };

            Label lblTitle = new Label { Text = title, Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = AppColors.Gray900, Location = new Point(20, 20), AutoSize = true };
            Label lblSub = new Label { Text = subtitle, Font = new Font("Segoe UI", 9), ForeColor = AppColors.Gray500, Location = new Point(20, 45), AutoSize = true };

            // Panel line = new Panel { Size = new Size(660, 1), BackColor = AppColors.Gray100, Location = new Point(20, 70) };

            card.Controls.Add(lblTitle);
            card.Controls.Add(lblSub);
            // card.Controls.Add(line); // Optional line
            return card;
        }

        // Helper to create Inputs
        private Panel CreateLabeledInput(string label, string value, int x, int y, bool isPassword = false)
        {
            Panel pWrapper = new Panel { Location = new Point(x, y), Size = new Size(280, 70), BackColor = Color.Transparent };
            Label lbl = new Label { Text = label, Font = new Font("Segoe UI", 9, FontStyle.Bold), ForeColor = AppColors.Gray700, Location = new Point(0, 0), AutoSize = true };

            ModernPanel pInput = new ModernPanel { Location = new Point(0, 25), Size = new Size(280, 38), BorderColor = AppColors.Gray300, BorderRadius = 6, BackColor = AppColors.White };
            TextBox txt = new TextBox
            {
                BorderStyle = BorderStyle.None,
                Location = new Point(10, 10),
                Width = 250,
                Text = value,
                Font = new Font("Segoe UI", 10),
                ForeColor = AppColors.Gray900
            };
            if (isPassword) txt.UseSystemPasswordChar = true;

            pInput.Controls.Add(txt);
            pWrapper.Controls.Add(lbl);
            pWrapper.Controls.Add(pInput);
            return pWrapper;
        }

        // Panel có bo tròn góc và viền
        public class ModernPanel : Panel
        {
            public int BorderRadius { get; set; } = 0;
            public Color BorderColor { get; set; } = Color.Transparent;

            public ModernPanel() { this.DoubleBuffered = true; }

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                if (BorderRadius > 0)
                {
                    RectangleF rect = new RectangleF(0, 0, this.Width - 1, this.Height - 1);
                    GraphicsPath path = GetRoundedPath(rect, BorderRadius);

                    // Fill background (cần thiết nếu panel trong suốt hoặc đè lên cái khác)
                    using (SolidBrush brush = new SolidBrush(this.BackColor))
                    {
                        e.Graphics.FillPath(brush, path);
                    }

                    // Draw Border
                    if (BorderColor != Color.Transparent)
                    {
                        using (Pen pen = new Pen(BorderColor, 1))
                        {
                            e.Graphics.DrawPath(pen, path);
                        }
                    }
                }
            }

            private GraphicsPath GetRoundedPath(RectangleF rect, float radius)
            {
                GraphicsPath path = new GraphicsPath();
                path.StartFigure();
                path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
                path.AddArc(rect.Width - radius, rect.Y, radius, radius, 270, 90);
                path.AddArc(rect.Width - radius, rect.Height - radius, radius, radius, 0, 90);
                path.AddArc(rect.X, rect.Height - radius, radius, radius, 90, 90);
                path.CloseFigure();
                return path;
            }
        }

        // Button hiện đại, phẳng, hover đổi màu
        public class ModernButton : Button
        {
            public int BorderRadius { get; set; } = 4;
            public Color BorderColor { get; set; } = Color.Transparent;
            public Color HoverColor { get; set; } = Color.Empty;

            private Color _originalBackColor;

            public ModernButton()
            {
                this.FlatStyle = FlatStyle.Flat;
                this.FlatAppearance.BorderSize = 0;
                this.Cursor = Cursors.Hand;
                this.DoubleBuffered = true;
            }

            protected override void OnMouseEnter(EventArgs e)
            {
                base.OnMouseEnter(e);
                _originalBackColor = this.BackColor;
                if (HoverColor != Color.Empty) this.BackColor = HoverColor;
                else this.BackColor = ControlPaint.Light(this.BackColor);
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                base.OnMouseLeave(e);
                this.BackColor = _originalBackColor;
            }

            protected override void OnPaint(PaintEventArgs pevent)
            {
                pevent.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                RectangleF rect = new RectangleF(0, 0, this.Width - 1, this.Height - 1);
                GraphicsPath path = new GraphicsPath();
                float r = BorderRadius;

                path.AddArc(rect.X, rect.Y, r, r, 180, 90);
                path.AddArc(rect.Width - r, rect.Y, r, r, 270, 90);
                path.AddArc(rect.Width - r, rect.Height - r, r, r, 0, 90);
                path.AddArc(rect.X, rect.Height - r, r, r, 90, 90);
                path.CloseFigure();

                this.Region = new Region(path);

                using (SolidBrush brush = new SolidBrush(this.BackColor))
                {
                    pevent.Graphics.FillPath(brush, path);
                }

                if (BorderColor != Color.Transparent)
                {
                    using (Pen pen = new Pen(BorderColor, 1))
                    {
                        pevent.Graphics.DrawPath(pen, path);
                    }
                }

                // Draw Text center
                TextRenderer.DrawText(pevent.Graphics, this.Text, this.Font, this.ClientRectangle, this.ForeColor, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

        #endregion
    }
}