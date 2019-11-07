using CefSharp.WinForms;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Gobchat.WebRenderer
{

    public partial class OverlayCefEmbedForm : Form
    {
        private readonly ChromiumWebBrowser browser;

        private IntPtr browserHandle;

        public OverlayCefEmbedForm()
        {
            InitializeComponent();

            browser = new CefSharp.WinForms.ChromiumWebBrowser("about:blank")
            {
                Dock = DockStyle.Fill
            };


            browser.HandleCreated += (object sender, EventArgs e) => browserHandle = browser.Handle;
            browser.IsBrowserInitializedChanged += OnEvent_IsBrowserInitializedChanged;


            browser.FrameLoadStart += OnEvent_FrameLoadStart;

            var htmlpath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"ui\gobchat.html");
            var ok = System.IO.File.Exists(htmlpath);
            var uri = new UriBuilder() { Scheme = Uri.UriSchemeFile, Host = "", Path = htmlpath }.Uri.AbsoluteUri;
            browser.Load(uri);

            Controls.Add(browser);
        }

        private void OnEvent_FrameLoadStart(object sender, CefSharp.FrameLoadStartEventArgs e)
        {
            var initScript = @"(async () => {
                await CefSharp.BindObjectAsync('GobchatAPI')
            })();";
            e.Frame.ExecuteJavaScriptAsync(initScript, "init");
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private void OnEvent_IsBrowserInitializedChanged(object sender, EventArgs e)
        {
            if (!browser.IsBrowserInitialized)
                return;




        }

    }
}
