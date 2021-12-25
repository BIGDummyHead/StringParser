using System;

namespace CommandParser
{
    /// <summary>
    /// A base attribute used for attributes above commands.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public abstract class BaseCommandAttribute : Attribute
    {

        /// <summary>
        /// Before the command is executed
        /// </summary>
        /// <param name="classInstance">The instance the command will be invoked with</param>
        /// <param name="methodParams">The parameters the command will be invoked with</param>
        /// <returns>Should the command continue?</returns>
        /// <remarks>If returns false, the command will not execute and neither will <see cref="AfterCommandExecute(object, object[], object?)"/></remarks>
        public virtual bool BeforeCommandExecute(object classInstance, object[] methodParams)
        {
            return true;
        }

#nullable enable
        /// <summary>
        /// After the command has executed
        /// </summary>
        /// <param name="classInstance">The instance the command was invoked with</param>
        /// <param name="methodParams">The parameters the command was invoked with</param>
        /// <param name="returnInstance">Return instance of the command if any</param>
        public virtual void AfterCommandExecute(object classInstance, object[] methodParams, object? returnInstance)
        {

        }
#nullable disable
    }
}
