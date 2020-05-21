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
using Gobchat.Memory.Actor;
using Gobchat.Memory.Chat;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Gobchat.Module.MemoryReader.Internal
{
    internal sealed class MemoryReaderManager : IMemoryReaderManager, IDisposable
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Code Quality", "IDE0069:Disposable fields should be disposed", Justification = "Disposed in dispose")]
        private FFXIVMemoryReader _memoryReader;

        private IDIContext _container;
        private IndependendBackgroundWorker _worker;
        private volatile ConnectionState _connectionState = ConnectionState.NotInitialized;

        public MemoryReaderManager(IDIContext container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _worker = new IndependendBackgroundWorker();

            var sharlayanResourceFolder = System.IO.Path.Combine(GobchatContext.ResourceLocation, @"sharlayan");
            System.IO.Directory.CreateDirectory(sharlayanResourceFolder);

            //needs to be done on the same thread as dispose, anchore it to ui thread, because that one never changes
            var synchronizer = container.Resolve<IUISynchronizer>();
            _memoryReader = synchronizer.RunSync(() => new FFXIVMemoryReader());
            _memoryReader.LocalCacheDirectory = sharlayanResourceFolder;
            _memoryReader.Initialize();

            _memoryReader.OnProcessChanged += MemoryReader_OnProcessChanged;
            _memoryReader.OnWindowFocusChanged += MemoryReader_OnWindowFocusChanged;
            _worker.Start(Task_ConnectMemoryReader);
        }

        public void Dispose()
        {
            _worker.Dispose();
            _worker = null;

            var synchronizer = _container.Resolve<IUISynchronizer>();
            synchronizer.RunSync(() => _memoryReader.Dispose());

            _memoryReader = null;
            _container = null;
        }

        #region public properties

        public ConnectionState ConnectionState => _connectionState;
        public bool IsConnected => _connectionState == ConnectionState.Connected;

        public bool ChatLogAvailable => _memoryReader.ChatLogAvailable;

        public bool PlayerCharactersAvailable => _memoryReader.PlayerCharactersAvailable;

        public bool ObserveGameWindow
        {
            get => _memoryReader.ObserveGameWindow;
            set => _memoryReader.ObserveGameWindow = value;
        }

        public event EventHandler<ConnectionEventArgs> OnConnectionState;

        public event EventHandler<WindowFocusChangedEventArgs> OnWindowFocusChanged;

        #endregion public properties

        #region event handler

        private void MemoryReader_OnProcessChanged(object sender, ProcessChangeEventArgs e)
        {
            if (e.IsProcessValid)
            {
                logger.Info("FFXIV process detected");
            }
            else
            {
                logger.Info("No FFXIV process detected");
                SetConnectionState(ConnectionState.NotFound);
                _worker.Start(Task_ConnectMemoryReader);
            }
        }

        private void MemoryReader_OnWindowFocusChanged(object sender, WindowFocusChangedEventArgs e)
        {
            OnWindowFocusChanged?.Invoke(this, e);
        }

        #endregion event handler

        private void Task_ConnectMemoryReader(CancellationToken cancellationToken)
        {
            SetConnectionState(ConnectionState.Searching);

            while (!cancellationToken.IsCancellationRequested && !_memoryReader.FFXIVProcessValid)
            {
                _memoryReader.CheckFFXIVProcess();
                if (_memoryReader.FFXIVProcessValid)
                    break;
                Thread.Sleep(1000);
            }

            if (_memoryReader.FFXIVProcessValid)
                SetConnectionState(ConnectionState.Connected);
            else
                SetConnectionState(ConnectionState.NotFound);
        }

        private void SetConnectionState(ConnectionState state)
        {
            if (_connectionState == state)
                return;
            _connectionState = state;
            OnConnectionState?.Invoke(this, new ConnectionEventArgs(state));
        }

        public List<PlayerCharacter> GetPlayerCharacters()
        {
            return _memoryReader.GetPlayerCharacters();
        }

        public List<ChatlogItem> GetNewestChatlog()
        {
            return _memoryReader.GetNewestChatlog();
        }
    }
}