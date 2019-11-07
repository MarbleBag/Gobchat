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

        public ChatlogItem Build(Sharlayan.Core.ChatLogItem item)
        {
            if (item == null)
                return null;

            if (!Int32.TryParse(item.Code, NumberStyles.HexNumber, CultureInfo.CurrentCulture, out int channel))
                return null; //TODO

            var segments = Tokenizer(item.Bytes, 8, item.Bytes.Length);

            return new ChatlogItem(item.TimeStamp, channel, segments);
        }

        public ChatlogItem Build()
        {
            var test = new byte[] { 58, 58, 2, 39, 7, 8, 1, 1, 1, 255, 1, 3, 2, 72, 4, 242, 1, 244, 3, 2, 73, 4, 242, 1, 245, 3, 238, 130, 187, 2, 73, 2, 1, 3, 2, 72, 2, 1, 3, 79, 102, 32, 116, 104, 101, 32, 49, 52, 32, 112, 97, 114, 116, 105, 101, 115, 32, 99, 117, 114, 114, 101, 110, 116, 108, 121, 32, 114, 101, 99, 114, 117, 105, 116, 105, 110, 103, 44, 32, 97, 108, 108, 32, 109, 97, 116, 99, 104, 32, 121, 111, 117, 114, 32, 115, 101, 97, 114, 99, 104, 32, 99, 111, 110, 100, 105, 116, 105, 111, 110, 115, 46, 2, 39, 7, 207, 1, 1, 1, 255, 1, 3 };

            var tokens = Tokenizer(test, 0, test.Length);

            return new ChatlogItem(DateTime.Now, 0, tokens);
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
                                var linkType = BitConverter.ToString(tokenData, 0, idx - 1).Replace("-", "");
                                var linkValue = BitConverter.ToString(tokenData, idx, tokenData.Length - idx - 1).Replace("-", "");
                                // Keeps: 0x0227 | (YY-)+ | (ZZ-)+
                                tokens.Add(new LinkToken(trigger, linkType, linkValue));
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

            if(DebugMode)
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