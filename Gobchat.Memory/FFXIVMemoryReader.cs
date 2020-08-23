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

using NLog;
using Sharlayan.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gobchat.Memory
{
    public sealed class FFXIVMemoryReader : IDisposable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly ProcessConnector _processConnector = new ProcessConnector();

        private readonly Chat.ChatlogMemoryReader _chatlogProcessor = new Chat.ChatlogMemoryReader();
        private readonly Actor.PlayerLocationMemoryReader _locationProcessor = new Actor.PlayerLocationMemoryReader();

        private readonly Window.WindowObserver _windowScanner = new Window.WindowObserver();
        private bool _windowVisible = true;

        public bool ObserveGameWindow
        {
            get { return _windowScanner.Enabled; }
            set
            {
                if (value)
                    _windowScanner.StartObserving();
                else
                    _windowScanner.StopObserving();
            }
        }

        public string LocalCacheDirectory
        {
            get
            {
                return Sharlayan.MemoryHandler.Instance.LocalCacheDirectory;
            }
            set
            {
                Sharlayan.MemoryHandler.Instance.LocalCacheDirectory = value;
            }
        }

        /// <summary>
        /// Fired when the currently tracked FFXIV process changes
        /// </summary>
        public event EventHandler<ProcessChangeEventArgs> OnProcessChanged;

        public event EventHandler OnProcessScanned;

        /// <summary>
        /// Fired when the currently tracked FFXIV window is moved into the foreground or into the background
        /// </summary>
        public event EventHandler<WindowFocusChangedEventArgs> OnWindowFocusChanged;

        /// <summary>
        /// Needs to be disposed on the same thread it was created
        /// </summary>
        public FFXIVMemoryReader()
        {
            _processConnector.OnConnectionLost += ProcessConnector_OnConnectionLost;
        }

        public void Initialize()
        {
            Sharlayan.MemoryHandler.Instance.ExceptionEvent += Sharlayan_ExceptionEvent;
            _windowScanner.ActiveWindowChangedEvent += OnEvent_ActiveWindowChangedEvent;
        }

        private void Sharlayan_ExceptionEvent(object sender, Sharlayan.Events.ExceptionEvent e)
        {
            if (e.LevelIsError)
                logger.Fatal(e.Exception, () => $"Memory error in {e.Sender}");
            else
                logger.Warn(e.Exception, () => $"Memory error in {e.Sender}");
        }

        public void Dispose()
        {
            _processConnector.Disconnect();
            Sharlayan.MemoryHandler.Instance.ExceptionEvent -= Sharlayan_ExceptionEvent;
            _windowScanner.ActiveWindowChangedEvent -= OnEvent_ActiveWindowChangedEvent;
            _windowScanner.Dispose();
        }

        private void OnEvent_ActiveWindowChangedEvent(object sender, Window.WindowObserver.ActiveWindowChangedEventArgs e)
        {
            if (!FFXIVProcessValid || e.ProcessId != FFXIVProcessId)
                return;

            logger.Debug(() => e.ToString());

            switch (e.EventType)
            {
                case Window.WindowObserver.EventTypeEnum.Maximizeed:
                case Window.WindowObserver.EventTypeEnum.Minimizeed:
                    var isVisible = e.EventType == Window.WindowObserver.EventTypeEnum.Maximizeed;
                    if (isVisible != _windowVisible)
                    {
                        _windowVisible = !_windowVisible;
                        OnWindowFocusChanged?.Invoke(this, new WindowFocusChangedEventArgs(_windowVisible));
                    }
                    break;
            }
        }

        #region process handling

        public bool FFXIVProcessValid { get { return _processConnector.FFXIVProcessValid; } }

        public int FFXIVProcessId { get { return _processConnector.FFXIVProcessId; } }

        public List<int> GetFFXIVProcesses()
        {
            return _processConnector.GetFFXIVProcesses();
        }

        public bool TryConnectingToFFXIV(int processId = -1)
        {
            if (_processConnector.FFXIVProcessValid)
                if (processId <= 0 || processId == _processConnector.FFXIVProcessId)
                    return true; //nothing to do

            if (processId <= 0)
            {
                processId = _processConnector.GetFFXIVProcesses().FirstOrDefault();
                if (processId <= 0)
                    return false;
            }

            var isConnected = ConnectToFFXIV(processId);
            OnProcessScanned?.Invoke(this, new EventArgs());
            if (isConnected)
                OnProcessChanged?.Invoke(this, new ProcessChangeEventArgs(FFXIVProcessValid, FFXIVProcessId));
            return isConnected;
        }

        private bool ConnectToFFXIV(int processId)
        {
            if (!_processConnector.ConnectToProcess(processId))
                return false;

            var signaturesOfInterest = new string[] { Sharlayan.Signatures.ChatLogKey, Sharlayan.Signatures.CharacterMapKey };
            var availableSignatures = Sharlayan.Scanner.Instance.Locations.Values.Select(e => e.Key).ToArray();
            var foundSignatures = Array.FindAll(availableSignatures, (e) => signaturesOfInterest.Contains(e));
            logger.Info($"Signatures found: {string.Join(", ", foundSignatures)}");

            return true;
        }

        private void ProcessConnector_OnConnectionLost(object sender, EventArgs e)
        {
            OnProcessChanged?.Invoke(this, new ProcessChangeEventArgs(FFXIVProcessValid, FFXIVProcessId));
        }

        #endregion process handling

        #region feature - chat log

        public bool ChatLogAvailable { get { return _chatlogProcessor.ChatLogAvailable; } }

        public List<Chat.ChatlogItem> GetNewestChatlog()
        {
            if (!FFXIVProcessValid)
                return new List<Chat.ChatlogItem>();
            return _chatlogProcessor.GetNewestChatlog();
        }

        #endregion feature - chat log

        #region feature - character data

        public bool PlayerCharactersAvailable { get { return _locationProcessor.LocationAvailable; } }

        public List<Actor.PlayerCharacter> GetPlayerCharacters()
        {
            if (!FFXIVProcessValid)
                return new List<Actor.PlayerCharacter>();
            return _locationProcessor.GetPlayerCharacters();
        }

        #endregion feature - character data
    }
}