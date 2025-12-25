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
            richTextBoxMessage.Location = new Point(53, 141);
            richTextBoxMessage.Margin = new Padding(3, 4, 3, 4);
            richTextBoxMessage.Name = "richTextBoxMessage";
            richTextBoxMessage.ReadOnly = true;
            richTextBoxMessage.Size = new Size(505, 359);
            richTextBoxMessage.TabIndex = 19;
            richTextBoxMessage.Text = "";
            // 
            // buttonListen
            // 
            buttonListen.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            buttonListen.Location = new Point(366, 51);
            buttonListen.Margin = new Padding(3, 4, 3, 4);
            buttonListen.Name = "buttonListen";
            buttonListen.Size = new Size(192, 39);
            buttonListen.TabIndex = 18;
            buttonListen.Text = "Listen";
            buttonListen.UseVisualStyleBackColor = true;
            buttonListen.Click += buttonListen_Click;
            // 
            // textBoxPortNumber
            // 
            textBoxPortNumber.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBoxPortNumber.Location = new Point(103, 53);
            textBoxPortNumber.Margin = new Padding(3, 4, 3, 4);
            textBoxPortNumber.Name = "textBoxPortNumber";
            textBoxPortNumber.Size = new Size(145, 32);
            textBoxPortNumber.TabIndex = 17;
            textBoxPortNumber.Text = "1010";
            // 
            // labelMessage
            // 
            labelMessage.AutoSize = true;
            labelMessage.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelMessage.Location = new Point(53, 109);
            labelMessage.Name = "labelMessage";
            labelMessage.Size = new Size(88, 28);
            labelMessage.TabIndex = 16;
            labelMessage.Text = "Message";
            // 
            // labelPortNumber
            // 
            labelPortNumber.AutoSize = true;
            labelPortNumber.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            labelPortNumber.Location = new Point(53, 56);
            labelPortNumber.Name = "labelPortNumber";
            labelPortNumber.Size = new Size(48, 28);
            labelPortNumber.TabIndex = 15;
            labelPortNumber.Text = "Port";
            // 
            // FormTCPServer
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(622, 537);
            Controls.Add(richTextBoxMessage);
            Controls.Add(buttonListen);
            Controls.Add(textBoxPortNumber);
            Controls.Add(labelMessage);
            Controls.Add(labelPortNumber);
            Margin = new Padding(3, 4, 3, 4);
            Name = "FormTCPServer";
            Text = "TCP Server";
            Load += FormTCPServer_Load;
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