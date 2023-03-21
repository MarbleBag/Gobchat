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
using Gobchat.Core.Chat;
using Gobchat.Memory.Chat;
using System.Collections.Concurrent;
using Gobchat.Core.Util.Extension.Queue;
using Gobchat.Module.Actor;

namespace Gobchat.Module.Chat.Internal
{
    internal sealed partial class ChatManager : IChatManager
    {
        private const int MAX_NUMBER_OF_MESSAGES_PER_UPDATE = int.MaxValue;
        private const string APP_MESSAGE_SOURCE = "Gobchat"; //TODO automate that

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly ConcurrentQueue<ChatMessage> _messageQueue = new ConcurrentQueue<ChatMessage>();

        private readonly ChatlogCleaner _chatlogCleaner;
        private readonly ChatMessageBuilder _chatMessageBuilder;
        private readonly ChatMessageActorDataSetter _chatMessageActorData;
        private readonly ChatMessageTriggerGroupSetter _chatMessageTriggerGroups;

        private DateTime _lastDispatchedMessage;
        private TimeSpan _outdatedMessageFilter = TimeSpan.FromSeconds(10);

        private ChatChannel[] _visibleChannels = Array.Empty<ChatChannel>();

        public bool FilterOutdatedMessages { get; set; } = false;

        public TimeSpan OutdatedMessageFilter
        {
            get => _outdatedMessageFilter;
            set => _outdatedMessageFilter = value == null ? _outdatedMessageFilter : value;
        }

        public event EventHandler<ChatMessageEventArgs> OnChatMessage;

        public bool Enable { get; set; }

        public IChatManagerConfig Config { get => _config; }
        private readonly ChatManagerConfig _config;

        public ChatManager(IAutotranslateProvider autotranslateProvider, IActorManager actorManager)
        {
            _config = new ChatManagerConfig(this);

            _chatlogCleaner = new ChatlogCleaner(autotranslateProvider);
            _chatMessageBuilder = new ChatMessageBuilder();
            _chatMessageActorData = new ChatMessageActorDataSetter(actorManager);
            _chatMessageTriggerGroups = new ChatMessageTriggerGroupSetter();

            _lastDispatchedMessage = DateTime.Now.AddYears(-1);
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

        private void EnqueueMessage(CleanedChatlogItem message)
        {
            EnqueueMessage(message.Timestamp, message.Channel, message.Source, message.Message);
        }

        public void EnqueueMessage(DateTime timestamp, FFXIVChatChannel channel, string source, string message)
        {
            var channelData = GobchatChannelMapping.GetChannel(channel);
            EnqueueMessage(timestamp, channelData.ChatChannel, source, message);
        }

        public void EnqueueMessage(SystemMessageType type, string message)
        {
            var channel = type == SystemMessageType.Error ? ChatChannel.GobchatError : ChatChannel.GobchatInfo;
            EnqueueMessage(DateTime.Now, channel, APP_MESSAGE_SOURCE, message);
        }

        public void EnqueueMessage(DateTime timestamp, ChatChannel channel, string source, string message)
        {
            try
            {
                var chatMessage = _chatMessageBuilder.BuildChatMessage(timestamp, channel, source, message);
                chatMessage.Source.IsApp = APP_MESSAGE_SOURCE.Equals(source); //not the best solution
                _messageQueue.Enqueue(chatMessage);
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Error in chatmessage creation. Message: {message}");
            }
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
                    logger.Debug(() => $"Preprocess message: {message}");
                    _chatMessageActorData.SetActorData(message);
                    _chatMessageTriggerGroups.SetTriggerGroup(message);
                    _chatMessageBuilder.FormatChatMessage(message);
                    logger.Debug(() => $"Postprocess message: {message}");
                    result.Add(message);
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

            if (FilterOutdatedMessages)
            {
                var lastAcceptedTimstamp = _lastDispatchedMessage.Subtract(_outdatedMessageFilter);
                chatMessages = chatMessages.Where(msg => lastAcceptedTimstamp < msg.Timestamp).ToList();
                _lastDispatchedMessage = chatMessages
                    .Where(msg => !msg.Source.IsApp)
                    .Select(msg => msg.Timestamp)
                    .DefaultIfEmpty(_lastDispatchedMessage)
                    .Max();
            }

            if (chatMessages.Count > 0)
                OnChatMessage?.Invoke(this, new ChatMessageEventArgs(chatMessages));
        }
    }
}