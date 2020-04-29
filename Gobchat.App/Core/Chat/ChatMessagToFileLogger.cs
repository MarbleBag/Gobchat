/*******************************************************************************
 * Copyright (C) 2019.2020 MarbleBag
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
using System.Linq;
using System.IO;
using Gobchat.Core.Runtime;
using System.Globalization;
using Gobchat.Core.Util.Extension;
using Gobchat.Core.Util.Extension.Queue;

namespace Gobchat.Core.Chat
{
    public sealed class ChatMessageToFileLogger : IDisposable
    {
        private readonly Queue<ChatMessage> _pendingMessages = new Queue<ChatMessage>();
        private ChatChannel[] _logChannels = Array.Empty<ChatChannel>();
        private string _fileHandle;

        public List<ChatChannel> LogChannels
        {
            get => _logChannels.ToList();
            set => _logChannels = value.ToArrayOrEmpty();
        }

        public bool Active { get; set; }

        public ChatMessageToFileLogger()
        {
        }

        public void Dispose()
        {
            Flush();
        }

        public void Flush()
        {
            if (_pendingMessages.Count < 1)
                return;

            if (_fileHandle == null)
            {
                var logFolder = AbstractGobchatApplicationContext.UserLogLocation;
                Directory.CreateDirectory(logFolder);
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm", CultureInfo.InvariantCulture);
                var fileName = $"chatlog_{timestamp}.log";
                _fileHandle = Path.Combine(logFolder, fileName);
            }

            var logLines = _pendingMessages.DequeueAll().Select(e =>
            { // until a new chatlog cleaner is written, keep it compatible with https://github.com/MarbleBag/FF14-Chatlog-Cleaner
                return $"00|{e.Timestamp.ToString("o", CultureInfo.InvariantCulture)}|{((int)(e.Channel)).ToString("x4", CultureInfo.InvariantCulture)}|{e.Source}|{e.Message}|";
            });

            File.AppendAllLines(_fileHandle, logLines);
        }

        public void Log(ChatMessage message)
        {
            if (Active && _logChannels.Contains(message.Channel))
                _pendingMessages.Enqueue(message);
        }
    }
}