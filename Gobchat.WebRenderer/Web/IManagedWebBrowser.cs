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

using System;
using System.Drawing;
using CefSharp;

namespace Gobchat.UI.Web
{
    public interface IManagedWebBrowser
    {
        event EventHandler<BrowserConsoleLogEventArgs> BrowserConsoleLog;

        event EventHandler<BrowserErrorEventArgs> BrowserError;

        /// <summary>
        /// Will be fired as soon as the browser is initialized. Each registered listener will be unregistered afterwards.
        /// When the browser is already initialized newly added listener will fire immediately and will not be registered.
        /// </summary>
        event EventHandler<BrowserInitializedEventArgs> BrowserInitialized;

        event EventHandler<BrowserLoadPageEventArgs> BrowserLoadPage;

        event EventHandler<BrowserLoadPageEventArgs> BrowserLoadPageDone;

        bool IsBrowserInitialized { get; }
        Size Size { get; set; }

        bool BindBrowserAPI(IBrowserAPI api, bool isApiAsync);

        void ShowDevTools();

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
            HttpStatusCode = httpStatusCode;
            Url = url;
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
            ErrorCode = errorCode.ToString();
            ErrorText = errorText;
            Url = url;
        }
    }

    public class BrowserConsoleLogEventArgs : EventArgs
    {
        public string Message { get; }
        public string Source { get; }
        public int Line { get; }

        public BrowserConsoleLogEventArgs(string message, string source, int line)
        {
            Message = message;
            Source = source;
            Line = line;
        }
    }

    #endregion EventArgs
}