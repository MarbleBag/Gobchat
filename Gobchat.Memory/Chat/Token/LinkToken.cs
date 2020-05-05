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

namespace Gobchat.Memory.Chat.Token
{
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

}