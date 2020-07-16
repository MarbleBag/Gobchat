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

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace Gobchat.Core.Config
{
    public sealed class JsonConfigLoader
    {
        private readonly Dictionary<int, IJsonTransformer> _converters = new Dictionary<int, IJsonTransformer>();

        public JsonConfigLoader()
        {
        }

        public void AddConverter(int targetVersion, IJsonTransformer transformer)
        {
            if (transformer == null) throw new ArgumentNullException(nameof(transformer));
            _converters.Add(targetVersion, transformer);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="configPath"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="ConfigLoadException"></exception>
        public JObject LoadConfig(string configPath)
        {
            JObject configObject = LoadJsonFromFile(configPath);
            return LoadConfig(configObject);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="configObject"></param>
        /// <returns></returns>
        /// <exception cref="VersionProperyException"></exception>
        public JObject LoadConfig(JObject configObject)
        {
            ValidateIsConfig(configObject);
            return UpgradeConfig(configObject);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="configPath"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException">If the file can't be found</exception>
        public JObject LoadJsonFromFile(string configPath)
        {
            if (!System.IO.File.Exists(configPath))
                throw new FileNotFoundException(configPath);

            using (var stream = File.OpenText(configPath))
            using (var reader = new JsonTextReader(stream))
            {
                JObject configObject = (JObject)JToken.ReadFrom(reader);
                return configObject;
            }
        }

        private void ValidateIsConfig(JObject configObject)
        {
            if (configObject == null)
                throw new ConfigLoadException("Config object is null");

            if (configObject["version"] == null)
                throw new MissingPropertyException("version");

            if (configObject["version"].Type != JTokenType.Integer)
                throw new InvalidPropertyTypeException("version", System.Enum.GetName(typeof(JTokenType), JTokenType.Integer), System.Enum.GetName(typeof(JTokenType), configObject["version"].Type));

            if (configObject["profile"] is JObject profile)
            {
                if (configObject["profile"]["id"] == null)
                    throw new MissingPropertyException("profile.id");
                if (configObject["profile"]["name"] == null)
                    throw new MissingPropertyException("profile.name");
            }
            else
            {
                throw new MissingPropertyException("profile");
            }
        }

        private JObject UpgradeConfig(JObject configObject)
        {
            var version = GetVersion(configObject);

            do
            {
                if (!_converters.TryGetValue(version, out IJsonTransformer converter))
                    break;

                configObject = converter.Transform(configObject);

                var newVersion = GetVersion(configObject);
                if (newVersion < version)
                    throw new ConfigLoadException($"Version converter {version} is defect");
                version = newVersion;
            } while (true);

            return configObject;
        }

        private int GetVersion(JObject configObject)
        {
            var jToken = configObject["version"];

            if (jToken.Type == JTokenType.String)
            {
                if (int.TryParse((string)jToken, out int result))
                {
                    configObject["version"] = result;
                    jToken = configObject["version"];
                }
            }

            long version = jToken.Value<long>();
            return (int)version;
        }
    }
}