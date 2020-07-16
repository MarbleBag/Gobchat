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
        private readonly IList<IJsonFunction> _transformers = new List<IJsonFunction>();

        public JsonConfigLoader()
        {
        }

        public void AddFunction(IJsonFunction transformer)
        {
            if (transformer == null) throw new ArgumentNullException(nameof(transformer));
            _transformers.Add(transformer);
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
            return TransformConfig(configObject);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="configPath"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException">If the file can't be found</exception>
        private JObject LoadJsonFromFile(string configPath)
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

        private JObject TransformConfig(JObject configObject)
        {
            foreach (var transformer in _transformers)
                configObject = transformer.Apply(configObject);
            return configObject;
        }
    }
}