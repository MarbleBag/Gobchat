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

using System.Collections.Generic;
using System.Linq;
using System;
using Gobchat.Core.Util.Extension;

namespace Gobchat.Core.Chat
{
    public sealed class ChatMessageSegmentFormatter
    {
        private ReplaceTypeByToken _replacer = new ReplaceTypeByToken();
        private FormatConfig[] _formats = Array.Empty<FormatConfig>();

        public IEnumerable<FormatConfig> Formats
        {
            get => _formats;
            set => _formats = value.ToArrayOrEmpty();
        }

        public void Format(ChatMessage message)
        {
            foreach (var format in _formats)
            {
                if (!format.Active)
                    continue;
                _replacer.Format = format;
                message.FormatSegments(_replacer);
            }
        }
    }
}