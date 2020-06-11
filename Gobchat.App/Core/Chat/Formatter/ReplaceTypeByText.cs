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

namespace Gobchat.Core.Chat
{
    internal sealed class ReplaceTypeByText : IReplacer
    {
        public MessageSegmentType SegmentType { get; set; } = MessageSegmentType.Undefined;
        public List<Regex> Pattern { get; } = new List<Regex>();

        public bool StartReplace(ChatMessage message)
        {
            return Pattern.Count > 0;
        }

        public void EndReplace()
        {
        }

        public void Segment(SegmentMarker marker, MessageSegmentType currentType, string text)
        {
            var matches = new List<(int Start, int End)>();

            foreach (var pattern in Pattern)
            {
                var match = pattern.Match(text);
                while (match.Success)
                {
                    matches.Add((match.Index, match.Index + match.Length));
                    match = match.NextMatch();
                }
            }

            matches.Sort((a, b) => a.Start - b.Start);

            for (var i = 1; i < matches.Count;)
            {
                var previous = matches[i - 1];
                var current = matches[i];
                if (current.Start <= previous.End)
                {
                    previous.End = System.Math.Max(previous.End, current.End);
                    matches.RemoveAt(i);
                }
                else
                {
                    i += 1;
                }
            }

            marker.NewMark(currentType, 0);
            foreach (var match in matches)
            {
                marker.Mark.End = match.Start;
                marker.NewMark(SegmentType, match.Start, match.End);
                marker.NewMark(currentType, match.End);
            }
            marker.Mark.End = text.Length;
        }

        /*
       private List<(int, int)> FindAllWordMatches(string[] words, string textLine)
       {
           bool IsBoundary(int index)
           {
               if (index < 0 || textLine.Length <= index) return true; //start & end
               var c = textLine[index];
               var isLetter = c.toLowerCase() != c.toUpperCase(); //works for a lot of character, but not for letters who don't have a diffrent lower and upper case :(
               return !isLetter; //as long it's not a letter, it's okay!
           }

           void SearchByIndexOf(List<(int, int)> result)
           {
               int startIndex, endIndex, index;
               foreach (var word in words)
               {
                   var length = word.Length;
                   startIndex = 0;
                   while ((index = textLine.IndexOf(word, startIndex)) > -1)
                   {
                       endIndex = index + length;
                       if (IsBoundary(index - 1) && IsBoundary(endIndex))
                       {
                           result.Add((index, endIndex));
                       }
                       startIndex = endIndex;
                   }
               }
           }

           var result = new List<(int, int)>();
           SearchByIndexOf(result);
           result.Sort((a, b) => { return a.Item1 - b.Item1 });

           var merged = new List<(int, int)>();
           if (result.Count > 0)
               merged.Add(result[0]);

           for (var i = 1; i < result.Count; ++i)
           {
               var last = merged[merged.Count - 1];
               var next = result[i];
               if (next.Item1 <= last.Item2)
               {
                   last.Item2 = next.Item2;
               }
               else
               {
                   merged.Add(next);
               }
           }

           return merged;
       }
       */
    }
}