/*******************************************************************************
 * Copyright (C) 2019 MarbleBag
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
using Gobchat.UI.Forms.Extension;
using Gobchat.UI.Forms.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace Gobchat.UI.Forms
{
    internal class ManagedWebBrowser : CefSharp.OffScreen.ChromiumWebBrowser, CefSharp.Internals.IRenderWebBrowser, IDisposable, IManagedWebBrowser
    {
        private class BrowserWrapper : CefSharp.OffScreen.ChromiumWebBrowser, CefSharp.Internals.IRenderWebBrowser
        {
            public delegate void OnPaint(CefSharp.PaintElementType type, CefSharp.Structs.Rect dirtyRect, IntPtr buffer, int width, int height);
            public delegate void OnCursorChange(IntPtr cursor, CefSharp.Enums.CursorType type, CefSharp.Structs.CursorInfo customCursorInfo);

            public OnPaint OnPaintDelegate;
            public OnCursorChange OnCursorChangeDelegate;

            public BrowserWrapper() { }

            void CefSharp.Internals.IRenderWebBrowser.OnPaint(CefSharp.PaintElementType type, CefSharp.Structs.Rect dirtyRect, IntPtr buffer, int width, int height)
            {
                OnPaintDelegate.Invoke(type, dirtyRect, buffer, width, height);
            }

            void CefSharp.Internals.IRenderWebBrowser.OnCursorChange(IntPtr cursor, CefSharp.Enums.CursorType type, CefSharp.Structs.CursorInfo customCursorInfo)
            {
                OnCursorChangeDelegate.Invoke(cursor, type, customCursorInfo);
            }
        }


        private readonly CefOverlayForm Form;

        private CefSharp.OffScreen.ChromiumWebBrowser CefBrowser { get { return this; } }

        private readonly MouseEventHelper mouseEventHelper = new MouseEventHelper();
        private List<IBrowserAPI> _availableAPIs = new List<IBrowserAPI>();

        public event EventHandler<BrowserConsoleLogEventArgs> BrowserConsoleLog;
        public event EventHandler<BrowserErrorEventArgs> BrowserError;
        public event EventHandler<BrowserLoadPageEventArgs> BrowserLoadPage;
        public event EventHandler<BrowserLoadPageEventArgs> BrowserLoadPageDone;
        public new event EventHandler<BrowserInitializedEventArgs> BrowserInitialized;

        event EventHandler<BrowserInitializedEventArgs> IManagedWebBrowser.BrowserInitialized
        {
            add { BrowserInitialized += value; }
            remove { BrowserInitialized -= value; }
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

            CefBrowser.BrowserInitialized += OnEvent_BrowserInitialized;
            CefBrowser.FrameLoadStart += OnEvent_FrameLoadStart;
            CefBrowser.FrameLoadEnd += OnEvent_FrameLoadEnd;
            CefBrowser.LoadError += OnEvent_LoadError;
            CefBrowser.ConsoleMessage += OnEvent_ConsoleMessage;
        }



        #region BrowserEventHandling

        private void OnEvent_FrameLoadStart(object sender, FrameLoadStartEventArgs e)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.Append("(async () => {");
            foreach (var boundAPI in _availableAPIs)
            {
                builder.Append("await CefSharp.BindObjectAsync('");
                builder.Append(boundAPI.APIName);
                builder.AppendLine("')");
            }
            builder.Append("})();");
            var awaitAPIScript = builder.ToString();
            e.Frame.ExecuteJavaScriptAsync(awaitAPIScript, "Initialize");

            BrowserLoadPage?.Invoke(this, new BrowserLoadPageEventArgs(0, e.Url));
        }

        private void OnEvent_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            BrowserLoadPageDone?.Invoke(this, new BrowserLoadPageEventArgs(e.HttpStatusCode, e.Url));
        }

        private void OnEvent_LoadError(object sender, LoadErrorEventArgs e)
        {
            BrowserError?.Invoke(sender, new BrowserErrorEventArgs(e.ErrorCode, e.ErrorText, e.FailedUrl));
        }

        private void OnEvent_ConsoleMessage(object sender, ConsoleMessageEventArgs e)
        {
            BrowserConsoleLog?.Invoke(sender, new BrowserConsoleLogEventArgs(e.Message, e.Source, e.Line));
        }

        private void OnEvent_BrowserInitialized(object sender, EventArgs e)
        {
            if (CefBrowser.IsBrowserInitialized)
                BrowserInitialized?.Invoke(this, new BrowserInitializedEventArgs());
        }

        #endregion


        void CefSharp.Internals.IRenderWebBrowser.OnPaint(CefSharp.PaintElementType type, CefSharp.Structs.Rect dirtyRect, IntPtr buffer, int width, int height)
        {
            Form.OnBrowserRequestsPainting(dirtyRect, buffer, width, height);
        }

        void CefSharp.Internals.IRenderWebBrowser.OnCursorChange(IntPtr cursor, CefSharp.Enums.CursorType type, CefSharp.Structs.CursorInfo customCursorInfo)
        {
            Form.SyncInvoke(() => Form.Cursor = new Cursor(cursor));
        }

        internal void StartBrowser(int width, int height)
        {
            var cefWindowInfo = new CefSharp.WindowInfo();
            cefWindowInfo.SetAsWindowless(IntPtr.Zero);
            cefWindowInfo.Width = width;
            cefWindowInfo.Height = height;

            var cefBrowserSettings = new CefSharp.BrowserSettings();
            cefBrowserSettings.WindowlessFrameRate = 60;

            CefBrowser.CreateBrowser(cefWindowInfo, cefBrowserSettings);
        }

        public void CloseBrowser(bool forceClose)
        {
            CefBrowser.GetBrowser().CloseBrowser(forceClose);
        }

        public new void Load(string url)
        {
            base.Load(url);
        }

        public void Reload()
        {
            CefBrowser.Reload();
        }

        public new void Dispose()
        {
            base.Dispose();
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
            return true;
        }

        public void ExecuteScript(string script)
        {
            CefBrowser.GetMainFrame().ExecuteJavaScriptAsync(script);
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
            if(keyEvent.Type == KeyEventType.KeyUp)
            {
                if (keyEvent.Modifiers.HasFlag(CefEventFlags.ControlDown))
                {
                    if((keyEvent.WindowsKeyCode == 'C' || keyEvent.WindowsKeyCode == 'c'))
                    {
                        CefBrowser.GetFocusedFrame().Copy();
                        sendToBrowser = false;
                    }
                }
            }

            if(sendToBrowser)
                CefBrowser.GetBrowserHost().SendKeyEvent(keyEvent);
        }


    }
}
