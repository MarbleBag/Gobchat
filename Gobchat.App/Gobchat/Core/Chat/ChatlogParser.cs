﻿/*******************************************************************************
 * Copyright (C) 2019 MarbleBag
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

using Gobchat.Memory.Chat;
using Gobchat.Memory.Chat.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Gobchat.Core.Chat
{
    internal class ChatlogParser
    {
        public ChatMessage Process(ChatlogItem item)
        {
            if (!IsChatlogItemValid(item))
                return null;

            var text = CleanChatLog(item);
            var source = ExtractSource(text, out int readIdx);
            var message = ExtractMsg(text, readIdx);

            //TODO check for errors /exception handling

            return new ChatMessage(item.TimeStamp, source, item.Channel, message);
        }


        //Not the best solution, but works by simplifying the task
        private string CleanChatLog(ChatlogItem item)
        {
            Regex regex = new Regex(@"^\w+\b");

            var tokens = item.Tokens;
            StringBuilder builder = new StringBuilder();
            for(int idx = 0; idx < tokens.Count; ++idx)
            {
                if(tokens[idx] is Gobchat.Memory.Chat.Token.TextToken txtToken)
                {
                    builder.Append(txtToken.GetText());
                }
                else if(tokens[idx] is Gobchat.Memory.Chat.Token.ServerDelimiterToken slToken)
                {
                    builder.Append(" ");
                    idx += 1;
                    if (tokens.Count < idx && tokens[idx] is Gobchat.Memory.Chat.Token.TextToken txtToken1)
                    {
                        var txt = txtToken1.GetText();
                        var match = regex.Match(txt);
                        if (match.Success)
                        {
                            var serverName = match.Value;
                            builder.Append("[").Append(serverName).Append("] ");
                            builder.Append(txt, serverName.Length, txt.Length - serverName.Length);
                        }
                        else
                        {
                            builder.Append(txt);
                        }                       
                    }
                }
                
                else if(tokens[idx] is Gobchat.Memory.Chat.Token.AutotranslateToken atToken)
                {
                    builder.Append(" [Autotranslate Goes Here] ");
                }
                else if(tokens[idx] is Gobchat.Memory.Chat.Token.LinkToken linkToken)
                {
                    //skip for now
                }
            }

            return builder.ToString();
        }

        private string ExtractSource(string text, out int readIdx)
        {
            var regex = new Regex(@"^:(.*):");
            var match = regex.Match(text);
            if (match.Success)
            {
                readIdx = match.Index + match.Length;
                return match.Groups[1].Value;
            }
            else
            {
                throw new ArgumentException(""); //TODO not useful
            }
        }

        private string ExtractMsg(string text, int lastReadIdx)
        {
            return text.Substring(lastReadIdx);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="chatlogItem"></param>
        /// <returns>true when the chatlog is valid</returns>
        private bool IsChatlogItemValid(ChatlogItem chatlogItem)
        {
            var tokens = chatlogItem.Tokens;
            if (tokens.Count == 0) return false;
            if (tokens[0] is Gobchat.Memory.Chat.Token.TextToken txtToken)
            {
                var txt = txtToken.GetText();
                if (txt == null || txt.Length == 0)
                    return false;
                return txt.StartsWith(":", System.StringComparison.InvariantCulture);
            }
            else
            {
                return false;
            }
        }
    }
}
