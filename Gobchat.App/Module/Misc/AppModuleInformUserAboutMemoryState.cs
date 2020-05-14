﻿/*******************************************************************************
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

using Gobchat.Core.Chat;
using Gobchat.Core.Runtime;
using Gobchat.Memory;
using Gobchat.Module.Chat;
using System;

namespace Gobchat.Module.Misc
{
    public sealed class AppModuleInformUserAboutMemoryState : IApplicationModule
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private IDIContext _container;
        private IChatManager _chatManager;
        private FFXIVMemoryReader _memoryReader;

        private volatile bool _lastMemoryState;
        private volatile bool _reportDone;
        private object _lock = new object();

        /// <summary>
        /// Requires: <see cref="IChatManager"/> <br></br>
        /// Requires: <see cref="FFXIVMemoryReader"/> <br></br>
        /// <br></br>
        /// </summary>
        public AppModuleInformUserAboutMemoryState()
        {
        }

        public void Initialize(ApplicationStartupHandler handler, IDIContext container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _chatManager = _container.Resolve<IChatManager>();
            _memoryReader = _container.Resolve<FFXIVMemoryReader>();

            _memoryReader.OnProcessChanged += MemoryReader_OnProcessChanged;
            CheckReport();
        }

        public void Dispose()
        {
            _memoryReader.OnProcessChanged -= MemoryReader_OnProcessChanged;

            _chatManager = null;
            _container = null;
        }


        private void MemoryReader_OnProcessChanged(object sender, ProcessChangeEventArgs e)
        {
            CheckReport();
        }

        private void CheckReport()
        {
            lock (_lock)
            {
                if (_reportDone)
                {
                    if (_lastMemoryState != _memoryReader.FFXIVProcessValid)
                        _lastMemoryState = ReportToUser();
                }
                else
                {
                    _lastMemoryState = ReportToUser();
                    _reportDone = true;
                }
            }
        }

        private bool ReportToUser()
        {
            lock (_lock)
            {
                var state = _memoryReader.FFXIVProcessValid;
                if (state)
                {
                    if (_memoryReader.ChatLogAvailable)
                        _chatManager.EnqueueMessage(SystemMessageType.Info, "FFXIV detected.");
                    else
                        _chatManager.EnqueueMessage(SystemMessageType.Error, "Can't access FFXIV chatlog. Restart Gobchat with admin rights.");
                }
                else
                    _chatManager.EnqueueMessage(SystemMessageType.Error, "Can't find a running instance of FFXIV.");
                return state;
            }
        }
    }
}