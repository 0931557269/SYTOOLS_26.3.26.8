using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using Microsoft.Web.WebView2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SYTOOLS.Properties;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Web;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;
using System.Net.Sockets;
using System.Diagnostics.Eventing.Reader;
using System.Reflection.Emit;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Microsoft.Web.WebView2.Wpf;


namespace SYTOOLS
{
    public partial class Form1 : Form
    {

        private readonly Timer TestTimer = new Timer();

        [DllImport("user32.dll", EntryPoint = "FindWindow", CharSet = CharSet.Auto)]
        private extern static IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        public const int WM_CLOSE = 0x10;  // 定時關閉MessageBox視窗 

        private int WM_SYSCOMMAND = 0x112;
        private long SC_MAXIMIZE = 0xF030;
        private long SC_MINIMIZE = 0xF020;
        private long SC_CLOSE = 0xF060;

        public object Server { get; private set; }
        public object MapPath { get; private set; }

        public Form1()
        {    // 定時關閉 MsgBox 用 ( 計時器 )
            TestTimer.Tick += new EventHandler(Call);
            TestTimer.Interval = 500 * 1;
            TestTimer.Enabled = true;

            InitializeComponent();
            this.Resize += new System.EventHandler(this.Form_Resize);
            //暫不用   WebView21.NavigationStarting += EnsureHttps;    // 提示 : http:// 不安全  :https:// 建議
            InitializeAsync();

            InitWebView();

            //監聽 Enter Key  TrxtBoxurl.Text 前往 按鍵 開始 結束 一區
            this.KeyPreview = true;
            this.KeyDown += new KeyEventHandler(Form1_KeyDown);
            this.Load += new EventHandler(Form1_LoadurlEnter);
            //監聽 Enter Key  TrxtBoxurl.Text 前往 按鍵 結束 一區
        }


        private async void InitWebView()
        {
            await WebView21.EnsureCoreWebView2Async();
            // 攔截一般導航
            WebView21.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
            // 攔截新視窗 target="_blank"
            WebView21.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
            // 攔截 WebBrowser1 的新視窗事件

            WebView21.CoreWebView2.SourceChanged += CoreWebView2_SourceChanged;
            WebBrowser1.NewWindow += WebBrowser1_NewWindow;
        }

        private void WebBrowser1_NewWindow(object sender, CancelEventArgs e)
        // 攔截 WebBrowser1 的新視窗事件 傳送 URL 到 Form2 WebBrowser1
        {
            // 1. 阻止系統自動開啟 IE 視窗
            e.Cancel = true;

            // 2. 獲取點擊的 URL
            string url = WebBrowser1.Document.ActiveElement.GetAttribute("href");

            // 如果 href 是空的（可能是透過 JavaScript 跳轉），嘗試獲取目前滑鼠下的元素
            if (string.IsNullOrEmpty(url))
            {
                url = WebBrowser1.StatusText; // 有時 URL 會暫存在狀態列
               // MessageBox.Show("攔截到新視窗請求111：" + url);
               // e.Cancel = true;
            }

            if (!string.IsNullOrEmpty(url))
            {
                // 3. 呼叫 Form2 並傳入網址
                Form2 f2 = new Form2(url);    
                f2.Show();
               // MessageBox.Show("攔截到新視窗請求222：" + url);
            }
           // throw new NotImplementedException(); 開IE 視窗
        }

        // 使用 urlList.txt 比對 是否使用 Form2 WebBrowser1 開啟新視窗 還是 使用 預設瀏覽器 開啟 URL
        // 攔截新視窗 target="_blank" 傳送 URL 到 Form2 WebBrowser1
        private void CoreWebView2_NewWindowRequested(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NewWindowRequestedEventArgs e)
        {
            string LocalPath = Path.Combine(Application.StartupPath, "urlList.txt");
            string url = e.Uri;
            string decodedUrl = Uri.UnescapeDataString(url);
            TextBoxurl.Text = decodedUrl;
            // http:// → 外部瀏覽器

            // 1. 確保檔案存在
            if (File.Exists(LocalPath))
            {
                // 2. 讀取檔案中所有的行 (每一行代表一個關鍵字或網址)
                string[] urlTargets = File.ReadAllLines(LocalPath);

                // 3. 檢查目前的 decodedUrl 是否包含清單中的任何一項
                bool isMatch = false;
                foreach (string target in urlTargets)
                {
                    // 排除空白行，並判斷是否包含關鍵字
                    if (!string.IsNullOrWhiteSpace(target) && decodedUrl.Contains(target.Trim()))
                    {
                        isMatch = true;
                        break;
                    }
                }

                // 4. 如果匹配成功，開啟 Form2
                if (isMatch)
                {
                    Form2 f2 = new Form2(decodedUrl);
                    f2.Show();
                }
                else
                {                // 5. 如果沒有匹配，則使用預設瀏覽器開啟 URL           
                    string edgePath = @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe";

                    if (!System.IO.File.Exists(edgePath))
                        edgePath = @"C:\Program Files\Microsoft\Edge\Application\msedge.exe";

                    if (System.IO.File.Exists(edgePath))
                    {
                        string args = string.Format(
                            "--new-window --window-size=600,400 --window-position=100,100 \"{0}\"",
                            url.Replace("\"", "")   // 避免 URL 內有引號炸掉
                        );

                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = edgePath,
                            Arguments = args,
                            UseShellExecute = true,
                            WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal
                        });
                    }
                    else
                    {                  // 如果 Edge 不存在，則退回使用預設瀏覽器開啟 URL
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url)
                        {
                            UseShellExecute = true
                        });
                    }
                }

                e.Handled = true;
            }
        }

        // 無 target="_blank" 的超連結 直接在 WebView2 開啟
        private void CoreWebView2_NavigationStarting(object sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {   
            string url = e.Uri;
            string decodedUrl = Uri.UnescapeDataString(url);
            TextBoxurl.Text = decodedUrl;
            //  e.Cancel = true;  // 取消導航，讓 WebView2 不會自動導航到該 URL
            
        }

        // 攔截 WebView2 的 SourceChanged 事件，當 URL 以 .asp 結尾時，停止導航並開啟外部瀏覽器
        private void CoreWebView2_SourceChanged(object sender, CoreWebView2SourceChangedEventArgs e)
        {
            string url = WebView21.Source.ToString();
            string decodedUrl = Uri.UnescapeDataString(url);

            TextBoxurl.Text = decodedUrl;

            if (url.EndsWith(".asp", StringComparison.OrdinalIgnoreCase))
            {
                WebView21.Stop();

                Form2 f2 = new Form2(decodedUrl);
                f2.Show();
            }
        }

        //清理事件  釋放資源
        protected override void OnClosed(EventArgs e)
        {
            // 1. 停止導覽或清理事件（選配，增加穩定性）
            // 1. 先處理 Timer (這很重要，防止視窗關了 Timer 還在動)
            if (TestTimer != null)
            {
                TestTimer.Stop();
                TestTimer.Dispose();
              //  TestTimer = null;
            }

            // 2. 安全釋放資源
            if (WebView21 != null)
            {
                WebView21.Dispose();
                WebView21 = null; // 避免重複釋放
            }

            // 1. 停止 Timer (避免視窗關閉後還在執行計時邏輯)
            if (TestTimer != null)
            {
                TestTimer.Stop();
                TestTimer.Dispose();
            }

            // 2. 釋放 WebBrowser1
            if (WebBrowser1 != null)
            {
                // 如果有掛載事件處理器，建議先移除（避免事件造成的殘留）
                // WebBrowser1.DocumentCompleted -= YourMethod; 

                WebBrowser1.Dispose();
                WebBrowser1 = null;
            }

            // 3. 呼叫父類別方法   // 3. 呼叫基底類別方法
            base.OnClosed(e);
        }

        //自訂視窗 尺吋
        private void Size_Solidyear()
        {
               this.WindowState = FormWindowState.Normal;
            this.Width = 850;
            int x = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Size.Width - this.Size.Width - 10;     //- 最右邊 + 最左邊
            int y = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Size.Height - this.Size.Height - 50;   //- 最下方 + 最下方
            this.SetDesktopLocation(x, y);
            this.Top = 0;  // 靠上 Top
        }

        //版本 起始 比對 更新
        private void Call(object sender, EventArgs e)
        {
            try
            {
                // HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://www.google.com.tw/");
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://192.168.3.155/SYTOOLS_Version/Version.txt");
                request.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko";
                request.Method = "GET";

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                    TestTimer.Enabled = false;
                RadioButton1.Checked = true;

                if (RadioButton1.Checked == true)
                    this.RadioButton1.Text = "已檢測 Version";
                   else
                  {
                    //  this.RadioButton1.Text = "外部 OR Version";
                  }

                //  MessageBox.Show("網址運行  正常");
                BtnCheckUpdate.PerformClick();

                if (response.StatusCode != HttpStatusCode.OK)
                  {
                    TestTimer.Enabled = false;
                    RadioButton1.Checked = false;//   MessageBox.Show("網址運行  異常");
         
                  }
                    response.Close();
            }
            //catch (Exception exception)
            catch (Exception)
            {
                //  MessageBox.Show(exception.Message);
            }   
        }

        //    取的超連結網址 回饋  AddressBar.Text
        async void InitializeAsync()
        {
            await WebView21.EnsureCoreWebView2Async(null);
            WebView21.CoreWebView2.WebMessageReceived += UpdateAddressBar;
            await WebView21.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync("window.chrome.webview.postMessage(window.document.URL);");  //  AddressBar.Text 取的超連結網址                                                                                                                                 //  await WebView21.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync("window.chrome.webview.addEventListener(\'message\', event => alert(event.data));");
        }

        //   取的超連結網址 回饋  AddressBar.Text
        void UpdateAddressBar(object sender, CoreWebView2WebMessageReceivedEventArgs args)
        {
            String uri = args.TryGetWebMessageAsString();
            TextBoxurl.Text = uri;
            WebView21.CoreWebView2.PostWebMessageAsString(uri);
        }

        // 最大化 自動配合視窗大小
        private void Form_Resize(object sender, EventArgs e)
        {
            WebView21.Size = this.ClientSize - new System.Drawing.Size(WebView21.Location);
            WebBrowser1.Size = this.ClientSize - new System.Drawing.Size(WebBrowser1.Location);
            GoButton.Left = this.ClientSize.Width - GoButton.Width;
            TextBoxurl.Width = GoButton.Left - TextBoxurl.Left;
        }

        //監聽 Enter Key  TrxtBoxurl.Text 前往 按鍵 開始 結束 二區
        private void Form1_LoadurlEnter(object sender, EventArgs e)
        {
            //   Buttongo.Text = "提交";
            GoButton.Click += new EventHandler(GoButton_Click);
            //   Buttongo.Location = new Point(10, 70);
            this.Controls.Add(GoButton);
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
                WebView21.CoreWebView2.Reload();
            }
        }

        void BtnSubmit_Click(object sender, EventArgs e)
        {
            // MessageBox.Show("hello world!");
            GoButton.PerformClick();
        }

        //監聽 Enter Key  TrxtBoxurl.Text 前往 按鍵 結束 二區
        private void Form1_Load(object sender, EventArgs e)
        {
            TextBoxurl.Text = this.WebView21.Source.ToString();  // 起始網址 回填 網址列
            ToolStrip1.Visible = true;
            ToolStrip2.Visible = false;

            BtnCheckUpdate.Left = 10; BtnCheckUpdate.Top = 55; // 手動 測試更新

            Label_IP.Left = 10; Label_IP.Top = 43;  // 內部 IP
            Intel_IP.Left = 135; Intel_IP.Top = 60;  // 外部 IP  //Intel_IP

            RadioButton1.Left = 120; RadioButton1.Top = 43;  // 是否檢測到 Version.txt 版本  http://192.168.3.155/SYTOOLS_Version/Version.txt

            //Capture
            LblCapture.Left = 220; LblCapture.Top = 48;

            WEBGoBack.Left = 250; WEBGoBack.Top = 42;         //上一頁
            WEBGoForward.Left = 310; WEBGoForward.Top = 42;   //下一頁

            Label1.Left = 360; Label1.Top = 48;  //網址:
            TextBoxurl.Left = 400; TextBoxurl.Top = 45; TextBoxurl.Width = 500;  //網址列 TEXT
            GoButton.Left = 690; GoButton.Top = 45; GoButton.Width = 80;//前往按紐

            WebView21.Left = 0;  //靠左                        
            WebView21.Width = 850;   //寬
            WebView21.Height = 350;   //高
            WebView21.Top = 75;      //靠上

            WebBrowser1.Left = 0;  WebBrowser1.Width = 850;  WebBrowser1.Height = 350;   WebBrowser1.Top = 75;
            //   WebBrowser1.Visible = false;  //隱藏
            //   WebBrowser1.Visible = true;   //顯示
            Size = new Size(850, 520);   // Form1 預設 寬 / 高

            // 起始 FORM1 位置  (目前 右上角 )
            int x = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Size.Width - this.Size.Width - 10;     //- 最右邊 + 最左邊
            int y = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Size.Height - this.Size.Height - 50;   //- 最下方 + 最下方
            this.SetDesktopLocation(x - x, y); // 螢幕置 左
            this.Top = 0;  // 靠上 Top

            // 視窗 上下層 顯示  隱藏
            //   this.WebView21.BringToFront();  // 最頂層
            //this.WebView21.SendToBack();  // 最底層
            this.WebView21.BringToFront();  // 最頂層
                                            //   WebView21.Visible = true;   //顯示
            this.WebView21.Source = new Uri("http://192.168.3.155/indexweb.html");                          //    WebView21.Visible = false;  //隱藏

            //this.WebBrowser1.BringToFront();  // 最頂層
            this.WebBrowser1.SendToBack();  // 最底層
                                            //   this.WebBrowser1.SendToBack();  // 最底層
                                            //   WebBrowser1.Visible = true;   //顯示    //   WebBrowser1.Visible = false;  //隱藏
            this.WebBrowser1.Navigate("http://192.168.3.155/indexweb.html");  // 內部網頁網址
            

            //抓本機IP 192.168.xxx.xxx 開始
            string localIP = string.Empty;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                try
                {
                    socket.Connect("8.8.8.8", 65530);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    localIP = endPoint.Address.ToString();
                }
                catch { }
            {
                Console.WriteLine("內網IP:" + localIP);
                this.Label_IP.Text = ("內網IP:" + localIP);
            }
            //抓本機IP 192.168.xxx.xxx 結束
        }


        // 前往按鈕 ( 是否有 fttp:// ) 並自動加入 以防錯誤
        private void GoButton_Click(object sender, EventArgs e)
        {
            this.WebView21.BringToFront();
            WebBrowser1.Visible = false;
            this.Height = 600;

            String url = TextBoxurl.Text;
            if (url.Length > 0)
            {
                if (!url.StartsWith("http"))
                {
                    url = "http://" + url;
                }
               
                if (WebBrowser1.Visible == true)
                {
                    WebBrowser1.Visible = true;
                }
                WebView21.CoreWebView2.Navigate(url);
            }

            if (WebView21 != null && WebView21.CoreWebView2 != null)
            {
                //   WebView21.CoreWebView2.Navigate(TextBoxurl.Text);
                WebView21.CoreWebView2.Navigate(url);
            }
        }

        // 前往外部 按鈕 ToolStripButton1_Clic
        private void ToolStripButton1_Click(object sender, EventArgs e)
        {
            ToolStrip2.Visible = true;
            ToolStrip1.Visible = false;
            TextBoxurl.Text = "http://http://192.168.3.155/indexweb.html";
            WebView21.CoreWebView2.Navigate(TextBoxurl.Text);
            this.Height = 110;
            WebBrowser1.Visible = false;  //隱藏   //   WebBrowser1.Visible = true;   //顯示
            Size_Solidyear();    //自訂視窗 尺吋
        }

        // 返回 內部 按鈕 ToolStripButton2_Clic
        private void ToolStripButton2_Click(object sender, EventArgs e)
        {
            ToolStrip1.Visible = true;
            ToolStrip2.Visible = false;
            TextBoxurl.Text = "http://192.168.3.155/indexweb.html";
            WebView21.CoreWebView2.Navigate(TextBoxurl.Text);
            // WebBrowser1.Visible = false;  //隱藏   //   WebBrowser1.Visible = true;   //顯示
            WebBrowser1.Visible = true;   //顯示
                                                     //  WebView21.CoreWebView2.Navigate("http://192.168.3.155/indexweb.html");
            WebBrowser1.Navigate("http://192.168.3.155/indexweb.html");
            
            // Size_Solidyear();    //自訂視窗 尺吋
            this.Height = 530;
        }

        // 上一頁
        private void WEBGoBack_Click(object sender, EventArgs e)
        {
            WebView21.Visible =  true;
            WebBrowser1.Visible = false;  //隱藏
            WebView21.CoreWebView2.GoBack();
        }
        // 下一頁
        private void WEBGoForward_Click(object sender, EventArgs e)
        {
            WebView21.Visible = true;
            WebBrowser1.Visible = false;  //隱藏
            WebView21.CoreWebView2.GoForward();
        }

        //版本更新 (手動)
        private void BtnCheckUpdate_Click(object sender, System.EventArgs  e)
        {
            // 獲取當前主程式版本號
            Version currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
            //發送請求，獲取最新主程式版本號
            WebClient client = new WebClient();
            string versionString = client.DownloadString("http://192.168.3.155/SYTOOLS_Version/Version.txt");

            Version latestVersion = new Version(versionString);

            this.Text  ="SYTOOLS 版本 " + latestVersion.ToString();  // 表提顯示 目前版本

            // 比較版本號，如果有新版本則提示用戶更新
            if (latestVersion > currentVersion)

            {
                DialogResult result = MessageBox.Show("發現新版本，請按 確定 更新？  ", "版本更新",MessageBoxButtons.OKCancel);

                if (result == DialogResult.OK)
                {
                    // 下载最新版本的主程式
                    client.DownloadFile("http://192.168.3.155/SYTOOLS_Version/urlList.txt", "urlList.txt");
                    client.DownloadFile("http://192.168.3.155/SYTOOLS_Version/Release.exe", "Release.exe");
                    client.DownloadFile("http://192.168.3.155/SYTOOLS_Version/SYTOOLS.exe", "SYTOOLS_NEW.exe");
                   // client.DownloadFile("http://192.168.3.155/SYTOOLS_Version/copy.exe", "copy.exe");
                    client.DownloadFile("http://192.168.3.155/SYTOOLS_Version/copy_NEW.exe", "copy_NEW.exe");//2026/03/12 26.3.12.8  新版製作

                    // 運行以下載更新安裝程序
                   // Process.Start("copy.exe");
                    Process.Start("copy_NEW.exe");//2026/03/12 26.3.12.8  新版製作
                    // 關閉當前程序
                    Application.Exit();
                }
            }
            else
            {
                client.DownloadFile("http://192.168.3.155/SYTOOLS_Version/urlList.txt", "urlList.txt");
                //  MessageBox.Show("當前已是最新版本。 ", "提示");
                //   Process.Start("syerp.exe"); 
                // Application.Exit();
                StartKiller();
                // MessageBox.Show("已是最自動關閉MessageBox視窗", "MessageBox");
                MessageBox.Show("當前已是最新版本。自動關閉", "MessageBox");
            }
        }


        // MessageBox 定時 
        private void StartKiller()
        {
            Timer timer = new Timer
            {
                Interval = 1000 //1秒啟動 = 1000
            };
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Start();
        }

        // MessageBox 定時 關閉
        private void Timer_Tick(object sender, EventArgs e)
        {
            KillMessageBox();
            //停止Timer 
            ((Timer)sender).Stop();
        }

        //  MessageBox 名稱 (定時關閉)
        private void KillMessageBox()
        {
            //按照MessageBox的標題，找到MessageBox的視窗 
            IntPtr ptr = FindWindow(null, "MessageBox");
            if (ptr != IntPtr.Zero)
            {
                //找到則關閉MessageBox視窗 
                PostMessage(ptr, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            }
        }
      
        // TW IEERP
        private void ToolStripButtonTWIEERP_Click(object sender, EventArgs e)
        {
            this.WebBrowser1.BringToFront();
            WebBrowser1.Visible = true;
            WebBrowser1.Navigate("http://192.168.3.2/IEERP/IELogin.htm");
            // this.Height = 110;
            this.WindowState = FormWindowState.Normal;
            this.Height = 700;
            this.Width = 1000;
            
            // 起始 FORM1 位置  (目前 右上角 )
            int x = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Size.Width - this.Size.Width - 10;     //- 最右邊 + 最左邊
            int y = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Size.Height - this.Size.Height - 50;   //- 最下方 + 最下方
            this.SetDesktopLocation(x-x/2, y);
            this.Top = 0;  // 靠上 Top
            this.Left = 0;  // 靠左 Top
        }

        // CN IEERP
        private void ToolStripButtonCNIEERP_Click(object sender, EventArgs e)
        {
            this.WebBrowser1.BringToFront();
            WebBrowser1.Visible = true;
            WebBrowser1.Navigate("http://192.168.1.23/IEERP/");
            // this.Height = 110;
            this.WindowState = FormWindowState.Normal;
            this.Height = 700;
            this.Width = 1000;
            
            // 起始 FORM1 位置  (目前 右上角 )
            int x = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Size.Width - this.Size.Width - 10;     //- 最右邊 + 最左邊
            int y = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Size.Height - this.Size.Height - 50;   //- 最下方 + 最下方
            this.SetDesktopLocation(x - x / 2, y);
            this.Top = 0;  // 靠上 Top
            this.Left = 0;  // 靠左 Top
        }

        // // 內部 WebMail 主按鈕 Solidyear.com.tw
        private void ToolStripSplitButtonWEBMAIL1_ButtonClick(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://192.168.2.2/webmail");
            WebBrowser1.Visible = false;  //隱藏
                                          //   WebBrowser1.Visible = true;   //顯示
            this.Height = 110;  
            Size_Solidyear();    //自訂視窗 尺吋
        }

        // 內部 WebMail  Solidyear.com.tw
        private void SolidyearcomtwToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://192.168.2.2/webmail");

            WebBrowser1.Visible = false;  //隱藏
                                          //   WebBrowser1.Visible = true;   //顯示
            this.Height = 110;    
            Size_Solidyear();    //自訂視窗 尺吋
        }

        // 內部 WebMail 主按鈕 Solidtek.com.cn
        private void SolidtekcomcnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://192.168.1.1:888/webmail");
            WebBrowser1.Visible = false;  //隱藏
                                          //   WebBrowser1.Visible = true;   //顯示
            this.Height = 110;     
            Size_Solidyear();    //自訂視窗 尺吋
        }

        // 內部 CPS
        private void ToolStripButtonCPS1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://192.168.2.4/eDocument/MyUtility/login.jsp");
            WebBrowser1.Visible = false;  //隱藏
                                          //   WebBrowser1.Visible = true;   //顯示
            this.Height = 110;       
            Size_Solidyear();    //自訂視窗 尺吋
        }

        // TIPTOP 正式區
        private void ToolStripButtonTIPTOP_Click(object sender, EventArgs e)
        {
            WebBrowser1.Visible = true;
           // WebBrowser1.Navigate("http://192.168.3.155");
            WebBrowser1.Navigate("http://192.168.2.50/gas/wa/r/gdc-tiptop-udm-intranet");
            // this.Height = 110;
            this.Height = 110;         
            Size_Solidyear();    //自訂視窗 尺吋
        }

        // TOPTEST 測試區
        private void ToolStripButtonTOPTEST_Click(object sender, EventArgs e)
        {
            WebBrowser1.Visible = true;
          //  WebBrowser1.Navigate("http://192.168.3.155");
            WebBrowser1.Navigate("http://192.168.2.50/gas/wa/r/gdc-toptest-udm-intranet");
            //  this.Height = 110;
            this.Height = 110;          
            Size_Solidyear();    //自訂視窗 尺吋
        }

        // 外部  TW BPM
        private void ToolStripButtonTWBPM1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://202.133.250.143:8086/NaNaWeb/GP//ForwardIndex;jsessionid=uNWiHhlyW3cF0x8M7S2SFNtaieW2rF4U1_d9R15B.easyflow?hdnMethod=findIndexForward");
            WebBrowser1.Visible = false;  //隱藏
                                          //   WebBrowser1.Visible = true;   //顯示
            this.Height = 110;        
            Size_Solidyear();    //自訂視窗 尺吋
        }

        // 外部  CN BPM
        private void ToolStripButtonCNBPM1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://192.168.1.8/bpm");
            WebBrowser1.Visible = false;  //隱藏
                                          //   WebBrowser1.Visible = true;   //顯示
            this.Height = 110;        
            Size_Solidyear();    //自訂視窗 尺吋
        }

        // 內部  CN ISO
        private void ToolStripButtonCNISO_Click(object sender, EventArgs e)
        {
            WebView21.Visible = false;
            WebBrowser1.Visible = true;  //隱藏
            WebBrowser1.Navigate("http://192.168.1.2");

            // this.Height = 110;
            this.WindowState = FormWindowState.Normal;
                this.Height = 600;
                this.Width =850;
            
                // 起始 FORM1 位置  (目前 右上角 )
                int x = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Size.Width - this.Size.Width - 10;     //- 最右邊 + 最左邊
                int y = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Size.Height - this.Size.Height - 50;   //- 最下方 + 最下方
                this.SetDesktopLocation(x - x / 2, y);
                this.Top = 0;  // 靠上 Top
                this.Left = 0;  // 靠左 Top
        }
        

        // 內部 HR
        private void ToolStripButtonHR_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://192.168.2.44:8080/login/toCompanyLogin.action?d=192");
            WebBrowser1.Visible = false;  //隱藏
                                          //   WebBrowser1.Visible = true;   //顯示
                                          // this.Height = 110;
            this.Height = 110;
            Size_Solidyear();    //自訂視窗 尺吋
        }

        // EDGE 瀏覽器
        private void ToolStripButtonEDGE_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.google.com.tw");
            WebBrowser1.Visible = false;  //隱藏       //   WebBrowser1.Visible = true;   //顯示
            this.Height = 110;
            Size_Solidyear();    //自訂視窗 尺吋
        }

        // IE.Vbs 瀏覽器
        private void ToolStripButtonIE_Click(object sender, EventArgs e)
        {
            // 判斷 192.168.1.2_ISO.vbs 檔案是否存在
            string FileName = "C:\\Solidyear\\SYTOOLS\\InternetExplorer.vbs";
            string FileNameiso = "C:\\Solidyear\\SYTOOLS\\InternetExplorer.exe";
            this.Height = 110;
            if (System.IO.File.Exists(FileName))
            {
                //  MessageBox.Show(FileName + " 檔案存在");

                // FileName 是要執行的檔案
                Process notePad = new Process();
                notePad.StartInfo.FileName = (FileName);
                notePad.Start();
                
                Size_Solidyear();    //自訂視窗 尺吋
            }
            else
            {
                WebClient mywebClient = new WebClient();
                mywebClient.DownloadFile("http://192.168.3.155/SYTOOLS_Version/InternetExplorer.exe", @"C:\\SolidYear\\SYTOOLS\\InternetExplorer.exe");

                if (System.IO.File.Exists(FileNameiso))
                {
                    //  MessageBox.Show(FileName + " 檔案存在");
                    // FileName 是要執行的檔案
                    Process notePad = new Process();
                    notePad.StartInfo.FileName = (FileNameiso);
                    notePad.Start();
                    Size_Solidyear();    //自訂視窗 尺吋
                }

            }
        }

        // Chrome 瀏覽器
        private void ToolStripButtonChrome_Click(object sender, EventArgs e)
        {
            // 判斷 192.168.1.2_ISO.vbs 檔案是否存在
            string FileName = "C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe";
            string FileNameiso = "C:\\SolidYear\\SYTOOLS\\ChromeSetup.exe";
            this.Height = 110;
            if (System.IO.File.Exists(FileName))
            {
                //  MessageBox.Show(FileName + " 檔案存在");
                // FileName 是要執行的檔案
                Process notePad = new Process();
                notePad.StartInfo.FileName = (FileName);
                notePad.Start();
               
                Size_Solidyear();    //自訂視窗 尺吋
            }
            else
            {
                WebClient mywebClient = new WebClient();
                mywebClient.DownloadFile("https://dl.google.com/tag/s/appguid%3D%7B8A69D345-D564-463C-AFF1-A69D9E530F96%7D%26iid%3D%7BF4A706D2-6312-FFAB-249B-ABC9D14AA41F%7D%26lang%3Dzh-TW%26browser%3D5%26usagestats%3D1%26appname%3DGoogle%2520Chrome%26needsadmin%3Dprefers%26ap%3Dx64-stable-statsdef_1%26brand%3DYTUH%26installdataindex%3Dempty/update2/installers/ChromeSetup.exe", @"C:\\SolidYear\\SYTOOLS\\ChromeSetup.exe");
                //  System.Diagnostics.Process.Start("https://www.google.com/intl/zh-TW/chrome/next-steps.html?brand=YTUH&statcb=1&installdataindex=empty&defaultbrowser=0#");
                WebBrowser1.Visible = false;  //隱藏       //   WebBrowser1.Visible = true;   //顯示
                this.Height = 110;
                if (System.IO.File.Exists(FileNameiso))
                {
                    //  MessageBox.Show(FileName + " 檔案存在");
                    // FileName 是要執行的檔案
                    Process notePad = new Process();
                    notePad.StartInfo.FileName = (FileNameiso);
                    notePad.Start();
                    Size_Solidyear();    //自訂視窗 尺吋
                }
            }
        }

        // Firefox 瀏覽器
        private void ToolStripButtonFirefox_Click(object sender, EventArgs e)
        {
            {
                // 判斷 192.168.1.2_ISO.vbs 檔案是否存在
                string FileName = "C:\\Program Files\\Mozilla Firefox\\firefox.exe";
                string FileNameiso = "C:\\SolidYear\\SYTOOLS\\firefox.exe";
                this.Height = 110;
                if (System.IO.File.Exists(FileName))
                {
                    //  MessageBox.Show(FileName + " 檔案存在");
                    // FileName 是要執行的檔案
                    Process notePad = new Process();
                    notePad.StartInfo.FileName = (FileName);
                    notePad.Start();
                    Size_Solidyear();    //自訂視窗 尺吋
                }
                else
                {
                    WebClient mywebClient = new WebClient();
                    mywebClient.DownloadFile("https://download-installer.cdn.mozilla.net/pub/firefox/releases/116.0.2/win32/zh-TW/Firefox%20Installer.exe", @"C:\\SolidYear\\SYTOOLS\\firefox.exe");

                    WebBrowser1.Visible = false;  //隱藏       //   WebBrowser1.Visible = true;   //顯示
                    this.Height = 110;
                    if (System.IO.File.Exists(FileNameiso))
                    {
                        //  MessageBox.Show(FileName + " 檔案存在");
                        // FileName 是要執行的檔案
                        Process notePad = new Process();
                        notePad.StartInfo.FileName = (FileNameiso);
                        notePad.Start();    
                        Size_Solidyear();    //自訂視窗 尺吋
                    }
                }
            }
        }

        // IE 系統選項
        private void ToolStripButtonIEOptions_Click(object sender, EventArgs e)
        {
            // 判斷 192.168.1.2_ISO.vbs 檔案是否存在
            string FileName = "C:\\SolidYear\\SYTOOLS\\rundll32.dll.bat";
            string FileNameiso = "C:\\SolidYear\\SYTOOLS\\rundll32.dll.exe";
            this.Height = 110;
            if (System.IO.File.Exists(FileName))
            {
                //  MessageBox.Show(FileName + " 檔案存在");
                // FileName 是要執行的檔案
                Process notePad = new Process();
                notePad.StartInfo.FileName = (FileName);
                notePad.Start();    
                Size_Solidyear();    //自訂視窗 尺吋
            }
            else
            {
                WebClient mywebClient = new WebClient();
                mywebClient.DownloadFile("http://192.168.3.155/SYTOOLS_Version/rundll32.dll.exe", @"C:\\SolidYear\\SYTOOLS\\rundll32.dll.exe");

                WebBrowser1.Visible = false;  //隱藏       //   WebBrowser1.Visible = true;   //顯示
                this.Height = 110;
                if (System.IO.File.Exists(FileNameiso))
                {
                    //  MessageBox.Show(FileName + " 檔案存在");
                    // FileName 是要執行的檔案
                    Process notePad = new Process();
                    notePad.StartInfo.FileName = (FileNameiso);
                    notePad.Start();      
                    Size_Solidyear();    //自訂視窗 尺吋
                }
            }
        }

        //視窗最小化
        private void ToolStripButtonMin_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;  //視窗最小化
            // this.Close(); //關閉視窗        
        }

        private void ToolStripButtonMax_Click(object sender, EventArgs e)
        {
            //窗体最大化 / 還原
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
                Size_Solidyear();    //自訂視窗 尺吋
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;              
            }
        }

        // 關閉 視窗
        private void ToolStripButtonClose_Click(object sender, EventArgs e)
        {
            this.Close();   
        }


        // 外部 WebMail 主按鈕 Solidyear.com.tw
        private void ToolStripSplitButtonWEBMAIL2_ButtonClick(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://mail.solidyear.com.tw/webmail");
            WebBrowser1.Visible = false;  //隱藏
                                          //   WebBrowser1.Visible = true;   //顯示
            this.Height = 110;
            Size_Solidyear();    //自訂視窗 尺吋
        }

        // 外部 WebMail  Solidyear.com.tw
        private void SolidyearcomtwToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://mail.solidyear.com.tw/webmail");
            WebBrowser1.Visible = false;  //隱藏
                                          //   WebBrowser1.Visible = true;   //顯示
            this.Height = 110;
            Size_Solidyear();    //自訂視窗 尺吋
        }

        // 外部 WebMail  Solidtek.com.cn
        private void SolidtekcomcnToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://mail.solidtek.com.cn:888/webmail");
            WebBrowser1.Visible = false;  //隱藏  //   WebBrowser1.Visible = true;   //顯示                        
            this.Height = 110;
            Size_Solidyear();    //自訂視窗 尺吋
        }

        // 外部 CPS
        private void ToolStripButtonCPS2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://cps.solidyear.com.tw/eDocument/MyUtility/login.jsp");
            WebBrowser1.Visible = false;  //隱藏    //   WebBrowser1.Visible = true;   //顯示                             
            this.Height = 110;
            Size_Solidyear();    //自訂視窗 尺吋
        }

        // TW BPM
        private void ToolStripButtonTWBPM2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://202.133.250.143:8086/NaNaWeb/GP//ForwardIndex;jsessionid=uNWiHhlyW3cF0x8M7S2SFNtaieW2rF4U1_d9R15B.easyflow?hdnMethod=findIndexForward");
            WebBrowser1.Visible = false;  //隱藏     //   WebBrowser1.Visible = true;   //顯示                          
            this.Height = 110;
            Size_Solidyear();    //自訂視窗 尺吋
        }

        // CN BPM
        private void ToolStripButtonCNBPM2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://113.105.179.204:8018/bpm/");
            WebBrowser1.Visible = false;  //隱藏      //   WebBrowser1.Visible = true;   //顯示                         
            this.Height = 110;
            Size_Solidyear();    //自訂視窗 尺吋
        }


        //視窗最小化
        private void ToolStripButtonMin2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;  //視窗最小化
        }

        //窗体最大化 / 還原
        private void ToolStripButtonMax2_Click(object sender, EventArgs e)
        {
            //窗体最大化 / 還原
            if (this.WindowState == FormWindowState.Maximized)
            {
                this.WindowState = FormWindowState.Normal;
                
                Size_Solidyear();    //自訂視窗 尺吋
            }
            else
            {
                this.WindowState = FormWindowState.Maximized;  
            }
        }

        // 關閉視窗
        private void ToolStripButtonClose2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void 關閉程式ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // NotifyIcon1 程式執行 右下角 狀態圖示  滑鼠左鍵 
        private void NotifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Size_Solidyear();    //自訂視窗 尺吋
            
            if (e.Button == MouseButtons.Left)
            {
             //窗体最大化 / 還原
                if (this.WindowState == FormWindowState.Maximized)
                {
                    this.WindowState = FormWindowState.Normal;
                    
                    Size_Solidyear();    //自訂視窗 尺吋
                }
                else
                {
                    //   this.WindowState = FormWindowState.Maximized;
                    //   LabelWeb.Text = "最大化";
                    this.WindowState = FormWindowState.Normal;  
                    Size_Solidyear();    //自訂視窗 尺吋
                }
            
            }
        }

        private void Intel_IP_Click(object sender, EventArgs e)
        {
            //抓 外部 IP 192.168.xxx.xxx 開始
            string ExterIP;
            var regex = @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b";
            using (var webclient = new WebClient())
            {
                try
                {
                    var rawRes = webclient.DownloadString("http://checkip.dyndns.org/");
                    ExterIP = Regex.Match(rawRes, regex).Value;
                    Console.WriteLine("外網IP:" + ExterIP);
                    this.Intel_IP.Text = ("外網IP:" + ExterIP);
                }
                catch { }
            }
        }

        private void LblCapture_Click(object sender, EventArgs e)
        {
            try
            {
                // ⭐ Windows 10/11 剪取工具（最推薦）
                System.Diagnostics.Process.Start("explorer.exe", "ms-screenclip:");
            }
            catch
            {
                try
                {
                    // ⭐ 舊版剪取工具
                    System.Diagnostics.Process.Start("snippingtool.exe");
                }
                catch
                {
                    MessageBox.Show("剪取工具不存在！");
                }
            }
        }
    }
}

