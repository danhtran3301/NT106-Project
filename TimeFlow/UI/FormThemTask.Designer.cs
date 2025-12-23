namespace TimeFlow.UI
{
    partial class FormThemTask
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormThemTask));
            pictureBox1 = new PictureBox();
            button2 = new Button();
            richTextBox1 = new RichTextBox();
            textBox1 = new TextBox();
            labelTaskDescription = new Label();
            labelTaskFrequency = new Label();
            labelTaskTime = new Label();
            labelTaskName = new Label();
            label1 = new Label();
            dateTimePicker1 = new DateTimePicker();
            dateTimePicker2 = new DateTimePicker();
            comboBoxPriority = new ComboBox();
            labelPriority = new Label();
            comboBoxCategory = new ComboBox();
            labelCategory = new Label();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(-6, 1);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(950, 700);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            pictureBox1.Click += pictureBox1_Click;
            // 
            // button2
            // 
            button2.BackColor = SystemColors.ActiveCaption;
            button2.Font = new Font("Times New Roman", 16.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            button2.ForeColor = SystemColors.ActiveCaptionText;
            button2.Location = new Point(42, 590);
            button2.Name = "button2";
            button2.Size = new Size(232, 48);
            button2.TabIndex = 28;
            button2.Text = "Submit ";
            button2.UseVisualStyleBackColor = false;
            button2.Click += button2_Click_1;
            // 
            // richTextBox1
            // 
            richTextBox1.Location = new Point(161, 440);
            richTextBox1.Margin = new Padding(3, 4, 3, 4);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(318, 62);
            richTextBox1.TabIndex = 26;
            richTextBox1.Text = "";
            // 
            // textBox1
            // 
            textBox1.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox1.Location = new Point(161, 88);
            textBox1.Margin = new Padding(3, 4, 3, 4);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(417, 32);
            textBox1.TabIndex = 22;
            // 
            // labelTaskDescription
            // 
            labelTaskDescription.AutoSize = true;
            labelTaskDescription.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelTaskDescription.Location = new Point(31, 460);
            labelTaskDescription.Name = "labelTaskDescription";
            labelTaskDescription.Size = new Size(82, 25);
            labelTaskDescription.TabIndex = 21;
            labelTaskDescription.Text = "Yêu cầu ";
            labelTaskDescription.Click += labelTaskDescription_Click;
            // 
            // labelTaskFrequency
            // 
            labelTaskFrequency.AutoSize = true;
            labelTaskFrequency.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelTaskFrequency.Location = new Point(15, 253);
            labelTaskFrequency.Name = "labelTaskFrequency";
            labelTaskFrequency.Size = new Size(138, 25);
            labelTaskFrequency.TabIndex = 19;
            labelTaskFrequency.Text = "Ngày  kết thúc ";
            // 
            // labelTaskTime
            // 
            labelTaskTime.AutoSize = true;
            labelTaskTime.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelTaskTime.Location = new Point(15, 169);
            labelTaskTime.Name = "labelTaskTime";
            labelTaskTime.Size = new Size(130, 25);
            labelTaskTime.TabIndex = 18;
            labelTaskTime.Text = "Ngày bắt đầu ";
            labelTaskTime.Click += labelTaskTime_Click;
            // 
            // labelTaskName
            // 
            labelTaskName.AutoSize = true;
            labelTaskName.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelTaskName.Location = new Point(21, 95);
            labelTaskName.Name = "labelTaskName";
            labelTaskName.Size = new Size(124, 25);
            labelTaskName.TabIndex = 16;
            labelTaskName.Text = "Tên nhiệm vụ";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Book Antiqua", 22.2F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(132, 22);
            label1.Name = "label1";
            label1.Size = new Size(293, 44);
            label1.TabIndex = 15;
            label1.Text = "Thêm nhiệm vụ";
            // 
            // dateTimePicker1
            // 
            dateTimePicker1.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dateTimePicker1.Location = new Point(161, 162);
            dateTimePicker1.Margin = new Padding(3, 4, 3, 4);
            dateTimePicker1.Name = "dateTimePicker1";
            dateTimePicker1.Size = new Size(417, 32);
            dateTimePicker1.TabIndex = 24;
            // 
            // dateTimePicker2
            // 
            dateTimePicker2.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dateTimePicker2.Location = new Point(161, 253);
            dateTimePicker2.Margin = new Padding(3, 4, 3, 4);
            dateTimePicker2.Name = "dateTimePicker2";
            dateTimePicker2.Size = new Size(417, 32);
            dateTimePicker2.TabIndex = 29;
            // 
            // comboBoxPriority
            // 
            comboBoxPriority.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxPriority.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            comboBoxPriority.FormattingEnabled = true;
            comboBoxPriority.Location = new Point(161, 315);
            comboBoxPriority.Name = "comboBoxPriority";
            comboBoxPriority.Size = new Size(250, 33);
            comboBoxPriority.TabIndex = 30;
            // 
            // labelPriority
            // 
            labelPriority.AutoSize = true;
            labelPriority.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelPriority.Location = new Point(15, 318);
            labelPriority.Name = "labelPriority";
            labelPriority.Size = new Size(114, 25);
            labelPriority.TabIndex = 31;
            labelPriority.Text = "Độ ưu tiên";
            // 
            // comboBoxCategory
            // 
            comboBoxCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxCategory.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            comboBoxCategory.FormattingEnabled = true;
            comboBoxCategory.Location = new Point(161, 377);
            comboBoxCategory.Name = "comboBoxCategory";
            comboBoxCategory.Size = new Size(250, 33);
            comboBoxCategory.TabIndex = 32;
            // 
            // labelCategory
            // 
            labelCategory.AutoSize = true;
            labelCategory.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelCategory.Location = new Point(15, 380);
            labelCategory.Name = "labelCategory";
            labelCategory.Size = new Size(94, 25);
            labelCategory.TabIndex = 33;
            labelCategory.Text = "Danh mục";
            // 
            // FormThemTask
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(936, 700);
            Controls.Add(labelCategory);
            Controls.Add(comboBoxCategory);
            Controls.Add(labelPriority);
            Controls.Add(comboBoxPriority);
            Controls.Add(dateTimePicker2);
            Controls.Add(button2);
            Controls.Add(richTextBox1);
            Controls.Add(dateTimePicker1);
            Controls.Add(textBox1);
            Controls.Add(labelTaskDescription);
            Controls.Add(labelTaskFrequency);
            Controls.Add(labelTaskTime);
            Controls.Add(labelTaskName);
            Controls.Add(label1);
            Controls.Add(pictureBox1);
            Margin = new Padding(3, 4, 3, 4);
            Name = "FormThemTask";
            Text = "Thêm nhiệm vụ";
            Load += FormThemTask_Load;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBox1;
        private Button button2;
        private RichTextBox richTextBox1;
        private TextBox textBox1;
        private Label labelTaskDescription;
        private Label labelTaskFrequency;
        private Label labelTaskTime;
        private Label labelTaskName;
        private Label label1;
        private DateTimePicker dateTimePicker1;
        private DateTimePicker dateTimePicker2;
        private ComboBox comboBoxPriority;
        private Label labelPriority;
        private ComboBox comboBoxCategory;
        private Label labelCategory;
    }
}
