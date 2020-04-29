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
using System.Text.RegularExpressions;
using System.Linq;
using System;

namespace Gobchat.Core.Chat
{
    public sealed class ChatMessageMentionFinder
    {
        private string[] _mentions = Array.Empty<string>();
        private readonly ReplaceTypeByText _replacer = new ReplaceTypeByText();

        public MessageSegmentType MessageSegmentType
        {
            get => _replacer.SegmentType;
            set => _replacer.SegmentType = value;
        }

        public void SetMentions(IEnumerable<string> mentions)
        {
            if (mentions == null)
                throw new ArgumentNullException(nameof(mentions));
            _mentions = mentions.ToArray();
            var pattern = _mentions.Select(t => new Regex($"\b{Regex.Escape(t)}\b", RegexOptions.IgnoreCase));

            _replacer.Pattern.Clear();
            _replacer.Pattern.AddRange(pattern);
        }

        public IEnumerable<string> GetMentions()
        {
            return _mentions.ToList();
        }

        public void MarkMentions(ChatMessage message)
        {
            if (_mentions.Length == 0)
                return;

            message.FormatSegments(_replacer);
        }
    }
}