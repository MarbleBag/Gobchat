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

using Gobchat.Core.Chat;
using Gobchat.Core.Config;
using Gobchat.Core.Runtime;
using Gobchat.Module.Chat;
using Gobchat.Module.Misc.Chatlogger.Internal;
using System;
using System.Collections.Generic;
using System.IO;
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
            _chatLogger.LogChannels = Enum.GetValues(typeof(ChatChannel)).Cast<ChatChannel>(); //will log everything that comes from the chat manager

            _configManager = _container.Resolve<IConfigManager>();
            _configManager.AddPropertyChangeListener("behaviour.chatlog.active", true, true, ConfigManager_UpdateWriteLog);
            _configManager.AddPropertyChangeListener("behaviour.chatlog.path", true, true, ConfigManager_UpdateLogPath);

            // _configManager.AddPropertyChangeListener("behaviour.channel.visible", true, true, ConfigManager_UpdateLogChannels);

            _chatManager = _container.Resolve<IChatManager>();
            _chatManager.OnChatMessage += ChatManager_ChatMessageEvent;
        }

        public void Dispose()
        {
            _configManager.RemovePropertyChangeListener(ConfigManager_UpdateWriteLog);
            _configManager.RemovePropertyChangeListener(ConfigManager_UpdateLogPath);
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
            _chatLogger.Active = sender.GetProperty<bool>("behaviour.chatlog.active");
        }

        private void ConfigManager_UpdateLogPath(IConfigManager sender, ProfilePropertyChangedCollectionEventArgs evt)
        {
            var path = sender.GetProperty<string>("behaviour.chatlog.path");

            if (path == null || path.Length == 0)
                path = Path.Combine(GobchatContext.AppDataLocation, "log");

            if (!Path.IsPathRooted(path))
                path = Path.Combine(GobchatContext.AppDataLocation, path);

            _chatLogger.LogFolder = path;
        }

        private void ConfigManager_UpdateLogChannels(IConfigManager sender, ProfilePropertyChangedCollectionEventArgs evt)
        {
            // _chatLogger.LogChannels = sender.GetProperty<List<long>>("behaviour.channel.visible").Select(i => (ChatChannel)i).ToList();
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