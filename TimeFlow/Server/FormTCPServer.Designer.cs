namespace TimeFlow.Server
{
    partial class FormTCPServer
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
            richTextBoxMessage = new RichTextBox();
            buttonListen = new Button();
            textBoxPortNumber = new TextBox();
            labelMessage = new Label();
            labelPortNumber = new Label();
            SuspendLayout();
            // 
            // richTextBoxMessage
            // 
            richTextBoxMessage.Location = new Point(46, 106);
            richTextBoxMessage.Name = "richTextBoxMessage";
            richTextBoxMessage.ReadOnly = true;
            richTextBoxMessage.Size = new Size(442, 270);
            richTextBoxMessage.TabIndex = 19;
            richTextBoxMessage.Text = "";
            // 
            // buttonListen
            // 
            buttonListen.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            buttonListen.Location = new Point(320, 38);
            buttonListen.Name = "buttonListen";
            buttonListen.Size = new Size(168, 29);
            buttonListen.TabIndex = 18;
            buttonListen.Text = "Listen";
            buttonListen.UseVisualStyleBackColor = true;
            buttonListen.Click += buttonListen_Click;
            // 
            // textBoxPortNumber
            // 
            textBoxPortNumber.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBoxPortNumber.Location = new Point(90, 40);
            textBoxPortNumber.Name = "textBoxPortNumber";
            textBoxPortNumber.Size = new Size(127, 27);
            textBoxPortNumber.TabIndex = 17;
            textBoxPortNumber.Text = "1010";
            // 
            // labelMessage
            // 
            labelMessage.AutoSize = true;
            labelMessage.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelMessage.Location = new Point(46, 82);
            labelMessage.Name = "labelMessage";
            labelMessage.Size = new Size(71, 21);
            labelMessage.TabIndex = 16;
            labelMessage.Text = "Message";
            // 
            // labelPortNumber
            // 
            labelPortNumber.AutoSize = true;
            labelPortNumber.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelPortNumber.Location = new Point(46, 42);
            labelPortNumber.Name = "labelPortNumber";
            labelPortNumber.Size = new Size(38, 21);
            labelPortNumber.TabIndex = 15;
            labelPortNumber.Text = "Port";
            // 
            // FormTCPServer
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(544, 403);
            Controls.Add(richTextBoxMessage);
            Controls.Add(buttonListen);
            Controls.Add(textBoxPortNumber);
            Controls.Add(labelMessage);
            Controls.Add(labelPortNumber);
            Name = "FormTCPServer";
            Text = "TCP Server";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private RichTextBox richTextBoxMessage;
        private Button buttonListen;
        private TextBox textBoxPortNumber;
        private Label labelMessage;
        private Label labelPortNumber;
    }
}