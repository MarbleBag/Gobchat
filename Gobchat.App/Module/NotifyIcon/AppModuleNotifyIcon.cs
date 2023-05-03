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

using Gobchat.Core.Runtime;
using Gobchat.Core.UI;
using NLog;
using System;
using System.Windows.Forms;

namespace Gobchat.Module.NotifyIcon
{
    public sealed class AppModuleNotifyIcon : IApplicationModule
    {
        private readonly static Logger logger = LogManager.GetCurrentClassLogger();

        public const string NotifyIconManagerId = "Gobchat.NotifyIconManager";

        private IUIManager _manager;

        /// <summary>
        /// Requires: <see cref="IUIManager"/> <br></br>
        /// <br></br>
        /// Installs UI element: <see cref="INotifyIconManager"/> <br></br>
        /// </summary>
        public AppModuleNotifyIcon()
        {
        }

        public void Initialize(ApplicationStartupHandler handler, IDIContext container)
        {
            _manager = container.Resolve<IUIManager>();

            _manager.CreateUIElement(NotifyIconManagerId, () =>
            {
                var notifyIconManager = new NotifyIconManager(new[] { "app", "debug", "close" }, "app")
                {
                    Text = "Gobchat",
                    Icon = Gobchat.Resources.GobIcon,
                    Visible = true
                };

                var closeMenu = new ToolStripMenuItem(Resources.Module_NotifyIcon_UI_Close);
                closeMenu.Click += OnEvent_MenuItem_Close;
                notifyIconManager.AddMenuToGroup("close", "close", closeMenu);

                return notifyIconManager;
            });
        }

        private void OnEvent_MenuItem_Close(object sender, EventArgs e)
        {
            logger.Info("User requests shutdown");
            GobchatApplicationContext.ExitGobchat();
        }

        public void Dispose()
        {
            if (_manager == null)
                return;
            _manager.DisposeUIElement(NotifyIconManagerId);
            _manager = null;
        }
    }
}