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

using System.Collections.Generic;

namespace Gobchat.Core.Chat
{
    internal sealed class ReplaceTypeByToken : IReplacer
    {
        private bool _lastSegmentClosed;
        public FormatConfig Format { get; set; }

        public ReplaceTypeByToken()
        {
        }

        private int MatchesToken(string text, int textOffset, ICollection<string> tokens)
        {
            foreach (var token in tokens)
            {
                var match = textOffset + token.Length <= text.Length;
                for (var n = 0; n < token.Length && match; ++n)
                    match &= token[n] == text[textOffset + n];

                if (match)
                    return token.Length;
            }
            return 0;
        }

        public bool StartReplace(ChatMessage message)
        {
            _lastSegmentClosed = true;
            return true;
        }

        public void EndReplace()
        {
        }

        public void Segment(SegmentMarker marker, MessageSegmentType originalType, string text)
        {
            if (originalType != MessageSegmentType.Undefined)
                return; //already defined

            if (_lastSegmentClosed)
                marker.NewMark(originalType, 0, 0); //start mark, needs to be removed if no other mark is found
            else
                marker.NewMark(Format.Type, 0, 0); //this should not be deleted, even if no more marks are found

            var match = !_lastSegmentClosed;

            for (var i = 0; i < text.Length; ++i)
            {
                if (match)
                {
                    var tokenLength = MatchesToken(text, i, Format.EndTokens);
                    if (0 < tokenLength)
                    { // closing marker found
                        match = false;
                        marker.Mark.End = i + tokenLength;
                        marker.NewMark(originalType, i + tokenLength, i + tokenLength);
                        i = i + tokenLength - 1;
                    }
                }
                else
                {
                    var tokenLength = MatchesToken(text, i, Format.StartTokens);
                    if (0 < tokenLength)
                    { // opening marker found
                        match = true;
                        marker.Mark.End = i;
                        marker.NewMark(Format.Type, i, i + tokenLength);
                        i = i + tokenLength - 1;
                    }
                }
            }

            marker.Mark.End = text.Length;
            if (_lastSegmentClosed)
                if (marker.Count == 1) //no matches found, clear all marks and continue
                    marker.Clear();
            _lastSegmentClosed = !match;
        }
    }
}