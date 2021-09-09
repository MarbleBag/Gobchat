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
using System.Globalization;
using Gobchat.Core.Chat;
using Gobchat.Core.Util.Extension;
using Gobchat.Core.Util.Extension.Queue;


namespace Gobchat.Module.Misc.Chatlogger.Internal
{
    public abstract class ChatLoggerBase : IChatLogger
    {
        private readonly Queue<string> _pendingMessages = new Queue<string>();

        protected readonly string _loggerId;
        protected readonly object _synchronizationLock = new object();

        private ChatChannel[] _logChannels = Array.Empty<ChatChannel>();
        private string _fileHandle;
        private bool _containsChatLogs;

        public ChatLoggerBase(string loggerId)
        {
            _loggerId = loggerId ?? throw new ArgumentNullException(nameof(loggerId));
        }

        public IEnumerable<ChatChannel> LogChannels
        {
            get => _logChannels.ToArray();
            set => _logChannels = value.ToArrayOrEmpty();
        }

        public bool Active { get; set; }

        public string LogFolder { get; private set; }

        public ChatLoggerBase()
        {
        }

        abstract protected string FormatLine(ChatMessage msg);

        public void SetLogFolder(string folder)
        {
            if (folder == null || folder.Length == 0)
                throw new ArgumentNullException(nameof(folder));

            if (folder.Equals(LogFolder))
                return;

            lock (_synchronizationLock)
            {
                if (_fileHandle != null)
                    Flush();

                _fileHandle = null;
                LogFolder = folder;
            }
        }

        public void Log(ChatMessage message)
        {
            if (Active && _logChannels.Contains(message.Channel))
            {
                lock (_synchronizationLock)
                {
                    _pendingMessages.Enqueue(FormatLine(message));
                    _containsChatLogs = true;
                }
            }
        }

        protected void InternalLog(string message)
        {
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
                    if (!_containsChatLogs)
                        return; //only create a file if there is at least one chat log!

                    Directory.CreateDirectory(LogFolder);
                    var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm", CultureInfo.InvariantCulture);
                    var fileName = $"chatlog_{timestamp}.log";
                    _fileHandle = Path.Combine(LogFolder, fileName);

                    if (!File.Exists(_fileHandle))                    
                        WriteLineToFile($"Chatlogger Id: {_loggerId}");                    
                }

                WriteLinesToFile(_pendingMessages.DequeueAll());
                _containsChatLogs = false;
            }
        }

        private void WriteLineToFile(string line)
        {
            if (line != null && line.Length > 0)
                WriteLinesToFile(new string[] { line });
        }

        private void WriteLinesToFile(IEnumerable<string> lines)
        {
            if (lines != null)
                File.AppendAllLines(_fileHandle, lines, System.Text.Encoding.UTF8);
        }

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