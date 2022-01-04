using CommandParser.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace CommandParser
{
    /// <summary>
    /// Converts string into type instances
    /// </summary>
    public sealed class StringConverter
    {
        internal readonly List<ConverterHelper> helpers = new List<ConverterHelper>();

        /// <summary>
        /// Allows you to cast a string to a Type.  
        /// </summary>
        /// <typeparam name="T">Any type that can be handled by the <see cref="IConverter{T}"/> or simple conversions</typeparam>
        /// <param name="parse">Formatted string.</param>
        /// <param name="converted">The casted string</param>
        /// <returns>True if the cast was successful</returns>
        public bool CastString<T>(string parse, out T converted)
        {
            bool ret = CastString(parse, typeof(T), out object conversion, out _);

            converted = (T)conversion;
            return ret;
        }


        /// <summary>
        /// Cast a string to a type.
        /// </summary>
        /// <param name="from">Parsing</param>
        /// <param name="castType">Type to cast to</param>
        /// <param name="converted">Converted object</param>
        /// <param name="error">Any errors passed out</param>
        /// <returns>True if the cast was successful</returns>
        public bool CastString(string from, Type castType, out object converted, out string error)
        {
            if (UseConverter(castType, from, out converted))
            {
                error = string.Empty;
                return true;
            }

            return GlobalCastString(from, castType, out converted, out error);
        }

        /// <summary>
        /// Global cast string to type instance 
        /// </summary>
        /// <param name="from">Parsing</param>
        /// <param name="castType">Type to cast to</param>
        /// <param name="converted">Converted object</param>
        /// <param name="error">Any errors passed out</param>
        /// <returns>True if the cast was successful</returns>
        /// <remarks>Does not use any <see cref="IConverter{T}"/></remarks>
        public static bool GlobalCastString(string from, Type castType, out object converted, out string error)
        {
            try
            {
                if (castType.IsClass) //checks for default constructors with strings
                {
                    if (castType.GetConstructor(new Type[] { typeof(string) }) == null)
                        throw new Exceptions.InvalidConversionException(typeof(string), castType);

                    error = string.Empty;
                    converted = Activator.CreateInstance(castType, from);
                    return true;
                }

                //trys to convert string to type
                TypeConverter con = TypeDescriptor.GetConverter(castType);

                error = string.Empty;
                converted = con.ConvertFromString(from);
                return true;
            }
            catch (Exception e)
            {
                error = e.Message;

            }

            converted = null;
            return false;
        }

        /// <summary>
        /// Checks if a type can be converted from string, by checking registered conversion types
        /// </summary>
        /// <returns></returns>
        public bool CanConvert(Type c)
        {
            return helpers.Any(x => x.ConversionType == c);
        }

        /// <summary>
        /// Checks if a type can be converted from string, by checking registered conversion types
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool CanConvert<T>()
        {
            return CanConvert(typeof(T));
        }

        /// <summary>
        /// Uses a converter in the registration
        /// </summary>
        public bool UseConverter(Type type, string parse, out object converted)
        {
            if (!CanConvert(type))
            {
                converted = default;
                return false;
            }

            converted = helpers.FirstOrDefault(x => x.ConversionType == type).Convert(parse);
            return true;
        }

        /// <summary>
        /// Uses a converter in the registration
        /// </summary>
        public bool UseConverter<T>(string parse, out T converted)
        {
            bool ret = UseConverter(typeof(T), parse, out object con);

            converted = default;

            if (ret)
                converted = (T)con;

            return ret;
        }

        /// <summary>
        /// Register a generic converter that can convert a string type into your T type.
        /// </summary>
        /// <typeparam name="T">Conversion choice</typeparam>
        /// <param name="converter">Provided handler</param>
        public void RegisterConverter<T>(IConverter<T> converter)
        {
            if (CanConvert<T>())
                return;

            helpers.Add(ConverterHelper.Create(converter));
        }

        /// <summary>
        /// Register a lambda converter
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="converter"></param>
        public void RegisterConverter<T>(Func<string, T> converter)
        {

            ConverterHelper helper = ConverterHelper.Create(converter);

            if (CanConvert<T>())
                return;

            helpers.Add(helper);
        }

        /// <summary>
        /// Gets rid of a converter of a specific type
        /// </summary>
        /// <param name="converter"></param>

        public void UnRegisterConverter(Type converter)
        {
            if (!CanConvert(converter))
                return;

            helpers.Remove(helpers.FirstOrDefault(x => x.ConversionType == converter));
        }

        /// <summary>
        /// Gets rid of a converter of specific type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void UnRegisterConverter<T>() => UnRegisterConverter(typeof(T));
    }
}
