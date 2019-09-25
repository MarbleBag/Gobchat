using System;

namespace Gobchat
{
    public static class EnumHelper
    {
        public static T ToEnum<T>(this string str) where T : struct, IConvertible
        {
            Type enumType = typeof(T);
            if (!enumType.IsEnum)
                throw new Exception("T must be an Enumeration type.");
            return Enum.TryParse<T>(str, true, out T val) ? val : default(T);
        }

        public static T ToEnum<T>(this int intValue) where T : struct, IConvertible
        {
            Type enumType = typeof(T);
            if (!enumType.IsEnum)
                throw new Exception("T must be an Enumeration type.");
            return (T)Enum.ToObject(enumType, intValue);
        }
    }
}
