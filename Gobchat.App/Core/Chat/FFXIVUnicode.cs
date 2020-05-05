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

using System.Globalization;
using System.Linq;

namespace Gobchat.Core.Chat
{
    public sealed class FFXIVUnicode
    {
        public string Symbol { get; }
        public int Value { get; }

        public FFXIVUnicode(int value)
        {
            Value = value;
            Symbol = char.ConvertFromUtf32(value);
        }

        public FFXIVUnicode(string unicodeString)
        {
            Symbol = unicodeString;
            Value = char.ConvertToUtf32(unicodeString, 0);
            //Value = int.Parse(string.Join(string.Empty, unicodeString.Select(e=>(int)e)), CultureInfo.InvariantCulture);
        }
    }
}