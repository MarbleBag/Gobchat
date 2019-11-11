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
                if (!(element is JValue jValue))
                    continue;
                if (jValue.Value == null)
                    continue;

                if (jValue.Value is string)
                {
                    if (Enum.TryParse<TEnum>((string)jValue.Value, out var enumValue))
                        enumResult.Add(enumValue);
                    continue;
                }

                if (MathUtil.IsNumber(jValue.Value))
                {
                    //why you do dis c#
                    enumResult.Add((TEnum)(object)(int)(long)jValue.Value);
                    continue;
                }
            }
            return enumResult;
        }
    }
}