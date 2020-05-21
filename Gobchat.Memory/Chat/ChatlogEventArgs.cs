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
    [ObsoleteAttribute]
    public class ChatlogEventArgs : System.EventArgs
    {
        public List<ChatlogItem> ChatlogItems { get; }

        public ChatlogEventArgs(List<ChatlogItem> chatlogItems)
        {
            ChatlogItems = chatlogItems ?? throw new ArgumentNullException(nameof(chatlogItems));
        }
    }
}