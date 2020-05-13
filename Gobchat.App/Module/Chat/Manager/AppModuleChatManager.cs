/*******************************************************************************
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
using Gobchat.Core.Config;
using Gobchat.Core.Runtime;
using Gobchat.Memory;
using System.Threading;
using Gobchat.Core.Resource;
using System.Globalization;
using Gobchat.Module.Chat.Internal;
using Newtonsoft.Json.Linq;
using Gobchat.Module.Actor;

namespace Gobchat.Module.Chat
{
    public sealed class AppModuleChatManager : IApplicationModule
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private IDIContext _container;
        private IConfigManager _configManager;

        private FFXIVMemoryReader _memoryReader;
        private ChatManager _chatManager;

        private IndependendBackgroundWorker _updater;

        private long _updateInterval;

        /// <summary>
        ///
        /// Requires: <see cref="IGobchatConfig"/> <br></br>
        /// Requires: <see cref="FFXIVMemoryReader"/> <br></br>
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
            _memoryReader = _container.Resolve<FFXIVMemoryReader>();
            var actorManager = _container.Resolve<IActorManager>();

            var languagePath = System.IO.Path.Combine(GobchatContext.ResourceLocation, @"lang");
            var resourceResolvers = new IResourceLocator[] { new LocalFolderResourceResolver(languagePath) };
            var autotranslateProvider = new AutotranslateProvider(resourceResolvers, "autotranslate", new CultureInfo("en"));

            _chatManager = new ChatManager(autotranslateProvider, actorManager);

            _configManager.AddPropertyChangeListener("behaviour.channel", true, true, ConfigManager_UpdateChannelProperties);
            _configManager.AddPropertyChangeListener("behaviour.autodetectEmoteInSay", true, true, ConfigManager_UpdateAutodetectProperties);
            _configManager.AddPropertyChangeListener("behaviour.segment", true, true, ConfigManager_UpdateFormaterProperties);
            _configManager.AddPropertyChangeListener("behaviour.mentions", true, true, ConfigManager_UpdateMentions);
            _configManager.AddPropertyChangeListener("behaviour.chatUpdateInterval", true, true, ConfigManager_UpdateChatInterval);
            _configManager.AddPropertyChangeListener("behaviour.language", true, true, ConfigManager_UpdateLanguage);
            _configManager.AddPropertyChangeListener("behaviour.fadeout", true, true, ConfigManager_UpdateRangeFilter);

            _container.Register<IChatManager>((c, p) => _chatManager);

            _updater = new IndependendBackgroundWorker();
            _updater.Start(UpdateJob);
        }

        public void Dispose()
        {
            _configManager.RemovePropertyChangeListener(ConfigManager_UpdateChannelProperties);
            _configManager.RemovePropertyChangeListener(ConfigManager_UpdateAutodetectProperties);
            _configManager.RemovePropertyChangeListener(ConfigManager_UpdateFormaterProperties);
            _configManager.RemovePropertyChangeListener(ConfigManager_UpdateChatInterval);
            _configManager.RemovePropertyChangeListener(ConfigManager_UpdateLanguage);
            _configManager.RemovePropertyChangeListener(ConfigManager_UpdateRangeFilter);

            _updater.Dispose();

            _updater = null;
            _chatManager = null;
            _container = null;
            _configManager = null;
            _memoryReader = null;
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
            if (_memoryReader.FFXIVProcessValid)
            {
                var chatlogs = _memoryReader.GetNewestChatlog();
                foreach (var chatlog in chatlogs)
                    _chatManager.EnqueueMessage(chatlog);
            }

            _chatManager.UpdateManager();
        }

        private void ConfigManager_UpdateChatInterval(IConfigManager config, ProfilePropertyChangedCollectionEventArgs evt)
        {
            _updateInterval = config.GetProperty<long>("behaviour.chatUpdateInterval");
        }

        private void ConfigManager_UpdateChannelProperties(IConfigManager config, ProfilePropertyChangedCollectionEventArgs evt)
        {
            _chatManager.Config.VisibleChannels = config.GetProperty<List<long>>("behaviour.channel.visible").Select(i => (ChatChannel)i).ToArray();
            _chatManager.Config.FormatChannels = config.GetProperty<List<long>>("behaviour.channel.roleplay").Select(i => (ChatChannel)i).ToArray();
            _chatManager.Config.MentionChannels = config.GetProperty<List<long>>("behaviour.channel.mention").Select(i => (ChatChannel)i).ToArray();
            _chatManager.Config.CutOffChannels = config.GetProperty<List<long>>("behaviour.channel.fadeout").Select(i => (ChatChannel)i).ToArray();
        }

        private void ConfigManager_UpdateAutodetectProperties(IConfigManager config, ProfilePropertyChangedCollectionEventArgs evt)
        {
            _chatManager.Config.DetecteEmoteInSayChannel = config.GetProperty<bool>("behaviour.autodetectEmoteInSay");
        }

        private void ConfigManager_UpdateFormaterProperties(IConfigManager config, ProfilePropertyChangedCollectionEventArgs evt)
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

        private void ConfigManager_UpdateMentions(IConfigManager config, ProfilePropertyChangedCollectionEventArgs evt)
        {
            var ids = config.GetProperty<List<string>>("behaviour.mentions.order");
            var list = config.GetProperty<JToken>("behaviour.mentions.data");

            var mentions = new List<string>();
            foreach (var id in ids)
            {
                var data = list[id];
                foreach (var trigger in data["trigger"].ToObject<List<string>>())
                    mentions.Add(trigger);
            }
            _chatManager.Config.Mentions = mentions.ToArray();
        }

        private void ConfigManager_UpdateLanguage(IConfigManager config, ProfilePropertyChangedCollectionEventArgs evt)
        {
            var selectedLanguage = config.GetProperty<string>("behaviour.language");
            var autotranslateProvider = _chatManager.Config.AutotranslateProvider as AutotranslateProvider;
            autotranslateProvider?.LoadCulture(new CultureInfo(selectedLanguage));
        }

        private void ConfigManager_UpdateRangeFilter(IConfigManager config, ProfilePropertyChangedCollectionEventArgs evt)
        {
            _chatManager.Config.EnableCutOff = config.GetProperty<bool>("behaviour.fadeout.active");
            _chatManager.Config.CutOffDistance = config.GetProperty<long>("behaviour.fadeout.cutoff");
            _chatManager.Config.FadeOutDistance = config.GetProperty<long>("behaviour.fadeout.falloff");
            //_chatManager.Config.CutOffConsiderMentions = config.GetProperty<bool>("behaviour.fadeout.mention");
        }
    }
}