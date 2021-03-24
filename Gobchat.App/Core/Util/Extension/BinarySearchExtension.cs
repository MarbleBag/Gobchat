/*******************************************************************************
 * Copyright (C) 2019-2021 MarbleBag
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

namespace Gobchat.Core.Util.Extension
{
    public static class BinarySearchExtension
    {
        public delegate int DistanceTo<T, G>(T obj, G value);

        public static int BinarySearchClosest<T, G>(this IList<T> array, G value, DistanceTo<T, G> distanceFunction)
        {
            if (array.Count == 0)
                return -1;

            if (array.Count == 1)
                return 0;

            int low = 0;
            int high = array.Count - 1;

            int lastIdx = 0;
            int diff = int.MaxValue;

            while (low <= high)
            {
                var mid = (low + high) / 2;
                var result = distanceFunction(array[mid], value);
                if (result == 0)
                    return mid;

                var abs = Math.Abs(result);
                if (abs < diff)
                {
                    diff = abs;
                    lastIdx = mid;
                }

                if (result > 0)
                    high = mid - 1;
                else
                    low = mid + 1;
            }

            return lastIdx;
        }

        public static int BinarySearchUpper<T, G>(this IList<T> array, G value, DistanceTo<T, G> distanceFunction)
        {
            var idx = BinarySearchClosest(array, value, distanceFunction);
            if (idx < 0) return idx;
            var distance = distanceFunction(array[idx], value);
            if (distance < 0)
                return array.Count == idx ? idx : idx + 1;
            else
                return idx;
        }

        public static int BinarySearchLower<T, G>(this IList<T> array, G value, DistanceTo<T, G> distanceFunction)
        {
            var idx = BinarySearchClosest(array, value, distanceFunction);
            if (idx < 0) return idx;
            var distance = distanceFunction(array[idx], value);
            if (distance > 0)
                return 0 == idx ? idx : idx - 1;
            else
                return idx;
        }
    }
}