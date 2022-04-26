using System;
using System.Threading.Tasks;

namespace StringParser.Interfaces
{
    /// <summary>
    /// An interface for String to Object Conversion
    /// </summary>
    public interface IStringConverter
    {
        /// <summary>
        /// Check if a type can be converted from a registered <see cref="IConverter{T}"/>
        /// </summary>
        /// <param name="c">The type to check</param>
        /// <returns>True if the type has a registered converter.</returns>
        bool CanConvert(Type c);
        /// <summary>
        /// Check if a type can be converted from a registered <see cref="IConverter{T}"/>
        /// </summary>
        /// <typeparam name="T">The type to check</typeparam>
        /// <returns>True if the type has a registered converter.</returns>
        bool CanConvert<T>();
        /// <summary>
        /// Cast a string to a type instance
        /// </summary>
        /// <param name="before">Any arguments that come before, used for <see cref="IConverter{T}.Convert(object[], string, object[])"/></param>
        /// <param name="parse">Parsing</param>
        /// <param name="after">Any arguments that come after, used for <see cref="IConverter{T}.Convert(object[], string, object[])"/></param>
        /// <param name="castType">The type to cast to</param>
        /// <param name="converted">The converted object</param>
        /// <param name="error">Any errors found</param>
        /// <returns>True if a conversion was successful</returns>
        bool CastString(object[] before, string parse, object[] after, Type castType, out ValueTask<object> converted, out string error);
        /// <summary>
        /// Cast a string to a type instance
        /// </summary>
        /// <param name="before">Any arguments that come before, used for <see cref="IConverter{T}.Convert(object[], string, object[])"/></param>
        /// <param name="parse">Parsing</param>
        /// <param name="after">Any arguments that come after, used for <see cref="IConverter{T}.Convert(object[], string, object[])"/></param>
        /// <typeparam name="T">The type to cast to</typeparam>
        /// <param name="converted">The converted object</param>
        /// <param name="error">Any errors found</param>
        /// <returns>True if a conversion was successful</returns>
        bool CastString<T>(object[] before, string parse, object[] after, out ValueTask<T> converted, out string error);
        /// <summary>
        /// Dynamically create a <see cref="IConverter{T}"/> using a <see cref="Func{T1,T2,T3, TResult}"/>
        /// </summary>
        /// <typeparam name="T">The type the converter will return.</typeparam>
        /// <param name="converter">The dynamically created <see cref="IConverter{T}"/></param>
        void RegisterConverter<T>(Func<object[], string, object[], ValueTask<T>> converter);
        /// <summary>
        /// Register a <see cref="IConverter{T}"/> to the <seealso cref="IStringConverter"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="converter"></param>
        void RegisterConverter<T>(IConverter<T> converter);
        /// <summary>
        /// Unregister a converter of a specific type.
        /// </summary>
        /// <param name="converter">The type of converter</param>
        void UnRegisterConverter(Type converter);
        /// <summary>
        /// Unregister a converter of a specific type.
        /// </summary>
        /// <typeparam name="T">The type of converter</typeparam>
        void UnRegisterConverter<T>();
        /// <summary>
        /// Use a converter that exist.
        /// </summary>
        /// <param name="type">The type of converter</param>
        /// <param name="before">Arguments before used for <see cref="IConverter{T}.Convert(object[], string, object[])"/></param>
        /// <param name="parse"></param>
        /// <param name="after">Arguments after used for <see cref="IConverter{T}.Convert(object[], string, object[])"/></param>
        /// <param name="converted">The converted object.</param>
        /// <returns>True if the converter of <see cref="Type"/> <paramref name="type"/> exist.</returns>
        bool UseConverter(Type type, object[] before, string parse, object[] after, out ValueTask<object> converted);
        /// <summary>
        /// Use a converter that exist.
        /// </summary>
        /// <typeparam name="T">The type of converter</typeparam>
        /// <param name="before">Arguments before used for <see cref="IConverter{T}.Convert(object[], string, object[])"/></param>
        /// <param name="parse"></param>
        /// <param name="after">Arguments after used for <see cref="IConverter{T}.Convert(object[], string, object[])"/></param>
        /// <param name="converted">The converted object.</param>
        /// <returns>True if the converter of <typeparamref name="T"/> exist.</returns>
        bool UseConverter<T>(object[] before, string parse, object[] after, out ValueTask<T> converted);
    }
}