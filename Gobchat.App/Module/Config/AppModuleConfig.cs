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

namespace Gobchat.Core.Module
{
    public sealed class AppModuleConfig : IApplicationModule
    {
        public GobchatConfigManager ConfigManager { get; private set; }

        public void Initialize(ApplicationStartupHandler handler, IDIContext container)
        {
            var resourceLocation = container.Resolve<string>("ResourceLocation");
            var userLocation = container.Resolve<string>("UserConfigLocation");

            if (resourceLocation == null)
                throw new ArgumentNullException(nameof(resourceLocation));
            if (userLocation == null)
                throw new ArgumentNullException(nameof(userLocation));

            var defaultConfigPath = System.IO.Path.Combine(resourceLocation, @"default_gobconfig.json");
            var userConfigPath = System.IO.Path.Combine(userLocation, @"gobconfig.json");

            //TODO
            if (!System.IO.File.Exists(defaultConfigPath))
                throw new ArgumentNullException(nameof(defaultConfigPath));
            //  if (!System.IO.File.Exists(userConfigPath))
            //      throw new ArgumentNullException(nameof(userConfigPath));

            ConfigManager = new GobchatConfigManager(defaultConfigPath, userConfigPath);
            ConfigManager.LoadConfig();

            container.Register<GobchatConfigManager>((c, p) => ConfigManager);
        }

        public void Dispose(IDIContext container)
        {
            ConfigManager?.SaveConfig();
            ConfigManager = null;
        }
    }
}