using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TimeFlow.Authentication;
using TimeFlow.Server;
using TimeFlow;
using TimeFlow.UI;
// using TimeFlow.Tasks; // Tạm comment vì TaskList chưa có

namespace TimeFlow
{
    public partial class FormGiaoDien : Form
    {
        // Dictionary để track các form đã tạo (singleton pattern)
        private static Dictionary<Type, Form> _openForms = new Dictionary<Type, Form>();

        public FormGiaoDien()
        {
            InitializeComponent();
            
            // Gán sự kiện cho các buttons
            button1.Click += Button1_Click;  // Your Task
            button2.Click += Button2_Click;  // Group (Chat)
            button3.Click += Button3_Click;  // New Task
            button4.Click += Button4_Click;  // Submit Task
            
            // Gán sự kiện cho pictureBox (Settings icon)
            pictureBox4.Click += PictureBox4_Click; // Settings
            
            // Gán sự kiện cho label username (User Info)
            label1.Click += Label1_Click;
        }

        /// <summary>
        /// Helper method để navigate giữa các forms một cách mượt mà
        /// </summary>
        private void NavigateToForm<T>() where T : Form, new()
        {
            try
            {
                Form targetForm;

                // Kiểm tra xem form đã được tạo chưa (singleton)
                if (_openForms.ContainsKey(typeof(T)))
                {
                    targetForm = _openForms[typeof(T)];
                }
                else
                {
                    // Tạo form mới
                    targetForm = new T();
                    _openForms[typeof(T)] = targetForm;

                    // Đăng ký sự kiện FormClosed để cleanup
                    targetForm.FormClosed += (s, e) =>
                    {
                        _openForms.Remove(typeof(T));
                        this.Show(); // Show lại FormGiaoDien khi form đóng
                        this.Activate(); // Bring to front
                    };
                }

                // Hide FormGiaoDien hiện tại
                this.Hide();

                // Show form mục tiêu
                targetForm.Show();
                targetForm.Activate(); // Bring to front
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể mở form: {ex.Message}", 
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Show(); // Đảm bảo FormGiaoDien vẫn hiển thị nếu có lỗi
            }
        }

        /// <summary>
        /// Special navigation cho FormThongTinNguoiDung (có parameters)
        /// </summary>
        private void NavigateToUserInfo(string username, string email)
        {
            try
            {
                Form targetForm;

                // Kiểm tra xem form đã được tạo chưa
                if (_openForms.ContainsKey(typeof(FormThongTinNguoiDung)))
                {
                    targetForm = _openForms[typeof(FormThongTinNguoiDung)];
                }
                else
                {
                    // Tạo form mới với parameters
                    targetForm = new FormThongTinNguoiDung(username, email);
                    _openForms[typeof(FormThongTinNguoiDung)] = targetForm;

                    // Đăng ký sự kiện FormClosed
                    targetForm.FormClosed += (s, e) =>
                    {
                        _openForms.Remove(typeof(FormThongTinNguoiDung));
                        this.Show();
                        this.Activate();
                    };
                }

                this.Hide();
                targetForm.Show();
                targetForm.Activate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể mở User Info: {ex.Message}", 
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Show();
            }
        }

        // Your Task - Tạm thời thông báo
        private void Button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Task List đang được phát triển!", "Thông báo", 
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            // TODO: Uncomment khi TaskList sẵn sàng
            // NavigateToForm<TaskList>();
        }

        // Group - Mở Chat với single-window pattern
        private void Button2_Click(object sender, EventArgs e)
        {
            NavigateToForm<ChatForm>();
        }

        // New Task - Mở FormThemTask
        private void Button3_Click(object sender, EventArgs e)
        {
            NavigateToForm<FormThemTask>();
        }

        // Submit Task - Mở FormTaskDetail
        private void Button4_Click(object sender, EventArgs e)
        {
            NavigateToForm<FormTaskDetail>();
        }

        // Settings Icon - Mở FormSettings
        private void PictureBox4_Click(object sender, EventArgs e)
        {
            NavigateToForm<FormSettings>();
        }

        // Username Label - Mở FormThongTinNguoiDung
        private void Label1_Click(object sender, EventArgs e)
        {
            // TODO: Lấy thông tin user từ session/token thực tế
            NavigateToUserInfo("DemoUser", "demo@example.com");
        }

        // Các event handlers cũ - giữ lại để không bị lỗi
        private void label1_Click(object sender, EventArgs e)
        {
            Label1_Click(sender, e);
        }

        private void GiaDien_Load(object sender, EventArgs e)
        {
            this.Text = "TimeFlow - Main Dashboard";
        }

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {
        }

        private void label2_Click(object sender, EventArgs e)
        {
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
        }

        private void label3_Click(object sender, EventArgs e)
        {
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Label1_Click(sender, e);
        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {
        }

        private void label11_Click(object sender, EventArgs e)
        {
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Button1_Click(sender, e);
        }

        private void label12_Click(object sender, EventArgs e)
        {
        }

        private void label13_Click(object sender, EventArgs e)
        {
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
        }

        /// <summary>
        /// Override FormClosing để cleanup tất cả forms
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // Đóng tất cả các forms đang mở
            foreach (var form in _openForms.Values.ToList())
            {
                if (form != null && !form.IsDisposed)
                {
                    form.FormClosed -= null; // Remove event handler để tránh show lại FormGiaoDien
                    form.Close();
                }
            }

            _openForms.Clear();
        }
    }

    public class RoundButton : Button
    {
        public int BorderRadius { get; set; } = 20;

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            RectangleF rectSurface = new RectangleF(0, 0, this.Width, this.Height);
            RectangleF rectBorder = new RectangleF(1, 1, this.Width - 2, this.Height - 2);

            using (GraphicsPath pathSurface = GetFigurePath(rectSurface, BorderRadius))
            using (GraphicsPath pathBorder = GetFigurePath(rectBorder, BorderRadius - 1))
            using (Pen penSurface = new Pen(this.Parent.BackColor, 2))
            using (Pen penBorder = new Pen(Color.CadetBlue, 1.75f))
            {
                this.Region = new Region(pathSurface);
                e.Graphics.DrawPath(penSurface, pathSurface);
            }
        }

        private GraphicsPath GetFigurePath(RectangleF rect, float radius)
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
}
