using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gobchat.Core.Util.Extension.Queue
{
    public static class ConcurrentQueueExtension
    {
        public static System.Collections.Generic.IEnumerable<T> DequeueMultiple<T>(this System.Collections.Concurrent.ConcurrentQueue<T> queue, uint size)
        {
            for (uint i = 0u; i < size && queue.Count > 0; i++)
            {
                if (queue.TryDequeue(out T value))
                {
                    yield return value;
                }
            }
        }
    }
}