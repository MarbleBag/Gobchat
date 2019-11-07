﻿using System;
using System.Drawing;
using CefSharp;

namespace Gobchat.UI.Forms
{
    public interface IBrowserAPI
    {        string APIName { get; }
    }

    public interface IManagedWebBrowser
    {
        bool IsBrowserInitialized { get; }
        Size Size { get; set; }

        event EventHandler<BrowserConsoleLogEventArgs> BrowserConsoleLog;
        event EventHandler<BrowserErrorEventArgs> BrowserError;
        event EventHandler<BrowserInitializedEventArgs> BrowserInitialized;
        event EventHandler<BrowserLoadPageEventArgs> BrowserLoadPage;
        event EventHandler<BrowserLoadPageEventArgs> BrowserLoadPageDone;

        bool BindBrowserAPI(IBrowserAPI api, bool isApiAsync);
        void CloseBrowser(bool forceClose);
        void Dispose();
        void ExecuteScript(string script);
        void Load(string url);
        void Reload();
        void SendKeyEvent(KeyEvent keyEvent);
        void SendMouseKeyEvent(int x, int y, MouseButtonType button, bool isKeyDown);
        void SendMouseMoveEvent(int x, int y, MouseButtonType button);
        void SendMouseWheeleEvent(int x, int y, int delta, bool isVertical);
        void SendMoveOrResizeStartedEvent();
    }

    #region EventArgs
    public class BrowserLoadPageEventArgs : EventArgs
    {
        public int HttpStatusCode { get; }
        public string Url { get; }

        public BrowserLoadPageEventArgs(int httpStatusCode, string url)
        {
            this.HttpStatusCode = httpStatusCode;
            this.Url = url;
        }
    }

    public class BrowserInitializedEventArgs : EventArgs
    {

    }

    public class BrowserErrorEventArgs : EventArgs
    {
        public string ErrorCode { get; }
        public string ErrorText { get; }
        public string Url { get; }
        public BrowserErrorEventArgs(CefErrorCode errorCode, string errorText, string url)
        {
            this.ErrorCode = errorCode.ToString();
            this.ErrorText = errorText;
            this.Url = url;
        }
    }

    public class BrowserConsoleLogEventArgs : EventArgs
    {
        public string Message { get; }
        public string Source { get; }
        public int Line { get; }
        public BrowserConsoleLogEventArgs(string message, string source, int line)
        {
            this.Message = message;
            this.Source = source;
            this.Line = line;
        }
    }

    #endregion
}