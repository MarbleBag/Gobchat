/*******************************************************************************
 * Copyright (C) 2019 MarbleBag
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
    public class FFXIVMemoryProcessor
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private readonly FFXIVProcessFinder _processFinder = new FFXIVProcessFinder();

        private readonly Chat.ChatlogReader _chatlogProcessor = new Chat.ChatlogReader();
        private readonly Chat.ChatlogBuilder _chatlogBuilder = new Chat.ChatlogBuilder();

        private readonly Window.WindowObserver _windowScanner = new Window.WindowObserver();

        public bool FFXIVProcessValid { get { return _processFinder.FFXIVProcessValid; } }

        public int FFXIVProcessId { get { return _processFinder.FFXIVProcessId; } }

        public bool ChatLogAvailable { get { return Sharlayan.Scanner.Instance.Locations.ContainsKey(Sharlayan.Signatures.ChatLogKey); } }

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
        public event EventHandler<ProcessChangeEventArgs> ProcessChangeEvent;

        /// <summary>
        /// Fired when the currently tracked FFXIV window is moved into the foreground or into the background
        /// </summary>
        public event EventHandler<WindowFocusChangedEventArgs> WindowFocusChangedEvent;

        /// <summary>
        /// Fired when new FFXIV chatlog entries are read
        /// </summary>
        public event EventHandler<Chat.ChatlogEventArgs> ChatlogEvent;

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

        private void OnEvent_ActiveWindowChangedEvent(object sender, Window.WindowObserver.ActiveWindowChangedEventArgs e)
        {
            if (!FFXIVProcessValid)
                return;
            WindowFocusChangedEvent?.Invoke(this, new WindowFocusChangedEventArgs(e.ProcessId == FFXIVProcessId));
        }

        public void Update()
        {
            CheckProcess();
            if (FFXIVProcessValid)
            {
                var logs = _chatlogProcessor.Query();
                if (logs.Count > 0 && ChatlogEvent != null)
                {
                    List<Chat.ChatlogItem> items = new List<Chat.ChatlogItem>();
                    foreach (var item in logs)
                    {
                        try
                        {
                            items.Add(_chatlogBuilder.Process(item));
                        }
                        catch (Chat.ChatBuildException e)
                        {
                            //TODO handle this
                            logger.Error(() => "Error in processing chat item");
                            logger.Error(() => $"Chat Item {item.Line}");
                            logger.Error(e);
                        }
                    }
                    ChatlogEvent.Invoke(this, new Chat.ChatlogEventArgs(items));
                }
            }

            // List<ChatlogItem> items = new List<ChatlogItem>();
            // items.Add(chatlogBuilder.Build());
            // ChatlogEvent?.Invoke(this, new ChatlogEvent(items));
        }

        private void CheckProcess()
        {
            //TODO MUTEX
            var processChanged = _processFinder.CheckProcess();
            if (!processChanged)
                return; //nothing to do

            ProcessChangeEvent?.Invoke(this, new ProcessChangeEventArgs(FFXIVProcessValid, FFXIVProcessId));
        }
    }
}