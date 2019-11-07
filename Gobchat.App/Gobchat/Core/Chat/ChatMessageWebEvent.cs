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

namespace Gobchat.Core.Chat
{
    internal class ChatMessageWebEvent : UI.Web.JavascriptEvents.JSEvent
    {
        public string timestamp;
        public int type;
        public string source;
        public string message;

        public ChatMessageWebEvent(ChatMessage message) : base("ChatMessageEvent")
        {
            this.timestamp = message.Timestamp.ToString("HH:mm");
            this.type = message.MessageType;
            this.source = message.Source;
            this.message = message.Message;
        }
    }
}
