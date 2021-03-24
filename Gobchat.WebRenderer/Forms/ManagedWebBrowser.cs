/*******************************************************************************
 * Copyright (C) 2019-2021 MarbleBag
 *
 * This program is free software: you can redistribute it and/or modify it under
 * the terms of the GNU Affero General Public License as published by the Free
 * Software Foundation, version 3.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program. If not, see <https://www.gnu.org/licenses/>
 *
 * SPDX-License-Identifier: AGPL-3.0-only
 *******************************************************************************/

using CefSharp;
using CefSharp.Event;
using Gobchat.UI.Forms.Helper;
using Gobchat.UI.Web;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gobchat.UI.Forms
{
    internal sealed class ManagedWebBrowser : CefSharp.OffScreen.ChromiumWebBrowser, CefSharp.Internals.IRenderWebBrowser, IDisposable, IManagedWebBrowser
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        //unused - yet
        private class BrowserWrapper : CefSharp.OffScreen.ChromiumWebBrowser, CefSharp.Internals.IRenderWebBrowser
        {
            public delegate void OnPaint(CefSharp.PaintElementType type, CefSharp.Structs.Rect dirtyRect, IntPtr buffer, int width, int height);

            public delegate void OnCursorChange(IntPtr cursor, CefSharp.Enums.CursorType type, CefSharp.Structs.CursorInfo customCursorInfo);

            public OnPaint OnPaintDelegate;
            public OnCursorChange OnCursorChangeDelegate;

            public BrowserWrapper()
            {
            }

            void CefSharp.Internals.IRenderWebBrowser.OnPaint(CefSharp.PaintElementType type, CefSharp.Structs.Rect dirtyRect, IntPtr buffer, int width, int height)
            {
                OnPaintDelegate.Invoke(type, dirtyRect, buffer, width, height);
            }

            void CefSharp.Internals.IRenderWebBrowser.OnCursorChange(IntPtr cursor, CefSharp.Enums.CursorType type, CefSharp.Structs.CursorInfo customCursorInfo)
            {
                OnCursorChangeDelegate.Invoke(cursor, type, customCursorInfo);
            }
        }

        private readonly object lockObj = new object();

        private readonly CefOverlayForm Form;

        private CefSharp.OffScreen.ChromiumWebBrowser CefBrowser { get { return this; } }

        private readonly MouseEventHelper mouseEventHelper = new MouseEventHelper();
        private readonly List<IBrowserAPI> _availableAPIs = new List<IBrowserAPI>();

        public event EventHandler<BrowserConsoleLogEventArgs> OnBrowserConsoleLog;

        public event EventHandler<BrowserErrorEventArgs> OnBrowserError;

        public event EventHandler<BrowserLoadPageEventArgs> OnBrowserLoadPage;

        public event EventHandler<BrowserLoadPageEventArgs> OnBrowserLoadPageDone;

        public new event EventHandler<BrowserInitializedEventArgs> BrowserInitialized;

        public event EventHandler<BrowserAPIBindingEventArgs> OnResolveBrowserAPI;

        event EventHandler<BrowserInitializedEventArgs> IManagedWebBrowser.OnBrowserInitialized
        {
            //someone may register after the original event has already fired
            add
            {
                lock (lockObj)
                {
                    if (CefBrowser.IsBrowserInitialized)
                        value.Invoke(this, new BrowserInitializedEventArgs());
                    else
                        BrowserInitialized += value;
                }
            }

            remove
            {
                BrowserInitialized -= value;
            }
        }

        public new bool IsBrowserInitialized { get { return base.IsBrowserInitialized; } }

        /// <summary>
        /// Get/set the size of the Chromium viewport in pixels. This also changes the size of the next rendered bitmap.
        /// </summary>
        public new System.Drawing.Size Size { get { return base.Size; } set { base.Size = value; } }

        /// <summary>
        /// Fired if a new page starts to load
        /// </summary>
        //public event EventHandler<BrowserLoadPageEventArgs> BrowserLoadPage;
        /// <summary>
        /// Fired if a new page is loaded
        /// </summary>
        //public event EventHandler<BrowserLoadPageEventArgs> BrowserLoadPageDone;
        //public new event EventHandler<BrowserInitializedEventArgs> BrowserInitialized;
        // public event EventHandler<BrowserConsoleLogEventArgs> BrowserConsoleLog;
        //public event EventHandler<BrowserErrorEventArgs> BrowserError;

        public ManagedWebBrowser(string address = "", BrowserSettings browserSettings = null,
            RequestContext requestContext = null, CefOverlayForm form = null) :
            base(address, browserSettings, requestContext, false)
        {
            Form = form;

            /*CefBrowser = new BrowserWrapper()
            {
                OnPaintDelegate = (t, d, b, w, h) => Form.OnBrowserRequestsPainting(d, b, w, h),
                OnCursorChangeDelegate = (cursor, t, c) => Form.SyncInvoke(() => Form.Cursor = new Cursor(cursor))
            };*/

            MenuHandler = new CustomContextMenuHandler(); //deactives context menu
            DownloadHandler = new CustomDownloadHandler();
            //LifeSpanHandler = new CustomLifeSpanHandler(); //TODO use to set icon and handle window.open
#if DEBUG
            KeyboardHandler = new CustomKeyboardHandler();
#endif

            CefBrowser.BrowserInitialized += OnEvent_BrowserInitialized;
            CefBrowser.FrameLoadStart += OnEvent_FrameLoadStart;
            CefBrowser.FrameLoadEnd += OnEvent_FrameLoadEnd;
            CefBrowser.LoadError += OnEvent_LoadError;
            CefBrowser.ConsoleMessage += OnEvent_ConsoleMessage;
            CefBrowser.JavascriptObjectRepository.ResolveObject += OnEvent_JavascriptResolveObject;
        }

        #region BrowserEventHandling

        private void OnEvent_FrameLoadStart(object sender, FrameLoadStartEventArgs e)
        {
            JSAwaitAPIBinding(e.Frame, _availableAPIs.ToArray());
            OnBrowserLoadPage?.Invoke(this, new BrowserLoadPageEventArgs(0, e.Url));
        }

        private void OnEvent_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            OnBrowserLoadPageDone?.Invoke(this, new BrowserLoadPageEventArgs(e.HttpStatusCode, e.Url));
        }

        private void OnEvent_LoadError(object sender, LoadErrorEventArgs e)
        {
            OnBrowserError?.Invoke(sender, new BrowserErrorEventArgs(e.ErrorCode, e.ErrorText, e.FailedUrl));
        }

        private void OnEvent_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            OnBrowserConsoleLog?.Invoke(sender, new BrowserConsoleLogEventArgs(e.Message, e.Source, e.Line));
        }

        private void OnEvent_BrowserInitialized(object sender, EventArgs e)
        {
            lock (lockObj)
            {
                if (CefBrowser.IsBrowserInitialized)
                {
                    var eventHandler = BrowserInitialized;
                    eventHandler?.Invoke(this, new BrowserInitializedEventArgs());
                    BrowserInitialized = null;
                }
            }
        }

        private void OnEvent_JavascriptResolveObject(object sender, JavascriptBindingEventArgs e)
        {
            OnResolveBrowserAPI?.Invoke(sender, new BrowserAPIBindingEventArgs(e.ObjectName, this));
        }

        #endregion BrowserEventHandling

        void CefSharp.Internals.IRenderWebBrowser.OnPaint(CefSharp.PaintElementType type, CefSharp.Structs.Rect dirtyRect, IntPtr buffer, int width, int height)
        {
            Form.OnBrowserRequestsPainting(dirtyRect, buffer, width, height);
        }

        void CefSharp.Internals.IRenderWebBrowser.OnCursorChange(IntPtr cursor, CefSharp.Enums.CursorType type, CefSharp.Structs.CursorInfo customCursorInfo)
        {
            Form.InvokeSyncOnUI(f => f.Cursor = new Cursor(cursor));
        }

        internal void StartBrowser(int width, int height)
        {
            var cefWindowInfo = new CefSharp.WindowInfo();
            cefWindowInfo.SetAsWindowless(IntPtr.Zero);
            cefWindowInfo.Width = width;
            cefWindowInfo.Height = height;

            var cefBrowserSettings = new CefSharp.BrowserSettings();
            cefBrowserSettings.WindowlessFrameRate = 60;
            cefBrowserSettings.FileAccessFromFileUrls = CefState.Enabled;
            cefBrowserSettings.UniversalAccessFromFileUrls = CefState.Enabled;
            cefBrowserSettings.WebSecurity = CefState.Enabled;
            cefBrowserSettings.Javascript = CefState.Enabled;
            cefBrowserSettings.LocalStorage = CefState.Enabled;
            cefBrowserSettings.Plugins = CefState.Disabled;

            CefBrowser.CreateBrowser(cefWindowInfo, cefBrowserSettings);
        }

        public void ShowDevTools()
        {
            if (IsBrowserInitialized)
                CefBrowser.ShowDevTools();
        }

        public void CloseBrowser(bool forceClose)
        {
            CefBrowser.GetBrowser().CloseBrowser(forceClose);
        }

        public new void Load(string url)
        {
            CefBrowser.Load(url);
        }

        public void Reload()
        {
            CefBrowser.Reload();
        }

        public new void Dispose()
        {
            logger.Debug($"Disposing {nameof(ManagedWebBrowser)}");
            CefBrowser.Dispose();
        }

        public bool BindBrowserAPI(IBrowserAPI api, bool isApiAsync)
        {
            foreach (var boundAPI in _availableAPIs)
            {
                if (boundAPI.APIName.Equals(api.APIName))
                    return false;
            }

            _availableAPIs.Add(api);
            CefBrowser.JavascriptObjectRepository.Register(api.APIName, api, isAsync: isApiAsync, options: CefSharp.BindingOptions.DefaultBinder);

            if (this.CefBrowser.IsBrowserInitialized && this.GetMainFrame() != null)
                AwaitAPIBinding(api);

            return true;
        }

        public bool UnbindBrowserAPI(IBrowserAPI api)
        {
            var removed = _availableAPIs.RemoveAll(e => e.APIName.Equals(api.APIName) && e == api);
            if (removed == 0)
                return false;

            if (!CefBrowser.JavascriptObjectRepository.IsBound(api.APIName))
                return false;

            var script = $@"(async ()=>{{ return await CefSharp.DeleteBoundObject('{ api.APIName}') }})();";

            if (this.CefBrowser.CanExecuteJavascriptInMainFrame)
            {
                var promise = this.GetMainFrame().EvaluateScriptAsync(script, "Gobchat");
                if (promise.Status == System.Threading.Tasks.TaskStatus.WaitingForActivation)
                {
                    promise.Wait(TimeSpan.FromSeconds(5));
                    if (promise.Status != System.Threading.Tasks.TaskStatus.WaitingForActivation)
                        promise.GetAwaiter().GetResult();
                }
                else
                    promise.GetAwaiter().GetResult();
            }

            return CefBrowser.JavascriptObjectRepository.UnRegister(api.APIName);
        }

        private void AwaitAPIBinding(params IBrowserAPI[] apis)
        {
            JSAwaitAPIBinding(this.GetMainFrame(), apis);
        }

        private void JSAwaitAPIBinding(IFrame frame, IEnumerable<IBrowserAPI> apis)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.Append("(async () => {");
            foreach (var boundAPI in apis)
            {
                builder.Append("await CefSharp.BindObjectAsync('");
                builder.Append(boundAPI.APIName);
                builder.Append("')");
            }
            builder.AppendLine("})();");
            var awaitAPIScript = builder.ToString();
            frame.ExecuteJavaScriptAsync(awaitAPIScript, "Initialize");
        }

        public void ExecuteScript(string script)
        {
            CefBrowser.GetMainFrame().ExecuteJavaScriptAsync(code: script, scriptUrl: "injected");
        }

        public async Task<IJavascriptResponse> EvaluateScript(string script, TimeSpan? timeout = null)
        {
            var response = await CefBrowser.GetMainFrame().EvaluateScriptAsync(script: script, scriptUrl: "injected", timeout: timeout);
            return new global::Gobchat.UI.Web.JavascriptResponse(response);
        }

        public void SendMoveOrResizeStartedEvent()
        {
            if (IsBrowserInitialized)
                CefBrowser.GetBrowserHost().NotifyMoveOrResizeStarted();
        }

        public void SendMouseKeyEvent(int x, int y, MouseButtonType button, bool isKeyDown)
        {
            if (!IsBrowserInitialized)
                return;

            mouseEventHelper.ProcessClick(x, y, button, isKeyDown);
            var mouseClickCount = mouseEventHelper.ClickCount;
            var mouseEvent = mouseEventHelper.GetMouseEvent(x, y, button);
            CefBrowser.GetBrowserHost().SendMouseClickEvent(mouseEvent, button, !isKeyDown, mouseClickCount);
        }

        public void SendMouseMoveEvent(int x, int y, MouseButtonType button)
        {
            if (!IsBrowserInitialized)
                return;

            var mouseEvent = mouseEventHelper.GetMouseEvent(x, y, button);
            CefBrowser.GetBrowserHost().SendMouseMoveEvent(mouseEvent, false);
        }

        public void SendMouseWheeleEvent(int x, int y, int delta, bool isVertical)
        {
            if (!IsBrowserInitialized)
                return;

            var deltaX = !isVertical ? delta : 0; // horizontal scrolling
            var deltaY = isVertical ? delta : 0;  // vertical scrolling
            var mouseEvent = mouseEventHelper.GetMouseEvent(x, y);
            CefBrowser.GetBrowserHost().SendMouseWheelEvent(mouseEvent, deltaX, deltaY);
        }

        public void SendKeyEvent(KeyEvent keyEvent)
        {
            if (!IsBrowserInitialized)
                return;

            var sendToBrowser = true;
            if (keyEvent.Type == KeyEventType.KeyUp)
            {
                if (keyEvent.Modifiers.HasFlag(CefEventFlags.ControlDown))
                {
                    if (keyEvent.WindowsKeyCode == 'C' || keyEvent.WindowsKeyCode == 'c') // ctrl + c
                    {
                        CefBrowser.GetFocusedFrame().Copy();
                        sendToBrowser = false;
                    }
                }
            }

            if (sendToBrowser)
                CefBrowser.GetBrowserHost().SendKeyEvent(keyEvent);
        }
    }
}