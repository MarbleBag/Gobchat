﻿/*******************************************************************************
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

            if (json["behaviour"] is JObject behaviour)
            {
                if (behaviour["channel"] is JObject channel)
                {
                    if (channel["roleplay"] is JArray roleplay)
                        ConvertValueToEnumAndReplace<Chat.FFXIVChatChannel>(roleplay);

                    if (channel["mention"] is JArray mention)
                        ConvertValueToEnumAndReplace<Chat.FFXIVChatChannel>(mention);

                    if (channel["visible"] is JArray visible)
                        ConvertValueToEnumAndReplace<Chat.FFXIVChatChannel>(visible);

                    if (channel["rangefilter"] is JArray rangefilter)
                        ConvertValueToEnumAndReplace<Chat.FFXIVChatChannel>(rangefilter);
                }

                if (behaviour["segment"] is JObject segments)
                {
                    if (segments["data"] is JObject data)
                    {
                        foreach (var segment in data.Values())
                        {
                            if (TryConvertValueToEnum<Chat.MessageSegmentType>(segment["type"], out var eValue))
                                segment["type"] = new JValue(eValue);
                        }
                    }
                }
            }

            return json;
        }

        private void ConvertValueToEnumAndReplace<TEnum>(JArray list) where TEnum : struct, IConvertible
        {
            var enums = ConvertValueToEnum<Chat.FFXIVChatChannel>(list);
            list.Clear();
            enums.ForEach(e => list.Add(e));
        }

        private List<TEnum> ConvertValueToEnum<TEnum>(JArray list) where TEnum : struct, IConvertible
        {
            var typeT = typeof(TEnum);
            if (!typeT.IsEnum)
                throw new ArgumentException("Not an enum");

            List<TEnum> enumResult = new List<TEnum>();
            foreach (var element in list)
            {
                if (TryConvertValueToEnum(element, out TEnum enumValue))
                    enumResult.Add(enumValue);
            }

            return enumResult;
        }

        private bool TryConvertValueToEnum<TEnum>(JToken value, out TEnum e) where TEnum : struct, IConvertible
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

    internal sealed class EnumToStringTransformer : IJsonTransformer
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
                        ConvertEnumToStringAndReplace<Chat.FFXIVChatChannel>(roleplay);

                    if (channel["mention"] is JArray mention)
                        ConvertEnumToStringAndReplace<Chat.FFXIVChatChannel>(mention);

                    if (channel["visible"] is JArray visible)
                        ConvertEnumToStringAndReplace<Chat.FFXIVChatChannel>(visible);

                    if (channel["rangefilter"] is JArray rangefilter)
                        ConvertEnumToStringAndReplace<Chat.FFXIVChatChannel>(rangefilter);
                }

                if (behaviour["segment"] is JObject segments)
                {
                    if (segments["data"] is JObject data)
                    {
                        foreach (var segment in data.Values())
                        {
                            if (TryConvertEnumToString<Chat.MessageSegmentType>(segment["type"], out var name))
                                segment["type"] = name;
                            else
                                segment["type"] = "SAY";
                        }
                    }
                }
            }

            return json;
        }

        private void ConvertEnumToStringAndReplace<TEnum>(JArray list) where TEnum : struct, IConvertible
        {
            var enums = ConvertEnumToString<Chat.FFXIVChatChannel>(list);
            list.Clear();
            enums.ForEach(e => list.Add(e));
        }

        private List<string> ConvertEnumToString<TEnum>(JArray list) where TEnum : struct, IConvertible
        {
            var typeT = typeof(TEnum);
            if (!typeT.IsEnum)
                throw new ArgumentException("Not an enum");

            List<string> result = new List<string>();
            foreach (var element in list)
            {
                if (TryConvertEnumToString<TEnum>(element, out var name))
                    result.Add(name);
            }
            return result;
        }

        private string ConvertEnumToString<TEnum>(JToken value) where TEnum : struct, IConvertible
        {
            var typeT = typeof(TEnum);
            return Enum.GetName(typeT, (int)(long)value);
        }

        private bool TryConvertEnumToString<TEnum>(JToken value, out string enumName) where TEnum : struct, IConvertible
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