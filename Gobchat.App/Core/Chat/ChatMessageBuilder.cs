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
using System.Collections.Generic;
using System.Linq;
using Gobchat.Core.Chat;
using Gobchat.Core.Util.Extension;

namespace Gobchat.Core.Chat
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Threadsafe via get/set")]
    public sealed class ChatMessageBuilder
    {
        private static readonly ChatChannel[] PlayerChannels = {
        ChatChannel.SAY, ChatChannel.EMOTE, ChatChannel.YELL, ChatChannel.SHOUT, ChatChannel.TELL_SEND, ChatChannel.TELL_RECIEVE, ChatChannel.PARTY, ChatChannel.GUILD, ChatChannel.ALLIANCE,
        ChatChannel.ANIMATED_EMOTE,
        ChatChannel.WORLD_LINKSHELL_1, ChatChannel.WORLD_LINKSHELL_2, ChatChannel.WORLD_LINKSHELL_3, ChatChannel.WORLD_LINKSHELL_4,
        ChatChannel.WORLD_LINKSHELL_5, ChatChannel.WORLD_LINKSHELL_6, ChatChannel.WORLD_LINKSHELL_7, ChatChannel.WORLD_LINKSHELL_8,
        ChatChannel.LINKSHELL_1, ChatChannel.LINKSHELL_2, ChatChannel.LINKSHELL_3, ChatChannel.LINKSHELL_4,
        ChatChannel.LINKSHELL_5, ChatChannel.LINKSHELL_6, ChatChannel.LINKSHELL_7, ChatChannel.LINKSHELL_8,
        };

        private static readonly int[] GroupUnicodes = FFXIVUnicodes.GroupUnicodes.Select(e => e.Value).ToArray();
        private static readonly int[] PartyUnicodes = FFXIVUnicodes.PartyUnicodes.Select(e => e.Value).ToArray();
        private static readonly int[] RaidUnicodes = FFXIVUnicodes.RaidUnicodes.Select(e => e.Value).ToArray();

        private ChatChannel[] _formateChannels = Array.Empty<ChatChannel>();
        private ChatChannel[] _mentionChannels = Array.Empty<ChatChannel>();

        private readonly ChatMessageSegmentFormatter _formater = new ChatMessageSegmentFormatter();
        private readonly ChatMessageMentionFinder _mentionFinder = new ChatMessageMentionFinder();

        public bool DetecteEmoteInSayChannel { get; set; }

        public List<ChatChannel> FormatChannels
        {
            get => _formateChannels.ToList();
            set => _formateChannels = value.ToArrayOrEmpty();
        }

        public List<ChatChannel> MentionChannels
        {
            get => _mentionChannels.ToList();
            set => _mentionChannels = value.ToArrayOrEmpty();
        }

        public ChatMessageBuilder()
        {
            _mentionFinder.MessageSegmentType = MessageSegmentType.MENTION;
        }

        public ChatMessage BuildChatMessage(CleanedChatlogItem message)
        {
            return BuildChatMessage(message.Timestamp, message.Channel, message.Source, message.Message);
        }

        public ChatMessage BuildChatMessage(DateTime time, ChatChannel channel, string source, string message)
        {
            var chatMessage = new ChatMessage()
            {
                Timestamp = time,
                Channel = channel
            };

            SetMessageSource(chatMessage, source);
            chatMessage.Message.Add(new MessageSegment(MessageSegmentType.UNDEFINED, message));

            return chatMessage;
        }

        private void SetMessageSource(ChatMessage chatMessage, string source)
        {
            chatMessage.Source = new ChatMessageSource(source);

            if (source != null && source.Length > 0 && PlayerChannels.Contains(chatMessage.Channel))
            {
                var readIdx = 0;
                int GetUnicodeIndex(int[] unicodes)
                {
                    var cp = (int)source[readIdx];
                    if (0xD800 <= cp && cp <= 0xDFFF)    //surrogate pair
                        cp = (cp - 0xD800) << 10 + (source[readIdx + 1] - 0xDC00 + 0x10000);
                    return Array.IndexOf(unicodes, cp);
                }

                int lookupIdx;
                if (ChatChannel.PARTY == chatMessage.Channel)
                { // check for party number
                    lookupIdx = GetUnicodeIndex(PartyUnicodes);
                    if (lookupIdx >= 0)
                    {
                        chatMessage.Source.Party = lookupIdx + 1;
                        chatMessage.Source.Prefix = (chatMessage.Source.Prefix ?? "") + $"[{lookupIdx + 1}]"; //move prefix building somewhere else!
                        readIdx += 1; //party unicodes should be of size 1
                    }
                }
                else if (ChatChannel.ALLIANCE == chatMessage.Channel)
                { // check for alliance letter
                    lookupIdx = GetUnicodeIndex(RaidUnicodes);
                    if (lookupIdx >= 0)
                    {
                        chatMessage.Source.Alliance = lookupIdx + 1;
                        chatMessage.Source.Prefix = (chatMessage.Source.Prefix ?? "") + $"[{char.ConvertFromUtf32(lookupIdx + 'A')}]"; //for now
                        readIdx += 1; //raid unicodes should be of size 1
                    }
                }

                //check if source starts with a player assigned group letter
                lookupIdx = GetUnicodeIndex(GroupUnicodes);
                if (lookupIdx >= 0)
                {
                    chatMessage.Source.FFGroup = lookupIdx + 1;
                    chatMessage.Source.Prefix = (chatMessage.Source.Prefix ?? "") + FFXIVUnicodes.GroupUnicodes[lookupIdx].Symbol;
                    readIdx += 1;
                }

                chatMessage.Source.CharacterName = chatMessage.Source.Source.Substring(readIdx);
            }
        }

        public void FormatChatMessage(Gobchat.Core.Chat.ChatMessage chatMessage)
        {
            if (_formateChannels.Contains(chatMessage.Channel))
            {
                _formater.Format(chatMessage);
                if (DetecteEmoteInSayChannel && chatMessage.Channel == ChatChannel.SAY)
                {
                    var containsSay = chatMessage.Message.Any(e => e.Type == MessageSegmentType.SAY);
                    if (containsSay)
                        SetUndefinedTo(chatMessage, MessageSegmentType.EMOTE);
                }
            }

            if (_mentionChannels.Contains(chatMessage.Channel))
                _mentionFinder.MarkMentions(chatMessage);

            SetDefaultTypes(chatMessage);
        }

        private void SetDefaultTypes(ChatMessage chatMessage)
        {
            switch (chatMessage.Channel)
            {
                case ChatChannel.SAY:
                    SetUndefinedTo(chatMessage, MessageSegmentType.SAY);
                    break;

                case ChatChannel.EMOTE:
                    SetUndefinedTo(chatMessage, MessageSegmentType.EMOTE);
                    break;
            }
        }

        private static void SetUndefinedTo(ChatMessage chatMessage, MessageSegmentType newType)
        {
            foreach (var message in chatMessage.Message)
                if (message.Type == MessageSegmentType.UNDEFINED)
                    message.Type = newType;
        }
    }
}