using StringParser.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace StringParser
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member. Intentional IStringConverter contains XML Definitions
    public sealed class StringConverter : IStringConverter
    {
        /// <summary>
        /// Global cast string to type instance 
        /// </summary>
        /// <param name="from">Parsing</param>
        /// <param name="castType">Type to cast to</param>
        /// <param name="converted">Converted object</param>
        /// <param name="error">Any errors passed out</param>
        /// <returns>True if the cast was successful</returns>
        /// <remarks>Does not use any <see cref="IConverter{T}"/></remarks>
        public static bool GlobalCastString(string from, Type castType, out ValueTask<object> converted, out string error)
        {
            try
            {
                if (castType.IsClass) //checks for default constructors with strings
                {
                    if (castType.GetConstructor(new Type[] { typeof(string) }) == null)
                    {
                        throw new Exceptions.InvalidConversionException(typeof(string), castType);
                    }

                    error = string.Empty;
                    converted = ValueTask.FromResult(Activator.CreateInstance(castType, from));
                    return true;
                }

                //trys to convert string to type
                TypeConverter con = TypeDescriptor.GetConverter(castType);

                error = string.Empty;
                converted = ValueTask.FromResult(con.ConvertFromString(from));
                return true;
            }
            catch (Exception e)
            {
                error = e.Message;

            }

            converted = ValueTask.FromResult<object>(null);
            return false;
        }


        internal readonly List<ConverterHelper> helpers = new();

        /// <summary>
        /// Types registered for conversion
        /// </summary>
        public IReadOnlyDictionary<Type, Func<object[], string, object[], ValueTask<object>>> RegisteredTypes => helpers.ToDictionary(x => x.ConversionType, x => x.Convert);

        public bool CastString<T>(object[] before, string parse, object[] after, out ValueTask<T> converted, out string error)
        {
            bool ret = CastString(before, parse, after, typeof(T), out ValueTask<object> conversion, out error);

            converted = ValueTask.FromResult((T)conversion.Result);
            return ret;
        }

        public bool CastString(object[] before, string from, object[] after, Type castType, out ValueTask<object> converted, out string error)
        {
            if (UseConverter(castType, before, from, after, out converted))
            {
                error = string.Empty;
                return true;
            }

            return GlobalCastString(from, castType, out converted, out error);
        }

        public bool CanConvert(Type c)
        {
            return helpers.Any(x => x.ConversionType == c);
        }

        public bool CanConvert<T>()
        {
            return CanConvert(typeof(T));
        }

        public bool UseConverter(Type type, object[] before, string parse, object[] after, out ValueTask<object> converted)
        {
            if (!CanConvert(type))
            {
                converted = default;
                return false;
            }

            converted = helpers.FirstOrDefault(x => x.ConversionType == type).Convert(before, parse, after);
            return true;
        }

        public bool UseConverter<T>(object[] before, string parse, object[] after, out ValueTask<T> converted)
        {
            bool ret = UseConverter(typeof(T), before, parse, after, out ValueTask<object> con);

            converted = default;

            if (ret)
            {
                converted = ValueTask.FromResult((T)con.Result);
            }

            return ret;
        }

        public void RegisterConverter<T>(IConverter<T> converter)
        {
            if (CanConvert<T>())
            {
                return;
            }

            helpers.Add(ConverterHelper.Create(converter));
        }

        public void RegisterConverter<T>(Func<object[], string, object[], ValueTask<T>> converter)
        {
            if (CanConvert<T>())
            {
                return;
            }

            ConverterHelper helper = ConverterHelper.Create(converter);

            helpers.Add(helper);
        }

        public void UnRegisterConverter(Type converter)
        {
            if (!CanConvert(converter))
            {
                return;
            }

            _ = helpers.Remove(helpers.FirstOrDefault(x => x.ConversionType == converter));
        }

        public void UnRegisterConverter<T>()
        {
            UnRegisterConverter(typeof(T));
        }
#pragma warning restore CS1591 //Intentional IStringConverter contains XML Definitions
    }
}
