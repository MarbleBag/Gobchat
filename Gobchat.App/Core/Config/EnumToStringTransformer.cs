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

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Gobchat.Core.Config
{
    public sealed class EnumToStringTransformer : IJsonTransformer
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
                        ConvertEnumToStringAndReplace<Chat.ChannelEnum>(roleplay);

                    if (channel["mention"] is JArray mention)
                        ConvertEnumToStringAndReplace<Chat.ChannelEnum>(mention);

                    if (channel["visible"] is JArray visible)
                        ConvertEnumToStringAndReplace<Chat.ChannelEnum>(visible);
                }
            }

            return json;
        }

        private void ConvertEnumToStringAndReplace<TEnum>(JArray list) where TEnum : struct, IConvertible
        {
            var enums = ConvertEnumToString<Chat.ChannelEnum>(list);
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
                var name = Enum.GetName(typeT, (int)(long)element);
                result.Add(name);
            }
            return result;
        }
    }
}