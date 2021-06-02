/*******************************************************************************
 * Copyright (C) 2019-2021 MarbleBag
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
using System.Text.RegularExpressions;

namespace Gobchat.LogConverter.Logs.ACT
{
    [LogAttribute("ACTv1")]
    public sealed class ACTv1LogParser : ILogParser
    {
        private List<Entry> _results = new List<Entry>();
        private readonly Regex _regex = new Regex(@"^\d{2}\|(?<time>.+)\|(?<channel>[0-9a-fA-F]+)\|(?<source>.+)?\|(?<msg>.+)?\|$");

        public bool NeedMore { get; private set; } = false;

        public IEnumerable<Entry> GetResults()
        {
            var results = _results;
            _results = new List<Entry>();
            return results;
        }

        public void Read(string line)
        {
            var match = _regex.Match(line);
            if (!match.Success)
                return;

            var timeGroup = match.Groups["time"];
            var channelGroup = match.Groups["channel"];
            var sourceGroup = match.Groups["source"];
            var messageGroup = match.Groups["msg"];
            var channelValue = Int32.Parse(channelGroup.Value, NumberStyles.HexNumber, CultureInfo.InvariantCulture);

            var entry = new Entry()
            {
                Time = DateTime.ParseExact(timeGroup.Value, "o", CultureInfo.InvariantCulture),
                Channel = GetChannel(channelValue)
            };

            if (sourceGroup.Success)
                entry.Source = sourceGroup.Value;
            if (messageGroup.Success)
                entry.Message = messageGroup.Value;

            _results.Add(entry);
        }

        private static ChatChannel GetChannel(int value)
        {
            // special cases, they were removed from FFXIVChatChannel, because they are Gobchat specific
            if (value == 0x01FFFF)
                return ChatChannel.GobchatInfo;
            if (value == 0x02FFFF)
                return ChatChannel.GobchatError;

            var ffxivChannel = (FFXIVChatChannel)value;
            var data = GobchatChannelMapping.GetChannel(ffxivChannel);
            return data.ChatChannel;
        }
    }

    [LogAttribute("ACTv1")]
    public class ACTv1Formater : ILogFormater
    {
        public string Format(Entry entry)
        {
            var channel = GetChannel(entry.Channel); // loss of data in some cases
            return $"00|{entry.Time.ToString("o", CultureInfo.InvariantCulture)}|{((int)channel).ToString("x4", CultureInfo.InvariantCulture)}|{entry.Source}|{entry.Message}|";
        }

        public static int GetChannel(ChatChannel channel)
        {
            switch (channel)
            {
                case ChatChannel.GobchatInfo:
                    return 0x01FFFF;

                case ChatChannel.GobchatError:
                    return 0x02FFFF;

                default:
                    var channelData = GobchatChannelMapping.GetChannel(channel); // loss of data in some cases
                    return (int)(channelData.ClientChannel.Length == 0 ? 0 : channelData.ClientChannel[0]);
            }
        }
    }
}