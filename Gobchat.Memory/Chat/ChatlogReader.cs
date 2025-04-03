/*******************************************************************************
 * Copyright (C) 2019-2025 MarbleBag
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

using NLog;
using Sharlayan;
using Sharlayan.Models.ReadResults;
using System.Collections.Generic;

namespace Gobchat.Memory.Chat
{
    internal sealed class ChatlogReader
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private int _previousArrayIndex = 0;
        private int _previousOffset = 0;
        private bool _chatlogException = false;

        public bool ChatLogAvailable { get { return Sharlayan.Reader.CanGetChatLog() && MemoryHandler.Instance.IsAttached; } }

        public ChatlogReader()
        {
        }

        private void Reset()
        {
            logger.Info("Reseting ChatLogReader array index");
            _previousArrayIndex = 0;
            _previousOffset = 0;
        }

        // ChatLogReaderException
        public List<Sharlayan.Core.ChatLogItem> Query()
        {
            _chatlogException = false;

            Sharlayan.MemoryHandler.Instance.ExceptionEvent += ResetChatlogProcessorOnException;
            ChatLogResult readResult = Reader.GetChatLog(_previousArrayIndex, _previousOffset);
            Sharlayan.MemoryHandler.Instance.ExceptionEvent -= ResetChatlogProcessorOnException;

            if (_chatlogException)
            {
                Reset();
            }
            else
            {
                _previousArrayIndex = readResult.PreviousArrayIndex;
                _previousOffset = readResult.PreviousOffset;
            }

            return readResult.ChatLogItems;
        }

        private void ResetChatlogProcessorOnException(object sender, Sharlayan.Events.ExceptionEvent evt)
        {
            if (evt.Exception is Sharlayan.Reader.ChatLogReaderException)
                _chatlogException = true;
        }
    }
}