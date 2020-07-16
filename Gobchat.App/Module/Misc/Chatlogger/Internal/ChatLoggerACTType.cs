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

using System.Linq;
using System.Globalization;
using Gobchat.Core.Chat;

namespace Gobchat.Module.Misc.Chatlogger.Internal
{
    public sealed class ChatLoggerACTType : ChatLoggerBase
    {
        protected override string LoggerId => "ACT1v";

        protected override string FormatLine(ChatMessage msg)
        {
            // until a new chatlog cleaner is written, keep it compatible with https://github.com/MarbleBag/FF14-Chatlog-Cleaner
            return $"00|{msg.Timestamp.ToString("o", CultureInfo.InvariantCulture)}|{((int)msg.Channel).ToString("x4", CultureInfo.InvariantCulture)}|{msg.Source}|{msg.Content.Select(e => e.Text)}|";
        }
    }
}