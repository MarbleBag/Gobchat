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

using System.Text;
using System.Text.RegularExpressions;

namespace Gobchat.Core.Chat
{
    public sealed class ChatlogCleaner
    {
        private const string InformationSeperator = "\u001f"; //FF14 now uses \u001f since 5.2 instead of '\u003a' to separate source from message
        private static readonly string SourceRegEx = $"^{InformationSeperator}(.*?){InformationSeperator}";

        public IAutotranslateProvider AutotranslateProvider
        {
            get => _autotranslateProvider;
            set => _autotranslateProvider = value ?? throw new System.ArgumentNullException(nameof(AutotranslateProvider));
        }

        private IAutotranslateProvider _autotranslateProvider;

        public ChatlogCleaner(IAutotranslateProvider autotranslateLookup)
        {
            AutotranslateProvider = autotranslateLookup;
        }

        /// <summary>
        /// Turns a chatlog item that was read from memory into a cleaned version that consists of the message and source as strings, but also contains the original timestamp and channel. Any links, autotranslation and other elements, which contain additional informations beside text, are processed and turned into strings.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>A cleaned chatlog item or null, if the chatlog was not valid</returns>
        public CleanedChatlogItem Clean(Memory.Chat.ChatlogItem item)
        {
            if (item == null)
                throw new System.ArgumentNullException(nameof(item));

            if (!IsChatlogItemValid(item))
                return null;

            var text = CombineChatlogTokens(item);
            var source = ExtractSource(text, out int readIdx);
            var message = ExtractMsg(text, readIdx);

            return new CleanedChatlogItem(item.TimeStamp, (ChatChannel)item.Channel, source, message);
        }

        //Not the best solution, but works by simplifying the task
        private string CombineChatlogTokens(Memory.Chat.ChatlogItem item)
        {
            var playerServerRegEx = new Regex(@"^\w+\b");
            var tokens = item.Tokens;
            var builder = new StringBuilder();
            for (int idx = 0; idx < tokens.Count; ++idx)
            {
                if (tokens[idx] is Memory.Chat.Token.TextToken txtToken)
                {
                    builder.Append(txtToken.GetText());
                }
                else if (tokens[idx] is Memory.Chat.Token.ServerDelimiterToken)
                {
                    builder.Append(" ");
                    idx += 1;
                    if (idx < tokens.Count && tokens[idx] is Memory.Chat.Token.TextToken txtToken1)
                    {
                        var txt = txtToken1.GetText();
                        var match = playerServerRegEx.Match(txt);
                        if (match.Success)
                        {
                            var serverName = match.Value;
                            builder.Append("[").Append(serverName).Append("]");
                            builder.Append(txt, serverName.Length, txt.Length - serverName.Length);
                        }
                        else
                        {
                            builder.Append(txt);
                        }
                    }
                }
                else if (tokens[idx] is Memory.Chat.Token.AutotranslateToken atToken)
                {
                    var key = atToken.GetKey();
                    var autotranslatetxt = AutotranslateProvider.GetTranslationFor(key);
                    if (autotranslatetxt != null)
                        builder.Append($" {autotranslatetxt} ");
                    else
                        builder.Append($" [AT {key}] ");
                }
                else if (tokens[idx] is Memory.Chat.Token.LinkToken linkToken)
                {
                    //skip for now
                }
            }

            return builder.ToString();
        }

        private string ExtractSource(string text, out int readIdx)
        {
            var regex = new Regex(SourceRegEx);
            var match = regex.Match(text);
            if (match.Success)
            {
                readIdx = match.Index + match.Length;
                return match.Groups[1].Value;
            }
            else
            {
                throw new System.ArgumentException(""); //TODO not useful
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
        private bool IsChatlogItemValid(Memory.Chat.ChatlogItem chatlogItem)
        {
            var tokens = chatlogItem.Tokens;
            if (tokens.Count == 0) return false;
            if (tokens[0] is Memory.Chat.Token.TextToken txtToken)
            {
                var txt = txtToken.GetText();
                if (txt == null || txt.Length == 0)
                    return false;
                return txt.StartsWith(InformationSeperator, System.StringComparison.InvariantCulture);
            }
            else
            {
                return false;
            }
        }
    }

    public sealed class CleanedChatlogItem
    {
        public System.DateTime Timestamp { get; }
        public ChatChannel Channel { get; }
        public string Source { get; }
        public string Message { get; }

        public CleanedChatlogItem(System.DateTime timestamp, ChatChannel channel, string source, string message)
        {
            Timestamp = timestamp;
            Channel = channel;
            Source = source ?? throw new System.ArgumentNullException(nameof(source));
            Message = message ?? throw new System.ArgumentNullException(nameof(message));
        }

        public override string ToString()
        {
            return $"{GetType().Name} => time:{Timestamp} | channel:{Channel} | source:'{Source}'  | msg:'{Message}'";
        }
    }
}