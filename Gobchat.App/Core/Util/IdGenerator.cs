using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gobchat.Core.Util
{
    internal sealed class IdGenerator
    {
        public static string GenerateId(int lenght)
        {
            var builder = new System.Text.StringBuilder();
            while (lenght > 0)
            {
                var path = System.IO.Path.GetRandomFileName().Replace(".", "");
                if (path.Length > lenght)
                    builder.Append(path.Substring(0, lenght));
                else
                    builder.Append(path);
                lenght -= path.Length;
            }
            return builder.ToString();
        }

        public static string GenerateNewId(int length, ICollection<string> usedIds)
        {
            do
            {
                var id = GenerateId(length);
                if (!usedIds.Contains(id))
                    return id;
            } while (true);
        }
    }
}