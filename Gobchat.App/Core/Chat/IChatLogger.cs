﻿/*******************************************************************************
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

namespace Gobchat.Core.Chat
{
    public interface IChatLogger : IDisposable
    {
        IEnumerable<ChatChannel> LogChannels { get; set; }
        bool Active { get; set; }

        string LogFolder { get; set; }

        void Log(ChatMessage message);

        void Flush();
    }
}