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

using Gobchat.Core.Runtime;
using Gobchat.Memory;
using Gobchat.Module.MemoryReader.Internal;
using System;

namespace Gobchat.Module.MemoryReader
{
    internal sealed class AppModuleMemoryReader : IApplicationModule
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private IDIContext _container;
        private FFXIVMemoryManager _memoryReaderManager;

        /// <summary>
        /// Requires: <see cref="IUISynchronizer"/> <br></br>
        /// Provides: <see cref="IMemoryReaderManager"/> <br></br>
        /// </summary>
        public AppModuleMemoryReader()
        {
        }

        public void Initialize(ApplicationStartupHandler handler, IDIContext container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _memoryReaderManager = new FFXIVMemoryManager(container);

            _container.Register<IMemoryReaderManager>((c, p) => _memoryReaderManager);
        }

        public void Dispose()
        {
            _container?.Unregister<FFXIVMemoryReader>();
            _memoryReaderManager?.Dispose();

            _memoryReaderManager = null;
            _container = null;
        }
    }
}