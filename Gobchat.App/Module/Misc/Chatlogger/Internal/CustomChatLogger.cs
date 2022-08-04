/*******************************************************************************
 * Copyright (C) 2019-2022 MarbleBag
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
using System.Text;
using System.Text.RegularExpressions;
using Gobchat.Core.Chat;
using Gobchat.Module.Language;

namespace Gobchat.Module.Misc.Chatlogger.Internal
{
    public sealed class CustomChatLogger : ChatLoggerBase
    {
        #region formaters

        private interface IFormater
        {
            string Format(ChatMessage msg);
        }

        private sealed class TimeFormater : IFormater
        {
            public string Format(ChatMessage msg)
            {
                return msg.Timestamp.ToString("HH':'mm':'ss", System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        private sealed class TimeShortFormater : IFormater
        {
            public string Format(ChatMessage msg)
            {
                return msg.Timestamp.ToString("HH':'mm", System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        private sealed class TimeFullFormater : IFormater
        {
            public string Format(ChatMessage msg)
            {
                return msg.Timestamp.ToString("HH':'mm':'ssK", System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        private sealed class DateFormater : IFormater
        {
            public string Format(ChatMessage msg)
            {
                return msg.Timestamp.ToString("yyyy'-'MM'-'dd", System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        private sealed class ChannelNameFormater : IFormater
        {
            public string Format(ChatMessage msg)
            {
                return msg.Channel.ToString();
            }
        }

        private sealed class ChannelNameTranslatedFormater : IFormater
        {
            public string Format(ChatMessage msg)
            {
                var data = GobchatChannelMapping.GetChannel(msg.Channel);
                return WebUIResources.ResourceManager.GetString(data.TranslationId);
            }
        }

        private sealed class ChannelNamePaddedLeftFormater : IFormater
        {
            private static readonly int _characters;

            static ChannelNamePaddedLeftFormater()
            {
                _characters = Enum.GetValues(typeof(ChatChannel)).Cast<ChatChannel>().Select(e => e.ToString().Length).Max();
            }

            public string Format(ChatMessage msg)
            {
                return msg.Channel.ToString().PadLeft(_characters);
            }
        }

        private sealed class ChannelNamePaddedRightFormater : IFormater
        {
            private static readonly int _characters;

            static ChannelNamePaddedRightFormater()
            {
                _characters = Enum.GetValues(typeof(ChatChannel)).Cast<ChatChannel>().Select(e => e.ToString().Length).Max();
            }

            public string Format(ChatMessage msg)
            {
                return msg.Channel.ToString().PadRight(_characters);
            }
        }

        private sealed class SenderFormater : IFormater
        {
            public string Format(ChatMessage msg)
            {
                return msg.Source.Original;
            }
        }

        private sealed class ChannelSpecificSenderFormater : IFormater
        {

            private readonly ILocaleManager localeManager;

            public string Format(ChatMessage msg)
            {
                var sender = msg.Source.Original;
                switch (msg.Channel)
                {
                    case ChatChannel.GobchatInfo:
                    case ChatChannel.GobchatError:
                        return $"[{sender}]";
                    case ChatChannel.Echo:
                        return "Echo:";
                    case ChatChannel.Emote:
                        return sender;
                    case ChatChannel.TellSend:
                        return $">> {sender}:";
                    case ChatChannel.TellRecieve:
                        return $"{sender} >>";
                    case ChatChannel.Error:
                        return null;
                    case ChatChannel.AnimatedEmote:
                        return null;
                    case ChatChannel.Party:
                        return $"({sender})";
                    case ChatChannel.Alliance:
                        return $"<{sender}>";
                    case ChatChannel.Guild:
                    case ChatChannel.LinkShell_1:
                    case ChatChannel.LinkShell_2:
                    case ChatChannel.LinkShell_3:
                    case ChatChannel.LinkShell_4:
                    case ChatChannel.LinkShell_5:
                    case ChatChannel.LinkShell_6:
                    case ChatChannel.LinkShell_7:
                    case ChatChannel.LinkShell_8:
                    case ChatChannel.CrossWorldLinkShell_1:
                    case ChatChannel.CrossWorldLinkShell_2:
                    case ChatChannel.CrossWorldLinkShell_3:
                    case ChatChannel.CrossWorldLinkShell_4:
                    case ChatChannel.CrossWorldLinkShell_5:
                    case ChatChannel.CrossWorldLinkShell_6:
                    case ChatChannel.CrossWorldLinkShell_7:
                    case ChatChannel.CrossWorldLinkShell_8:
                        var abbreviationId = GobchatChannelMapping.GetChannel(msg.Channel).AbbreviationId;
                        var abbreviation = WebUIResources.ResourceManager.GetString(abbreviationId);
                        return $"[{abbreviation}]<{sender}>";
                    default:
                        return sender != null ? $"{sender}:" : null;
                }
            }
        }

        private sealed class MessageFormater : IFormater
        {
            private readonly StringBuilder _builder = new StringBuilder();
            public string Format(ChatMessage msg)
            {
                _builder.Clear();
                foreach (var msgPart in msg.Content)
                    _builder.Append(msgPart.Text);
                return _builder.ToString();
            }
        }

        private sealed class BreakFormater : IFormater
        {
            public string Format(ChatMessage msg)
            {
                return Environment.NewLine;
            }
        }

        private static readonly IDictionary<string, IFormater> FormaterByName = new Dictionary<string, IFormater>();
        static CustomChatLogger()
        {
            FormaterByName.Add("TIME", new TimeFormater());
            FormaterByName.Add("TIME-SHORT", new TimeShortFormater());
            FormaterByName.Add("TIME-FULL", new TimeFullFormater());
            FormaterByName.Add("DATE", new DateFormater());
            FormaterByName.Add("CHANNEL", new ChannelNameFormater());
            FormaterByName.Add("CHANNEL-PADL", new ChannelNamePaddedLeftFormater());
            FormaterByName.Add("CHANNEL-PADR", new ChannelNamePaddedRightFormater());
            FormaterByName.Add("SENDER-CHA", new ChannelSpecificSenderFormater());
            FormaterByName.Add("SENDER", new SenderFormater());
            FormaterByName.Add("MESSAGE", new MessageFormater());
            FormaterByName.Add("BREAK", new BreakFormater());
        }

        #endregion

        private IFormater[] _formaters = Array.Empty<IFormater>();
        private object[] _logArgs = Array.Empty<object>();
        private string _logTemplate = "";
        private string _formatWrittenToFile;

        public string LogFormat { get; private set; } = null;

        public void SetLogFormat(string format)
        {
            if (format == null || format.Length == 0)
                throw new ArgumentNullException(nameof(format));

            if (format.Equals(LogFormat))
                return;

            lock (_synchronizationLock)
            {
                if (format.Equals(LogFormat))
                    return;

                Flush();

                var matches = Regex.Matches(format, @"{(?<name>\w+([_-]\w+)*)}");
                var formaters = new List<IFormater>();

                var templateBuilder = new StringBuilder();
                var lastReadCharIndex = 0;

                foreach (Match match in matches)
                {
                    if (!match.Success)
                        continue;

                    if (lastReadCharIndex < match.Index)
                        templateBuilder.Append(format.Substring(lastReadCharIndex, match.Index - lastReadCharIndex));
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

                if (lastReadCharIndex < format.Length)
                    templateBuilder.Append(format.Substring(lastReadCharIndex, format.Length - lastReadCharIndex));

                LogFormat = format;
                _logTemplate = templateBuilder.ToString();
                _formaters = formaters.ToArray();
                _logArgs = new object[_formaters.Length];
                OnNewLogFormat();
            }
        }

        public CustomChatLogger() : base("CCLv1")
        {
        }

        protected override string FormatMessage(ChatMessage msg)
        {
            for (var i = 0; i < _formaters.Length; ++i)
                _logArgs[i] = _formaters[i].Format(msg);
            return string.Format(_logTemplate, _logArgs);
        }

        private void OnNewLogFormat()
        {
            if (FileHandle == null)
                return;
            if (_formatWrittenToFile == LogFormat)  // needs only to be written, if not already done
                return;

            LogMessage($"Chatlogger format:{LogFormat}");
            _formatWrittenToFile = LogFormat;
        }

        override protected void OnFileChange()
        {
            if (LogFormat == null)
                return;

            WriteMessageToFile($"Chatlogger format:{LogFormat}");
            _formatWrittenToFile = LogFormat;
        }

    }
}