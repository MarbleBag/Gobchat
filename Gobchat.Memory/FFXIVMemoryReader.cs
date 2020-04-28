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
using System;
using System.Collections.Generic;

namespace Gobchat.Memory
{
    public sealed class FFXIVMemoryReader : IDisposable
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly FFXIVProcessFinder _processFinder = new FFXIVProcessFinder();

        private readonly Chat.ChatlogMemoryReader _chatlogProcessor = new Chat.ChatlogMemoryReader();
        private readonly Actor.PlayerLocationMemoryReader _locationProcessor = new Actor.PlayerLocationMemoryReader();

        private readonly Window.WindowObserver _windowScanner = new Window.WindowObserver();
        private bool _windowVisible = true;

        public bool FFXIVProcessValid { get { return _processFinder.FFXIVProcessValid; } }

        public int FFXIVProcessId { get { return _processFinder.FFXIVProcessId; } }

        public bool ChatLogAvailable { get { return _chatlogProcessor.ChatLogAvailable; } }

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

        /// <summary>
        /// Fired when the currently tracked FFXIV window is moved into the foreground or into the background
        /// </summary>
        public event EventHandler<WindowFocusChangedEventArgs> OnWindowFocusChanged;

        [Obsolete]
        /// <summary>
        /// Fired when new FFXIV chatlog entries are read
        /// </summary>
        public event EventHandler<Chat.ChatlogEventArgs> OnChatlog;

        [Obsolete]
        public event EventHandler<Actor.PlayerEventArgs> OnActor;

        public void Initialize()
        {
            Sharlayan.MemoryHandler.Instance.ExceptionEvent += (s, e) =>
            {
                if (e.LevelIsError)
                    logger.Fatal(e.Exception, () => $"Memory error in {e.Sender}");
                else
                    logger.Warn(e.Exception, () => $"Memory error in {e.Sender}");
            };

            _windowScanner.ActiveWindowChangedEvent += OnEvent_ActiveWindowChangedEvent;
        }

        public void Dispose()
        {
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

        [Obsolete]
        public void Update()
        {
            CheckFFXIVProcess();
            if (FFXIVProcessValid)
            {
                if (OnActor != null)
                {
                    var result = _locationProcessor.GetPlayerData();
                    if (result.Count > 0)
                        OnActor?.Invoke(this, new Actor.PlayerEventArgs(result));
                }

                if (OnChatlog != null)
                {
                    var result = _chatlogProcessor.GetNewestChatlog();
                    if (result.Count > 0)
                        OnChatlog?.Invoke(this, new Chat.ChatlogEventArgs(result));
                }
            }
        }

        public void CheckFFXIVProcess()
        {
            var processChanged = _processFinder.CheckProcess();
            if (!processChanged)
                return; //nothing to do

            OnProcessChanged?.Invoke(this, new ProcessChangeEventArgs(FFXIVProcessValid, FFXIVProcessId));
        }

        public List<Chat.ChatlogItem> GetNewestChatlog()
        {
            if (!FFXIVProcessValid)
                return new List<Chat.ChatlogItem>();
            return _chatlogProcessor.GetNewestChatlog();
        }

        public List<Actor.PlayerData> GetPlayerData()
        {
            if (!FFXIVProcessValid)
                return new List<Actor.PlayerData>();
            return _locationProcessor.GetPlayerData();
        }
    }
}