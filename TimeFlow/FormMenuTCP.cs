using BT3_LTMCB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Exercise3
{
    public partial class FormMenuTCP : Form
    {
        public FormMenuTCP()
        {
            InitializeComponent();
        }
        private void buttonTCPServer_Click(object sender, EventArgs e)
        {
            FormTCPServer formTCPServer = new FormTCPServer();
            formTCPServer.Show();
        }
        private void buttonTCPClient_Click(object sender, EventArgs e)
        {
            FormDangNhap formTCPClient = new FormDangNhap();
            formTCPClient.Show();
        }
    }
}
