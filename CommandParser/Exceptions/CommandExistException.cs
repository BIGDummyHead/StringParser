using System;

namespace CommandParser.Exceptions
{
    /// <summary>
    /// Thrown when a command exist
    /// </summary>
    public sealed class CommandExistException : Exception
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="commandName"></param>
        public CommandExistException(string commandName) : base("Command exist", new Exception($"'{commandName} already exist'"))
        {

        }
    }
}
