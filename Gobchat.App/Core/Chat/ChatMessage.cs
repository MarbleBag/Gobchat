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

using System.Linq;

namespace Gobchat.Core.Chat
{
    public sealed class ChatMessage
    {
        public ChatMessageSource Source { get; set; } = null;
        public System.DateTime Timestamp { get; set; }
        public ChatChannel Channel { get; set; } = ChatChannel.None;
        public System.Collections.Generic.List<MessageSegment> Content { get; } = new System.Collections.Generic.List<MessageSegment>();

        public bool ContainsMentions { get; set; } = false;

        public ChatMessage()
        {
        }

        public override string ToString()
        {
            return $"ChatMessage[{Source.Original}: {string.Join("", Content.Select(c => c.Text))}]";
        }
    }

    public sealed class ChatMessageSource
    {
        public string Original { get; }
        public string CharacterName { get; set; } = null;
        public string TriggerGroupId { get; internal set; } = null;
        public int FfGroup { get; set; } = -1;
        public int Party { get; set; } = -1;
        public int Alliance { get; set; } = -1;
        public int Visibility { get; set; } = 100; //100 = full visible, 0 = invisible
        public bool IsAPlayer { get; set; } = false;
        public bool IsUser { get; set; } = false;

        public ChatMessageSource(string source)
        {
            Original = source;
        }
    }

    public sealed class MessageSegment
    {
        public MessageSegmentType Type { get; set; }
        public string Text { get; set; }

        public MessageSegment(MessageSegmentType type, string message)
        {
            this.Type = type;
            this.Text = message;
        }

        public override string ToString()
        {
            return $"[{Type}] {Text}";
        }
    }
}