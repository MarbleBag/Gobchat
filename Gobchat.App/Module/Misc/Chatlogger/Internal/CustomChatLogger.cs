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

using System.Collections.Generic;
using System.Text;
using Gobchat.Core.Chat;

namespace Gobchat.Module.Misc.Chatlogger.Internal
{
    public sealed class CustomChatLogger : ChatLoggerBase
    {
        private readonly StringBuilder _builder = new StringBuilder();

        private string _messageFormat = ""; //default

        public string MessageFormat
        {
            get
            {
                return _messageFormat;
            }
            set
            {
                _messageFormat = value;
                //TODO parse format                
            }
        }

        public CustomChatLogger() : base()
        {
        }

        protected override string LoggerId => "CCLv1";

        protected override IEnumerable<string> OnFileCreation()
        {
            return null;
        }

        protected void ParseMessageFormat(string format)
        {
            //TODO parse
            //TODO write to file
        }

        protected override string FormatLine(ChatMessage msg)
        {
            return null;
        }

    }
}