﻿/*******************************************************************************
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Gobchat.Core.Runtime;
using Gobchat.Core.Config;
using Gobchat.Core.Util.Extension.Queue;

namespace Gobchat.Core.Chat
{
    internal sealed class ChatMessageToFileLogger : IDisposable
    {
        private GobchatConfigManager _configManager;
        private Queue<ChatMessage> _pendingMessages = new Queue<ChatMessage>();
        private string _fileHandle;

        public ChatMessageToFileLogger(GobchatConfigManager configManager)
        {
            _configManager = configManager;
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
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm");
                var fileName = $"chatlog_{timestamp}.log";
                _fileHandle = Path.Combine(logFolder, fileName);
            }

            var logLines = _pendingMessages.DequeueAll().Select(e =>
            { // until a new chatlog cleaner is written, keep it compatible with https://github.com/MarbleBag/FF14-Chatlog-Cleaner
                return $"00|{e.Timestamp.ToString("o")}|{e.MessageType.ToString("x4")}|{e.Source}|{e.Message}|";
            });

            File.AppendAllLines(_fileHandle, logLines);
        }

        public void Log(ChatMessage message)
        {
            var doLog = _configManager.UserConfig.GetProperty<bool>("behaviour.writeChatLog");
            if (!doLog)
                return;

            var visibleChannels = _configManager.UserConfig.GetProperty<List<long>>("behaviour.channel.visible");
            var checkForValue = new Newtonsoft.Json.Linq.JValue((ChannelEnum)message.MessageType);
            // visibleChannels.Cast<Newtonsoft.Json.Linq.JValue>().Any(e => e.Value )

            if (visibleChannels.Contains(message.MessageType))//todo
                _pendingMessages.Enqueue(message);
        }
    }
}