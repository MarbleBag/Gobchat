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
using Gobchat.Core.Chat;
using Gobchat.Core.Config;
using Gobchat.Memory.Chat;

namespace Gobchat.Module.Chat
{
    public interface IChatManager
    {
        event EventHandler<ChatMessageEventArgs> OnChatMessage;

        bool Enable { get; set; }

        void EnqueueMessage(ChatlogItem chatlogItem);

        void EnqueueMessage(DateTime timestamp, ChatChannel channel, string source, string message);

        void EnqueueMessage(SystemMessageType type, string message);

        void UpdateManager();
    }
}