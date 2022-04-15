/*******************************************************************************
 * Copyright (C) 2019-2022 MarbleBag
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

namespace Gobchat.Core.Util
{
    public static class EnumUtil
    {
        public static TEnum? ObjectToEnum<TEnum>(object obj) where TEnum : struct, IConvertible
        {
            var type = typeof(TEnum);
            if (!type.IsEnum)
                throw new ArgumentException("enum type expected", nameof(TEnum));

            if (obj == null)
                return null;

            if (obj is TEnum e)
                return e;

            if (global::Gobchat.Core.Util.MathUtil.IsNumber(obj))
                return (TEnum)obj;

            if (obj is string str)
                if (Enum.TryParse<TEnum>(str, true, out var enumValue))
                    return enumValue;

            return null;
        }
    }
}