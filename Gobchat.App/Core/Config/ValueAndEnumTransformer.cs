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

using Gobchat.Core.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Gobchat.Core.Config
{
    internal sealed class ValueToEnumTransformer : IJsonTransformer
    {
        public JObject Transform(JObject json)
        {
            if (json == null)
                return null;

            JsonUtil.ReplaceArrayIfAvailable(json, "behaviour.channel.roleplay", JsonUtil.ConvertArrayToEnum<Chat.ChatChannel>);
            JsonUtil.ReplaceArrayIfAvailable(json, "behaviour.channel.mention", JsonUtil.ConvertArrayToEnum<Chat.ChatChannel>);
            JsonUtil.ReplaceArrayIfAvailable(json, "behaviour.channel.visible", JsonUtil.ConvertArrayToEnum<Chat.ChatChannel>);
            JsonUtil.ReplaceArrayIfAvailable(json, "behaviour.channel.rangefilter", JsonUtil.ConvertArrayToEnum<Chat.ChatChannel>);

            JsonUtil.AccessIfAvailable(json, "behaviour.segment.data", (jToken) =>
            {
                if (jToken is JObject data)
                    foreach (var segment in data.Values())
                        if (JsonUtil.TryConvertValueToEnum<Chat.MessageSegmentType>(segment["type"], out var eValue))
                            segment["type"] = new JValue(eValue);
            });

            return json;
        }
    }

    internal sealed class EnumToStringTransformer : IJsonTransformer
    {
        public JObject Transform(JObject json)
        {
            if (json == null)
                return null;

            JsonUtil.ReplaceArrayIfAvailable(json, "behaviour.channel.roleplay", ConvertEnumArrayToString<Chat.ChatChannel>);
            JsonUtil.ReplaceArrayIfAvailable(json, "behaviour.channel.mention", ConvertEnumArrayToString<Chat.ChatChannel>);
            JsonUtil.ReplaceArrayIfAvailable(json, "behaviour.channel.visible", ConvertEnumArrayToString<Chat.ChatChannel>);
            JsonUtil.ReplaceArrayIfAvailable(json, "behaviour.channel.rangefilter", ConvertEnumArrayToString<Chat.ChatChannel>);

            JsonUtil.AccessIfAvailable(json, "behaviour.segment.data", (jToken) =>
            {
                if (jToken is JObject data)
                {
                    foreach (var segment in data.Values())
                    {
                        if (TryConvertEnumToString<Chat.MessageSegmentType>(segment["type"], out var name))
                            segment["type"] = name;
                        else
                            segment["type"] = "SAY";
                    }
                }
            });

            return json;
        }

        private static JArray ConvertEnumArrayToString<TEnum>(JArray array) where TEnum : struct, IConvertible
        {
            var typeT = typeof(TEnum);
            if (!typeT.IsEnum)
                throw new ArgumentException("Not an enum");
            var newArray = new JArray();
            foreach (var element in array)
                if (TryConvertEnumToString<TEnum>(element, out var name))
                    newArray.Add(name.ToUpperInvariant());
            return newArray;
        }

        private static bool TryConvertEnumToString<TEnum>(JToken value, out string enumName) where TEnum : struct, IConvertible
        {
            enumName = null;

            if (!(value is JValue jValue))
                return false;

            if (jValue.Value == null)
                return false;

            var eType = typeof(TEnum);

            if (jValue.Type == JTokenType.Integer || jValue.Type == JTokenType.Bytes)
            {
                enumName = Enum.GetName(eType, (int)(long)jValue);
                return true;
            }

            if (Enum.IsDefined(eType, jValue.Value))
            {
                enumName = Enum.GetName(eType, (int)(long)jValue);
                return true;
            }

            if (jValue.Type == JTokenType.String)
            {
                if (int.TryParse((string)jValue, out var iValue))
                {
                    enumName = Enum.GetName(eType, iValue);
                    return true;
                }
            }

            return false;
        }
    }
}