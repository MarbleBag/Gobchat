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

namespace Gobchat.Memory.Chat
{
    public class ChatlogItem
    {
        public DateTime TimeStamp { get; }
        public int Channel { get; }
        public List<IChatlogToken> Tokens { get; }

        public ChatlogItem(DateTime timeStamp, int channel, List<IChatlogToken> tokens)
        {
            if (timeStamp == null)
                throw new ArgumentNullException(nameof(timeStamp));
            TimeStamp = timeStamp;
            Channel = channel;
            Tokens = tokens;
        }

        public override string ToString()
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            Tokens.ForEach(item => builder.Append(item).Append(", "));
            return $"{TimeStamp} [{Channel}] | {builder.ToString()}";
        }
    }

    public interface IChatlogToken
    {

    }

    public class TextToken : IChatlogToken
    {
        public byte[] Data { get; }

        public TextToken(byte[] data)
        {
            this.Data = data;
        }

        public string GetText()
        {
            return System.Text.Encoding.UTF8.GetString(Data);
        }

        public override string ToString()
        {
            return $"Text[{GetText()}]";
        }
    }

    public class AutotranslateToken : IChatlogToken
    {
        public byte[] Code { get; }

        public AutotranslateToken(byte[] code)
        {
            this.Code = code;
        }

        public string GetKey()
        {
            return BitConverter.ToString(Code).Replace("-", "");
        }

        public override string ToString()
        {
            return $"Autotranslate[key={GetKey()}]";
        }
    }

    public class LinkToken : IChatlogToken
    {
        public string Trigger { get; }
        public string LinkType { get; }
        // CF010101 - seems to indicate the 'end' of the link
        // 01010101 - Used for players in the send part / or log in - no clue
        // 01014301 - Used for the first player in animated emote
        // 01014302 - Used for the second player in animated emote
        // 010143XX - Seems to be general player link, with XX as increasing counter for multiple players, followed by player id (doesn't change for same player)
        // 010F2501 - Player link in CWLS 1
        // 01104401 - Player link in LS 4
        // 08010101 - Party search notification link
        // 03F26FD80201 - Item link (from own inventory)

        public string LinkValue { get; }

        public LinkToken(string trigger, string linkType, string linkValue)
        {
            Trigger = trigger;
            LinkType = linkType;
            LinkValue = linkValue;
        }

        public override string ToString()
        {
            return $"LinkToken[0x{Trigger} | 0x{LinkType} -> 0x{LinkValue}]";
        }
    }

    public class ServerDelimiterToken : IChatlogToken
    {
        private byte[] Code { get; }

        public ServerDelimiterToken(byte[] code)
        {
            this.Code = code;
        }

        public override string ToString()
        {
            return $"ServerDelimiter[0x{BitConverter.ToString(Code).Replace("-", "")}]";
        }
    }

    public class UnknownToken : IChatlogToken
    {
        public string Trigger { get; }
        public byte[] Code { get; }

        public UnknownToken(string trigger, byte[] code)
        {
            Trigger = trigger;
            Code = code;
        }

        public override string ToString()
        {
            return $"Unknown[0x{Trigger} -> 0x{BitConverter.ToString(Code).Replace("-", "")}]";
        }
    }
}