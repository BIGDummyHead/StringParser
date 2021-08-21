namespace CommandParser
{
    /// <summary>
    /// The command attribute, used over any method within a <see cref="BaseCommandModule"/>
    /// </summary>
    public sealed class CommandAttribute : BaseCommandAttribute
    {
        /// <summary>
        /// The name of the command
        /// </summary>
        public string CommandName { get; internal set; }

        /// <summary>
        /// Is the command using the name of the method to invoke?
        /// </summary>
        public bool UsingMethodName { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">The name of the command</param>
        public CommandAttribute(string name)
        {
            CommandName = name;
            UsingMethodName = false;
        }

        /// <summary>
        /// Use the method name instead of custom name
        /// </summary>
        public CommandAttribute()
        {
            UsingMethodName = true;
        }
    }
}
