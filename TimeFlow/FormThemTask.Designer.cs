namespace TimeFlow
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
            label1 = new Label();
            labelTaskName = new Label();
            labelTaskDate = new Label();
            labelTaskTime = new Label();
            labelTaskFrequency = new Label();
            labelTaskCategory = new Label();
            labelTaskDescription = new Label();
            textBox1 = new TextBox();
            comboBox1 = new ComboBox();
            dateTimePicker1 = new DateTimePicker();
            comboBox2 = new ComboBox();
            richTextBox1 = new RichTextBox();
            textBox2 = new TextBox();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new Font("Segoe UI Semibold", 14.25F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label1.Location = new Point(319, 32);
            label1.Name = "label1";
            label1.Size = new Size(145, 25);
            label1.TabIndex = 0;
            label1.Text = "Thêm nhiệm vụ";
            // 
            // labelTaskName
            // 
            labelTaskName.AutoSize = true;
            labelTaskName.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelTaskName.Location = new Point(164, 75);
            labelTaskName.Name = "labelTaskName";
            labelTaskName.Size = new Size(96, 20);
            labelTaskName.TabIndex = 1;
            labelTaskName.Text = "Tên nhiệm vụ";
            // 
            // labelTaskDate
            // 
            labelTaskDate.AutoSize = true;
            labelTaskDate.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelTaskDate.Location = new Point(164, 165);
            labelTaskDate.Name = "labelTaskDate";
            labelTaskDate.Size = new Size(101, 20);
            labelTaskDate.TabIndex = 2;
            labelTaskDate.Text = "Ngày đến hạn";
            // 
            // labelTaskTime
            // 
            labelTaskTime.AutoSize = true;
            labelTaskTime.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelTaskTime.Location = new Point(164, 213);
            labelTaskTime.Name = "labelTaskTime";
            labelTaskTime.Size = new Size(71, 20);
            labelTaskTime.TabIndex = 3;
            labelTaskTime.Text = "Thời gian";
            // 
            // labelTaskFrequency
            // 
            labelTaskFrequency.AutoSize = true;
            labelTaskFrequency.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelTaskFrequency.Location = new Point(164, 263);
            labelTaskFrequency.Name = "labelTaskFrequency";
            labelTaskFrequency.Size = new Size(64, 20);
            labelTaskFrequency.TabIndex = 4;
            labelTaskFrequency.Text = "Tần suất";
            // 
            // labelTaskCategory
            // 
            labelTaskCategory.AutoSize = true;
            labelTaskCategory.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelTaskCategory.Location = new Point(164, 123);
            labelTaskCategory.Name = "labelTaskCategory";
            labelTaskCategory.Size = new Size(62, 20);
            labelTaskCategory.TabIndex = 5;
            labelTaskCategory.Text = "Thể loại";
            // 
            // labelTaskDescription
            // 
            labelTaskDescription.AutoSize = true;
            labelTaskDescription.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelTaskDescription.Location = new Point(164, 320);
            labelTaskDescription.Name = "labelTaskDescription";
            labelTaskDescription.Size = new Size(48, 20);
            labelTaskDescription.TabIndex = 6;
            labelTaskDescription.Text = "Mô tả";
            // 
            // textBox1
            // 
            textBox1.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox1.Location = new Point(266, 72);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(349, 27);
            textBox1.TabIndex = 7;
            // 
            // comboBox1
            // 
            comboBox1.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(266, 120);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(349, 28);
            comboBox1.TabIndex = 8;
            // 
            // dateTimePicker1
            // 
            dateTimePicker1.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            dateTimePicker1.Location = new Point(266, 160);
            dateTimePicker1.Name = "dateTimePicker1";
            dateTimePicker1.Size = new Size(349, 27);
            dateTimePicker1.TabIndex = 9;
            // 
            // comboBox2
            // 
            comboBox2.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            comboBox2.FormattingEnabled = true;
            comboBox2.Location = new Point(266, 260);
            comboBox2.Name = "comboBox2";
            comboBox2.Size = new Size(349, 28);
            comboBox2.TabIndex = 10;
            // 
            // richTextBox1
            // 
            richTextBox1.Location = new Point(266, 321);
            richTextBox1.Name = "richTextBox1";
            richTextBox1.Size = new Size(349, 96);
            richTextBox1.TabIndex = 11;
            richTextBox1.Text = "";
            // 
            // textBox2
            // 
            textBox2.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox2.Location = new Point(266, 210);
            textBox2.Name = "textBox2";
            textBox2.Size = new Size(349, 27);
            textBox2.TabIndex = 12;
            // 
            // FormThemTask
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(textBox2);
            Controls.Add(richTextBox1);
            Controls.Add(comboBox2);
            Controls.Add(dateTimePicker1);
            Controls.Add(comboBox1);
            Controls.Add(textBox1);
            Controls.Add(labelTaskDescription);
            Controls.Add(labelTaskCategory);
            Controls.Add(labelTaskFrequency);
            Controls.Add(labelTaskTime);
            Controls.Add(labelTaskDate);
            Controls.Add(labelTaskName);
            Controls.Add(label1);
            Name = "FormThemTask";
            Text = "Thêm nhiệm vụ";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private Label labelTaskName;
        private Label labelTaskDate;
        private Label labelTaskTime;
        private Label labelTaskFrequency;
        private Label labelTaskCategory;
        private Label labelTaskDescription;
        private TextBox textBox1;
        private ComboBox comboBox1;
        private DateTimePicker dateTimePicker1;
        private ComboBox comboBox2;
        private RichTextBox richTextBox1;
        private TextBox textBox2;
    }
}