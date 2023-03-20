/*******************************************************************************
 * Copyright (C) 2019-2022 MarbleBag
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
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace Gobchat.UI.Web
{
    public interface IManagedWebBrowser
    {
        event EventHandler<BrowserConsoleLogEventArgs> OnBrowserConsoleLog;

        event EventHandler<BrowserErrorEventArgs> OnBrowserError;

        /// <summary>
        /// Will be fired as soon as the browser is initialized. Each registered listener will be unregistered afterwards.
        /// When the browser is already initialized newly added listener will fire immediately and will not be registered.
        /// </summary>
        event EventHandler<BrowserInitializedEventArgs> OnBrowserInitialized;

        event EventHandler<BrowserLoadPageEventArgs> OnBrowserLoadPage;

        event EventHandler<BrowserLoadPageEventArgs> OnBrowserLoadPageDone;

        event EventHandler<BrowserAPIBindingEventArgs> OnResolveBrowserAPI;

        event EventHandler<RedirectResourceRequestEventArgs> OnRedirectableResourceRequests;

        bool IsBrowserInitialized { get; }
        Size Size { get; set; }

        bool BindBrowserAPI(IBrowserAPI api, bool isApiAsync);

        bool UnbindBrowserAPI(IBrowserAPI api);

        void ShowDevTools();

        void CloseBrowser(bool forceClose);

        void Dispose();

        void ExecuteScript(string script);

        Task<IJavascriptResponse> EvaluateScript(string script, TimeSpan? timeout);

        void Load(string url);

        void Reload();

        void SendKeyEvent(KeyEvent keyEvent);

        void SendMouseKeyEvent(int x, int y, MouseButtonType button, bool isKeyDown);

        void SendMouseMoveEvent(int x, int y, MouseButtonType button);

        void SendMouseWheeleEvent(int x, int y, int delta, bool isVertical);

        void SendMoveOrResizeStartedEvent();
    }

    #region EventArgs

    public class RedirectResourceRequestEventArgs : EventArgs
    {
        public enum Type
        {
            Unknown,
            Stylesheet,
            Script
        }

        public string OriginalUri { get; }

        public Type ResourceType { get; }

        public string RedirectUri { get; set; }

        public RedirectResourceRequestEventArgs(string originalUri, Type resourceType)
        {
            OriginalUri = originalUri;
            ResourceType = resourceType;
        }
    }

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

    public sealed class BrowserAPIBindingEventArgs : System.EventArgs
    {
        public string APIName { get; }
        public IManagedWebBrowser Browser { get; }

        public BrowserAPIBindingEventArgs(string apiName, IManagedWebBrowser browser)
        {
            APIName = apiName ?? throw new ArgumentNullException(nameof(apiName));
            Browser = browser ?? throw new ArgumentNullException(nameof(browser));
        }
    }

    #endregion EventArgs
}