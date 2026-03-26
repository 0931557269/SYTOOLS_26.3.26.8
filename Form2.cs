using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace SYTOOLS
{
    public partial class Form2 : Form
    {
        public Form2(string url)
        {

            InitializeComponent();
            this.Resize += new System.EventHandler(this.Form_Resize);

            string decodedUrl = Uri.UnescapeDataString(url);

            TextBoxURL.Text = decodedUrl;

            //監聽 Enter Key  TrxtBoxurl.Text 前往 按鍵 開始 結束 一區
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(Form1_KeyDown);
            this.Load += new EventHandler(Form1_LoadurlEnter);
            Url = url;
            //監聽 Enter Key  TrxtBoxurl.Text 前往 按鍵 結束 一區
        }

        
        //監聽 Enter Key  TrxtBoxurl.Text 前往 按鍵 開始 結束 二區
        private void Form1_LoadurlEnter(object sender, EventArgs e)
        {   
            //   Buttongo.Location = new Point(10, 70);
            this.Controls.Add(WEBGoBack);
        }


        void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;   //將 Handled 設置為 true，指示已經處理過 KeyPress 事件
                BtnSubmit_Click(sender, e);
            }

            if (e.KeyCode == Keys.F5)
            {
                // WebView21.CoreWebView2.Reload();
                WebBrowser1.Document.ExecCommand("Refresh", false, null);//真正意义上的F5刷新
            }
        }


        void BtnSubmit_Click(object sender, EventArgs e)
        {
            // MessageBox.Show("hello world!");
            WEBGoBack.PerformClick();
        }


        //監聽 Enter Key  TrxtBoxurl.Text 前往 按鍵 結束 二區
        public string Url { get; internal set; }
        public object GoButton { get; private set; }


        private void GoButton_Click(object sender, EventArgs e)
        {
            WebBrowser1.GoBack();
            WebBrowser1.Navigate(TextBoxURL.Text);
        }

        private void TextBoxURL_TextChanged(object sender, EventArgs e)
        {
            // WebBrowser1.Navigate(TextBoxURL.Text);
        }

        private void Form2_Load(object sender, EventArgs e)
        {
              WEBGoBack.Left = 0; WEBGoBack.Width = 100;  // 上一頁
            TextBoxURL.Left = 100; TextBoxURL.Width = 1200; // 網址欄
           
            WebBrowser1.Top = 25; WebBrowser1.Left = 0; WebBrowser1.Width = this.ClientSize.Width; WebBrowser1.Height = this.ClientSize.Height - WebBrowser1.Top;  // 瀏覽器區域
            //   this.WebView21.BringToFront();  // 最頂層
            TextBoxURL.BringToFront();
            WEBGoBack.BringToFront();
            //   this.WebBrowser1.SendToBack();  // 最底層
            WebBrowser1.SendToBack();

            this.WebBrowser1.Navigate(this.Url);
                TextBoxURL.Text = this.Url;
            // WebBrowser1.MaximumSize = new Size(800, 800);
           // this.WindowState = FormWindowState.Maximized;
        }

        // 最大化 自動配合視窗大小
        private void Form_Resize(object sender, EventArgs e)
        {
            WebBrowser1.Size = this.ClientSize - new System.Drawing.Size(WebBrowser1.Location);
            WEBGoBack.Left = this.ClientSize.Width - WEBGoBack.Width;
            TextBoxURL.Width = WEBGoBack.Left - TextBoxURL.Left;
        }
       

        private void WebBrowser1_DocumentCompleted_1(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (e.Url != WebBrowser1.Url) return;

            string url = e.Url.ToString();
            string decodedUrl = WebUtility.UrlDecode(url);

            TextBoxURL.Text = decodedUrl;
        }
    }
}
