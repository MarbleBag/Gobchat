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

namespace Gobchat.Core.Util.Extension.Queue
{
    public static class ConcurrentQueueExtension
    {
        public static System.Collections.Generic.IEnumerable<T> DequeueMultiple<T>(this System.Collections.Concurrent.ConcurrentQueue<T> queue, uint size)
        {
            for (uint i = 0u; i < size && queue.Count > 0; i++)
            {
                if (queue.TryDequeue(out T value))
                    yield return value;
            }
        }
    }

    public static class QueueExtension
    {
        public static System.Collections.Generic.IEnumerable<T> DequeueMultiple<T>(this System.Collections.Generic.Queue<T> queue, uint size)
        {
            for (uint i = 0u; i < size && queue.Count > 0; i++)
                yield return queue.Dequeue();
        }

        public static System.Collections.Generic.IEnumerable<T> DequeueAll<T>(this System.Collections.Generic.Queue<T> queue)
        {
            while (queue.Count > 0)
                yield return queue.Dequeue();
        }
    }
}