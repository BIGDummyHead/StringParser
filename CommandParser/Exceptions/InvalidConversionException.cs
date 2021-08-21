using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandParser.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class InvalidConversionException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="convertFrom"></param>
        /// <param name="convertTo"></param>
        public InvalidConversionException(Type convertFrom, Type convertTo) : base($"Cannot convert '{convertFrom.Name}' to '{convertTo.Name}'")
        {

        }
    }
}
