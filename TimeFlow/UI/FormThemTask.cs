using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TimeFlow.Authentication; // Để lấy SessionManager
using TimeFlow.Models;         // ✅ QUAN TRỌNG: Để dùng TaskItem chuẩn

namespace TimeFlow.UI
{
    public partial class FormThemTask : Form
    {
        
        private FormGiaoDien parentForm;
        private DateTime? preSelectedDate; // Ngày được chọn từ calendar
        private int? _taskIdToEdit = null; // ID của task đang edit (nếu có)

        public FormThemTask()
        {
            InitializeComponent();
        }

        public FormThemTask(FormGiaoDien parent)
        {
            InitializeComponent();
            parentForm = parent;
        }

        // ✅ Constructor mới với ngày được chọn trước
        public FormThemTask(FormGiaoDien parent, DateTime selectedDate)
        {
            InitializeComponent();
            parentForm = parent;
            preSelectedDate = selectedDate;
        }

        // ✅ Constructor để edit task
        public FormThemTask(int taskId) : this()
        {
            _taskIdToEdit = taskId;
            this.Text = "Chỉnh sửa Task";
        }

        private void FormThemTask_Load(object sender, EventArgs e)
        {
            // ✅ Set ngày bắt đầu nếu có ngày được chọn trước
            if (preSelectedDate.HasValue)
            {
                dateTimePicker1.Value = preSelectedDate.Value;
                // dateTimePicker2.Value = preSelectedDate.Value.AddDays(1); 
            }
            else
            {
                dateTimePicker1.Value = DateTime.Now;
                // dateTimePicker2.Value = DateTime.Now.AddDays(1);
            }

            // TODO: Nếu đang chỉnh sửa task, cần implement method GetTaskById trong FormGiaoDien
            // if (_taskIdToEdit.HasValue)
            // {
            //     var taskToEdit = parentForm?.GetTaskById(_taskIdToEdit.Value);
            //     if (taskToEdit != null)
            //     {
            //         textBox1.Text = taskToEdit.Title;
            //         richTextBox1.Text = taskToEdit.Description;
            //         dateTimePicker1.Value = taskToEdit.DueDate;
            //         this.Text = "Chỉnh sửa Task";
            //     }
            // }
        }



        // Các hàm sự kiện rỗng (Giữ lại để không lỗi Designer)
        private void textBox1_TextChanged(object sender, EventArgs e) { }
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e) { }
        private void richTextBox1_TextChanged(object sender, EventArgs e) { }
        private void pictureBox1_Click(object sender, EventArgs e) { }
        private void labelTaskDescription_Click(object sender, EventArgs e) { }
        private void labelTaskTime_Click(object sender, EventArgs e) { }

        // Hàm button2_Click_1 này bị thừa (trùng lặp logic), tôi đã gộp lên trên rồi.
        // Bạn có thể xóa hoặc comment lại hàm này.
        private void button2_Click_1(object sender, EventArgs e)
        {  // ✅ Lấy thông tin task từ các control
            string title = textBox1.Text.Trim();
            DateTime startDate = dateTimePicker1.Value.Date;

            // Lưu ý: Model nhóm có vẻ chỉ có DueDate (Hạn chót), không có StartDate/EndDate riêng.
            // Tạm thời mình sẽ dùng StartDate làm DueDate.

            string description = richTextBox1.Text.Trim();

            // Validation
            if (string.IsNullOrEmpty(title))
            {
                MessageBox.Show("Vui lòng nhập tên nhiệm vụ!", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox1.Focus();
                return;
            }

            // ✅ Tạo task mới với thông tin đầy đủ theo Model CHUẨN
            var newTask = new TaskItem
            {
                TaskId = 0, // Database sẽ tự tăng, hoặc logic AddTaskFromForm sẽ xử lý
                Title = title,
                Description = description,

                // Map ngày tháng
                DueDate = startDate,

                // Map trạng thái (Thay vì IsCompleted = false)
                // Giả sử Enum: 1=New, 2=InProgress... Bạn cần check kỹ enum của nhóm.
                Status = (TimeFlow.Models.TaskStatus)1,
                // Các trường bắt buộc khác (nếu Model yêu cầu)
                Priority = TaskPriority.Medium, // Mặc định
                CreatedBy = 0, // Cần ID user (int), lấy từ SessionManager nếu có thể parse được
                IsGroupTask = false,
                CreatedAt = DateTime.Now
            };

            // Gọi hàm public ở GiaoDien để thêm task
            parentForm?.AddTaskFromForm(newTask);

            // Đóng form
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}