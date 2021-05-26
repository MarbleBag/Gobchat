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
using System.Text;
using System.Text.RegularExpressions;
using Gobchat.Core.Chat;

namespace Gobchat.Module.Misc.Chatlogger.Internal
{
    public sealed class CustomChatLogger : ChatLoggerBase
    {
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

        private sealed class SenderFormater : IFormater
        {
            public string Format(ChatMessage msg)
            {
                return msg.Source.Original;
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
            FormaterByName.Add("TIME-FULL", new TimeFullFormater());
            FormaterByName.Add("DATE", new DateFormater());
            FormaterByName.Add("CHANNEL", new ChannelNameFormater());
            FormaterByName.Add("SENDER", new SenderFormater());
            FormaterByName.Add("MESSAGE", new MessageFormater());
            FormaterByName.Add("BREAK", new BreakFormater());
        }

        private IFormater[] _formaters = Array.Empty<IFormater>();
        private object[] _logArgs = Array.Empty<object>();
        private string _logTemplate = "";

        public string LogFormat { get; private set; }

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

                var matches = Regex.Matches(format, @"\$(?<name>[\w_-]+)");
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
                InternalLog($"Chatlogger format:{LogFormat}");
            }

        }

        public CustomChatLogger() : base("CCLv1")
        {
        }

        protected override string FormatLine(ChatMessage msg)
        {
            for (var i = 0; i < _formaters.Length; ++i)
                _logArgs[i] = _formaters[i].Format(msg);
            return string.Format(_logTemplate, _logArgs);
        }

    }
}