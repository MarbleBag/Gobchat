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
    public class UnknownLinkToken : IChatlogToken
    {
        public string Trigger { get; }
        public string LinkType { get; }

        public UnknownLinkToken(string trigger, string linkType)
        {
            Trigger = trigger;
            LinkType = linkType;
        }

        public override string ToString()
        {
            return $"{nameof(this.GetType)}[0x{Trigger} | 0x{LinkType}]";
        }
    }

}