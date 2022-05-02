using System;

namespace StringParser.Exceptions
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
        /// <param name="reason"></param>
        public InvalidModuleException(Type type, string reason) : base($"'{type.Name}' is invalid because {reason}")
        {

        }
    }
}
