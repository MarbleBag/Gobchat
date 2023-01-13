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

using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using CefSharp;
using CefSharp.DevTools.Debugger;

namespace Gobchat.UI.Forms
{
    internal sealed class CustomResourceRequestHandler : CefSharp.Handler.ResourceRequestHandler
    {
        private readonly ManagedWebBrowser  managedWebBrowser;

        public CustomResourceRequestHandler(ManagedWebBrowser managedWebBrowser)
        {
            this.managedWebBrowser = managedWebBrowser;
        }

        private static string AppLocation { get { return AppDomain.CurrentDomain.BaseDirectory; } }

        protected override CefReturnValue OnBeforeResourceLoad(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            var isHandeld = managedWebBrowser.RedirectableResourceRequests(request);
            if (isHandeld)
                return CefReturnValue.Continue;
            return base.OnBeforeResourceLoad(chromiumWebBrowser, browser, frame, request, callback);

            
        }
    }

    internal sealed class CustomRequestHandler : CefSharp.Handler.RequestHandler
    {

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly ManagedWebBrowser managedWebBrowser;

        public CustomRequestHandler(ManagedWebBrowser managedWebBrowser)
        {
            this.managedWebBrowser = managedWebBrowser;
        }

        override protected IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            if (isDownload)
            {
                logger.Error($"Denied resource download request: {request.Url}");
                disableDefaultHandling = true;
                return null;
            }

            if (isNavigation)
                return null;

            if (request.Url.StartsWith("devtools://"))
                return null;



            if ("file://".Equals(requestInitiator))
            {
                switch (request.ResourceType)
                {
                    case ResourceType.Stylesheet:
                    case ResourceType.Script:
                        return new CustomResourceRequestHandler(managedWebBrowser);
                }
            }

            return null;
        }

        override protected bool OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect)
        {
            if (request.Url.StartsWith("file:///") || request.Url.Equals("devtools://devtools/devtools_app.html"))
                return false;

            logger.Error("Denied browser target", request.Url);
            return true; // cancels
        }
    }
}