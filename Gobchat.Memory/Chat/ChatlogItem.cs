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

    
}