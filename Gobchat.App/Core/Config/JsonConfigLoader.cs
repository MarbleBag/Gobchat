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
            JObject configObject = GetConfigFromFile(configPath);
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
            try
            {
                return UpgradeConfig(configObject);
            }
            catch (FileNotFoundException e)
            {
                throw new ConfigLoadException("", e);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="configPath"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException">If the version property is missing</exception>
        private JObject GetConfigFromFile(string configPath)
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
            if (jToken == null)
                throw new MissingPropertyException("version");

            if (jToken.Type == JTokenType.String)
            {
                if (int.TryParse((string)jToken, out int result))
                {
                    configObject["version"] = result;
                    jToken = configObject["version"];
                }
            }

            if (jToken.Type != JTokenType.Integer)
                throw new InvalidPropertyTypeException("version", "Integer", System.Enum.GetName(typeof(JTokenType), jToken.Type));

            long version = jToken.Value<long>();
            return (int)version;
        }
    }
}