/*******************************************************************************
 * Copyright (C) 2019-2023 MarbleBag
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
        public System.Collections.Generic.List<ChatMessageSegment> Content { get; } = new System.Collections.Generic.List<ChatMessageSegment>();

        public bool ContainsMentions { get => Content.Any(e => e.Type == MessageSegmentType.Mention); }

        public ChatMessage()
        {
        }

        public override string ToString()
        {
            return $"[{nameof(Source)}={Source}; {nameof(Timestamp)}={Timestamp}; {nameof(Channel)}={Channel}; {nameof(ContainsMentions)}={ContainsMentions}; {nameof(Content)}[{string.Join("", Content.Select(c => c.ToString()))}]]";
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
        public bool IsApp { get; set; } = false;

        public ChatMessageSource(string source)
        {
            Original = source;
        }

        public override string ToString()
        {
            return $"[{nameof(Original)}={Original}; {nameof(CharacterName)}={CharacterName}; {nameof(TriggerGroupId)}={TriggerGroupId}; {nameof(FfGroup)}={FfGroup}; {nameof(Party)}={Party}; {nameof(Alliance)}={Alliance}; {nameof(Visibility)}={Visibility}; {nameof(IsAPlayer)}={IsAPlayer}; {nameof(IsUser)}={IsUser}; {nameof(IsApp)}={IsApp}]";
        }
    }

    public sealed class ChatMessageSegment
    {
        public MessageSegmentType Type { get; set; }
        public string Text { get; set; }

        public ChatMessageSegment(MessageSegmentType type, string message)
        {
            this.Type = type;
            this.Text = message;
        }

        public override string ToString()
        {
            return $"[{Type}; {Text}]";
        }
    }
}