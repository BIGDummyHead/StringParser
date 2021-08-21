using System;
using System.Collections.Generic;

namespace CommandParser
{
    internal static class Extensions
    {
        public static bool ContainsKey<TVal>(this IReadOnlyDictionary<string, TVal> dict, string value, StringComparison comparison)
        {
            foreach (string val in dict.Keys)
            {
                if (val.Equals(value, comparison))
                {
                    return true;
                }
            }

            return false;
        }

        public static TVal GetValue<TVal>(this IReadOnlyDictionary<string, TVal> dict, string value, StringComparison comparison)
        {
            foreach (KeyValuePair<string, TVal> entry in dict)
            {
                if (entry.Key.Equals(value, comparison))
                {
                    return entry.Value;
                }
            }

            return default;
        }

        public static bool Inherits(this Type inheritsB, Type b)
        {
            if (inheritsB.BaseType == b)
                return true;
            else if (inheritsB.BaseType == typeof(Object))
                return false;

            return Inherits(inheritsB, b.BaseType);
        }
    }
}
