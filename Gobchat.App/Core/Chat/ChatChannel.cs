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
using System.Globalization;
using System.Linq;

namespace Gobchat.Core.Chat
{
    public enum FFXIVChatChannel : int
    {
        NONE = 0x0000,
        SAY = 0x000a,
        EMOTE = 0x001c,
        YELL = 0x001e,
        SHOUT = 0x000b,
        TELL_SEND = 0x000c,
        TELL_RECIEVE = 0x000d,
        PARTY = 0x000e,
        GUILD = 0x0018,
        ALLIANCE = 0x000f,

        NPC_TALK = 0x0044,
        NPC_DIALOGUE = 0x003d,
        ANIMATED_EMOTE = 0x001d,
        PARTYFINDER = 0x0048,
        ECHO = 0x0038,
        ERROR = 0x003c,

        RANDOM_SELF = 0x084A,
        RANDOM_PARTY = 0x104A,
        RANDOM_OTHER = 0x204A,

        TELEPORT = 0x001f,
        LOCATION = 0x0039,

        WORLD_LINKSHELL_1 = 0x0025,
        WORLD_LINKSHELL_2 = 0x0065,
        WORLD_LINKSHELL_3 = 0x0066,
        WORLD_LINKSHELL_4 = 0x0067,
        WORLD_LINKSHELL_5 = 0x0068,
        WORLD_LINKSHELL_6 = 0x0069,
        WORLD_LINKSHELL_7 = 0x006A,
        WORLD_LINKSHELL_8 = 0x006B,

        LINKSHELL_1 = 0x0010,
        LINKSHELL_2 = 0x0011,
        LINKSHELL_3 = 0x0012,
        LINKSHELL_4 = 0x0013,
        LINKSHELL_5 = 0x0014,
        LINKSHELL_6 = 0x0015,
        LINKSHELL_7 = 0x0016,
        LINKSHELL_8 = 0x0017,
    }

    public enum ChatChannel : int
    {
        None,
        Say,
        Emote,
        Yell,
        Shout,
        TellSend,
        TellRecieve,
        Party,
        Guild,
        Alliance,

        NPC_Dialogue,
        AnimatedEmote,
        PartyFinder,
        Echo,
        GameError,

        Random,

        Teleport,
        Location,

        CrossWorldLinkShell_1,
        CrossWorldLinkShell_2,
        CrossWorldLinkShell_3,
        CrossWorldLinkShell_4,
        CrossWorldLinkShell_5,
        CrossWorldLinkShell_6,
        CrossWorldLinkShell_7,
        CrossWorldLinkShell_8,

        LinkShell_1,
        LinkShell_2,
        LinkShell_3,
        LinkShell_4,
        LinkShell_5,
        LinkShell_6,
        LinkShell_7,
        LinkShell_8,

        GobchatInfo,
        GobchatError,
    }

    public class ChannelData
    {
        private static ChannelData Map(ChatChannel chatChannel)
        {
            return new ChannelData(chatChannel);
        }

        private static ChannelData Map(ChatChannel chatChannel, FFXIVChatChannel clientChannel)
        {
            return Map(chatChannel, new[] { clientChannel });
        }

        private static ChannelData Map(ChatChannel chatChannel, FFXIVChatChannel[] clientChannel)
        {
            return new ChannelData(chatChannel, clientChannel);
        }

        static ChannelData()
        {
            var channels = new List<ChannelData>()
            {
                Map(ChatChannel.None),
                Map(ChatChannel.Say, FFXIVChatChannel.SAY),
                Map(ChatChannel.Emote,   FFXIVChatChannel.EMOTE ),
                Map(ChatChannel.Yell,   FFXIVChatChannel.YELL ),
                Map(ChatChannel.Shout,   FFXIVChatChannel.SHOUT ),
                Map(ChatChannel.TellSend,   FFXIVChatChannel.TELL_SEND ),
                Map(ChatChannel.TellRecieve,   FFXIVChatChannel.TELL_RECIEVE ),
                Map(ChatChannel.Party,  FFXIVChatChannel.PARTY ),
                Map(ChatChannel.Guild,  FFXIVChatChannel.GUILD),
                Map(ChatChannel.Alliance,  FFXIVChatChannel.ALLIANCE),
                Map(ChatChannel.NPC_Dialogue,   new []{FFXIVChatChannel.NPC_TALK, FFXIVChatChannel.NPC_DIALOGUE }),
                Map(ChatChannel.AnimatedEmote,  FFXIVChatChannel.ANIMATED_EMOTE),
                Map(ChatChannel.PartyFinder,  FFXIVChatChannel.PARTYFINDER),
                Map(ChatChannel.Echo,  FFXIVChatChannel.ECHO),
                Map(ChatChannel.GameError,  FFXIVChatChannel.ERROR),
                Map(ChatChannel.Random,  new []{FFXIVChatChannel.RANDOM_SELF, FFXIVChatChannel.RANDOM_PARTY, FFXIVChatChannel.RANDOM_OTHER }),
                Map(ChatChannel.Teleport,  FFXIVChatChannel.TELEPORT),
                Map(ChatChannel.Location,  FFXIVChatChannel.LOCATION),

                Map(ChatChannel.LinkShell_1,  FFXIVChatChannel.LINKSHELL_1),
                Map(ChatChannel.LinkShell_2,  FFXIVChatChannel.LINKSHELL_2),
                Map(ChatChannel.LinkShell_3,  FFXIVChatChannel.LINKSHELL_3),
                Map(ChatChannel.LinkShell_4,  FFXIVChatChannel.LINKSHELL_4),
                Map(ChatChannel.LinkShell_5,  FFXIVChatChannel.LINKSHELL_5),
                Map(ChatChannel.LinkShell_6,  FFXIVChatChannel.LINKSHELL_6),
                Map(ChatChannel.LinkShell_7,  FFXIVChatChannel.LINKSHELL_7),
                Map(ChatChannel.LinkShell_8,  FFXIVChatChannel.LINKSHELL_8),

                Map(ChatChannel.CrossWorldLinkShell_1,  FFXIVChatChannel.WORLD_LINKSHELL_1),
                Map(ChatChannel.CrossWorldLinkShell_2,  FFXIVChatChannel.WORLD_LINKSHELL_2),
                Map(ChatChannel.CrossWorldLinkShell_3,  FFXIVChatChannel.WORLD_LINKSHELL_3),
                Map(ChatChannel.CrossWorldLinkShell_4,  FFXIVChatChannel.WORLD_LINKSHELL_4),
                Map(ChatChannel.CrossWorldLinkShell_5,  FFXIVChatChannel.WORLD_LINKSHELL_5),
                Map(ChatChannel.CrossWorldLinkShell_6,  FFXIVChatChannel.WORLD_LINKSHELL_6),
                Map(ChatChannel.CrossWorldLinkShell_7,  FFXIVChatChannel.WORLD_LINKSHELL_7),
                Map(ChatChannel.CrossWorldLinkShell_8,  FFXIVChatChannel.WORLD_LINKSHELL_8),

                Map(ChatChannel.GobchatInfo),
                Map(ChatChannel.GobchatError),
            };

            foreach (var channel in channels)
            {
                ChatMapping.Add(channel.ChatChannel, channel);
                foreach (var c in channel.ClientChannel)
                    ClientMapping.Add(c, channel);
            }

            var expectedValues = Enum.GetValues(typeof(ChatChannel));
            foreach (ChatChannel expectedChannel in expectedValues)
            {
                if (!ChatMapping.ContainsKey(expectedChannel))
                    throw new ArgumentException($"Missing mapping for {expectedChannel}"); //TODO
            }
        }

        private static string MakeTranslationName(ChatChannel chatChannel)
        {
            var enumName = Enum.GetName(typeof(ChatChannel), chatChannel);
            enumName = enumName.Replace("_", "-").ToLower(CultureInfo.InvariantCulture);
            return $"main.chat.channel.{enumName}";
        }

        public static ChannelData GetChannel(ChatChannel chatChannel)
        {
            return ChatMapping[chatChannel];
        }

        public static ChannelData GetChannel(FFXIVChatChannel clientChannel)
        {
            return ClientMapping[clientChannel];
        }

        private static readonly IDictionary<ChatChannel, ChannelData> ChatMapping = new Dictionary<ChatChannel, ChannelData>();
        private static readonly IDictionary<FFXIVChatChannel, ChannelData> ClientMapping = new Dictionary<FFXIVChatChannel, ChannelData>();

        public ChatChannel ChatChannel { get; }
        public FFXIVChatChannel[] ClientChannel { get; private set; } = Array.Empty<FFXIVChatChannel>();
        public string TranslationId { get; private set; }

        private ChannelData(ChatChannel chatChannel)
        {
            ChatChannel = chatChannel;
            TranslationId = MakeTranslationName(chatChannel);
        }

        private ChannelData(ChatChannel chatChannel, FFXIVChatChannel[] clientChannel)
        {
            ChatChannel = chatChannel;
            TranslationId = MakeTranslationName(chatChannel);
            ClientChannel = clientChannel ?? throw new ArgumentNullException(nameof(clientChannel));
        }

        private ChannelData(ChatChannel chatChannel, string translationId, FFXIVChatChannel[] clientChannel)
        {
            ChatChannel = chatChannel;
            TranslationId = translationId ?? throw new ArgumentNullException(nameof(translationId));
            ClientChannel = clientChannel ?? throw new ArgumentNullException(nameof(clientChannel));
        }
    }
}