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
    public enum ChatChannel : int
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

        GOBCHAT_INFO = 0x01FFFF,
        GOBCHAT_ERROR = 0x02FFFF,
    }

    public enum GChannel : int
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

        NPC_DIALOGUE = 0x003d,
        ANIMATED_EMOTE = 0x001d,
        PARTYFINDER = 0x0048,
        ECHO = 0x0038,
        ERROR = 0x003c,

        RANDOM = 0x084A,

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

        LinkShell_1 = 0x0010,
        LinkShell_2 = 0x0011,
        LinkShell_3 = 0x0012,
        LinkShell_4 = 0x0013,
        LinkShell_5 = 0x0014,
        LinkShell_6 = 0x0015,
        LinkShell_7 = 0x0016,
        LinkShell_8 = 0x0017,

        GOBCHAT_INFO = 0x01FFFF,
        GOBCHAT_ERROR = 0x02FFFF,
    }

    public class GobChannel
    {
        private static GobChannel Map(GChannel chatChannel)
        {
            return new GobChannel(chatChannel);
        }

        private static GobChannel Map(GChannel chatChannel, ChatChannel clientChannel)
        {
            return Map(chatChannel, new[] { clientChannel });
        }

        private static GobChannel Map(GChannel chatChannel, ChatChannel[] clientChannel)
        {
            return new GobChannel(chatChannel, clientChannel);
        }

        static GobChannel()
        {
            var channels = new List<GobChannel>()
            {
                Map(GChannel.NONE),
                Map(GChannel.SAY, ChatChannel.SAY),
                Map(GChannel.EMOTE,   ChatChannel.EMOTE ),
                Map(GChannel.YELL,   ChatChannel.YELL ),
                Map(GChannel.SHOUT,   ChatChannel.SHOUT ),
                Map(GChannel.TELL_SEND,   ChatChannel.TELL_SEND ),
                Map(GChannel.TELL_RECIEVE,   ChatChannel.TELL_RECIEVE ),
                Map(GChannel.PARTY,  ChatChannel.PARTY ),
                Map(GChannel.GUILD,  ChatChannel.GUILD),
                Map(GChannel.ALLIANCE,  ChatChannel.ALLIANCE),
                Map(GChannel.NPC_DIALOGUE,   new []{ChatChannel.NPC_TALK, ChatChannel.NPC_DIALOGUE }),
                Map(GChannel.ANIMATED_EMOTE,  ChatChannel.ANIMATED_EMOTE),
                Map(GChannel.PARTYFINDER,  ChatChannel.PARTYFINDER),
                Map(GChannel.ECHO,  ChatChannel.ECHO),
                Map(GChannel.ERROR,  ChatChannel.ERROR),
                Map(GChannel.RANDOM,  new []{ChatChannel.RANDOM_SELF, ChatChannel.RANDOM_PARTY, ChatChannel.RANDOM_OTHER }),
                Map(GChannel.TELEPORT,  ChatChannel.TELEPORT),
                Map(GChannel.LOCATION,  ChatChannel.LOCATION),

                Map(GChannel.LinkShell_1,  ChatChannel.LINKSHELL_1),
                Map(GChannel.LinkShell_2,  ChatChannel.LINKSHELL_2),
                Map(GChannel.LinkShell_3,  ChatChannel.LINKSHELL_3),
                Map(GChannel.LinkShell_4,  ChatChannel.LINKSHELL_4),
                Map(GChannel.LinkShell_5,  ChatChannel.LINKSHELL_5),
                Map(GChannel.LinkShell_6,  ChatChannel.LINKSHELL_6),
                Map(GChannel.LinkShell_7,  ChatChannel.LINKSHELL_7),
                Map(GChannel.LinkShell_8,  ChatChannel.LINKSHELL_8),

                Map(GChannel.WORLD_LINKSHELL_1,  ChatChannel.WORLD_LINKSHELL_1),
                Map(GChannel.WORLD_LINKSHELL_2,  ChatChannel.WORLD_LINKSHELL_2),
                Map(GChannel.WORLD_LINKSHELL_3,  ChatChannel.WORLD_LINKSHELL_3),
                Map(GChannel.WORLD_LINKSHELL_4,  ChatChannel.WORLD_LINKSHELL_4),
                Map(GChannel.WORLD_LINKSHELL_5,  ChatChannel.WORLD_LINKSHELL_5),
                Map(GChannel.WORLD_LINKSHELL_6,  ChatChannel.WORLD_LINKSHELL_6),
                Map(GChannel.WORLD_LINKSHELL_7,  ChatChannel.WORLD_LINKSHELL_7),
                Map(GChannel.WORLD_LINKSHELL_8,  ChatChannel.WORLD_LINKSHELL_8),

                Map(GChannel.GOBCHAT_INFO),
                Map(GChannel.GOBCHAT_ERROR),
            };

            foreach (var channel in channels)
            {
                ChatMapping.Add(channel.ChatChannelX, channel);
                foreach (var c in channel.ClientChannel)
                    ClientMapping.Add(c, channel);
            }

            var expectedValues = Enum.GetValues(typeof(GChannel));
            foreach (GChannel expectedChannels in expectedValues)
            {
                if (!ChatMapping.ContainsKey(expectedChannels))
                    throw new ArgumentException(); //TODO
            }
        }

        private static string MakeTranslationName(GChannel chatChannel)
        {
            var enumName = Enum.GetName(typeof(GChannel), chatChannel);
            enumName = enumName.Replace("_", "-").ToLower(CultureInfo.InvariantCulture);
            return $"main.chat.channel.{enumName}";
        }

        public static GobChannel GetChannel(GChannel chatChannel)
        {
            return ChatMapping[chatChannel];
        }

        public static GobChannel GetChannel(ChatChannel clientChannel)
        {
            return ClientMapping[clientChannel];
        }

        private static readonly IDictionary<GChannel, GobChannel> ChatMapping = new Dictionary<GChannel, GobChannel>();
        private static readonly IDictionary<ChatChannel, GobChannel> ClientMapping = new Dictionary<ChatChannel, GobChannel>();

        public GChannel ChatChannelX { get; }
        public ChatChannel[] ClientChannel { get; private set; } = Array.Empty<ChatChannel>();
        public string TranslationId { get; private set; }

        private GobChannel(GChannel chatChannel)
        {
            ChatChannelX = chatChannel;
            TranslationId = MakeTranslationName(chatChannel);
        }

        private GobChannel(GChannel chatChannel, ChatChannel[] clientChannel)
        {
            ChatChannelX = chatChannel;
            TranslationId = MakeTranslationName(chatChannel);
            ClientChannel = clientChannel ?? throw new ArgumentNullException(nameof(clientChannel));
        }

        private GobChannel(GChannel chatChannel, string translationId, ChatChannel[] clientChannel)
        {
            ChatChannelX = chatChannel;
            TranslationId = translationId ?? throw new ArgumentNullException(nameof(translationId));
            ClientChannel = clientChannel ?? throw new ArgumentNullException(nameof(clientChannel));
        }
    }
}