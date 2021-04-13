/*******************************************************************************
 * Copyright (C) 2019-2021 MarbleBag
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
using Gobchat.Module.MemoryReader;
using System;
using System.Threading.Tasks;

namespace Gobchat.Module.UI
{
    public sealed class AppModuleMemoryToUI : IApplicationModule
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private IDIContext _container;

        private IBrowserAPIManager _browserAPIManager;
        private IMemoryReaderManager _memoryManager;

        /// <summary>
        /// Requires: <see cref="IBrowserAPIManager"/> <br></br>
        /// Requires: <see cref="IMemoryReaderManager"/> <br></br>
        /// <br></br>
        /// </summary>
        public AppModuleMemoryToUI()
        {
        }

        public void Initialize(ApplicationStartupHandler handler, IDIContext container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _browserAPIManager = _container.Resolve<IBrowserAPIManager>();
            _memoryManager = _container.Resolve<IMemoryReaderManager>();

            _browserAPIManager.MemoryHandler = new BrowserMemoryHandler(this);
        }

        public void Dispose()
        {
            _browserAPIManager.MemoryHandler = null;
            _browserAPIManager = null;
            _memoryManager = null;
            _container = null;
        }

        private sealed class BrowserMemoryHandler : IBrowserMemoryHandler
        {
            private AppModuleMemoryToUI _module;

            public BrowserMemoryHandler(AppModuleMemoryToUI module)
            {
                _module = module ?? throw new ArgumentNullException(nameof(module));
            }

            public async Task<bool> AttachToFFXIVProcess(int id)
            {
                _module._memoryManager.ConnectTo(id);
                return true;
            }

            public async Task<int[]> GetAttachableFFXIVProcesses()
            {
                return _module._memoryManager.GetProcessIds().ToArray();
            }

            public async Task<(ConnectionState state, int id)> GetAttachedFFXIVProcess()
            {
                var state = _module._memoryManager.ConnectionState;
                var processId = _module._memoryManager.ConnectedProcessId;
                return (state, processId);
            }
        }
    }
}