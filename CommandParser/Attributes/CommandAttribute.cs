using System;
using System.Reflection;

namespace CommandParser
{
    /// <summary>
    /// The command attribute, used over any method within a <see cref="ICommandModule"/>
    /// </summary>
    /// <remarks>This command is special and cannot be replicated.</remarks>
    public class CommandAttribute : BaseCommandAttribute
    {
        /// <summary>
        /// The name of the command
        /// </summary>
        public string CommandName { get; internal set; }

        /// <summary>
        /// Is the command using the name of the method to invoke?
        /// </summary>
        public bool UsingMethodName { get; } = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">The name of the command</param>
        public CommandAttribute(string name)
        {
            CommandName = name;
        }

        /// <summary>
        /// Use the method name instead of custom name
        /// </summary>
        public CommandAttribute()
        {
            UsingMethodName = true;
        }

        /// <summary>
        /// When a command is unregistered
        /// </summary>
        /// <param name="from">The type that contained this command</param>
        /// <param name="method">The method that the command was on</param>
        public virtual void OnUnRegister(Type from, MethodInfo method)
        {
            
        }

        /// <summary>
        /// When a command is registered
        /// </summary>
        /// <param name="from">The type that contained this command</param>
        /// <param name="method">The method that the command was on</param>
        public virtual void OnRegister(Type from, MethodInfo method)
        {

        }
    }
}
