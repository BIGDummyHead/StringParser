using StringParser.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StringParser
{
    /// <summary>
    /// Extensions for the library
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Registers a lambda converter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler"></param>
        /// <param name="converter"></param>
        public static void RegisterConverter<T>(this Handler handler, Func<object[], string, object[], ValueTask<T>> converter)
        {
            handler.Converter.RegisterConverter(converter);
        }

        /// <summary>
        /// Fast array maker
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public static T[] FastArr<T>(params T[] items)
        {
            return items;
        }   

        /// <summary>
        /// Fast array maker
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static object[] FastArr(params object[] items)
        {
            return items;
        }

        /// <summary>
        /// Registers a generic converter
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="converter"></param>
        public static void RegisterConverter<T>(this Handler handler, IConverter<T> converter)
        {
            handler.Converter.RegisterConverter(converter);
        }

        /// <summary>
        /// Gets rid of a registered converter
        /// </summary>
        /// <param name="handler"></param>
        /// <param name="converterType"></param>
        public static void UnRegisterConverter(this Handler handler, Type converterType)
        {
            handler.Converter.UnRegisterConverter(converterType);
        }

        /// <summary>
        /// Gets rid of a registered converter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler"></param>
        public static void UnRegisterConverter<T>(this Handler handler)
        {
            handler.Converter.UnRegisterConverter<T>();
        }

        internal static int IndexOf<T>(this T[] indexer, T get)
        {
            for (int i = 0; i < indexer.Length; i++)
            {
                if (indexer[i].Equals(get))
                    return i;
            }

            return -1;
        }
        internal static T[] RemoveWhen<T>(this T[] ar, Func<T, bool> pred)
        {
            for (int i = 0; i < ar.Length; i++)
            {
                if (!pred(ar[i]))
                    ar[i] = default;
            }

            return ar.Where(x => x != null).ToArray();
        }

        internal static TVal GetValue<TVal>(this IReadOnlyDictionary<CollectedCommand, TVal> dict, CollectedCommand value)
        {
            foreach (KeyValuePair<CollectedCommand, TVal> entry in dict)
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
            if (b.IsInterface && inheritsB.GetInterface(b.Name) != null)
                return true;
            else if (inheritsB.BaseType == b)
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

        internal static int Stop(this int a, int min)
        {
            return a < min ? min : a;
        }
    }
}
