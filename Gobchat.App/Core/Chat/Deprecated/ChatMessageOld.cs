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

using System;

namespace Gobchat.Core.Chat
{
    [Obsolete]
    public sealed class ChatMessageOld
    {
        public DateTime Timestamp { get; }
        public string Source { get; }
        public int MessageType { get; }
        public string Message { get; }

        public ChatMessageOld(DateTime timestamp, string source, int messageType, string message)
        {
            Timestamp = timestamp;
            Source = source;
            MessageType = messageType;
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public override string ToString()
        {
            var className = nameof(ChatMessageOld);
            return $"{className} => time:{Timestamp} | type:{MessageType} | source:'{Source}'  | msg:'{Message}'";
        }
    }
}