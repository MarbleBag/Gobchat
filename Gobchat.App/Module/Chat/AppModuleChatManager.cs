﻿/*******************************************************************************
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

using System;
using System.Collections.Generic;
using System.Linq;
using Gobchat.Core.Chat;
using Gobchat.Core.Config;
using Gobchat.Core.Runtime;
using System.Threading;
using Gobchat.Module.Chat.Internal;
using Newtonsoft.Json.Linq;
using Gobchat.Module.Actor;
using Gobchat.Module.MemoryReader;
using Gobchat.Module.Language;

namespace Gobchat.Module.Chat
{
    public sealed class AppModuleChatManager : IApplicationModule
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private IDIContext _container;
        private IConfigManager _configManager;
        private IMemoryReaderManager _memoryManager;
        private ChatManager _chatManager;

        private IndependendBackgroundWorker _updater;

        private long _updateInterval;

        /// <summary>
        ///
        /// Requires: <see cref="IGobchatConfig"/> <br></br>
        /// Requires: <see cref="IMemoryReaderManager"/> <br></br>
        /// Requires: <see cref="IActorManager"/> <br></br>
        /// Provides: <see cref="IChatManager"/> <br></br>
        /// <br></br>
        /// </summary>
        public AppModuleChatManager()
        {
        }

        public void Initialize(ApplicationStartupHandler handler, IDIContext container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _configManager = _container.Resolve<IConfigManager>();
            _memoryManager = _container.Resolve<IMemoryReaderManager>();
            var actorManager = _container.Resolve<IActorManager>();

            var resourceBundle = _container.Resolve<ILocaleManager>().GetResourceBundle("autotranslate");
            var autotranslateProvider = new AutotranslateProvider(resourceBundle);

            _chatManager = new ChatManager(autotranslateProvider, actorManager);

            _configManager.AddPropertyChangeListener("behaviour.chat.updateInterval", true, true, ConfigManager_UpdateChatInterval);
            _configManager.AddPropertyChangeListener("behaviour.channel", true, true, ConfigManager_UpdateChannelProperties);
            _configManager.AddPropertyChangeListener("behaviour.segment", true, true, ConfigManager_UpdateFormaterProperties);
            _configManager.AddPropertyChangeListener("behaviour.groups", true, true, ConfigManager_UpdateTriggerGroupProperties);
            _configManager.AddPropertyChangeListener("behaviour.chat.autodetectEmoteInSay", true, true, ConfigManager_UpdateAutodetectProperties);
            _configManager.AddPropertyChangeListener("behaviour.language", true, true, ConfigManager_UpdateLanguage);
            _configManager.AddPropertyChangeListener("behaviour.rangefilter", true, true, ConfigManager_UpdateRangeFilter);

            _configManager.AddPropertyChangeListener("behaviour.mentions.trigger", true, true, ConfigManager_UpdateMentions);
            _configManager.AddPropertyChangeListener("behaviour.mentions.userCanTriggerMention", true, true, ConfigManager_UpdateUserMentionProperties);

            _configManager.AddPropertyChangeListener("behaviour.chattabs.data", true, true, ConfigManager_UpdateVisibleChannel);
            _configManager.AddPropertyChangeListener("behaviour.chattabs.data", true, true, ConfigManager_UpdateUpdateRangeFilterActive);

            _container.Register<IChatManager>((c, p) => _chatManager);

            _updater = new IndependendBackgroundWorker();
            _updater.Start(UpdateJob);
        }

        public void Dispose()
        {
            _configManager.RemovePropertyChangeListener(ConfigManager_UpdateUpdateRangeFilterActive);
            _configManager.RemovePropertyChangeListener(ConfigManager_UpdateVisibleChannel);

            _configManager.RemovePropertyChangeListener(ConfigManager_UpdateUserMentionProperties);
            _configManager.RemovePropertyChangeListener(ConfigManager_UpdateRangeFilter);
            _configManager.RemovePropertyChangeListener(ConfigManager_UpdateLanguage);
            _configManager.RemovePropertyChangeListener(ConfigManager_UpdateAutodetectProperties);
            _configManager.RemovePropertyChangeListener(ConfigManager_UpdateMentions);
            _configManager.RemovePropertyChangeListener(ConfigManager_UpdateTriggerGroupProperties);
            _configManager.RemovePropertyChangeListener(ConfigManager_UpdateFormaterProperties);
            _configManager.RemovePropertyChangeListener(ConfigManager_UpdateChannelProperties);
            _configManager.RemovePropertyChangeListener(ConfigManager_UpdateChatInterval);
            
            _updater.Dispose();

            _updater = null;
            _chatManager = null;
            _container = null;
            _configManager = null;
            _memoryManager = null;
        }

        private void UpdateJob(CancellationToken cancellationToken)
        {
            //TODO some start up logging
            try
            {
                var timer = new System.Diagnostics.Stopwatch();
                while (!cancellationToken.IsCancellationRequested)
                {
                    timer.Restart();

                    UpdateChatManager();

                    timer.Stop();
                    var timeSpend = timer.Elapsed;

                    if (cancellationToken.IsCancellationRequested)
                        break;

                    int waitTime = (int)Math.Max(0, _updateInterval - timeSpend.Milliseconds);
                    if (waitTime > 0)
                        Thread.Sleep(waitTime);
                }
            }
            finally
            {
                logger.Info("Chat updates concluded");
            }
        }

        private void UpdateChatManager()
        {
            try
            {
                if (_memoryManager.IsConnected)
                {
                    var chatlogs = _memoryManager.GetNewestChatlog();
                    foreach (var chatlog in chatlogs)
                        _chatManager.EnqueueMessage(chatlog);
                }

                _chatManager.UpdateManager();
            }
            catch (Exception e1)
            {
                logger.Error(e1);
                throw;
            }
        }

        private void ConfigManager_UpdateChatInterval(IConfigManager config, ProfilePropertyChangedCollectionEventArgs evt)
        {
            try
            {
                _updateInterval = config.GetProperty<long>("behaviour.chat.updateInterval");
            }
            catch (Exception e1)
            {
                logger.Error(e1);
                throw;
            }
        }

        private void ConfigManager_UpdateChannelProperties(IConfigManager config, ProfilePropertyChangedCollectionEventArgs evt)
        {
            try
            {
                // _chatManager.Config.VisibleChannels = config.GetProperty<List<long>>("behaviour.channel.visible").Select(i => (ChatChannel)i).ToArray();
                _chatManager.Config.FormatChannels = config.GetProperty<List<long>>("behaviour.channel.roleplay").Select(i => (ChatChannel)i).ToArray();
                _chatManager.Config.MentionChannels = config.GetProperty<List<long>>("behaviour.channel.mention").Select(i => (ChatChannel)i).ToArray();
                _chatManager.Config.CutOffChannels = config.GetProperty<List<long>>("behaviour.channel.rangefilter").Select(i => (ChatChannel)i).ToArray();
            }
            catch (Exception e1)
            {
                logger.Error(e1);
                throw;
            }
        }

        private void ConfigManager_UpdateVisibleChannel(IConfigManager config, ProfilePropertyChangedCollectionEventArgs evt)
        {
            try
            {
                var jTabs = GetVisibleChatTabs(config);
                var visibleChannels = jTabs
                    .Select(tab => tab["channel"]["visible"].ToObject<List<long>>())
                    .Select(channel => channel.Select(i => (ChatChannel)i))
                    .SelectMany(channel => channel)
                    .ToArray();

                _chatManager.Config.VisibleChannels = visibleChannels;
            }
            catch (Exception e1)
            {
                logger.Error(e1);
                throw;
            }
        }

        private void ConfigManager_UpdateUpdateRangeFilterActive(IConfigManager config, ProfilePropertyChangedCollectionEventArgs evt)
        {
            try
            {
                var jTabs = GetVisibleChatTabs(config);
                var activateRangeFilter = jTabs.Any(tab => tab["formatting"]["rangefilter"].ToObject<bool>());
                _chatManager.Config.EnableCutOff = activateRangeFilter;
            }
            catch (Exception e1)
            {
                logger.Error(e1);
                throw;
            }
        }

        private static List<JToken> GetVisibleChatTabs(IConfigManager config)
        {
            return config.GetProperty<JObject>("behaviour.chattabs.data")
               .Properties()
               .Select(p => p.Value)
               .Where(tab => tab.Value<bool>("visible"))
               .ToList();
        }

        private void ConfigManager_UpdateAutodetectProperties(IConfigManager config, ProfilePropertyChangedCollectionEventArgs evt)
        {
            try
            {
                _chatManager.Config.DetecteEmoteInSayChannel = config.GetProperty<bool>("behaviour.chat.autodetectEmoteInSay");
            }
            catch (Exception e1)
            {
                logger.Error(e1);
                throw;
            }
        }

        private void ConfigManager_UpdateUserMentionProperties(IConfigManager config, ProfilePropertyChangedCollectionEventArgs evt)
        {
            try
            {
                _chatManager.Config.ExcludeUserMention = !config.GetProperty<bool>("behaviour.mentions.userCanTriggerMention");
            }
            catch (Exception e1)
            {
                logger.Error(e1);
                throw;
            }
        }

        private void ConfigManager_UpdateFormaterProperties(IConfigManager config, ProfilePropertyChangedCollectionEventArgs evt)
        {
            try
            {
                var ids = config.GetProperty<List<string>>("behaviour.segment.order");
                var list = config.GetProperty<JToken>("behaviour.segment.data");
                var newValues = new List<FormatConfig>();
                foreach (var id in ids)
                {
                    var data = list[id];
                    var format = data.ToObject<FormatConfig>();
                    newValues.Add(format);
                }
                _chatManager.Config.Formats = newValues.ToArray();
            }
            catch (Exception e1)
            {
                logger.Error(e1);
                throw;
            }
        }

        private void ConfigManager_UpdateTriggerGroupProperties(IConfigManager config, ProfilePropertyChangedCollectionEventArgs evt)
        {
            try
            {
                var ids = config.GetProperty<List<string>>("behaviour.groups.sorting");
                var list = config.GetProperty<JToken>("behaviour.groups.data");
                var newValues = new List<TriggerGroup>();
                foreach (var id in ids)
                {
                    var data = list[id];
                    var format = data.ToObject<TriggerGroup>();
                    newValues.Add(format);
                }
                _chatManager.Config.TriggerGroups = newValues.ToArray();
            }
            catch (Exception e1)
            {
                logger.Error(e1);
                throw;
            }
        }

        private void ConfigManager_UpdateMentions(IConfigManager config, ProfilePropertyChangedCollectionEventArgs evt)
        {
            try
            {
                var triggers = config.GetProperty<List<string>>("behaviour.mentions.trigger");
                var newMentions = triggers.ToArray();
                logger.Debug(() => $"Set mentions to: {string.Join(", ", newMentions)}");
                _chatManager.Config.Mentions = newMentions;
            }
            catch (Exception e1)
            {
                logger.Error(e1);
                throw;
            }
        }

        private void ConfigManager_UpdateLanguage(IConfigManager config, ProfilePropertyChangedCollectionEventArgs evt)
        {
            try
            {
                var selectedLanguage = config.GetProperty<string>("behaviour.language");
                var autotranslateProvider = _chatManager.Config.AutotranslateProvider as AutotranslateProvider;
                autotranslateProvider?.SetLocale(selectedLanguage);
            }
            catch (Exception e1)
            {
                logger.Error(e1);
                throw;
            }
        }

        private void ConfigManager_UpdateRangeFilter(IConfigManager config, ProfilePropertyChangedCollectionEventArgs evt)
        {
            try
            {
                //_chatManager.Config.EnableCutOff = config.GetProperty<bool>("behaviour.rangefilter.active");
                _chatManager.Config.CutOffDistance = config.GetProperty<long>("behaviour.rangefilter.cutoff");
                _chatManager.Config.FadeOutDistance = config.GetProperty<long>("behaviour.rangefilter.fadeout");
            }
            catch (Exception e1)
            {
                logger.Error(e1);
                throw;
            }
        }
    }
}