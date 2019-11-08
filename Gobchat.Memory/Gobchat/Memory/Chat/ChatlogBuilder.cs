/*******************************************************************************
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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Gobchat.Memory.Chat
{
    internal class ChatlogBuilder
    {
        public bool DebugMode;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <exception cref="ChatBuildException">When an inner exception occures</exception>
        public ChatlogItem Build(Sharlayan.Core.ChatLogItem item)
        {
            if (item == null)
                return null;

            if (!Int32.TryParse(item.Code, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out int channel))
                return null; //TODO

            try
            {
                var segments = Tokenizer(item.Bytes, 8, item.Bytes.Length);
                return new ChatlogItem(item.TimeStamp, channel, segments);
            }
            catch (Exception e)
            {
                throw new ChatBuildException(e, item.Bytes, item.Line);
            }
        }


        private List<IChatlogToken> Tokenizer(byte[] data, int offset, int length)
        {
            List<IChatlogToken> tokens = new List<IChatlogToken>();
            int mark = offset;
            for (int index = mark; index < length; ++index)
            {
                if (0x02 == data[index]) //control character
                {
                    if (mark < index)
                    {
                        var tokenData = ExtractData(data, mark, index - mark);
                        tokens.Add(new TextToken(tokenData));
                    }

                    switch (data[index + 1])
                    {
                        case 0x01: // 0x0201XX -> not a control character, not clear what this one does
                            {
                                index += 2;
                                break;
                            }
                        case 0x2E: // 0x022EXX-(YY-)*-03 -> Autotranslate token
                            {
                                var tokenData = ExtractData(data, index + 2);
                                // Keeps: 0x022E | (YY-)*-03
                                tokens.Add(new AutotranslateToken(tokenData));
                                index += 2 + tokenData.Length;
                                break;
                            }
                        case 0x12: // 0x0212XX5903 -> Delimiter for server (it's the flower you can see in the chat)
                            {
                                var tokenData = ExtractData(data, index + 2); //should always be the same, but just in case it's not.
                                tokens.Add(new ServerDelimiterToken(tokenData));
                                index += 2 + tokenData.Length;
                                break;
                            }
                        case 0x27: // 0x0227XX -> seen around linked elements
                            {
                                // encloses a linkable element, like items, playername or other stuff that's clickable
                                // 0x02-27-XX-(YY-)+-FF-(ZZ-)+-03 pattern for this type of token
                                // XX Contains number of bytes to read afterwards
                                // YY a not closer defined number of bytes for type
                                // FF Used as a delimiter
                                // ZZ a not closer defined number of bytes for value
                                // 03 Indicates end of token

                                var trigger = BitConverter.ToString(data, index, 2).Replace("-", "");
                                var tokenData = ExtractData(data, index + 2);
                                var idx = Array.IndexOf<byte>(tokenData, 0xFF) + 1;

                                if (idx < 0)
                                { //sometimes the data doesn't contain a delimiter. Its not clear why and what it does.
                                  //TODO more research is needed
                                  //example: 	2, 39, 7, 3, 242, 111, 203, 2, 1, 3
                                    var linkType = BitConverter.ToString(tokenData).Replace("-", "");
                                    tokens.Add(new UnknownLinkToken(trigger, linkType));
                                }
                                else
                                {
                                    var linkType = BitConverter.ToString(tokenData, 0, idx - 1).Replace("-", "");
                                    var linkValue = BitConverter.ToString(tokenData, idx, tokenData.Length - idx - 1).Replace("-", "");
                                    // Keeps: 0x0227 | (YY-)+ | (ZZ-)+
                                    tokens.Add(new LinkToken(trigger, linkType, linkValue));
                                }

                                index += 2 + tokenData.Length;
                                break;
                            }
                        case 0x49: // 0x0249XX -> seen at localized server messages
                        default: // 0x02UU-XX-(YY-)*-03
                            {
                                var trigger = BitConverter.ToString(data, index, 2).Replace("-", "");
                                var tokenData = ExtractData(data, index + 2);
                                // Keeps: 0x02UU | (YY-)*-03
                                tokens.Add(new UnknownToken(trigger, tokenData));
                                index += 2 + tokenData.Length;
                                break;
                            }
                    }
                    mark = index + 1;
                }
            }

            if (mark < length)
            {
                var tokenData = ExtractData(data, mark, length - mark);
                tokens.Add(new TextToken(tokenData));
            }

            if (DebugMode)
                tokens.Add(new TextToken(data.Skip(offset).Take(length).ToArray()));

            return tokens;
        }

        private byte[] ExtractData(byte[] src, int index)
        {
            var length = src[index] & 0xFF;
            return ExtractData(src, index + 1, length);
        }

        private byte[] ExtractData(byte[] src, int index, int length)
        {
            var copy = new byte[length];
            Array.Copy(src, index, copy, 0, length);
            return copy;
        }
    }
}