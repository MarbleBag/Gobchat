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

using Gobchat.Core.Chat;
using Gobchat.Core.Runtime;
using Gobchat.Memory;
using Gobchat.Module.Chat;
using Gobchat.Module.MemoryReader;
using System;

namespace Gobchat.Module.Misc
{
    public sealed class AppModuleInformUserAboutMemoryState : IApplicationModule
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private IDIContext _container;
        private IChatManager _chatManager;
        private IMemoryReaderManager _memoryReader;

        private volatile bool _reportError;

        private readonly object _lock = new object();

        /// <summary>
        /// Requires: <see cref="IChatManager"/> <br></br>
        /// Requires: <see cref="IMemoryReaderManager"/> <br></br>
        /// <br></br>
        /// </summary>
        public AppModuleInformUserAboutMemoryState()
        {
        }

        public void Initialize(ApplicationStartupHandler handler, IDIContext container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _chatManager = _container.Resolve<IChatManager>();
            _memoryReader = _container.Resolve<IMemoryReaderManager>();

            _reportError = true; //report error on startup
            _memoryReader.OnConnectionStateChanged += MemoryReader_OnConnectionState;
            Report(_memoryReader.ConnectionState);
        }

        public void Dispose()
        {
            _memoryReader.OnConnectionStateChanged -= MemoryReader_OnConnectionState;

            _chatManager = null;
            _container = null;
        }

        private void MemoryReader_OnConnectionState(object sender, ConnectionEventArgs e)
        {
            Report(e.State);
        }

        private void Report(ConnectionState state)
        {
            lock (_lock)
            {
                switch (state)
                {
                    case ConnectionState.NotInitialized:
                        return;

                    case ConnectionState.Searching:
                    case ConnectionState.NotFound:
                        if (!_reportError)
                            return;

                        _reportError = false;
                        _chatManager.EnqueueMessage(SystemMessageType.Error, Resources.Module_Misc_Connection_NotFound);
                        break;

                    case ConnectionState.Connected:
                        _reportError = true;
                        if (_memoryReader.ChatLogAvailable)
                            _chatManager.EnqueueMessage(SystemMessageType.Info, Resources.Module_Misc_Connection_Found);
                        else
                            _chatManager.EnqueueMessage(SystemMessageType.Error, Resources.Module_Misc_Connection_AdminRights);
                        break;
                }
            }
        }
    }
}