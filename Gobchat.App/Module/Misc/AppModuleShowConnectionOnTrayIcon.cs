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

using Gobchat.Core.Runtime;
using Gobchat.Module.NotifyIcon;
using System;
using Gobchat.Core.UI;
using Gobchat.Module.MemoryReader;

namespace Gobchat.Module.Misc
{
    public sealed class AppModuleShowConnectionOnTrayIcon : IApplicationModule
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private IDIContext _container;

        /// <summary>
        /// Requires: <see cref="IMemoryReaderManager"/> <br></br>
        /// Requires: <see cref="IUIManager"/> <br></br>
        /// <br></br>
        /// Adds to UI element: <see cref="INotifyIconManager"/> <br></br>
        /// </summary>
        public AppModuleShowConnectionOnTrayIcon()
        {
        }

        public void Initialize(ApplicationStartupHandler handler, IDIContext container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            var memoryReader = _container.Resolve<IMemoryReaderManager>();
            memoryReader.OnConnectionStateChanged += MemoryReader_OnConnectionState;
            OnConnection(memoryReader.ConnectionState);
        }

        public void Dispose()
        {
            var memoryReader = _container.Resolve<IMemoryReaderManager>();
            memoryReader.OnConnectionStateChanged -= MemoryReader_OnConnectionState;
            _container = null;
        }

        private void MemoryReader_OnConnectionState(object sender, ConnectionEventArgs e)
        {
            OnConnection(e.State);
        }

        private void OnConnection(ConnectionState state)
        {
            var icon = state == ConnectionState.Connected ? Resources.GobTrayIconOn : Resources.GobTrayIconOff;
            var uiManager = _container.Resolve<IUIManager>();
            if (uiManager.TryGetUIElement<INotifyIconManager>(AppModuleNotifyIcon.NotifyIconManagerId, out var trayIcon))
                trayIcon.Icon = icon;
        }
    }
}