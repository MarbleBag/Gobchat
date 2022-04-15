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

namespace Gobchat.Core.Util.Extension.Queue
{
    public static class ConcurrentQueueExtension
    {
        public static System.Collections.Generic.IEnumerable<T> DequeueMultiple<T>(this System.Collections.Concurrent.ConcurrentQueue<T> queue, uint size)
        {
            //  for (uint i = 0u; i < size && queue.Count > 0; i++)
            //  {
            //      if (queue.TryDequeue(out T value))
            //          yield return value;
            //  }

            var length = (int)System.Math.Min(size, queue.Count);
            var result = new System.Collections.Generic.List<T>(length);
            for (int i = 0; i < length; i++)
            {
                if (queue.TryDequeue(out T value))
                    result.Add(value);
                else
                    break;
            }
            return result;
        }
    }

    public static class QueueExtension
    {
        public static System.Collections.Generic.IEnumerable<T> DequeueMultiple<T>(this System.Collections.Generic.Queue<T> queue, uint size)
        {
            var result = new T[System.Math.Min(size, queue.Count)];
            for (int i = 0; i < result.Length; i++)
                result[i] = queue.Dequeue();
            return result;
        }

        public static System.Collections.Generic.IEnumerable<T> DequeueAll<T>(this System.Collections.Generic.Queue<T> queue)
        {
            var result = new T[queue.Count];
            for (int i = 0; i < result.Length; i++)
                result[i] = queue.Dequeue();
            return result;
        }

        public static void EnqueueAll<T>(this System.Collections.Generic.Queue<T> queue, System.Collections.Generic.IEnumerable<T> items)
        {
            foreach (var item in items)            
                queue.Enqueue(item);
        }
    }
}