using CefSharp.OffScreen;
using System;
using System.Diagnostics;

namespace Gobchat.WebRenderer
{

    internal class WebRenderer : IWebRenderer, IDisposable
    {

        public ChromiumWebBrowser Browser { get; }

        public WebRenderer()
        {
            Browser = new ChromiumWebBrowser(address: "https://www.google.com/" /*"about:blank"*/, automaticallyCreateBrowser: true);
            Browser.FrameLoadStart += Browser_FrameLoadStart;
            Browser.FrameLoadEnd += Browser_FrameLoadEnd;
            Browser.BrowserInitialized += Browser_BrowserInitialized;
            Browser.ConsoleMessage += Browser_ConsoleMessage;
            Browser.LoadError += Browser_LoadError;
            Browser.LoadingStateChanged += Browser_LoadingStateChanged;


            var cefWindowInfo = new CefSharp.WindowInfo();
            cefWindowInfo.SetAsWindowless(IntPtr.Zero);
            cefWindowInfo.Width = 500;
            cefWindowInfo.Height = 500;

            var cefBrowserSettings = new CefSharp.BrowserSettings();
            cefBrowserSettings.WindowlessFrameRate = 60;

            //  browser.CreateBrowser(cefWindowInfo, cefBrowserSettings);

            cefWindowInfo.Dispose();
            cefBrowserSettings.Dispose();
        }



        private void Browser_LoadingStateChanged(object sender, CefSharp.LoadingStateChangedEventArgs e)
        {
            if (e.IsLoading)
            {
                Debug.WriteLine("Start loading page");
                return;
            }

            Debug.WriteLine("Page is loaded!");

        }

        private void Browser_BrowserInitialized(object sender, EventArgs e)
        {
            Browser.Size = new System.Drawing.Size(500, 500);
        }

        private void Browser_FrameLoadEnd(object sender, CefSharp.FrameLoadEndEventArgs e)
        {

        }

        private void Browser_FrameLoadStart(object sender, CefSharp.FrameLoadStartEventArgs e)
        {

        }

        private void Browser_LoadError(object sender, CefSharp.LoadErrorEventArgs e)
        {
            Debug.WriteLine($"{e.ErrorText}");
        }

        private void Browser_ConsoleMessage(object sender, CefSharp.ConsoleMessageEventArgs e)
        {
            Debug.WriteLine($"{e.Level} at {e.Source}/{e.Line}: {e.Message}");
        }

        public void Dispose()
        {

        }
    }
}
