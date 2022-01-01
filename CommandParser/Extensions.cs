using System;
using System.Collections.Generic;

namespace CommandParser
{
    /// <summary>
    /// Extensions for the library
    /// </summary>
    public static class Extensions
    {
        internal static TVal GetValue<TVal>(this IReadOnlyDictionary<CommandInfo, TVal> dict, CommandInfo value)
        {
            foreach (KeyValuePair<CommandInfo, TVal> entry in dict)
            {
                if (entry.Key == value)
                {
                    return entry.Value;
                }
            }

            return default;
        }

        internal static bool Inherits(this Type inheritsB, Type b)
        {
            if (inheritsB.BaseType == b)
                return true;
            else if (inheritsB.BaseType == typeof(Object))
                return false;

            return Inherits(inheritsB, b.BaseType);
        }

        internal static string Join(this string[] join)
        {
            string joined = string.Empty;

            foreach (string joinItem in join)
            {
                joined += joinItem + " ";
            }

            return joined.Trim();
        }
    }
}
