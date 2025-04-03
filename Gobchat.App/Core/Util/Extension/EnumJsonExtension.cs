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

using System;
using System.Linq;

namespace Gobchat.Core.Util.Extension
{
    public static class EnumJsonExtension
    {
        public static string EnumToJson(this Type type)
        {
            if (!type.IsEnum)
                throw new ArgumentException("enum type expected", nameof(type));

            var results =
                Enum.GetValues(type).Cast<object>()
                    .ToDictionary(enumValue => enumValue.ToString(), enumValue => (int)enumValue);

            return Newtonsoft.Json.JsonConvert.SerializeObject(results);
        }

        public static string EnumToJson(this Type type, Func<string, string> renameFunction)
        {
            if (!type.IsEnum)
                throw new ArgumentException("enum type expected", nameof(type));

            var results =
                Enum.GetValues(type).Cast<object>()
                    .ToDictionary(enumValue => renameFunction(enumValue.ToString()), enumValue => (int)enumValue);

            return Newtonsoft.Json.JsonConvert.SerializeObject(results);
        }

        public static TEnum? StringToEnum<TEnum>(this string str) where TEnum : struct, IConvertible
        {
            return global::Gobchat.Core.Util.EnumUtil.ObjectToEnum<TEnum>(str);
        }
    }
}