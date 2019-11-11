using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gobchat.Core.Gobchat.Core.Util
{
    public static class EnumUtil
    {
        public static string ConvertEnumToJavascript(Type type)
        {
            if (!type.IsEnum)
                throw new InvalidOperationException("enum expected");

            StringBuilder builder = new StringBuilder();
            return null;
        }
    }
}