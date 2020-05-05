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

using Gobchat.Core.Config;
using Gobchat.Core.Runtime;
using Gobchat.Memory;
using Gobchat.UI.Forms;
using System;

namespace Gobchat.Module.Overlay
{
    public sealed class AppModuleHideOnMinimize : IApplicationModule
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private IDIContext _container;
        private FFXIVMemoryReader _memoryReader;
        private IConfigManager _configManager;

        private bool _hideOnMinimize;

        /// <summary>
        /// Requires: <see cref="IUIManager"/> <br></br>
        /// Requires: <see cref="IGobchatConfig"/> <br></br>
        /// Requires: <see cref="FFXIVMemoryReader"/> <br></br>
        /// <br></br>
        /// Adds to UI element: <see cref="CefOverlayForm"/> <br></br>
        /// </summary>
        public AppModuleHideOnMinimize()
        {
        }

        public void Initialize(ApplicationStartupHandler handler, IDIContext container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));

            _memoryReader = _container.Resolve<FFXIVMemoryReader>();
            _configManager = _container.Resolve<IConfigManager>();
            _configManager.AddPropertyChangeListener("behaviour.hideOnMinimize", true, true, ConfigManager_UpdateHideOnMinimize);
            _memoryReader.OnWindowFocusChanged += MemoryReader_OnWindowFocusChanged;
        }

        public void Dispose()
        {
            _configManager.RemovePropertyChangeListener(ConfigManager_UpdateHideOnMinimize);
            _memoryReader.OnWindowFocusChanged -= MemoryReader_OnWindowFocusChanged;

            _container = null;
            _memoryReader = null;
            _configManager = null;
        }

        private void ConfigManager_UpdateHideOnMinimize(IConfigManager config, ProfilePropertyChangedCollectionEventArgs evt)
        {
            _hideOnMinimize = config.GetProperty<bool>("behaviour.hideOnMinimize");

            var uiManager = _container.Resolve<IUIManager>();
            uiManager.UISynchronizer.RunSync(() => _memoryReader.ObserveGameWindow = _hideOnMinimize);
        }

        private void MemoryReader_OnWindowFocusChanged(object sender, WindowFocusChangedEventArgs e)
        {
            if (!_hideOnMinimize)
                return;

            var uiManager = _container.Resolve<IUIManager>();
            if (uiManager.TryGetUIElement(AppModuleChatOverlay.OverlayUIId, out CefOverlayForm overlay))
                uiManager.UISynchronizer.RunAsync(() => overlay.Visible = e.IsInForeground);
        }
    }
}