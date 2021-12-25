using System;
using System.Collections.Generic;

namespace CommandParser
{
    internal static class Extensions
    {
        public static int GetIndex<T>(this IEnumerable<T> indexing, Func<T, bool> predicate)
        {
            int index = 0;
            foreach (T item in indexing)
            {
                if (predicate(item))
                    return index;

                index++;
            }

            return -1;
        }

        public static bool ContainsKey<TVal>(this IReadOnlyDictionary<CommandInfo, TVal> dict, CommandInfo value)
        {
            foreach (CommandInfo val in dict.Keys)
            {
                if (val == value)
                {
                    return true;
                }
            }

            return false;
        }

        public static TVal GetValue<TVal>(this IReadOnlyDictionary<CommandInfo, TVal> dict, CommandInfo value)
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

        public static bool Inherits(this Type inheritsB, Type b)
        {
            if (inheritsB.BaseType == b)
                return true;
            else if (inheritsB.BaseType == typeof(Object))
                return false;

            return Inherits(inheritsB, b.BaseType);
        }

        public static bool InRange(this int a, int min, int max)
        {
            if (max < min)
                return false;
            else if (max == min)
                return true;

            return a <= max && a >= min;
        }

    }
}
