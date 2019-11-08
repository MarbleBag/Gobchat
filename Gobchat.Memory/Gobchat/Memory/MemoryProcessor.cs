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

using System;
using System.Collections.Generic;

namespace Gobchat.Memory
{
    public class FFXIVMemoryProcessor
    {

        private readonly FFXIVProcessFinder processFinder = new FFXIVProcessFinder();
        private readonly Chat.ChatlogReader chatlogProcessor = new Chat.ChatlogReader();
        private readonly Chat.ChatlogBuilder chatlogBuilder = new Chat.ChatlogBuilder();

        public bool FFXIVProcessValid { get { return processFinder.FFXIVProcessValid; } }
        public int FFXIVProcessId { get { return processFinder.FFXIVProcessId; } }

        /// <summary>
        /// Fired when the currently tracked FFXIV process changes
        /// </summary>
        public event EventHandler<ProcessChangeEventArgs> ProcessChangeEvent;
        /// <summary>
        /// Fired when new FFXIV chatlog entries are read
        /// </summary>
        public event EventHandler<Chat.ChatlogEventArgs> ChatlogEvent;

        public void Initialize()
        {
            /* MemoryHandler.Instance.SignaturesFoundEvent += delegate (object sender, Sharlayan.Events.SignaturesFoundEvent e)
             {
                 foreach (KeyValuePair<string, Signature> kvp in e.Signatures)
                 {
                     Debug.WriteLine($"{kvp.Key} => {kvp.Value.GetAddress():X}");
                 }
             };

             this.ProcessChangeEvent += (object sender, ProcessChangeEvent e) => Debug.WriteLine($"FFXIV Process changed! {e.IsProcessValid} {e.ProcessId}");*/
        }

        public void Update()
        {
            CheckProcess();
            if (FFXIVProcessValid)
            {
                var logs = chatlogProcessor.Query();
                if (logs.Count > 0 && ChatlogEvent != null)
                {
                    List<Chat.ChatlogItem> items = new List<Chat.ChatlogItem>();
                    foreach (var item in logs)
                    {
                        try
                        {
                            items.Add(chatlogBuilder.Build(item));
                        }catch(Chat.ChatBuildException e)
                        {
                            //TODO handle this
                            System.Diagnostics.Debug.WriteLine($"ChatBuildException: Caused by {e.InnerException.GetType().Name}");
                            System.Diagnostics.Debug.Write($"{e.InnerException.StackTrace}");
                            System.Diagnostics.Debug.WriteLine("");
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
            var processChanged = processFinder.CheckProcess();
            if (!processChanged)
                return; //nothing to do

            ProcessChangeEvent?.Invoke(this, new ProcessChangeEventArgs(FFXIVProcessValid, FFXIVProcessId));
        }

    }
}


