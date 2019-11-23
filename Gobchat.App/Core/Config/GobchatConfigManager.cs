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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Gobchat.Core.Config
{
    public class GobchatConfigManager
    {
        public string DefaultConfigPath { get; set; }
        public string UserConfigPath { get; set; }

        public GobchatConfig DefaultConfig { get; private set; }

        public GobchatConfig UserConfig { get; private set; }

        public GobchatConfigManager(string defaultConfigPath, string userConfigPath)
        {
            DefaultConfigPath = defaultConfigPath;
            UserConfigPath = userConfigPath;
        }

        public void LoadConfig()
        {
            LoadDefaultConfig();
            LoadUserConfig();
        }

        public void LoadDefaultConfig()
        {
            var loader = new JsonConfigLoader();
            var defaultConfig = loader.LoadConfig(DefaultConfigPath);

            var finalizer = new StringToEnumTransformer();
            defaultConfig = finalizer.Transform(defaultConfig);
            DefaultConfig = new JsonGobchatConfig(defaultConfig);
        }

        public void LoadUserConfig()
        {
            JObject userConfig = null;
            if (File.Exists(UserConfigPath))
            {
                var loader = new JsonConfigLoader();
                userConfig = loader.LoadConfig(UserConfigPath);
            }

            JObject clonedDefaultConfig = DefaultConfig.ToJson();

            if (userConfig == null) //easy to solve
            {
                userConfig = clonedDefaultConfig;
            }
            else
            {
                var finalizer = new StringToEnumTransformer();
                userConfig = finalizer.Transform(userConfig);

                clonedDefaultConfig.Merge(userConfig, new JsonMergeSettings()
                {
                    MergeArrayHandling = MergeArrayHandling.Replace,
                    MergeNullValueHandling = MergeNullValueHandling.Merge,
                    PropertyNameComparison = StringComparison.OrdinalIgnoreCase,
                });

                userConfig = clonedDefaultConfig;
            }

            UserConfig = new JsonGobchatConfig(userConfig);
        }

        public void SaveConfig()
        {
            if (UserConfigPath == null)
                return;

            var directoryPath = Path.GetDirectoryName(UserConfigPath);
            Directory.CreateDirectory(directoryPath);

            //TODO calculate diff to default config
            //TODO only save diff
            var jsonDiff = UserConfig.ToJson();

            var finalizer = new EnumToStringTransformer();
            jsonDiff = finalizer.Transform(jsonDiff);

            try
            {
                using (StreamWriter file = File.CreateText(UserConfigPath))
                using (JsonTextWriter writer = new JsonTextWriter(file))
                {
                    writer.Formatting = Formatting.Indented;
                    jsonDiff.WriteTo(writer);
                }
            }
            catch (UnauthorizedAccessException e)
            {
                //TODO
                throw;
            }
        }
    }
}