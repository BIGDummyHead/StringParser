using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandParser.Exceptions
{
    /// <summary>
    /// When a module is invalid to be registered
    /// </summary>
    public sealed class InvalidModuleException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="middle"></param>
        public InvalidModuleException(Type type, string middle) : base($"'{type.Name}' is invalid because {middle}")
        {

        }
    }
}
