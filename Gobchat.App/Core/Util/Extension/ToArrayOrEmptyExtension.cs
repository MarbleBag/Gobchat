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
using System.Collections.Generic;
using System.Linq;

namespace Gobchat.Core.Util.Extension
{
    public static class ToArrayOrEmptyExtension
    {
        public static T[] ToArrayOrEmpty<T>(this ICollection<T> obj)
        {
            return obj == null || obj.Count == 0 ? Array.Empty<T>() : obj.ToArray();
        }

        public static T[] ToArrayOrEmpty<T>(this IEnumerable<T> obj)
        {
            var array = obj == null ? Array.Empty<T>() : obj.ToArray();
            if (array.Length == 0)
                return Array.Empty<T>();
            return array;
        }

        public static T[] ToArrayOrEmpty<T>(this T[] obj)
        {
            return obj == null || obj.Length == 0 ? Array.Empty<T>() : obj.ToArray();
        }
    }
}