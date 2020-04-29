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

namespace Gobchat.Core.Chat
{
    public sealed class ChatMessagesWebEvent : global::Gobchat.UI.Web.JavascriptEvents.JSEvent
    {
        public readonly System.Collections.Generic.List<ChatMessage> messages;

        public ChatMessagesWebEvent(ChatMessage message) : base("ChatMessagesEvent")
        {
            if (messages == null)
                throw new System.ArgumentNullException(nameof(message));
            this.messages = new System.Collections.Generic.List<ChatMessage>();
            this.messages.Add(message);
        }

        public ChatMessagesWebEvent(System.Collections.Generic.IEnumerable<ChatMessage> messages) : base("ChatMessagesEvent")
        {
            if (messages == null)
                throw new System.ArgumentNullException(nameof(messages));
            this.messages = new System.Collections.Generic.List<ChatMessage>(messages);
        }
    }
}