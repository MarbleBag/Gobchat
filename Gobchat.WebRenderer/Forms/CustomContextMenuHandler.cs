/*******************************************************************************
 * Copyright (C) 2019-2023 MarbleBag
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

namespace Gobchat.UI.Forms
{
    // Context menu does not work correctly and doesn't show up at the position of the right click. For now, it will be disabled.
    internal sealed class CustomContextMenuHandler : IContextMenuHandler
    {
        private const CefMenuCommand ShowDevTools = (CefMenuCommand)26501;
        private const CefMenuCommand CloseDevTools = (CefMenuCommand)26502;

        // howto: https://github.com/cefsharp/CefSharp/blob/935d3900ba2147f4786386596b62339087ff61b0/CefSharp.WinForms.Example/Handlers/MenuHandler.cs#L15
        void IContextMenuHandler.OnBeforeContextMenu(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model)
        {
            // context menu doesn't show up when it has no entries.
            // model.Clear();

#if DEBUG
            if (model.Count > 0)            
                model.AddSeparator();            
            model.AddItem(ShowDevTools, "Show DevTools");
            model.AddItem(CloseDevTools, "Close DevTools");

#else
            // context menu doesn't show up when it has no entries.
            model.Clear();
#endif
        }

        bool IContextMenuHandler.OnContextMenuCommand(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IContextMenuParams parameters, CefMenuCommand commandId, CefEventFlags eventFlags)
        {
            if (commandId == ShowDevTools)
            {
                //browser.GetHost().ShowDevTools();
                browser.ShowDevTools();
                return true;
            }

            if (commandId == CloseDevTools)
            {
                browser.GetHost().CloseDevTools();
                return true;
            }

            return false;
        }

        void IContextMenuHandler.OnContextMenuDismissed(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame)
        {
        }

        bool IContextMenuHandler.RunContextMenu(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IContextMenuParams parameters, IMenuModel model, IRunContextMenuCallback callback)
        {
            return false;
        }
    }
}