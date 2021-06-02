using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Gobchat.LogConverter.Logs.FCL
{
    [LogAttribute("FCLv1")]
    internal sealed class FCLv1LogParser : ILogParser
    {
        private List<Entry> _results = new List<Entry>();
        private Entry _entry;

        private readonly Regex _headerRegex = new Regex(@"^(?<channel>[a-zA-Z0-9_]+)\s+\[(?<time>.*)\]\s+(?<source>.*)?:$");

        public bool NeedMore { get => _entry != null; }

        public IEnumerable<Entry> GetResults()
        {
            var results = _results;
            _results = new List<Entry>();
            return results;
        }

        public void Read(string line)
        {
            if (_entry == null)
            {
                var match = _headerRegex.Match(line);
                if (!match.Success)
                    return;

                var timeGroup = match.Groups["time"];
                var channelGroup = match.Groups["channel"];
                var sourceGroup = match.Groups["source"];

                _entry = new Entry()
                {
                    Time = DateTime.ParseExact(timeGroup.Value, "yyyy'-'MM'-'dd' 'HH':'mm':'ssK", CultureInfo.InvariantCulture),
                    Channel = GetChannel(channelGroup.Value)
                };

                if (sourceGroup.Success)
                    _entry.Source = sourceGroup.Value;
            }
            else
            {
                _entry.Message = line;
                _results.Add(_entry);
                _entry = null;
            }
        }

        private static ChatChannel GetChannel(string value)
        {
            if (value == null || value.Length == 0)
                return ChatChannel.None;

            value = value.ToUpperInvariant();

            if (Enum.TryParse<ChatChannel>(value, true, out var gobChannel)) // will work for all logs which uses the new channel names
                return gobChannel;

            // special cases, they were removed from FFXIVChatChannel, because they are Gobchat specific
            if ("GOBCHAT_INFO".Equals(value))
                return ChatChannel.GobchatInfo;
            if ("GOBCHAT_ERROR".Equals(value))
                return ChatChannel.GobchatError;

            if (!Enum.TryParse<FFXIVChatChannel>(value, true, out var ffxivChannel)) // logs before 1.7.0 use the ffxiv chat channel names
                return ChatChannel.None; //no clue what's going on, may be corrupt

            var data = GobchatChannelMapping.GetChannel(ffxivChannel);
            return data.ChatChannel;
        }
    }

    [LogAttribute("FCLv1")]
    internal class FCLv1Formater : ILogFormater
    {
        private readonly StringBuilder _builder = new StringBuilder();

        public string Format(Entry entry)
        {
            try
            {
                _builder.Append(entry.Channel).Append(" ");
                var timeConverted = entry.Time.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ssK", CultureInfo.InvariantCulture);
                _builder.Append("[").Append(timeConverted).Append("] ");
                _builder.Append(entry.Source).AppendLine(":");
                _builder.AppendLine(entry.Message);
                var formatedLine = _builder.ToString();
                return formatedLine;
            }
            finally
            {
                _builder.Clear();
            }
        }
    }
}
