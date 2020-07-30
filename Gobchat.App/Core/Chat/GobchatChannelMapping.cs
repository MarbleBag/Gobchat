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
    public static class GobchatChannelMapping
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        static GobchatChannelMapping()
        {
            var channels = new List<ChannelData>()
            {
                Map(ChatChannel.None, relevant: false),
                Map(ChatChannel.Say, FFXIVChatChannel.SAY),
                Map(ChatChannel.Emote,   FFXIVChatChannel.EMOTE ),
                Map(ChatChannel.Yell,   FFXIVChatChannel.YELL ),
                Map(ChatChannel.Shout,   FFXIVChatChannel.SHOUT ),
                Map(ChatChannel.TellSend,   FFXIVChatChannel.TELL_SEND ),
                Map(ChatChannel.TellRecieve,   FFXIVChatChannel.TELL_RECIEVE ),
                Map(ChatChannel.Party,  FFXIVChatChannel.PARTY ),
                Map(ChatChannel.Guild,  FFXIVChatChannel.GUILD),
                Map(ChatChannel.Alliance,  FFXIVChatChannel.ALLIANCE),
                Map(ChatChannel.NPC_Dialog,   new []{FFXIVChatChannel.NPC_TALK, FFXIVChatChannel.NPC_DIALOGUE }),
                Map(ChatChannel.AnimatedEmote,  FFXIVChatChannel.ANIMATED_EMOTE),
                Map(ChatChannel.PartyFinder,  FFXIVChatChannel.PARTYFINDER, relevant: false),
                Map(ChatChannel.Echo,  FFXIVChatChannel.ECHO),
                Map(ChatChannel.Error,  FFXIVChatChannel.ERROR),
                Map(ChatChannel.Random,  new []{FFXIVChatChannel.RANDOM_SELF, FFXIVChatChannel.RANDOM_PARTY, FFXIVChatChannel.RANDOM_OTHER }),
                Map(ChatChannel.Teleport,  FFXIVChatChannel.TELEPORT, relevant: false),
                Map(ChatChannel.System,  FFXIVChatChannel.SYSTEM, relevant: false),

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

        private static readonly IDictionary<ChatChannel, ChannelData> ChatMapping = new Dictionary<ChatChannel, ChannelData>();
        private static readonly IDictionary<FFXIVChatChannel, ChannelData> ClientMapping = new Dictionary<FFXIVChatChannel, ChannelData>();

        private static ChannelData Map(ChatChannel chatChannel, bool relevant = true, string translationId = null, string configId = null)
        {
            return Map(chatChannel: chatChannel, clientChannel: null, relevant: relevant, translationId: translationId, configId: configId);
        }

        private static ChannelData Map(ChatChannel chatChannel, FFXIVChatChannel clientChannel, bool relevant = true, string translationId = null, string configId = null)
        {
            return Map(chatChannel: chatChannel, clientChannel: new[] { clientChannel }, relevant: relevant, translationId: translationId, configId: configId);
        }

        private static ChannelData Map(ChatChannel chatChannel, FFXIVChatChannel[] clientChannel, bool relevant = true, string translationId = null, string configId = null)
        {
            return new ChannelData(chatChannel: chatChannel, relevant: relevant, translationId: translationId, configId: configId, clientChannel: clientChannel);
        }

        public static ChannelData GetChannel(ChatChannel chatChannel)
        {
            return ChatMapping[chatChannel];
        }

        public static List<ChannelData> GetAllChannels()
        {
            return ChatMapping.Values.ToList();
        }

        public static ChannelData GetChannel(FFXIVChatChannel clientChannel)
        {
            if (ClientMapping.TryGetValue(clientChannel, out var channelData))
                return channelData;

            logger.Debug(() => $"No channel mapping for: {clientChannel}");
            return GetChannel(ChatChannel.None);
        }
    }

    public sealed class ChannelData
    {
        private static string MakeTranslationId(ChatChannel chatChannel)
        {
            var enumName = Enum.GetName(typeof(ChatChannel), chatChannel);
            enumName = enumName.Replace("_", "-").ToLower(CultureInfo.InvariantCulture);
            return $"main.chat.channel.{enumName}";
        }

        private static string MakeConfigId(ChatChannel chatChannel)
        {
            var enumName = Enum.GetName(typeof(ChatChannel), chatChannel);
            enumName = enumName.Replace("_", "-").ToLower(CultureInfo.InvariantCulture);
            return $"style.channel.{enumName}";
        }

        public ChatChannel ChatChannel { get; }
        public FFXIVChatChannel[] ClientChannel { get; private set; } = Array.Empty<FFXIVChatChannel>();

        public string InternalName
        {
            get
            {
                var enumName = Enum.GetName(typeof(ChatChannel), ChatChannel);
                return enumName.Replace("_", "-").ToLower(CultureInfo.InvariantCulture);
            }
        }

        public string TranslationId { get; private set; }
        public string TooltipId { get => $"{TranslationId}.tooltip"; }
        public string AbbreviationId { get => $"{TranslationId}.abbreviation"; }
        public string ConfigId { get; private set; }
        public bool Relevant { get; private set; }

        public ChannelData(ChatChannel chatChannel, bool relevant, string translationId = null, string configId = null, FFXIVChatChannel[] clientChannel = null)
        {
            ChatChannel = chatChannel;
            Relevant = relevant;

            if (clientChannel != null)
                ClientChannel = clientChannel;

            TranslationId = (translationId ?? MakeTranslationId(chatChannel)).ToLowerInvariant();
            ConfigId = (configId ?? MakeConfigId(chatChannel)).ToLowerInvariant();
        }
    }
}