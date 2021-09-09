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

using Gobchat.Core.Config;
using Gobchat.Core.Runtime;
using Gobchat.Module.Chat;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace Gobchat.Module.UI
{
    public sealed class AppModuleConfigToUI : IApplicationModule
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private IDIContext _container;
        private IBrowserAPIManager _browserAPIManager;
        private IConfigManager _configManager;
        private IChatManager _chatManager;

        /// <summary>
        /// Requires: <see cref="IBrowserAPIManager"/> <br></br>
        /// Requires: <see cref="IConfigManager"/> <br></br>
        /// Requires: <see cref="IChatManager"/> <br></br>
        /// <br></br>
        /// </summary>
        public AppModuleConfigToUI()
        {
        }

        public void Initialize(ApplicationStartupHandler handler, IDIContext container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _browserAPIManager = _container.Resolve<IBrowserAPIManager>();
            _configManager = _container.Resolve<IConfigManager>();
            _chatManager = _container.Resolve<IChatManager>();

            _browserAPIManager.ConfigHandler = new BrowserConfigHandler(this);

            _configManager.OnProfileChange += ConfigManager_SynchronizeJSConfig;
            _configManager.AddPropertyChangeListener("*", ConfigManager_SynchronizeJSConfig);
        }

        public void Dispose()
        {
            _configManager.OnProfileChange -= ConfigManager_SynchronizeJSConfig;
            _configManager.RemovePropertyChangeListener(ConfigManager_SynchronizeJSConfig);

            _browserAPIManager.ConfigHandler = null;
            _browserAPIManager = null;
            _chatManager = null;
            _configManager = null;
            _container = null;
        }

        private void ConfigManager_SynchronizeJSConfig(IConfigManager sender, ProfilePropertyChangedCollectionEventArgs evt)
        {
            if (!evt.Synchronizing)
                ConfigManager_SynchronizeJSConfig();
        }

        private void ConfigManager_SynchronizeJSConfig(object sender, ProfileChangedEventArgs evt)
        {
            if (!evt.Synchronizing)
                ConfigManager_SynchronizeJSConfig();
        }

        private void ConfigManager_SynchronizeJSConfig()
        {
            _browserAPIManager.DispatchEventToBrowser(new SynchronizeConfigWebEvent());
        }

        private sealed class BrowserConfigHandler : IBrowserConfigHandler
        {
            private readonly AppModuleConfigToUI _module;

            public BrowserConfigHandler(AppModuleConfigToUI module)
            {
                _module = module ?? throw new ArgumentNullException(nameof(module));
            }

            public async Task<JToken> GetConfigAsJson()
            {
                logger.Info("Sending config to ui");
                var configJson = _module._configManager.AsJson();
                return configJson;
            }

            public async Task<JToken> ParseProfile(string file)
            {
                return _module._configManager.ParseProfile(file);
            }

            public async Task SetActiveProfile(string profileId)
            {
                _module._configManager.ActiveProfileId = profileId;
            }

            public async Task SynchronizeConfig(JToken configJson)
            {
                logger.Info("Storing config from ui");
                _module._configManager.Synchronize(configJson);

                try
                { //try to do a early save
                    _module._configManager.SaveProfiles();
                    _module._chatManager.EnqueueMessage(Core.Chat.SystemMessageType.Info, Resources.Module_UI_Connector_Config_Profile_Saved);
                }
                catch (Exception e)
                {
                    logger.Warn(e, "Error on profile save");
                    _module._chatManager.EnqueueMessage(Core.Chat.SystemMessageType.Error, Resources.Module_UI_Connector_Config_Profile_Error);
                }
            }
        }
    }
}