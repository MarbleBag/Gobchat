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
using System.Linq;
using Gobchat.Core.Chat;
using Gobchat.Memory.Chat;
using Gobchat.Core.Util.Extension;
using System.Collections.Concurrent;
using Gobchat.Core.Util.Extension.Queue;

namespace Gobchat.Module.Chat.Internal
{
    internal sealed class ChatManager : IChatManager
    {
        private sealed class ChatMessageManagerConfig : IChatMessageManagerConfig
        {
            private readonly ChatManager _parent;

            public ChatMessageManagerConfig(ChatManager parent)
            {
                _parent = parent ?? throw new ArgumentNullException(nameof(parent));
            }

            public IAutotranslateProvider AutotranslateProvider
            {
                get => _parent._chatlogCleaner.AutotranslateProvider;
                set => _parent._chatlogCleaner.AutotranslateProvider = value ?? throw new ArgumentNullException(nameof(AutotranslateProvider));
            }

            public List<ChatChannel> VisibleChannels
            {
                get => _parent._visibleChannels.ToList();
                set => _parent._visibleChannels = value.ToArrayOrEmpty();
            }

            public List<ChatChannel> FormatChannels
            {
                get => _parent._chatMessageBuilder.FormatChannels;
                set => _parent._chatMessageBuilder.FormatChannels = value;
            }

            public List<FormatConfig> Formats
            {
                get => _parent._chatMessageBuilder.Formats;
                set => _parent._chatMessageBuilder.Formats = value;
            }

            public List<ChatChannel> MentionChannels
            {
                get => _parent._chatMessageBuilder.MentionChannels;
                set => _parent._chatMessageBuilder.MentionChannels = value;
            }

            public List<string> Mentions
            {
                get => _parent._chatMessageBuilder.Mentions;
                set => _parent._chatMessageBuilder.Mentions = value;
            }

            public bool DetecteEmoteInSayChannel
            {
                get => _parent._chatMessageBuilder.DetecteEmoteInSayChannel;
                set => _parent._chatMessageBuilder.DetecteEmoteInSayChannel = value;
            }
        }

        private const int MAX_NUMBER_OF_MESSAGES_PER_UPDATE = 10;

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly ConcurrentQueue<CleanedChatlogItem> _messageQueue = new ConcurrentQueue<CleanedChatlogItem>();

        private readonly ChatlogCleaner _chatlogCleaner;
        private readonly ChatMessageBuilder _chatMessageBuilder;

        private DateTime _lastDispatchedMessage;
        private TimeSpan _outdatedMessageFilter = TimeSpan.FromSeconds(10);

        public bool FilterOutdatedMessages { get; set; } = true;

        public TimeSpan OutdatedMessageFilter
        {
            get => _outdatedMessageFilter;
            set => _outdatedMessageFilter = value == null ? _outdatedMessageFilter : value;
        }

        public event EventHandler<ChatMessageEventArgs> OnChatMessage;

        public bool Enable { get; set; }

        public IChatMessageManagerConfig Config { get => _config; }
        private readonly ChatMessageManagerConfig _config;

        private ChatChannel[] _visibleChannels = Array.Empty<ChatChannel>();

        public ChatManager(IAutotranslateProvider autotranslateProvider)
        {
            _config = new ChatMessageManagerConfig(this);

            _chatlogCleaner = new ChatlogCleaner(autotranslateProvider);
            _chatMessageBuilder = new ChatMessageBuilder();

            _lastDispatchedMessage = DateTime.Now;
        }

        public void EnqueueMessage(ChatlogItem chatlogItem)
        {
            if (chatlogItem == null)
                throw new ArgumentNullException(nameof(chatlogItem));

            logger.Debug(() => "Memory Message: " + chatlogItem.ToString());

            try
            {
                var cleanedChatlogItem = _chatlogCleaner.Clean(chatlogItem);
                if (cleanedChatlogItem == null)
                {
                    logger.Info(() => "Invalid Message: " + chatlogItem.ToString());
                    return;
                }
                EnqueueMessage(cleanedChatlogItem);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error in chatlog processing. Log: {chatlogItem}");
            }
        }

        public void EnqueueMessage(DateTime timestamp, ChatChannel channel, string source, string message)
        {
            var cleanedChatlogItem = new CleanedChatlogItem(timestamp, channel, source, message);
            EnqueueMessage(cleanedChatlogItem);
        }

        public void EnqueueMessage(SystemMessageType type, string message)
        {
            ChatChannel channel = type == SystemMessageType.Error ? ChatChannel.GOBCHAT_ERROR : ChatChannel.GOBCHAT_INFO;
            EnqueueMessage(DateTime.Now, channel, "Gobchat", message);
        }

        private void EnqueueMessage(CleanedChatlogItem message)
        {
            _messageQueue.Enqueue(message);
        }

        private List<ChatMessage> PollMessages()
        {
            var result = new List<ChatMessage>();
            foreach (var message in _messageQueue.DequeueMultiple(MAX_NUMBER_OF_MESSAGES_PER_UPDATE))
            {
                if (!_visibleChannels.Contains(message.Channel))
                    continue;

                try
                {
                    var chatMessage = _chatMessageBuilder.BuildChatMessage(message);
                    _chatMessageBuilder.FormatChatMessage(chatMessage);
                    result.Add(chatMessage);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"Error in chatmessage creation. Message: {message}");
                }
            }
            return result;
        }

        public void UpdateManager()
        {
            if (!Enable || OnChatMessage == null)
                return;

            var chatMessages = PollMessages();
            var lastAcceptedTimstamp = _lastDispatchedMessage.Subtract(_outdatedMessageFilter);

            chatMessages = chatMessages.Where(msg =>
            {
                if (FilterOutdatedMessages && msg.Timestamp < lastAcceptedTimstamp)
                    return false;
                return true;
            }).ToList();

            _lastDispatchedMessage = chatMessages.Select(msg => msg.Timestamp).DefaultIfEmpty(_lastDispatchedMessage).Max();

            if (chatMessages.Count > 0)
                OnChatMessage?.Invoke(this, new ChatMessageEventArgs(chatMessages));
        }
    }
}