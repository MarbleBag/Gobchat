/*******************************************************************************
 * Copyright (C) 2019-2025 MarbleBag
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

namespace Gobchat.Core.Config
{
    public interface IJsonFunction
    {
        JObject Apply(JObject json);
    }

    internal sealed class JsonValueToEnum : IJsonFunction
    {
        public JObject Apply(JObject json)
        {
            if (json == null)
                return null;

            JsonUtil.ReplaceArrayIfAvailable(json, "behaviour.channel.roleplay", JsonUtil.ConvertArrayToEnum<Chat.ChatChannel>);
            JsonUtil.ReplaceArrayIfAvailable(json, "behaviour.channel.mention", JsonUtil.ConvertArrayToEnum<Chat.ChatChannel>);
            JsonUtil.ReplaceArrayIfAvailable(json, "behaviour.channel.rangefilter", JsonUtil.ConvertArrayToEnum<Chat.ChatChannel>);
            JsonUtil.ReplaceArrayIfAvailable(json, "behaviour.channel.log", JsonUtil.ConvertArrayToEnum<Chat.ChatChannel>);

            JsonUtil.AccessIfAvailable(json, "behaviour.segment.data", (jToken) =>
            {
                if (jToken is JObject data)
                    foreach (var segment in data.Values())
                        if (JsonUtil.TryConvertValueToEnum<Chat.MessageSegmentType>(segment["type"], out var eValue))
                            segment["type"] = new JValue(eValue);
            });

            foreach (var id in JsonUtil.GetKeysIfAvailable(json, "behaviour.chattabs.data"))
                JsonUtil.ReplaceArrayIfAvailable(json, $"behaviour.chattabs.data.{id}.channel.visible", JsonUtil.ConvertArrayToEnum<Chat.ChatChannel>);

            return json;
        }
    }

    internal sealed class JsonEnumToString : IJsonFunction
    {
        public JObject Apply(JObject json)
        {
            if (json == null)
                return null;

            JsonUtil.ReplaceArrayIfAvailable(json, "behaviour.channel.roleplay", JsonUtil.ConvertEnumArrayToString<Chat.ChatChannel>);
            JsonUtil.ReplaceArrayIfAvailable(json, "behaviour.channel.mention", JsonUtil.ConvertEnumArrayToString<Chat.ChatChannel>);
            JsonUtil.ReplaceArrayIfAvailable(json, "behaviour.channel.rangefilter", JsonUtil.ConvertEnumArrayToString<Chat.ChatChannel>);
            JsonUtil.ReplaceArrayIfAvailable(json, "behaviour.channel.log", JsonUtil.ConvertEnumArrayToString<Chat.ChatChannel>);

            JsonUtil.AccessIfAvailable(json, "behaviour.segment.data", (jToken) =>
            {
                if (jToken is JObject data)
                {
                    foreach (var segment in data.Values())
                    {
                        if (JsonUtil.TryConvertEnumToString<Chat.MessageSegmentType>(segment["type"], out var name))
                            segment["type"] = name;
                        else
                            segment["type"] = "SAY";
                    }
                }
            });

            foreach (var id in JsonUtil.GetKeysIfAvailable(json, "behaviour.chattabs.data"))
                JsonUtil.ReplaceArrayIfAvailable(json, $"behaviour.chattabs.data.{id}.channel.visible", JsonUtil.ConvertEnumArrayToString<Chat.ChatChannel>);

            return json;
        }
    }

    internal sealed class JsonValidateIsProfile : IJsonFunction
    {
        public JObject Apply(JObject configObject)
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

            return configObject;
        }
    }

    internal sealed class JsonConfigUpgrader : IJsonFunction
    {
        private readonly ConfigUpgrader _configUpgrader;

        public JsonConfigUpgrader(ConfigUpgrader configUpgrader)
        {
            this._configUpgrader = configUpgrader ?? throw new ArgumentNullException(nameof(configUpgrader));
        }

        public JObject Apply(JObject json)
        {
            return _configUpgrader.UpgradeConfig(json);
        }
    }
}