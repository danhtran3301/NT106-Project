using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using TimeFlow.Services;
using TimeFlow.UI.Components;

namespace TimeFlow.Tasks
{
    /// <summary>
    /// Form tạo group mới với option thêm members
    /// </summary>
    public class FormCreateGroup : Form
    {
        private TextBox txtGroupName;
        private TextBox txtDescription;
        private TextBox txtMemberUsername;
        private ListBox lstMembers;
        private CustomButton btnAddMember;
        private CustomButton btnRemoveMember;
        private CustomButton btnCreate;
        private CustomButton btnCancel;
        private Label lblError;
        
        private readonly TaskApiClient _taskApi;
        private readonly List<string> _membersToAdd = new List<string>();

        // Event khi group được tạo thành công
        public event EventHandler<GroupCreatedEventArgs> GroupCreated;

        public FormCreateGroup()
        {
            _taskApi = new TaskApiClient();
            InitializeUI();
        }

        private void InitializeUI()
        {
            this.Text = "Create New Group";
            this.Size = new Size(450, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;

            int yPos = 20;
            int labelWidth = 100;
            int inputWidth = 300;

            // Group Name
            Label lblGroupName = new Label
            {
                Text = "Group Name *",
                Location = new Point(20, yPos),
                Size = new Size(labelWidth, 25),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            this.Controls.Add(lblGroupName);

            txtGroupName = new TextBox
            {
                Location = new Point(130, yPos),
                Size = new Size(inputWidth, 30),
                Font = new Font("Segoe UI", 11F)
            };
            this.Controls.Add(txtGroupName);
            yPos += 45;

            // Description
            Label lblDescription = new Label
            {
                Text = "Description",
                Location = new Point(20, yPos),
                Size = new Size(labelWidth, 25),
                Font = new Font("Segoe UI", 10F)
            };
            this.Controls.Add(lblDescription);

            txtDescription = new TextBox
            {
                Location = new Point(130, yPos),
                Size = new Size(inputWidth, 60),
                Font = new Font("Segoe UI", 10F),
                Multiline = true
            };
            this.Controls.Add(txtDescription);
            yPos += 75;

            // Separator
            Panel separator = new Panel
            {
                Location = new Point(20, yPos),
                Size = new Size(400, 1),
                BackColor = AppColors.Gray200
            };
            this.Controls.Add(separator);
            yPos += 20;

            // Add Members Section
            Label lblMembers = new Label
            {
                Text = "Add Members (Optional)",
                Location = new Point(20, yPos),
                Size = new Size(200, 25),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            this.Controls.Add(lblMembers);
            yPos += 30;

            // Username input
            txtMemberUsername = new TextBox
            {
                Location = new Point(20, yPos),
                Size = new Size(220, 30),
                Font = new Font("Segoe UI", 10F),
                PlaceholderText = "Enter username..."
            };
            this.Controls.Add(txtMemberUsername);

            btnAddMember = new CustomButton
            {
                Text = "+ Add",
                Location = new Point(250, yPos),
                Size = new Size(80, 30),
                BackColor = AppColors.Green500,
                ForeColor = Color.White,
                HoverColor = AppColors.Green600,
                BorderRadius = 4,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnAddMember.Click += BtnAddMember_Click;
            this.Controls.Add(btnAddMember);
            yPos += 40;

            // Members list
            lstMembers = new ListBox
            {
                Location = new Point(20, yPos),
                Size = new Size(310, 100),
                Font = new Font("Segoe UI", 10F),
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(lstMembers);

            btnRemoveMember = new CustomButton
            {
                Text = "Remove",
                Location = new Point(340, yPos + 35),
                Size = new Size(80, 30),
                BackColor = AppColors.Red500,
                ForeColor = Color.White,
                HoverColor = AppColors.Red600,
                BorderRadius = 4,
                Font = new Font("Segoe UI", 9F)
            };
            btnRemoveMember.Click += BtnRemoveMember_Click;
            this.Controls.Add(btnRemoveMember);
            yPos += 115;

            // Error label
            lblError = new Label
            {
                Location = new Point(20, yPos),
                Size = new Size(400, 25),
                ForeColor = AppColors.Red500,
                Font = new Font("Segoe UI", 9F),
                Visible = false
            };
            this.Controls.Add(lblError);
            yPos += 30;

            // Buttons
            btnCancel = new CustomButton
            {
                Text = "Cancel",
                Location = new Point(200, yPos),
                Size = new Size(100, 40),
                BackColor = Color.White,
                ForeColor = AppColors.Gray700,
                HoverColor = AppColors.Gray100,
                BorderRadius = 4,
                BorderThickness = 1,
                BorderColor = AppColors.Gray300,
                Font = new Font("Segoe UI", 10F)
            };
            btnCancel.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancel);

            btnCreate = new CustomButton
            {
                Text = "Create Group",
                Location = new Point(310, yPos),
                Size = new Size(120, 40),
                BackColor = AppColors.Blue500,
                ForeColor = Color.White,
                HoverColor = AppColors.Blue600,
                BorderRadius = 4,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            btnCreate.Click += BtnCreate_Click;
            this.Controls.Add(btnCreate);

            // Enter key handling
            txtGroupName.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) BtnCreate_Click(s, e); };
            txtMemberUsername.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) BtnAddMember_Click(s, e); };
        }

        private void BtnAddMember_Click(object sender, EventArgs e)
        {
            string username = txtMemberUsername.Text.Trim();
            
            if (string.IsNullOrWhiteSpace(username))
            {
                ShowError("Please enter a username");
                return;
            }

            if (_membersToAdd.Contains(username))
            {
                ShowError("User already added");
                return;
            }

            // TODO: Validate user exists on server
            _membersToAdd.Add(username);
            lstMembers.Items.Add(username);
            txtMemberUsername.Clear();
            txtMemberUsername.Focus();
            HideError();
        }

        private void BtnRemoveMember_Click(object sender, EventArgs e)
        {
            if (lstMembers.SelectedIndex >= 0)
            {
                string username = lstMembers.SelectedItem.ToString();
                _membersToAdd.Remove(username);
                lstMembers.Items.RemoveAt(lstMembers.SelectedIndex);
            }
        }

        private async void BtnCreate_Click(object sender, EventArgs e)
        {
            string groupName = txtGroupName.Text.Trim();
            string description = txtDescription.Text.Trim();

            // Validation
            if (string.IsNullOrWhiteSpace(groupName))
            {
                ShowError("Group name is required");
                txtGroupName.Focus();
                return;
            }

            if (groupName.Length < 3)
            {
                ShowError("Group name must be at least 3 characters");
                txtGroupName.Focus();
                return;
            }

            try
            {
                btnCreate.Enabled = false;
                btnCreate.Text = "Creating...";
                this.Cursor = Cursors.WaitCursor;

                // Create group
                int groupId = await _taskApi.CreateGroupAsync(groupName, description);

                if (groupId > 0)
                {
                    // Add members if any
                    int addedCount = 0;
                    foreach (string username in _membersToAdd)
                    {
                        try
                        {
                            await _taskApi.AddGroupMemberAsync(groupId, username);
                            addedCount++;
                        }
                        catch
                        {
                            // Ignore failed member adds, can add later
                        }
                    }

                    // Raise event
                    GroupCreated?.Invoke(this, new GroupCreatedEventArgs
                    {
                        GroupId = groupId,
                        GroupName = groupName,
                        MembersAdded = addedCount
                    });

                    MessageBox.Show(
                        $"Group '{groupName}' created successfully!\n{addedCount} members added.",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information
                    );

                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else
                {
                    ShowError("Failed to create group. Please try again.");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Error: {ex.Message}");
            }
            finally
            {
                btnCreate.Enabled = true;
                btnCreate.Text = "Create Group";
                this.Cursor = Cursors.Default;
            }
        }

        private void ShowError(string message)
        {
            lblError.Text = message;
            lblError.Visible = true;
        }

        private void HideError()
        {
            lblError.Visible = false;
        }
    }

    public class GroupCreatedEventArgs : EventArgs
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }
        public int MembersAdded { get; set; }
    }
}
