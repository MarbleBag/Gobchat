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

using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Gobchat.Core.Config
{
    internal sealed class ConfigUpgrader
    {
        private readonly IConfigUpgradeStep[] _upgrader;

        public ConfigUpgrader()
        {
            _upgrader = new IConfigUpgradeStep[]
            {
           //     new ConfigUpgrader_v3(),
            //    new ConfigUpgrader_v16(),
            //    new ConfigUpgrader_v1700(),
            };
            Array.Sort(_upgrader, (x, y) => x.MinVersion - y.MinVersion);
        }

        public JObject UpgradeConfig(JObject configObject)
        {
            var version = GetVersion(configObject);

            do
            {
                var upgrader = _upgrader.Where(e => e.MinVersion >= version && e.MaxVersion <= version).FirstOrDefault(); //TODO select best upgrader
                if (upgrader == null)
                    break;

                configObject = upgrader.Upgrade(configObject);
                SetVersion(configObject, upgrader.TargetVersion);

                var newVersion = GetVersion(configObject);
                if (newVersion < version)
                    throw new ConfigLoadException($"Version converter {version} is defect");
                version = newVersion;
            } while (true);

            return configObject;
        }

        private void SetVersion(JObject configObject, int targetVersion)
        {
            configObject["version"] = targetVersion;
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

    internal interface IConfigUpgradeStep
    {
        int MinVersion { get; }
        int MaxVersion { get; }
        int TargetVersion { get; }

        JObject Upgrade(JToken jToken);
    }
}