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
using Gobchat.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Gobchat.Module.MemoryReader
{
    internal sealed class AppModuleMemoryReader : IApplicationModule
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private IDIContext _container;
        private FFXIVMemoryReader _memoryReader;

        private IndependendBackgroundWorker _worker;

        /// <summary>
        /// Requires: <see cref="IUISynchronizer"/> <br></br>
        /// Provides: <see cref="FFXIVMemoryReader"/> <br></br>
        /// </summary>
        public AppModuleMemoryReader()
        {
        }

        public void Initialize(ApplicationStartupHandler handler, IDIContext container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _worker = new IndependendBackgroundWorker();

            var sharlayanResourceFolder = System.IO.Path.Combine(AbstractGobchatApplicationContext.ResourceLocation, @"sharlayan");
            System.IO.Directory.CreateDirectory(sharlayanResourceFolder);

            //needs to be done on the same thread as dispose, anchore it to ui thread, because that one never changes
            var synchronizer = _container.Resolve<IUISynchronizer>();
            _memoryReader = synchronizer.RunSync(() => new FFXIVMemoryReader());
            _memoryReader.LocalCacheDirectory = sharlayanResourceFolder;
            _memoryReader.Initialize();

            _memoryReader.OnProcessChanged += MemoryReader_OnProcessChanged;
            _worker.Start(ConnectMemoryReader);

            container.Register<FFXIVMemoryReader>((c, p) => _memoryReader);
        }

        public void Dispose()
        {
            _container?.Unregister<FFXIVMemoryReader>();
            _worker.Dispose();
            _worker = null;

            var synchronizer = _container.Resolve<IUISynchronizer>();
            synchronizer.RunSync(() => _memoryReader?.Dispose());

            _memoryReader = null;
            _container = null;
        }

        private void ConnectMemoryReader(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && !_memoryReader.FFXIVProcessValid)
            {
                _memoryReader.CheckFFXIVProcess();
                if (_memoryReader.FFXIVProcessValid)
                    break;
                Thread.Sleep(1000);
            }

            if (_memoryReader.FFXIVProcessValid)
                logger.Info("FFXIV process detected");
        }

        private void MemoryReader_OnProcessChanged(object sender, ProcessChangeEventArgs e)
        {
            if (e.IsProcessValid)
                return;

            logger.Info("No FFXIV process detected");
            _worker.Start(ConnectMemoryReader);
        }
    }
}