using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gobchat.Core.Util
{
    public static class StringFormat
    {
        public static String Format(String format, object arg0, object arg1, object arg2)
        {
            return string.Format(CultureInfo.InvariantCulture, format, arg0, arg1, arg2);
        }

        public static String Format(String format, params object[] args)
        {
            return string.Format(CultureInfo.InvariantCulture, format, args);
        }

        public static String Format(String format, object arg0, object arg1)
        {
            return string.Format(CultureInfo.InvariantCulture, format, arg0, arg1);
        }

        public static String Format(String format, object arg0)
        {
            return string.Format(CultureInfo.InvariantCulture, format, arg0);
        }
    }
}