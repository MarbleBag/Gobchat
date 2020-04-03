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

namespace Gobchat.Memory.Chat
{
    internal sealed class ChatlogProcessor
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly Chat.ChatlogReader _reader = new Chat.ChatlogReader();
        private readonly Chat.ChatlogBuilder _builder = new Chat.ChatlogBuilder();

        /// <summary>
        /// Fired when new FFXIV chatlog entries are read
        /// </summary>
        public event EventHandler<Chat.ChatlogEventArgs> ChatlogEvent;

        public bool ChatLogAvailable => _reader.ChatLogAvailable;

        public void Update()
        {
            var logs = _reader.Query();
            if (logs.Count > 0 && ChatlogEvent != null)
            {
                var items = new List<Chat.ChatlogItem>();
                foreach (var item in logs)
                {
                    try
                    {
                        items.Add(_builder.Process(item));
                    }
                    catch (Chat.ChatBuildException e)
                    {
                        //TODO handle this
                        logger.Error(() => "Error in processing chat item");
                        logger.Error(() => $"Chat Item {item.Line}");
                        logger.Error(e);
                    }
                }
                ChatlogEvent?.Invoke(this, new Chat.ChatlogEventArgs(items));
            }
        }
    }
}