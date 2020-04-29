﻿/*******************************************************************************
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

namespace Gobchat.Core.Chat
{
    public sealed class ChatMessage
    {
        public ChatMessageSource Source { get; set; }
        public System.DateTime Timestamp { get; set; }
        public ChatChannel Channel { get; set; }
        public System.Collections.Generic.List<MessageSegment> Message { get; }

        public ChatMessage()
        {
            Source = null;
            Channel = (int)ChatChannel.NONE;
            Message = new System.Collections.Generic.List<MessageSegment>();
        }
    }

    public sealed class ChatMessageSource
    {
        public string Source { get; }
        public string Prefix { get; set; } = null;

        public string CharacterName { get; set; } = null;

        public int FFGroup { get; set; } = -1;
        public int Party { get; set; } = -1;
        public int Alliance { get; set; } = -1;

        public ChatMessageSource(string source)
        {
            Source = source;
        }
    }

    public sealed class MessageSegment
    {
        public MessageSegmentType Type { get; set; }
        public string Content { get; set; }

        public MessageSegment(MessageSegmentType type, string message)
        {
            this.Type = type;
            this.Content = message;
        }
    }
}