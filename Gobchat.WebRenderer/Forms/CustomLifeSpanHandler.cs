/*******************************************************************************
 * Copyright (C) 2019-2025 MarbleBag
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
using NLog;
using System.Diagnostics;

namespace Gobchat.UI.Forms
{
    internal sealed class CustomLifeSpanHandler : ILifeSpanHandler
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public CustomLifeSpanHandler()
        {

        }

        public bool DoClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
            return false;
        }

        public void OnAfterCreated(IWebBrowser chromiumWebBrowser, IBrowser newBrowser)
        {

        }

        public void OnBeforeClose(IWebBrowser chromiumWebBrowser, IBrowser currentBrowser)
        {
        }

        public bool OnBeforePopup(IWebBrowser chromiumWebBrowser, IBrowser currentBrowser, IFrame frame, string targetUrl, string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
        {
            newBrowser = null;
            return false;
        }
    }
}