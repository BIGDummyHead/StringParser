using System;
using System.Collections.Generic;
using System.Reflection;
using CommandParser.Info;

namespace CommandParser
{
    internal class EnumCompare<T> : IComparer<T> where T : Enum
    {
        public int Compare(T x, T y)
        {
            int numX = Convert.ToInt32(x);
            int numY = Convert.ToInt32(y);

            return numX < numY ? -1 : (numX > numY ? 1 : 0);
        }
    }

    /// <summary>
    /// Extensions for the library
    /// </summary>
    public static class Extensions
    {

        /// <summary>
        /// Gets all information about a module.
        /// </summary>
        public static IEnumerable<Collector> GetInfoOnModule(this CommandHandler handler, bool includeIgnore, Func<MethodInfo, object> collectOthers = null)
        {
            foreach (Type module in handler.Modules)
            {
                foreach (Collector c in Collector.GetInfo(module, includeIgnore, collectOthers))
                {
                    yield return c;
                }
            }
        }

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

            return joined.Trim() ;
        }
    }
}
