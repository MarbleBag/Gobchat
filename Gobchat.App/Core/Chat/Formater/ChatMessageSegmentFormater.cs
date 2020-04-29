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

namespace Gobchat.Core.Chat
{
    public static class ChatMessageSegmentFormater
    {
        public static void FormatSegments(this ChatMessage message, IReplacer replacer)
        {
            var processMessage = replacer.StartReplace(message);
            if (!processMessage)
                return;

            var messageSegments = message.Message.ToArray();
            message.Message.Clear();

            foreach (var messageSegment in messageSegments)
            {
                var marker = new SegmentMarker();
                replacer.Segment(marker, messageSegment.Type, messageSegment.Content);

                var replaceSegments = marker.Count > 0;
                if (!replaceSegments)
                {
                    message.Message.Add(messageSegment);
                    continue;
                }

                marker.Finish();
                foreach (var mark in marker.Marks)
                {
                    if (mark.End - mark.Start <= 0)
                        continue; //ignore empty marks

                    var newSegment = new MessageSegment(mark.Type, messageSegment.Content.Substring(mark.Start, mark.End));
                    message.Message.Add(newSegment);
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

        public int Count { get => _marks.Count; }

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