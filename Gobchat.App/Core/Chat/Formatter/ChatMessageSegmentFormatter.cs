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

    public static class ChatMessageExtension
    {
        public static void FormatSegments(this ChatMessage message, IReplacer replacer)
        {
            var processMessage = replacer.StartReplace(message);
            if (!processMessage)
                return;

            var messageSegments = message.Content.ToArray();
            message.Content.Clear();

            foreach (var messageSegment in messageSegments)
            {
                var marker = new SegmentMarker();
                replacer.Segment(marker, messageSegment.Type, messageSegment.Text);
                marker.Finish();

                var anyReplacements = marker.Count > 0;
                if (!anyReplacements) // no changes? Write unchanged message and skip rest of loop
                {
                    message.Content.Add(messageSegment);
                    continue;
                }

                foreach (var mark in marker.Marks)
                {
                    var substringStart = mark.Start;
                    var substringLength = Math.Min(messageSegment.Text.Length - mark.Start, mark.End - mark.Start);
                    if (substringLength <= 0)
                        continue; //ignore empty marks

                    var newSegment = new MessageSegment(mark.Type, messageSegment.Text.Substring(substringStart, substringLength));
                    message.Content.Add(newSegment);
                }
            }

            replacer.EndReplace();
        }
    }

    public sealed class Mark
    {
        public MessageSegmentType Type { get; set; }
        public int End { get; set; }
        public int Start { get; set; }
    }

    public sealed class SegmentMarker
    {
        public List<Mark> Marks
        {
            get => _marks.ToList();
        }

        public Mark Mark { get; private set; }

        public int Count { get => _marks.Count + (Mark == null ? 0 : 1); }

        private readonly List<Mark> _marks = new List<Mark>();

        public void Finish()
        {
            if (Mark != null)
                _marks.Add(Mark);
            Mark = null;
        }

        public void Clear()
        {
            Mark = null;
            _marks.Clear();
        }

        public void NewMark(MessageSegmentType type, int start, int end)
        {
            if (Mark != null)
                _marks.Add(Mark);
            Mark = new Mark()
            {
                Type = type,
                Start = start,
                End = end
            };
        }

        public void NewMark(MessageSegmentType type, int start)
        {
            NewMark(type, start, start);
        }
    }
}