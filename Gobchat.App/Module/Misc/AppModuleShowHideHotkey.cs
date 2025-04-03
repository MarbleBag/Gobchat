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

using Gobchat.Core.Config;
using Gobchat.Core.Runtime;
using Gobchat.Module.Chat;
using Gobchat.Module.Hotkey;
using Gobchat.Module.Overlay;
using Gobchat.UI.Forms;

namespace Gobchat.Module.Misc
{
    public sealed class AppModuleShowHideHotkey : IApplicationModule
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private ConfigHotkeyUpdater _hkShowHide;

        /// <summary>
        /// Requires: <see cref="IConfigManager"/> <br></br>
        /// Requires: <see cref="IHotkeyManager"/> <br></br>
        /// Requires: <see cref="IUIManager"/> <br></br>
        /// Requires: <see cref="IChatManager"/> <br></br>
        /// <br></br>
        /// Adds hotkey to show & hide overlay
        /// </summary>
        public AppModuleShowHideHotkey()
        {
        }

        public void Initialize(ApplicationStartupHandler handler, IDIContext container)
        {
            var uiManager = container.Resolve<IUIManager>();

            //TODO: hotkey management is still dumb
            _hkShowHide = new ConfigHotkeyUpdater(
                "behaviour.hotkeys.showhide",
                Resources.Module_Misc_Hotkey_ShowHide,
                container.Resolve<IConfigManager>(),
                "behaviour.hotkeys.showhide",
                container.Resolve<IHotkeyManager>(),
                container.Resolve<IChatManager>());

            _hkShowHide.OnHotkey += () =>
            {
                if (uiManager.TryGetUIElement<CefOverlayForm>(AppModuleChatOverlay.OverlayUIId, out var overlay))
                    uiManager.UISynchronizer.RunSync(() =>
                    {
                        overlay.Visible = !overlay.Visible;
                    });
            };
        }

        public void Dispose()
        {
            _hkShowHide.Dispose();
        }
    }
}