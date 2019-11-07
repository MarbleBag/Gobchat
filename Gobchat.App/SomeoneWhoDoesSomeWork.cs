using Gobchat.UI.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Gobchat.Memory;
using Gobchat.Memory.Chat;

namespace Gobchat.Core
{
    public class GobchatAPI : IBrowserAPI
    {
        private IManagedWebBrowser _browser;
        private Gobchat.UI.Web.JavascriptBuilder jsBuilder = new Gobchat.UI.Web.JavascriptBuilder();

        public GobchatAPI(IManagedWebBrowser browser)
        {
            this._browser = browser;
        }

        public string APIName => "GobchatAPI";

        public void Message(string message)
        {
            Debug.WriteLine("JSMSG: " + message?.Replace("{", "{{")?.Replace("}", "}}"));

            var reader = new Newtonsoft.Json.JsonTextReader(new System.IO.StringReader(message));
            var serializer = new Newtonsoft.Json.JsonSerializer();
            var obj = serializer.Deserialize<Dictionary<string, string>>(reader);

            if (obj.ContainsKey("event"))
            {
                var eventName = obj["event"];
                if ("LoadGobchatConfig".Equals(eventName))
                {
                    var script = jsBuilder.BuildCustomEventDispatcher(new Gobchat.UI.Web.JavascriptEvents.LoadGobchatConfigEvent(null));
                    _browser.ExecuteScript(script);
                }
            }
        }
    }

    public sealed class SomeoneWhoDoesSomeWork : IDisposable
    {
        private CefOverlayForm _overlay;
        private GobchatAPI _api;
        private Memory.FFXIVMemoryProcessor _memoryProcessor;
        private KeyboardHook _keyboardHook;
        private ChatlogParser _chatlogParser;

        private readonly object lockObj = new object();
        private bool browserInitialized = false;

        internal void Initialize(UI.Forms.CefOverlayForm overlay)
        {
            _overlay = overlay;

            _memoryProcessor = new Memory.FFXIVMemoryProcessor();
            _memoryProcessor.ProcessChangeEvent += MemoryProcessor_ProcessChangeEvent;
            _memoryProcessor.ChatlogEvent += MemoryProcessor_ChatlogEvent;
            _memoryProcessor.Initialize();

            _api = new GobchatAPI(_overlay.Browser);
            _overlay.Browser.BindBrowserAPI(_api, true);

            _overlay.Browser.BrowserInitialized += OnEvent_Browser_BrowserInitialized;
            if (_overlay.Browser.IsBrowserInitialized)
            {
                InitializeBrowser();
            }

            _chatlogParser = new ChatlogParser();

            //not working
            //_keyboardHook = new KeyboardHook();
            // _keyboardHook.RegisterHotKey(ModifierKeys.Control, System.Windows.Forms.Keys.U, () => _overlay.InvokeUIThread(true, () => _overlay.Visible = false));
            // _keyboardHook.RegisterHotKey(ModifierKeys.Control, System.Windows.Forms.Keys.M, () => Debug.WriteLine("Yay!"));
        }

        private void OnEvent_Browser_BrowserInitialized(object sender, EventArgs e)
        {
            InitializeBrowser();
        }

        private void InitializeBrowser()
        {
            if (browserInitialized) return;
            lock (lockObj)
            {
                if (browserInitialized) return;
                browserInitialized = true;
            }

            _overlay.Browser.BrowserInitialized -= OnEvent_Browser_BrowserInitialized;

            var htmlpath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"resources\ui\gobchat.html");
            var ok = System.IO.File.Exists(htmlpath);
            var uri = new UriBuilder() { Scheme = Uri.UriSchemeFile, Host = "", Path = htmlpath }.Uri.AbsoluteUri;
            _overlay.Browser.Load(uri);
            //_overlay.Browser.Load("www.google.com");
        }

        private void MemoryProcessor_ProcessChangeEvent(object sender, Memory.ProcessChangeEvent e)
        {
            Debug.WriteLine($"FFXIV Process detected! {e.IsProcessValid} {e.ProcessId}");
        }

        private void MemoryProcessor_ChatlogEvent(object sender, ChatlogEvent e)
        {
            DateTime timeFilter = DateTime.Now.Subtract(TimeSpan.FromSeconds(10));

            e.ChatlogItems.ForEach(item =>
            {
                if (item.TimeStamp < timeFilter)
                    return;
                _chatlogParser.Process(item);

                Debug.WriteLine($"Log: {item}");
                //TODO process each message
                // - filter unwanted tokens
                // - extract source
                // - pack players and server
                // - build a useful structure
                // - send to browser


            });
        }



        

        

        internal void Update()
        {
            _memoryProcessor.Update();

            if (_overlay.Browser.IsBrowserInitialized)
            {
                //TODO
            }
        }

        #region IDisposable Support
        private bool _disposedValue = false; // To detect redundant calls

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~SomeoneWhoDoesSomeWork()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);

            if (!_disposedValue)
            {

                _keyboardHook?.Dispose();
                _keyboardHook = null;

                _memoryProcessor = null;

                _api = null;

                _overlay = null;

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                _disposedValue = true;
            }
        }
        #endregion


    }
}
