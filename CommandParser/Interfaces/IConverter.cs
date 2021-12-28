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
        /// <returns>String parsed to T type</returns>
        T Convert(string parse);
    }

    internal struct ConverterHelper
    {
        public Type ConversionType { get; private set; }

        public Func<string, object> Convert { get; private set; }

        public static ConverterHelper Create<T>(IConverter<T> converter)
        {
            return new ConverterHelper
            {
                Convert = delegate (string parse)
                {
                    return converter.Convert(parse);
                },
                ConversionType = typeof(T)
            };
        }

        public static ConverterHelper Create<T>(Func<string, T> converter)
        {
            return new ConverterHelper
            {
                Convert = delegate (string parse)
                {
                    return converter(parse);
                },
                ConversionType = typeof(T)
            };
        }
    }

}
