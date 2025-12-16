using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DOANNT106;

namespace TimeFlow.UI
{
    public partial class FormThemTask : Form
    {
        private GiaoDien parentForm;
        private DateTime? preSelectedDate; // Ngày được chọn từ calendar

        public FormThemTask()
        {
            InitializeComponent();
        }

        public FormThemTask(GiaoDien parent)
        {
            InitializeComponent();
            parentForm = parent;
        }

        // ✅ Constructor mới với ngày được chọn trước
        public FormThemTask(GiaoDien parent, DateTime selectedDate)
        {
            InitializeComponent();
            parentForm = parent;
            preSelectedDate = selectedDate;
        }

        private void FormThemTask_Load(object sender, EventArgs e)
        {
            // ✅ Set ngày bắt đầu nếu có ngày được chọn trước
            if (preSelectedDate.HasValue)
            {
                dateTimePicker1.Value = preSelectedDate.Value;
                dateTimePicker2.Value = preSelectedDate.Value.AddDays(1); // Mặc định kết thúc sau 1 ngày
            }
            else
            {
                dateTimePicker1.Value = DateTime.Now;
                dateTimePicker2.Value = DateTime.Now.AddDays(1);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // ✅ Lấy thông tin task từ các control
            string title = textBox1.Text.Trim();
            DateTime startDate = dateTimePicker1.Value.Date;
            DateTime endDate = dateTimePicker2.Value.Date;
            string description = richTextBox1.Text.Trim(); // Nội dung yêu cầu

            // Validation
            if (string.IsNullOrEmpty(title))
            {
                MessageBox.Show("Vui lòng nhập tên nhiệm vụ!", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox1.Focus();
                return;
            }

            if (endDate < startDate)
            {
                MessageBox.Show("Ngày kết thúc không thể sớm hơn ngày bắt đầu!", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                dateTimePicker2.Focus();
                return;
            }

            // ✅ Tạo task mới với thông tin đầy đủ
            var newTask = new TaskItem
            {
                Id = 0,
                Title = title,
                Description = description, // Nội dung yêu cầu
                Date = startDate, // Ngày bắt đầu
                EndDate = endDate, // Cần thêm property này vào TaskItem
                IsCompleted = false,
                AssignedTo = SessionManager.Username ?? "Current User"
            };

            // Gọi hàm public ở GiaoDien để thêm task
            parentForm?.AddTaskFromForm(newTask);

            // Đóng form
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void textBox1_TextChanged(object sender, EventArgs e) { }
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e) { }
        private void richTextBox1_TextChanged(object sender, EventArgs e) { }
        private void pictureBox1_Click(object sender, EventArgs e) { }
        private void labelTaskDescription_Click(object sender, EventArgs e) { }
        private void labelTaskTime_Click(object sender, EventArgs e) { }

        private void button2_Click_1(object sender, EventArgs e)
        {
            TaskItem task = new TaskItem
            {
                Title = textBox1.Text,
                Description = richTextBox1.Text,
                Date = dateTimePicker1.Value,
                EndDate = dateTimePicker2.Value,
                IsCompleted = false
            };

            parentForm.AddTaskFromForm(task);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
    }

