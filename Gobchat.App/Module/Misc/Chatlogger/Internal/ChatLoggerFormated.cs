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
using System.Globalization;
using System.Text;
using Gobchat.Core.Chat;

namespace Gobchat.Module.Misc.Chatlogger.Internal
{
    public sealed class ChatLoggerFormated : ChatLoggerBase
    {
        private readonly StringBuilder _builder = new StringBuilder();

        public ChatLoggerFormated() : base()
        {
        }

        protected override string LoggerId => "FCLv1";

        protected override string FormatLine(ChatMessage msg)
        {
            // an improved logger based on https://github.com/MarbleBag/Gobchat/issues/48
            try
            {
                _builder.Append(msg.Channel).Append(" ");
                var timeConverted = msg.Timestamp.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ssK", CultureInfo.InvariantCulture);
                _builder.Append("[").Append(timeConverted).Append("] ");
                _builder.Append(msg.Source.Original).AppendLine(":");
                foreach (var msgPart in msg.Content)
                {
                    _builder.Append(msgPart.Text);
                }
                _builder.AppendLine();
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