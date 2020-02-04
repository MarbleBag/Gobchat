/*******************************************************************************
 * Copyright (C) 2019 MarbleBag
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
        private GobchatConfigManager _manager;
        public IGobchatConfigManager ConfigManager { get => _manager; }

        public void Initialize(ApplicationStartupHandler handler, IDIContext container)
        {
            var resourceLocation = container.Resolve<string>("ResourceLocation");
            var userLocation = container.Resolve<string>("UserConfigLocation");

            if (resourceLocation == null)
                throw new ArgumentNullException(nameof(resourceLocation));
            if (userLocation == null)
                throw new ArgumentNullException(nameof(userLocation));

            var defaultConfigPath = System.IO.Path.Combine(resourceLocation, @"default_profile.json");

            if (!System.IO.File.Exists(defaultConfigPath)) //TODO
                throw new ArgumentException(nameof(defaultConfigPath));

            _manager = new GobchatConfigManager(defaultConfigPath, userLocation);
            _manager.InitializeManager();

            container.Register<IGobchatConfigManager>((c, p) => ConfigManager);
        }

        public void Dispose(IDIContext container)
        {
            _manager?.SaveProfiles();
            _manager = null;
        }
    }
}