/*******************************************************************************
 * Copyright (C) 2019-2021 MarbleBag
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
    public abstract class ChatLoggerBase : IChatLogger
    {
        private readonly Queue<ChatMessage> _pendingMessages = new Queue<ChatMessage>();
        private ChatChannel[] _logChannels = Array.Empty<ChatChannel>();
        private string _logFolder;

        private string _fileHandle;

        protected readonly object _synchronizationLock = new object();

        abstract protected string LoggerId { get; }

        public IEnumerable<ChatChannel> LogChannels
        {
            get => _logChannels.ToArray();
            set => _logChannels = value.ToArrayOrEmpty();
        }

        public bool Active { get; set; }

        public string LogFolder
        {
            get => _logFolder;
            set
            {
                if (value == null || value.Length == 0)
                    throw new ArgumentNullException(nameof(LogFolder));

                if (value.Equals(_logFolder))
                    return;

                lock (_synchronizationLock)
                {
                    if (_fileHandle != null)
                        Flush();

                    _fileHandle = null;
                    _logFolder = value;
                }
            }
        }

        public ChatLoggerBase()
        {
        }

        public void Log(ChatMessage message)
        {
            if (Active && _logChannels.Contains(message.Channel))
                lock (_synchronizationLock)
                    _pendingMessages.Enqueue(message);
        }

        public void Flush()
        {
            if (_pendingMessages.Count < 1)
                return;

            lock (_synchronizationLock)
            {
                if (_fileHandle == null)
                {
                    Directory.CreateDirectory(_logFolder);
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm", CultureInfo.InvariantCulture);
                    var fileName = $"chatlog_{timestamp}.log";
                    _fileHandle = Path.Combine(_logFolder, fileName);

                    if (!File.Exists(_fileHandle))
                        File.AppendAllLines(_fileHandle, new string[] { $"Chatlogger Id: {LoggerId}" }, System.Text.Encoding.UTF8);
                }

                var logLines = _pendingMessages.DequeueAll().Select(e => FormatLine(e));
                File.AppendAllLines(_fileHandle, logLines, System.Text.Encoding.UTF8);
            }
        }

        abstract protected string FormatLine(ChatMessage msg);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool v)
        {
            Flush();
        }
    }
}