namespace Exercise3
{
    partial class FormMenuTCP
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
            buttonTCPServer = new Button();
            buttonTCPClient = new Button();
            SuspendLayout();
            // 
            // buttonTCPServer
            // 
            buttonTCPServer.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            buttonTCPServer.Location = new Point(29, 43);
            buttonTCPServer.Name = "buttonTCPServer";
            buttonTCPServer.Size = new Size(106, 30);
            buttonTCPServer.TabIndex = 0;
            buttonTCPServer.Text = "TCP Server";
            buttonTCPServer.UseVisualStyleBackColor = true;
            buttonTCPServer.Click += this.buttonTCPServer_Click;
            // 
            // buttonTCPClient
            // 
            buttonTCPClient.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 0);
            buttonTCPClient.Location = new Point(187, 43);
            buttonTCPClient.Name = "buttonTCPClient";
            buttonTCPClient.Size = new Size(106, 30);
            buttonTCPClient.TabIndex = 1;
            buttonTCPClient.Text = "TCP Client";
            buttonTCPClient.UseVisualStyleBackColor = true;
            buttonTCPClient.Click += this.buttonTCPClient_Click;
            // 
            // FormMenuTCP
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(324, 118);
            Controls.Add(buttonTCPClient);
            Controls.Add(buttonTCPServer);
            Name = "FormMenuTCP";
            Text = "Exercise3 - TCP";
            ResumeLayout(false);
        }

        #endregion

        private Button buttonTCPServer;
        private Button buttonTCPClient;
    }
}