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

using Gobchat.Core.Chat;
using Gobchat.Core.Config;
using Gobchat.Core.Runtime;
using Gobchat.Module.Chat;
using Gobchat.Module.Misc.Chatlogger.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Gobchat.Module.Misc.Chatlogger
{
    public sealed class AppModuleChatLogger : IApplicationModule
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private IDIContext _container;
        private IConfigManager _configManager;

        private IChatLogger _chatLogger;
        private IChatManager _chatManager;

        /// <summary>
        ///
        /// Requires: <see cref="IConfigManager"/> <br></br>
        /// Requires: <see cref="IChatManager"/> <br></br>
        /// <br></br>
        /// </summary>
        public AppModuleChatLogger()
        {
        }

        public void Initialize(ApplicationStartupHandler handler, IDIContext container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));

            _chatLogger = new ChatLoggerFormated();

            _configManager = _container.Resolve<IConfigManager>();
            _configManager.AddPropertyChangeListener("behaviour.writeChatLog", true, true, ConfigManager_UpdateWriteLog);
            _configManager.AddPropertyChangeListener("behaviour.channel.visible", true, true, ConfigManager_UpdateLogChannels);

            _chatManager = _container.Resolve<IChatManager>();
            _chatManager.OnChatMessage += ChatManager_ChatMessageEvent;
        }

        public void Dispose()
        {
            _configManager.RemovePropertyChangeListener(ConfigManager_UpdateWriteLog);
            _configManager.RemovePropertyChangeListener(ConfigManager_UpdateLogChannels);
            _configManager = null;

            _chatManager.OnChatMessage -= ChatManager_ChatMessageEvent;
            _chatManager = null;

            _chatLogger.Dispose();
            _chatLogger = null;

            _container = null;
        }

        private void ConfigManager_UpdateWriteLog(IConfigManager sender, ProfilePropertyChangedCollectionEventArgs evt)
        {
            _chatLogger.Active = sender.GetProperty<bool>("behaviour.writeChatLog");
        }

        private void ConfigManager_UpdateLogChannels(IConfigManager sender, ProfilePropertyChangedCollectionEventArgs evt)
        {
            _chatLogger.LogChannels = sender.GetProperty<List<long>>("behaviour.channel.visible").Select(i => (FFXIVChatChannel)i).ToList();
        }

        private void ChatManager_ChatMessageEvent(object sender, ChatMessageEventArgs e)
        {
            foreach (var message in e.Messages)
            {
                try
                {
                    _chatLogger.Log(message);
                }
                catch (Exception ex)
                {
                    logger.Fatal(ex);
                }
            }

            try
            {
                _chatLogger.Flush();
            }
            catch (Exception ex)
            {
                logger.Fatal(ex);
            }
        }
    }
}