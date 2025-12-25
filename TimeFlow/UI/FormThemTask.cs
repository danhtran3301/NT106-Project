using System;
using System.Drawing;
using System.Windows.Forms;
using TimeFlow.Models;
using TimeFlow.Services;

namespace TimeFlow.UI
{
    public partial class FormThemTask : Form
    {
        private readonly TaskApiClient _taskApi;
        private DateTime? preSelectedDate;
        private int? _taskIdToEdit;
        private TaskItem _taskToEdit;

        // Constructor mặc định
        public FormThemTask()
        {
            InitializeComponent();
            _taskApi = new TaskApiClient();
        }

        // Constructor với ngày được chọn trước (backward compatibility)
        public FormThemTask(FormGiaoDien parent, DateTime selectedDate) : this(selectedDate)
        {
            // Giữ lại để tương thích với code cũ
        }

        // Constructor với parent form (backward compatibility)
        public FormThemTask(FormGiaoDien parent) : this()
        {
            // Giữ lại để tương thích với code cũ
        }

        // Constructor với ngày được chọn trước
        public FormThemTask(DateTime selectedDate) : this()
        {
            preSelectedDate = selectedDate;
        }

        // Constructor để edit task
        public FormThemTask(int taskId) : this()
        {
            _taskIdToEdit = taskId;
            this.Text = "Chỉnh sửa Task";
        }

        private async void FormThemTask_Load(object sender, EventArgs e)
        {
            // Set ngày bắt đầu
            if (preSelectedDate.HasValue)
            {
                dateTimePicker1.Value = preSelectedDate.Value;
                dateTimePicker2.Value = preSelectedDate.Value.AddDays(1);
            }
            else
            {
                dateTimePicker1.Value = DateTime.Now;
                dateTimePicker2.Value = DateTime.Now.AddDays(1);
            }

            // ✅ Load Priority ComboBox
            comboBoxPriority.Items.Clear();
            comboBoxPriority.Items.Add("🟢 Low (Thấp)");
            comboBoxPriority.Items.Add("🟡 Medium (Trung bình)");
            comboBoxPriority.Items.Add("🔴 High (Cao)");
            comboBoxPriority.SelectedIndex = 1; // Default: Medium

            // ✅ Load Categories từ server
            await LoadCategoriesAsync();

            // Load task data nếu đang edit
            if (_taskIdToEdit.HasValue)
            {
                await LoadTaskForEdit(_taskIdToEdit.Value);
            }
        }

        // ✅ NEW: Load categories từ server
        private async System.Threading.Tasks.Task LoadCategoriesAsync()
        {
            try
            {
                var categories = await _taskApi.GetCategoriesAsync();
                
                comboBoxCategory.DisplayMember = "CategoryName";
                comboBoxCategory.ValueMember = "CategoryId";
                comboBoxCategory.DataSource = categories;

                // Default selection: first category
                if (comboBoxCategory.Items.Count > 0)
                {
                    comboBoxCategory.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể tải danh mục: {ex.Message}\n\nDanh mục sẽ được đặt là NULL.", 
                    "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                
                // Clear category if failed
                comboBoxCategory.DataSource = null;
                comboBoxCategory.Items.Clear();
            }
        }

        private async System.Threading.Tasks.Task LoadTaskForEdit(int taskId)
        {
            try
            {
                // Thay vì dùng TaskManager (dữ liệu giả), hãy gọi API lấy dữ liệu thật từ Server
                var taskDetail = await _taskApi.GetTaskDetailFullAsync(taskId);

                if (taskDetail != null)
                {
                    _taskToEdit = new TaskItem
                    {
                        TaskId = taskDetail.TaskId,
                        Title = taskDetail.Title,
                        Description = taskDetail.Description,
                        DueDate = taskDetail.DueDate,
                        Priority = taskDetail.Priority,
                        Status = taskDetail.Status,
                        CategoryId = taskDetail.CategoryId
                    };

                    // Hiển thị lên UI
                    textBox1.Text = _taskToEdit.Title;
                    richTextBox1.Text = _taskToEdit.Description ?? "";
                    if (_taskToEdit.DueDate.HasValue)
                        dateTimePicker1.Value = _taskToEdit.DueDate.Value;

                    comboBoxPriority.SelectedIndex = (int)_taskToEdit.Priority - 1;

                    // Chọn đúng Category trong ComboBox
                    if (_taskToEdit.CategoryId.HasValue)
                        comboBoxCategory.SelectedValue = _taskToEdit.CategoryId.Value;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu thật từ Server: {ex.Message}");
            }
        }
        private async void button2_Click_1(object sender, EventArgs e)
        {
            // Validation
            string title = textBox1.Text.Trim();
            if (string.IsNullOrEmpty(title))
            {
                MessageBox.Show("Vui lòng nhập tên nhiệm vụ!", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox1.Focus();
                return;
            }

            // Check authentication
            if (!SessionManager.IsAuthenticated || !SessionManager.CurrentUserId.HasValue)
            {
                MessageBox.Show("Vui lòng đăng nhập để thực hiện thao tác này!", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Close();
                return;
            }

            try
            {
                button2.Enabled = false;
                button2.Text = "Đang xử lý...";

                DateTime dueDate = dateTimePicker1.Value.Date;
                string description = richTextBox1.Text.Trim();

                // ✅ Validate DueDate before sending
                if (dueDate < DateTime.Now.Date.AddHours(-24))
                {
                    MessageBox.Show("Ngày hết hạn không thể quá 24 giờ trong quá khứ!", "Dữ liệu không hợp lệ",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    button2.Enabled = true;
                    button2.Text = "Submit";
                    return;
                }

                if (_taskIdToEdit.HasValue)
                {
                    // Update existing task
                    await UpdateTaskAsync(title, description, dueDate);
                }
                else
                {
                    // Create new task
                    await CreateTaskAsync(title, description, dueDate);
                }
            }
            catch (Data.Exceptions.ValidationException ex)
            {
                // ✅ Handle validation errors with field info
                string fieldName = ex.Field switch
                {
                    "Title" => "Tên nhiệm vụ",
                    "DueDate" => "Ngày hết hạn",
                    "Description" => "Mô tả",
                    "Priority" => "Độ ưu tiên",
                    "Status" => "Trạng thái",
                    "CreatedBy" => "Người tạo",
                    "CategoryId" => "Danh mục",
                    _ => ex.Field
                };

                MessageBox.Show(
                    $"Lỗi xác thực dữ liệu:\n\nTrường: {fieldName}\nLỗi: {ex.Message}",
                    "Dữ liệu không hợp lệ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                button2.Enabled = true;
                button2.Text = "Submit";
            }
            catch (Data.Exceptions.UnauthorizedException ex)
            {
                // ✅ Handle authorization errors
                MessageBox.Show(
                    $"Lỗi xác thực:\n{ex.Message}\n\nVui lòng đăng nhập lại.",
                    "Không có quyền",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                // Redirect to login
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
            catch (Exception ex)
            {
                // Generic errors
                MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                button2.Enabled = true;
                button2.Text = "Submit";
            }
        }

        private async System.Threading.Tasks.Task CreateTaskAsync(string title, string description, DateTime dueDate)
        {
            // ✅ Validate UserId
            if (!SessionManager.CurrentUserId.HasValue || SessionManager.CurrentUserId.Value <= 0)
            {
                throw new Exception("Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.");
            }

            // ✅ Get Priority from ComboBox
            TaskPriority priority = (TaskPriority)(comboBoxPriority.SelectedIndex + 1);

            // ✅ Get CategoryId from ComboBox (nullable)
            int? categoryId = null;
            if (comboBoxCategory.SelectedValue != null && comboBoxCategory.SelectedValue is int catId)
            {
                categoryId = catId;
            }

            var newTask = new TaskItem
            {
                Title = title,
                Description = description,
                DueDate = dueDate,
                Priority = priority, // ✅ From ComboBox
                Status = TimeFlow.Models.TaskStatus.Pending,
                CategoryId = categoryId, // ✅ From ComboBox (can be null)
                CreatedBy = SessionManager.CurrentUserId.Value,
                IsGroupTask = false,
                CreatedAt = DateTime.Now
            };

            int taskId = await _taskApi.CreateTaskAsync(newTask);

            if (taskId > 0)
            {
                MessageBox.Show("Task đã được tạo thành công!", "Thành công",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                throw new Exception("Không thể tạo task. Vui lòng thử lại!");
            }
        }

        private async System.Threading.Tasks.Task UpdateTaskAsync(string title, string description, DateTime dueDate)
        {
            if (_taskToEdit == null)
            {
                throw new Exception("Không tìm thấy thông tin task để cập nhật!");
            }

            // ✅ Get Priority from ComboBox
            TaskPriority priority = (TaskPriority)(comboBoxPriority.SelectedIndex + 1);

            // ✅ Get CategoryId from ComboBox
            int? categoryId = null;
            if (comboBoxCategory.SelectedValue != null && comboBoxCategory.SelectedValue is int catId)
            {
                categoryId = catId;
            }

            _taskToEdit.Title = title;
            _taskToEdit.Description = description;
            _taskToEdit.DueDate = dueDate;
            _taskToEdit.Priority = priority; // ✅ From ComboBox
            _taskToEdit.CategoryId = categoryId; // ✅ From ComboBox
            _taskToEdit.UpdatedAt = DateTime.Now;

            bool success = await _taskApi.UpdateTaskAsync(_taskToEdit);

            if (success)
            {
                MessageBox.Show("Task đã được cập nhật thành công!", "Thành công",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                throw new Exception("Không thể cập nhật task. Vui lòng thử lại!");
            }
        }

        // Empty event handlers for Designer
        private void textBox1_TextChanged(object sender, EventArgs e) { }
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e) { }
        private void richTextBox1_TextChanged(object sender, EventArgs e) { }
        private void pictureBox1_Click(object sender, EventArgs e) { }
        private void labelTaskDescription_Click(object sender, EventArgs e) { }
        private void labelTaskTime_Click(object sender, EventArgs e) { }
    }
}