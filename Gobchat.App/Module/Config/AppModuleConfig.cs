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

using Gobchat.Core.Config;
using Gobchat.Core.Runtime;
using System;

namespace Gobchat.Module.Config
{
    public sealed class AppModuleConfig : IApplicationModule
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private GobchatConfigManager _manager;
        public IConfigManager ConfigManager { get => _manager; }

        /// <summary>
        /// Provides: <see cref="IConfigManager"/> <br></br>
        /// </summary>
        public AppModuleConfig()
        {
        }

        public void Initialize(ApplicationStartupHandler handler, IDIContext container)
        {
            var resourceLocation = GobchatApplicationContext.ResourceLocation;
            var userLocation = GobchatApplicationContext.UserConfigLocation;

            var defaultConfigPath = System.IO.Path.Combine(resourceLocation, @"default_profile.json");

            if (!System.IO.File.Exists(defaultConfigPath)) //TODO
                throw new ArgumentException(nameof(defaultConfigPath));

            _manager = new GobchatConfigManager(defaultConfigPath, userLocation);
            _manager.InitializeManager();

            container.Register<IConfigManager>((c, p) => ConfigManager);
        }

        public void Dispose()
        {
            _manager?.SaveProfiles();
            _manager = null;
        }
    }
}