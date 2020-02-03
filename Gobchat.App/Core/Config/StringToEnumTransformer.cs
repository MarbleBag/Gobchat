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

using Gobchat.Core.Util;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Gobchat.Core.Config
{
    public sealed class StringToEnumTransformer : IJsonTransformer
    {
        public JObject Transform(JObject json)
        {
            if (json == null)
                return null;

            if (json["behaviour"] is JObject behaviour)
            {
                if (behaviour["channel"] is JObject channel)
                {
                    if (channel["roleplay"] is JArray roleplay)
                        ConvertStringToEnumAndReplace<Chat.ChannelEnum>(roleplay);

                    if (channel["mention"] is JArray mention)
                        ConvertStringToEnumAndReplace<Chat.ChannelEnum>(mention);

                    if (channel["visible"] is JArray visible)
                        ConvertStringToEnumAndReplace<Chat.ChannelEnum>(visible);
                }

                if (behaviour["segment"] is JObject segments)
                {
                    if (segments["data"] is JObject data)
                    {
                        foreach (var segment in data.Values())
                        {
                            if (TryConvertStringToEnum<Chat.MessageSegmentEnum>(segment["type"], out var eValue))
                                segment["type"] = new JValue(eValue);
                        }
                    }
                }
            }

            return json;
        }

        private void ConvertStringToEnumAndReplace<TEnum>(JArray list) where TEnum : struct, IConvertible
        {
            var enums = ConvertStringToEnum<Chat.ChannelEnum>(list);
            list.Clear();
            enums.ForEach(e => list.Add(e));
        }

        private List<TEnum> ConvertStringToEnum<TEnum>(JArray list) where TEnum : struct, IConvertible
        {
            var typeT = typeof(TEnum);
            if (!typeT.IsEnum)
                throw new ArgumentException("Not an enum");

            List<TEnum> enumResult = new List<TEnum>();
            foreach (var element in list)
            {
                if (TryConvertStringToEnum(element, out TEnum enumValue))
                    enumResult.Add(enumValue);
            }

            return enumResult;
        }

        private bool TryConvertStringToEnum<TEnum>(JToken value, out TEnum e) where TEnum : struct, IConvertible
        {
            e = default;

            if (!(value is JValue jValue))
                return false;

            if (jValue.Value == null)
                return false;

            if (jValue.Value is string)
            {
                if (Enum.TryParse<TEnum>((string)jValue.Value, out e))
                    return true;
                return false;
            }

            if (MathUtil.IsNumber(jValue.Value))
            {
                //why you do dis c#
                e = (TEnum)(object)(int)(long)jValue.Value;
                return true;
            }

            return false;
        }
    }
}