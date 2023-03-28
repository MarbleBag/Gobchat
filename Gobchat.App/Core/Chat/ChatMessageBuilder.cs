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

using System;
using System.Linq;
using System.Net.Configuration;
using Gobchat.Core.Util.Extension;

namespace Gobchat.Core.Chat
{
    public sealed class ChatMessageBuilder
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private static readonly ChatChannel[] PlayerChannels = {
            ChatChannel.Say, ChatChannel.Emote, ChatChannel.Yell, ChatChannel.Shout, ChatChannel.TellSend, ChatChannel.TellRecieve, ChatChannel.Party, ChatChannel.Guild, ChatChannel.Alliance,
            ChatChannel.AnimatedEmote,
            ChatChannel.CrossWorldLinkShell_1, ChatChannel.CrossWorldLinkShell_2, ChatChannel.CrossWorldLinkShell_3, ChatChannel.CrossWorldLinkShell_4,
            ChatChannel.CrossWorldLinkShell_5, ChatChannel.CrossWorldLinkShell_6, ChatChannel.CrossWorldLinkShell_7, ChatChannel.CrossWorldLinkShell_8,
            ChatChannel.LinkShell_1, ChatChannel.LinkShell_2, ChatChannel.LinkShell_3, ChatChannel.LinkShell_4,
            ChatChannel.LinkShell_5, ChatChannel.LinkShell_6, ChatChannel.LinkShell_7, ChatChannel.LinkShell_8,
        };

        private static readonly int[] GroupUnicodes = FFXIVUnicodes.GroupUnicodes.Select(e => e.Value).ToArray();
        private static readonly int[] PartyUnicodes = FFXIVUnicodes.PartyUnicodes.Select(e => e.Value).ToArray();
        private static readonly int[] RaidUnicodes = FFXIVUnicodes.RaidUnicodes.Select(e => e.Value).ToArray();

        private ChatChannel[] _formateChannels = Array.Empty<ChatChannel>();
        private ChatChannel[] _mentionChannels = Array.Empty<ChatChannel>();

        private readonly ChatMessageSegmentFormatter _formater = new ChatMessageSegmentFormatter();
        private readonly ChatMessageMentionFinder _mentionFinder = new ChatMessageMentionFinder();

        public bool DetecteEmoteInSayChannel { get; set; }

        public bool ExcludeUserMention { get; set; }

        public ChatChannel[] FormatChannels
        {
            get => _formateChannels.ToArray();
            set => _formateChannels = value.ToArrayOrEmpty();
        }

        public FormatConfig[] Formats
        {
            get => _formater.Formats.ToArray();
            set => _formater.Formats = value.ToArrayOrEmpty();
        }

        public ChatChannel[] MentionChannels
        {
            get => _mentionChannels.ToArray();
            set => _mentionChannels = value.ToArrayOrEmpty();
        }

        public string[] Mentions
        {
            get => _mentionFinder.Mentions.ToArray();
            set => _mentionFinder.Mentions = value;
        }

        public ChatMessageBuilder()
        {
            _mentionFinder.MessageSegmentType = MessageSegmentType.Mention;
        }

        public ChatMessage BuildChatMessage(DateTime time, ChatChannel channel, string source, string message)
        {
            var chatMessage = new ChatMessage()
            {
                Timestamp = time,
                Channel = channel
            };

            chatMessage.Content.Add(new ChatMessageSegment(MessageSegmentType.Undefined, message));
            SetMessageSource(chatMessage, source);

            return chatMessage;
        }

        private void SetMessageSource(ChatMessage chatMessage, string source)
        {
            chatMessage.Source = new ChatMessageSource(source)
            {
                IsAPlayer = PlayerChannels.Contains(chatMessage.Channel)
            };

            if (source != null && source.Length > 0 && chatMessage.Source.IsAPlayer)
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
                if (ChatChannel.Party == chatMessage.Channel)
                { // check for party number
                    lookupIdx = GetUnicodeIndex(PartyUnicodes);
                    if (lookupIdx >= 0)
                    {
                        chatMessage.Source.Party = lookupIdx;
                        // chatMessage.Source.Prefix = (chatMessage.Source.Prefix ?? "") + $"[{lookupIdx + 1}]"; //part of html now
                        readIdx += 1; //party unicodes should be of size 1
                    }
                }
                else if (ChatChannel.Alliance == chatMessage.Channel)
                { // check for alliance letter
                    lookupIdx = GetUnicodeIndex(RaidUnicodes);
                    if (lookupIdx >= 0)
                    {
                        chatMessage.Source.Alliance = lookupIdx;
                        // chatMessage.Source.Prefix = (chatMessage.Source.Prefix ?? "") + $"[{char.ConvertFromUtf32(lookupIdx + 'A')}]";
                        readIdx += 1; //raid unicodes should be of size 1
                    }
                }

                //check if source starts with a player assigned group letter
                lookupIdx = GetUnicodeIndex(GroupUnicodes);
                if (lookupIdx >= 0)
                {
                    chatMessage.Source.FfGroup = lookupIdx;
                    // chatMessage.Source.Prefix = (chatMessage.Source.Prefix ?? "") + FFXIVUnicodes.GroupUnicodes[lookupIdx].Symbol;
                    readIdx += 1;
                }

                chatMessage.Source.CharacterName = chatMessage.Source.Original.Substring(readIdx);

                if (chatMessage.Channel == ChatChannel.AnimatedEmote && chatMessage.Content.Count > 0)
                { // these are special
                    var serverStart = chatMessage.Source.CharacterName.Length + 1;
                    var msg = chatMessage.Content[0];

                    if (msg.Text.Length > serverStart && msg.Text[serverStart] == '[')
                    {
                        var serverEnd = msg.Text.IndexOf(']', serverStart);
                        if(serverEnd > 0)
                        {
                            var server = msg.Text.Substring(serverStart, serverEnd - serverStart + 1);
                            chatMessage.Source.CharacterName += " " + server;
                        }
                    }
                }
            }
        }

        public void FormatChatMessage(ChatMessage chatMessage)
        {
            if (_formateChannels.Contains(chatMessage.Channel))
            {
                _formater.Format(chatMessage);
                if (DetecteEmoteInSayChannel && chatMessage.Channel == ChatChannel.Say)
                {
                    var containsSay = chatMessage.Content.Any(e => e.Type == MessageSegmentType.Say);
                    if (containsSay)
                        SetUndefinedTo(chatMessage, MessageSegmentType.Emote);
                }
            }

            var scanForMentions =
                !(ExcludeUserMention && chatMessage.Source.IsUser) &&
                _mentionChannels.Contains(chatMessage.Channel);

            if (scanForMentions)
            {
                _mentionFinder.MarkMentions(chatMessage);
            }
            else
            {
                logger.Debug(() =>
                {
                    var userReason = !ExcludeUserMention && chatMessage.Source.IsUser;
                    var channelReason = !_mentionChannels.Contains(chatMessage.Channel);
                    return $"No mention scanning reason: User={userReason}, Channel={channelReason}";
                });
            }

            SetDefaultTypes(chatMessage);
        }

        private void SetDefaultTypes(ChatMessage chatMessage)
        {
            switch (chatMessage.Channel)
            {
                case ChatChannel.Say:
                    SetUndefinedTo(chatMessage, MessageSegmentType.Say);
                    break;

                case ChatChannel.Emote:
                    SetUndefinedTo(chatMessage, MessageSegmentType.Emote);
                    break;
            }
        }

        private static void SetUndefinedTo(ChatMessage chatMessage, MessageSegmentType newType)
        {
            foreach (var message in chatMessage.Content)
                if (message.Type == MessageSegmentType.Undefined)
                    message.Type = newType;
        }
    }
}