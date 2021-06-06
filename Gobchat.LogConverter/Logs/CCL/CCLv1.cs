using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Gobchat.LogConverter.Logs.CCL
{

    [LogAttribute("CCLv1")]
    public sealed class CCLv1Parser : IParser
    {

        private int _patternIdx = 0;
        private Regex[] _pattern = Array.Empty<Regex>();
        private IEnumerable<Match>[] _matches = Array.Empty<IEnumerable<Match>>();

        private List<Entry> _results = new List<Entry>();
        private Entry _entry;


        public bool NeedMore => (_patternIdx < _pattern.Length) && _entry != null;

        public int ReparseLines => 0;

        public IEnumerable<Entry> GetResults()
        {
            var results = _results;
            _results = new List<Entry>();
            return results;
        }

        public void Read(string line)
        {
            if (line.StartsWith("Chatlogger"))
                ReadCommand(line);
            else
                ReadChat(line);
        }

        private void ReadChat(string line)
        {
            if (_pattern.Length == 0)
                return;

            if (_entry == null)
                _entry = new Entry();

            if (_patternIdx < _pattern.Length)
                _matches[_patternIdx] = _pattern[_patternIdx++].Matches(line).Cast<Match>();

            if (_patternIdx == _pattern.Length)
            {
                var matches = _matches.SelectMany(e => e).ToArray();
                var channel = GetData("channel", matches);
                var time = GetData("time", matches);
                var date = GetData("date", matches);
                var sender = GetData("sender", matches);
                var msg = GetData("msg", matches);

                _entry.Channel = GetChannel(channel);
                _entry.Source = sender ?? "";
                _entry.Message = msg ?? "";

                if (date != null || time != null)
                    _entry.Time = DateTime.Parse($"{date} {time}", CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal | DateTimeStyles.AllowLeadingWhite | DateTimeStyles.AllowTrailingWhite);

                StoreEntry();
            }
        }

        private void StoreEntry()
        {
            if (_entry != null && (_entry.Source != "" || _entry.Message != ""))
                _results.Add(_entry);
            _entry = null;
            _patternIdx = 0;
        }

        private static string GetData(string type, IEnumerable<Match> matches)
        {
            foreach (var match in matches)
                if (match.Groups[type].Success)
                    return match.Groups[type].Value;
            return null;
        }

        private static ChatChannel GetChannel(string value)
        {
            if (value == null || value.Length == 0)
                return ChatChannel.None;
            value = value.ToUpperInvariant();
            if (Enum.TryParse<ChatChannel>(value, true, out var gobChannel))
                return gobChannel;
            return ChatChannel.None;
        }



        private void ReadCommand(string line)
        {
            if (line.StartsWith("Chatlogger format:"))
            {
                var format = line.Substring(line.IndexOf(":") + 1);
                var matches = Regex.Matches(format, @"{(?<name>\w+([_-]\w+)*)}");
                var regexBuilder = new StringBuilder(@"^");
                var regexByLine = new List<string>();
                var lastReadCharIndex = 0;
                foreach (Match match in matches)
                {
                    if (lastReadCharIndex < match.Index)
                        regexBuilder.Append(Regex.Escape(format.Substring(lastReadCharIndex, match.Index - lastReadCharIndex)));
                    lastReadCharIndex = match.Index + match.Length;

                    var code = match.Groups["name"].Value.ToUpperInvariant();
                    switch (code)
                    {
                        case "TIME":
                            regexBuilder.Append(@"(?<time>\d{2}:\d{2}:\d{2})");
                            break;
                        case "TIME-SHORT":
                            regexBuilder.Append(@"(?<time>\d{2}:\d{2})");
                            break;
                        case "TIME-FULL":
                            regexBuilder.Append(@"(?<time>\d{2}:\d{2}:\d{2}\+\d+:\d{2})");
                            break;
                        case "DATE":
                            regexBuilder.Append(@"(?<date>\d{4}-\d{2}-\d{2})");
                            break;
                        case "CHANNEL":
                            regexBuilder.Append(@"(?<channel>[\w_]+)");
                            break;
                        case "CHANNEL-PADL":
                            regexBuilder.Append(@"\s*?(?<channel>[\w_]+)");
                            break;
                        case "CHANNEL-PADR":
                            regexBuilder.Append(@"(?<channel>[\w_]+)\s*?");
                            break;
                        case "SENDER":
                            regexBuilder.Append(@"(?<sender>[\w -]+)");
                            break;
                        case "MESSAGE":
                            regexBuilder.Append(@"(?<msg>.+?)");
                            break;
                        case "BREAK":
                            regexByLine.Add(regexBuilder.Append(@"$").ToString());
                            regexBuilder.Clear().Append("^");
                            break;
                        default:
                            regexBuilder.Append(match.Value);
                            break;
                    }
                }

                if (lastReadCharIndex < format.Length)
                    regexByLine.Append(Regex.Escape(format.Substring(lastReadCharIndex, format.Length - lastReadCharIndex)));

                if (regexBuilder.Length > 0)
                    regexByLine.Add(regexBuilder.Append(@"$").ToString());

                _pattern = regexByLine.Select(e => new Regex(e, RegexOptions.None)).ToArray();
                _matches = new IEnumerable<Match>[_pattern.Length];

                StoreEntry();
            }
        }
    }

    [LogAttribute("CCLv1")]
    public sealed class CCLv1Formater : IFormaterWithSettings
    {
        #region formaters

        private interface IFormater
        {
            string Format(Entry msg);
        }

        private sealed class TimeFormater : CCLv1Formater.IFormater
        {
            public string Format(Entry msg)
            {
                return msg.Time.ToString("HH':'mm':'ss", System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        private sealed class TimeShortFormater : CCLv1Formater.IFormater
        {
            public string Format(Entry msg)
            {
                return msg.Time.ToString("HH':'mm", System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        private sealed class TimeFullFormater : CCLv1Formater.IFormater
        {
            public string Format(Entry msg)
            {
                return msg.Time.ToString("HH':'mm':'ssK", System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        private sealed class DateFormater : CCLv1Formater.IFormater
        {
            public string Format(Entry msg)
            {
                return msg.Time.ToString("yyyy'-'MM'-'dd", System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        private sealed class ChannelNameFormater : CCLv1Formater.IFormater
        {
            public string Format(Entry msg)
            {
                return msg.Channel.ToString();
            }
        }

        private sealed class ChannelNamePaddedLeftFormater : CCLv1Formater.IFormater
        {
            private static readonly int _characters;

            static ChannelNamePaddedLeftFormater()
            {
                _characters = Enum.GetValues(typeof(ChatChannel)).Cast<ChatChannel>().Select(e => e.ToString().Length).Max();
            }

            public string Format(Entry msg)
            {
                return msg.Channel.ToString().PadLeft(_characters);
            }
        }

        private sealed class ChannelNamePaddedRightFormater : CCLv1Formater.IFormater
        {
            private static readonly int _characters;

            static ChannelNamePaddedRightFormater()
            {
                _characters = Enum.GetValues(typeof(ChatChannel)).Cast<ChatChannel>().Select(e => e.ToString().Length).Max();
            }

            public string Format(Entry msg)
            {
                return msg.Channel.ToString().PadRight(_characters);
            }
        }

        private sealed class SenderFormater : CCLv1Formater.IFormater
        {
            public string Format(Entry msg)
            {
                return msg.Source;
            }
        }

        private sealed class MessageFormater : CCLv1Formater.IFormater
        {
            public string Format(Entry msg)
            {
                return msg.Message;
            }
        }

        private sealed class BreakFormater : CCLv1Formater.IFormater
        {
            public string Format(Entry msg)
            {
                return Environment.NewLine;
            }
        }

        private static readonly IDictionary<string, IFormater> FormaterByName = new Dictionary<string, IFormater>();
        static CCLv1Formater()
        {
            FormaterByName.Add("TIME", new TimeFormater());
            FormaterByName.Add("TIME-SHORT", new TimeShortFormater());
            FormaterByName.Add("TIME-FULL", new TimeFullFormater());
            FormaterByName.Add("DATE", new DateFormater());
            FormaterByName.Add("CHANNEL", new ChannelNameFormater());
            FormaterByName.Add("CHANNEL-PADL", new ChannelNamePaddedLeftFormater());
            FormaterByName.Add("CHANNEL-PADR", new ChannelNamePaddedRightFormater());
            FormaterByName.Add("SENDER", new SenderFormater());
            FormaterByName.Add("MESSAGE", new MessageFormater());
            FormaterByName.Add("BREAK", new BreakFormater());
        }

        #endregion

        private IFormater[] _formaters = Array.Empty<IFormater>();
        private object[] _logArgs = Array.Empty<object>();
        private string _logTemplate = "";
        private bool _logFormatChanged = false;
        private string _logFormat;

        public CCLv1Formater()
        {
            LogFormat = "{channel} [{date} {time-full}] {sender}: {message}";
        }

        public string LogFormat
        {
            get => _logFormat;
            set
            {
                if (value == null || value.Length == 0)
                    throw new ArgumentNullException(nameof(value));

                if (value.Equals(_logFormat))
                    return;

                if (value.Equals(_logFormat))
                    return;

                var matches = Regex.Matches(value, @"{(?<name>\w+([_-]\w+)*)}");
                var formaters = new List<IFormater>();

                var templateBuilder = new StringBuilder();
                var lastReadCharIndex = 0;

                foreach (Match match in matches)
                {
                    if (!match.Success)
                        continue;

                    if (lastReadCharIndex < match.Index)
                        templateBuilder.Append(value.Substring(lastReadCharIndex, match.Index - lastReadCharIndex));
                    lastReadCharIndex = match.Index + match.Length;

                    if (FormaterByName.TryGetValue(match.Groups["name"].Value.ToUpperInvariant(), out var formater))
                    {
                        templateBuilder.Append($"{{{formaters.Count}}}");
                        formaters.Add(formater);
                    }
                    else
                    {
                        templateBuilder.Append(match.Value);
                    }
                }

                if (lastReadCharIndex < value.Length)
                    templateBuilder.Append(value.Substring(lastReadCharIndex, value.Length - lastReadCharIndex));

                _logFormat = value;
                _logTemplate = templateBuilder.ToString();
                _formaters = formaters.ToArray();
                _logArgs = new object[_formaters.Length];
                _logFormatChanged = true;
            }
        }

        public IEnumerable<string> Format(Entry entry)
        {
            for (var i = 0; i < _formaters.Length; ++i)
                _logArgs[i] = _formaters[i].Format(entry);
            var result = string.Format(_logTemplate, _logArgs);

            if (_logFormatChanged)
            {
                _logFormatChanged = false;
                return new string[] { $"Chatlogger format:{_logFormat}", result };
            }
            else
            {
                return new string[] { result };
            }
        }

        public Control GetSettingForm()
        {
            var control = new Settings(this);
            return control;
        }
    }
}
