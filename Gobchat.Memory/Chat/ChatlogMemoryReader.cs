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
    internal sealed class ChatlogMemoryReader
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly Chat.ChatlogReader _reader = new Chat.ChatlogReader();
        private readonly Chat.ChatlogBuilder _builder = new Chat.ChatlogBuilder();

        public bool ChatLogAvailable => _reader.ChatLogAvailable;

        public List<Chat.ChatlogItem> GetNewestChatlog()
        {
            var rawLogs = _reader.Query();
            var result = new List<Chat.ChatlogItem>();

            foreach (var rawLog in rawLogs)
            {
                try
                {
                    result.Add(_builder.Process(rawLog));
                }
                catch (Chat.ChatBuildException e)
                {
                    //TODO handle this
                    logger.Error(() => "Error in processing chat item");
                    logger.Error(() => $"Chat Item {rawLog.Line}");
                    logger.Error(e);
                }
            }

            return result;
        }
    }
}