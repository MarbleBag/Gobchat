/*******************************************************************************
 * Copyright (C) 2019-2020 MarbleBag
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

using Gobchat.Core.Runtime;
using Gobchat.Module.Overlay;
using Gobchat.Module.UI.Internal;
using Gobchat.UI.Forms;
using Gobchat.UI.Web;
using System;
using System.Threading.Tasks;

namespace Gobchat.Module.UI
{
    public sealed class AppModuleBrowserAPIManager : IApplicationModule
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private IDIContext _container;
        private BrowserAPIManager _browserAPIManager;

        /// <summary>
        /// Requires: <see cref="IUIManager"/> <br></br>
        /// <br></br>
        /// Provides: <see cref="IBrowserAPIManager"/> <br></br>
        /// <br></br>
        /// Adds to UI element: <see cref="CefOverlayForm"/> <br></br>
        /// </summary>
        public AppModuleBrowserAPIManager()
        {
        }

        public void Initialize(ApplicationStartupHandler handler, IDIContext container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));

            var uiManager = _container.Resolve<IUIManager>();
            var overlay = uiManager.GetUIElement<CefOverlayForm>(AppModuleChatOverlay.OverlayUIId);

            _browserAPIManager = new BrowserAPIManager(overlay, uiManager.UISynchronizer);
            _container.Register<IBrowserAPIManager>((c, p) => _browserAPIManager);
        }

        public void Dispose()
        {
            _container.Unregister<IBrowserAPIManager>();
            _container = null;
            _browserAPIManager.Dispose();
            _browserAPIManager = null;
        }
    }
}