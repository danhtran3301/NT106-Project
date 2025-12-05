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

namespace TimeFlow
{
    public partial class FormSettings : Form
    {
        public FormSettings()
        {
            this.Text = "Application Settings";
            this.Size = new Size(1100, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = AppColors.Gray50;
            this.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular);
            this.WindowState = FormWindowState.Maximized;

            InitializeComponent();
        }


    }
}