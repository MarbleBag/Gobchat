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

using Sharlayan;
using Sharlayan.Models.ReadResults;
using System.Collections.Generic;

namespace Gobchat.Memory.Chat
{
    internal class ChatlogReader
    {

        private int previousArrayIndex = 0;
        private int previousOffset = 0;

        public ChatlogReader()
        {

        }

        public List<Sharlayan.Core.ChatLogItem> Query()
        {
            ChatLogResult readResult = Reader.GetChatLog(previousArrayIndex, previousOffset);
            previousArrayIndex = readResult.PreviousArrayIndex;
            previousOffset = readResult.PreviousOffset;
            return readResult.ChatLogItems;
        }

    }

}
