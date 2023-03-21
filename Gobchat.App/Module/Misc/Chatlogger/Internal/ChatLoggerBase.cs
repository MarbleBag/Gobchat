/*******************************************************************************
 * Copyright (C) 2019-2023 MarbleBag
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
        private bool _hasNonInternalMessage;

        protected string FileHandle { get; private set; }

        public IEnumerable<ChatChannel> LogChannels
        {
            get => _logChannels.ToArray();
            set => _logChannels = value.ToArrayOrEmpty();
        }

        public bool Active { get; set; }

        public string LogFolder { get; private set; }

        public ChatLoggerBase(string loggerId)
        {
            _loggerId = loggerId ?? throw new ArgumentNullException(nameof(loggerId));
        }

        abstract protected string FormatMessage(ChatMessage msg);

        virtual protected void OnFileChange() { }

        public void SetLogFolder(string folder)
        {
            if (folder == null || folder.Length == 0)
                throw new ArgumentNullException(nameof(folder));

            if (folder.Equals(LogFolder))
                return;

            lock (_synchronizationLock)
            {
                if (FileHandle != null)
                    Flush();

                FileHandle = null;
                LogFolder = folder;
            }
        }

        public void Log(ChatMessage message)
        {
            if (Active && _logChannels.Contains(message.Channel))
            {
                lock (_synchronizationLock)
                {
                    _pendingMessages.Enqueue(FormatMessage(message));
                    _hasNonInternalMessage = true;
                }
            }
        }



        private void CreateNewFile()
        {
            Directory.CreateDirectory(LogFolder);
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm", CultureInfo.InvariantCulture);
            var fileName = $"chatlog_{timestamp}.log";
            FileHandle = Path.Combine(LogFolder, fileName);
            WriteMessageToFile($"Chatlogger Id: {_loggerId}");
            OnFileChange();
        }

        public void Flush()
        {
            if (_pendingMessages.Count < 1)
                return;

            lock (_synchronizationLock)
            {
                if (FileHandle == null)
                {
                    if (!_hasNonInternalMessage)
                        return; //only create a new file if there is at least one non internal message!
                    CreateNewFile();
                }

                WriteMessagesToFile(_pendingMessages.DequeueAll());
                _hasNonInternalMessage = false;
            }
        }

        protected void LogMessage(string message)
        {
            lock (_synchronizationLock)
                _pendingMessages.Enqueue(message);
        }

        protected void WriteMessageToFile(string message)
        {
            if (message != null && message.Length > 0)
                WriteMessagesToFile(new string[] { message });
        }

        protected void WriteMessagesToFile(IEnumerable<string> messages)
        {
            if (messages != null)
                File.AppendAllLines(FileHandle, messages, System.Text.Encoding.UTF8);
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