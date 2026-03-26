namespace SYTOOLS
{
    partial class Form2
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form2));
            this.WEBGoBack = new System.Windows.Forms.Button();
            this.TextBoxURL = new System.Windows.Forms.TextBox();
            this.WebBrowser1 = new System.Windows.Forms.WebBrowser();
            this.SuspendLayout();
            // 
            // WEBGoBack
            // 
            this.WEBGoBack.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.WEBGoBack.Location = new System.Drawing.Point(2, 0);
            this.WEBGoBack.Name = "WEBGoBack";
            this.WEBGoBack.Size = new System.Drawing.Size(86, 22);
            this.WEBGoBack.TabIndex = 5;
            this.WEBGoBack.Text = "上一頁";
            this.WEBGoBack.UseVisualStyleBackColor = true;
            this.WEBGoBack.Click += new System.EventHandler(this.GoButton_Click);
            // 
            // TextBoxURL
            // 
            this.TextBoxURL.Location = new System.Drawing.Point(94, 0);
            this.TextBoxURL.Name = "TextBoxURL";
            this.TextBoxURL.Size = new System.Drawing.Size(651, 22);
            this.TextBoxURL.TabIndex = 4;
            this.TextBoxURL.TextChanged += new System.EventHandler(this.TextBoxURL_TextChanged);
            // 
            // WebBrowser1
            // 
            this.WebBrowser1.Location = new System.Drawing.Point(2, 28);
            this.WebBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.WebBrowser1.Name = "WebBrowser1";
            this.WebBrowser1.Size = new System.Drawing.Size(1291, 629);
            this.WebBrowser1.TabIndex = 3;
            this.WebBrowser1.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.WebBrowser1_DocumentCompleted_1);
            // 
            // Form2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1305, 656);
            this.Controls.Add(this.WEBGoBack);
            this.Controls.Add(this.TextBoxURL);
            this.Controls.Add(this.WebBrowser1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form2";
            this.Text = "Open CN IOS";
            this.Load += new System.EventHandler(this.Form2_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button WEBGoBack;
        private System.Windows.Forms.TextBox TextBoxURL;
        private System.Windows.Forms.WebBrowser WebBrowser1;
    }
}