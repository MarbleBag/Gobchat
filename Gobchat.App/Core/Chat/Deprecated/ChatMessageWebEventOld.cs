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

using System.Globalization;

namespace Gobchat.Core.Chat
{
    [System.Obsolete]
    public sealed class ChatMessageWebEventOld : global::Gobchat.UI.Web.JavascriptEvents.JSEvent
    {
        public readonly string timestamp;
        public readonly int type;
        public readonly string source;
        public readonly string message;

        public ChatMessageWebEventOld(ChatMessageOld message) : base("ChatMessageEvent")
        {
            if (message == null)
                throw new System.ArgumentNullException(nameof(message));

            this.timestamp = message.Timestamp.ToString("HH:mm", CultureInfo.InvariantCulture);
            this.type = message.MessageType;
            this.source = message.Source;
            this.message = message.Message;
        }
    }
}