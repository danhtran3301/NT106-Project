using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TimeFlow.Models;
using TimeFlow.Services;
using TimeFlow.UI.Components;

namespace TimeFlow.Tasks
{
    /// <summary>
    /// Form tạo Group Task với đầy đủ thuộc tính và cho phép chọn assignee
    /// </summary>
    public class FormCreateGroupTask : Form
    {
        // Controls
        private TextBox txtTitle;
        private TextBox txtDescription;
        private DateTimePicker dtpDueDate;
        private CheckBox chkHasDueDate;
        private ComboBox cmbPriority;
        private ComboBox cmbCategory;
        private ComboBox cmbAssignee;
        private CustomButton btnCreate;
        private CustomButton btnCancel;
        private Label lblError;
        private Label lblLoading;

        // Data
        private readonly TaskApiClient _taskApi;
        private readonly int _groupId;
        private readonly string _groupName;
        private List<Category> _categories = new List<Category>();
        private List<GroupMemberDto> _members = new List<GroupMemberDto>();

        // Event khi task được tạo thành công
        public event EventHandler<GroupTaskCreatedEventArgs> TaskCreated;

        public FormCreateGroupTask(int groupId, string groupName)
        {
            _groupId = groupId;
            _groupName = groupName;
            _taskApi = new TaskApiClient();
            InitializeUI();
            LoadDataAsync();
        }

        private void InitializeUI()
        {
            this.Text = $"Create Task - {_groupName}";
            this.Size = new Size(500, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

            int yPos = 20;
            int labelWidth = 100;
            int inputWidth = 350;
            int leftMargin = 20;

            // Header
            Label lblHeader = new Label
            {
                Text = "📝 New Group Task",
                Location = new Point(leftMargin, yPos),
                Size = new Size(inputWidth + labelWidth, 30),
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = AppColors.Gray800
            };
            this.Controls.Add(lblHeader);
            yPos += 45;

            // Title *
            Label lblTitle = new Label
            {
                Text = "Title *",
                Location = new Point(leftMargin, yPos + 3),
                Size = new Size(labelWidth, 25),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            this.Controls.Add(lblTitle);

            txtTitle = new TextBox
            {
                Location = new Point(leftMargin + labelWidth, yPos),
                Size = new Size(inputWidth, 30),
                Font = new Font("Segoe UI", 11F)
            };
            this.Controls.Add(txtTitle);
            yPos += 40;

            // Description
            Label lblDescription = new Label
            {
                Text = "Description",
                Location = new Point(leftMargin, yPos + 3),
                Size = new Size(labelWidth, 25),
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(lblDescription);

            txtDescription = new TextBox
            {
                Location = new Point(leftMargin + labelWidth, yPos),
                Size = new Size(inputWidth, 70),
                Font = new Font("Segoe UI", 10F),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            this.Controls.Add(txtDescription);
            yPos += 80;

            // Due Date
            Label lblDueDate = new Label
            {
                Text = "Due Date",
                Location = new Point(leftMargin, yPos + 3),
                Size = new Size(labelWidth, 25),
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(lblDueDate);

            chkHasDueDate = new CheckBox
            {
                Location = new Point(leftMargin + labelWidth, yPos + 5),
                Size = new Size(20, 20),
                Checked = true
            };
            chkHasDueDate.CheckedChanged += (s, e) => dtpDueDate.Enabled = chkHasDueDate.Checked;
            this.Controls.Add(chkHasDueDate);

            dtpDueDate = new DateTimePicker
            {
                Location = new Point(leftMargin + labelWidth + 25, yPos),
                Size = new Size(inputWidth - 25, 30),
                Font = new Font("Segoe UI", 10F),
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "dd/MM/yyyy HH:mm",
                Value = DateTime.Now.AddDays(1)
            };
            this.Controls.Add(dtpDueDate);
            yPos += 40;

            // Priority
            Label lblPriority = new Label
            {
                Text = "Priority",
                Location = new Point(leftMargin, yPos + 3),
                Size = new Size(labelWidth, 25),
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(lblPriority);

            cmbPriority = new ComboBox
            {
                Location = new Point(leftMargin + labelWidth, yPos),
                Size = new Size(150, 30),
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbPriority.Items.AddRange(new object[] { "🟢 Low", "🟡 Medium", "🔴 High" });
            cmbPriority.SelectedIndex = 1; // Default: Medium
            this.Controls.Add(cmbPriority);
            yPos += 40;

            // Category
            Label lblCategory = new Label
            {
                Text = "Category",
                Location = new Point(leftMargin, yPos + 3),
                Size = new Size(labelWidth, 25),
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(lblCategory);

            cmbCategory = new ComboBox
            {
                Location = new Point(leftMargin + labelWidth, yPos),
                Size = new Size(200, 30),
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            this.Controls.Add(cmbCategory);
            yPos += 40;

            // Assignee
            Label lblAssignee = new Label
            {
                Text = "Assign To",
                Location = new Point(leftMargin, yPos + 3),
                Size = new Size(labelWidth, 25),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = AppColors.Blue600
            };
            this.Controls.Add(lblAssignee);

            cmbAssignee = new ComboBox
            {
                Location = new Point(leftMargin + labelWidth, yPos),
                Size = new Size(inputWidth, 30),
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            this.Controls.Add(cmbAssignee);
            yPos += 50;

            // Loading label
            lblLoading = new Label
            {
                Text = "⏳ Loading members...",
                Location = new Point(leftMargin + labelWidth, yPos - 25),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = AppColors.Gray500,
                Visible = true
            };
            this.Controls.Add(lblLoading);

            // Error label
            lblError = new Label
            {
                Location = new Point(leftMargin, yPos),
                Size = new Size(inputWidth + labelWidth, 25),
                ForeColor = AppColors.Red500,
                Font = new Font("Segoe UI", 9F),
                Visible = false
            };
            this.Controls.Add(lblError);
            yPos += 30;

            // Separator
            Panel separator = new Panel
            {
                Location = new Point(leftMargin, yPos),
                Size = new Size(inputWidth + labelWidth, 1),
                BackColor = AppColors.Gray200
            };
            this.Controls.Add(separator);
            yPos += 20;

            // Buttons
            btnCancel = new CustomButton
            {
                Text = "Cancel",
                Location = new Point(240, yPos),
                Size = new Size(100, 40),
                BackColor = Color.White,
                ForeColor = AppColors.Gray700,
                HoverColor = AppColors.Gray100,
                BorderRadius = 6,
                BorderThickness = 1,
                BorderColor = AppColors.Gray300,
                Font = new Font("Segoe UI", 10F)
            };
            btnCancel.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancel);

            btnCreate = new CustomButton
            {
                Text = "Create Task",
                Location = new Point(350, yPos),
                Size = new Size(120, 40),
                BackColor = AppColors.Blue500,
                ForeColor = Color.White,
                HoverColor = AppColors.Blue600,
                BorderRadius = 6,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnCreate.Click += BtnCreate_Click;
            this.Controls.Add(btnCreate);

            // Enter key handling
            txtTitle.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) BtnCreate_Click(s, e); };
            this.AcceptButton = null; // Disable default accept to allow multiline
        }

        private async void LoadDataAsync()
        {
            try
            {
                btnCreate.Enabled = false;
                lblLoading.Visible = true;

                // Load categories và members song song
                var categoriesTask = _taskApi.GetCategoriesAsync();
                var membersTask = _taskApi.GetGroupMembersAsync(_groupId);

                await System.Threading.Tasks.Task.WhenAll(categoriesTask, membersTask);

                _categories = await categoriesTask;
                _members = await membersTask;

                // Populate categories
                cmbCategory.Items.Clear();
                foreach (var cat in _categories)
                {
                    cmbCategory.Items.Add(cat.CategoryName);
                }
                if (cmbCategory.Items.Count > 0)
                    cmbCategory.SelectedIndex = 0;

                // Populate assignees
                cmbAssignee.Items.Clear();
                cmbAssignee.Items.Add("-- Unassigned --");
                foreach (var member in _members)
                {
                    string displayName = !string.IsNullOrEmpty(member.FullName) 
                        ? $"{member.FullName} (@{member.Username})" 
                        : $"@{member.Username}";
                    
                    if (member.Role == "Admin")
                        displayName = "👑 " + displayName;
                    
                    cmbAssignee.Items.Add(displayName);
                }
                cmbAssignee.SelectedIndex = 0;

                lblLoading.Visible = false;
                btnCreate.Enabled = true;
            }
            catch (Exception ex)
            {
                lblLoading.Text = "Failed to load data";
                lblLoading.ForeColor = AppColors.Red500;
                ShowError($"Error loading data: {ex.Message}");
            }
        }

        private async void BtnCreate_Click(object sender, EventArgs e)
        {
            // Validation
            string title = txtTitle.Text.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                ShowError("Title is required");
                txtTitle.Focus();
                return;
            }

            if (title.Length < 3)
            {
                ShowError("Title must be at least 3 characters");
                txtTitle.Focus();
                return;
            }

            try
            {
                btnCreate.Enabled = false;
                btnCreate.Text = "Creating...";
                this.Cursor = Cursors.WaitCursor;
                HideError();

                // Get selected category
                int? categoryId = null;
                if (cmbCategory.SelectedIndex >= 0 && cmbCategory.SelectedIndex < _categories.Count)
                {
                    categoryId = _categories[cmbCategory.SelectedIndex].CategoryId;
                }

                // Get selected assignee
                int? assignedTo = null;
                if (cmbAssignee.SelectedIndex > 0) // 0 = Unassigned
                {
                    int memberIndex = cmbAssignee.SelectedIndex - 1;
                    if (memberIndex >= 0 && memberIndex < _members.Count)
                    {
                        assignedTo = _members[memberIndex].UserId;
                    }
                }

                // Get priority
                TaskPriority priority = cmbPriority.SelectedIndex switch
                {
                    0 => TaskPriority.Low,
                    2 => TaskPriority.High,
                    _ => TaskPriority.Medium
                };

                // Build task
                var newTask = new TaskItem
                {
                    Title = title,
                    Description = txtDescription.Text.Trim(),
                    DueDate = chkHasDueDate.Checked ? dtpDueDate.Value : null,
                    Priority = priority,
                    Status = TimeFlow.Models.TaskStatus.Pending,
                    CategoryId = categoryId,
                    IsGroupTask = true
                };

                // Create task
                int taskId = await _taskApi.CreateGroupTaskAsync(newTask, _groupId, assignedTo);

                if (taskId > 0)
                {
                    // Get assignee name for display
                    string assigneeName = "Unassigned";
                    if (assignedTo.HasValue)
                    {
                        var member = _members.FirstOrDefault(m => m.UserId == assignedTo.Value);
                        assigneeName = member?.FullName ?? member?.Username ?? "Unknown";
                    }

                    // Raise event
                    TaskCreated?.Invoke(this, new GroupTaskCreatedEventArgs
                    {
                        TaskId = taskId,
                        Title = title,
                        GroupId = _groupId,
                        AssignedTo = assignedTo,
                        AssigneeName = assigneeName
                    });

                    MessageBox.Show(
                        $"Task '{title}' created successfully!\n\n" +
                        $"Group: {_groupName}\n" +
                        $"Assigned to: {assigneeName}",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    ShowError("Failed to create task. Please try again.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error: {ex.Message}");
            }
            finally
            {
                btnCreate.Enabled = true;
                btnCreate.Text = "Create Task";
                this.Cursor = Cursors.Default;
            }
        }

        private void ShowError(string message)
        {
            lblError.Text = "⚠ " + message;
            lblError.Visible = true;
        }

        private void HideError()
        {
            lblError.Visible = false;
        }
    }

    public class GroupTaskCreatedEventArgs : EventArgs
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = "";
        public int GroupId { get; set; }
        public int? AssignedTo { get; set; }
        public string AssigneeName { get; set; } = "";
    }
}
