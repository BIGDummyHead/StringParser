using System;
using System.Threading.Tasks;

namespace CommandParser.Interfaces
{
    /// <summary>
    /// Helps handle conversions easier
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IConverter<T>
    {
        /// <summary>
        /// Convert from string to T
        /// </summary>
        /// <param name="parse">String to parse</param>
        /// <param name="afterArgs">Args that were passed in after</param>
        /// <param name="previousArgs">Args that were passed in before</param>
        /// <returns>String parsed to T type</returns>
        ValueTask<T> Convert(object[] previousArgs, string parse, object[] afterArgs);
    }

    internal struct ConverterHelper
    {
        public Type ConversionType { get; private set; }

        public Func<object[], string, object[], ValueTask<object>> Convert { get; private set; }

        public static ConverterHelper Create<T>(IConverter<T> converter)
        {
            return new ConverterHelper
            {
                Convert = async delegate (object[] before, string parse, object[] after)
                {
                    return await converter.Convert(before, parse, after);
                },
                ConversionType = typeof(T)
            };
        }

        public static ConverterHelper Create<T>(Func<string, ValueTask<T>> converter)
        {
            return new ConverterHelper
            {
                Convert = async delegate (object[] before, string parse, object[] after)
                {
                    return await converter(parse);
                },
                ConversionType = typeof(T)
            };
        }
    }

}
