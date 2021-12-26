using System;

namespace CommandParser
{
    /// <summary>
    /// Configuration for <see cref="CommandHandler"/>
    /// </summary>
    public sealed class HandlerConfig
    {
        /// <summary>
        /// A default config 
        /// </summary>
        /// <remarks>Creates a new instance of <see cref="HandlerConfig"/> on get.</remarks>
        public static HandlerConfig Default => new HandlerConfig()
        {
        };

        /// <summary>
        /// The prefix before each command, does not effect the names of your commands.
        /// </summary>
        public string Prefix { get; init; } = "";

        /// <summary>
        /// Does your command require a prefix to be invoked
        /// </summary>
        public bool HasPrefix => Prefix != string.Empty;

        /// <summary>
        /// Determines if one false from <see cref="BaseCommandAttribute.BeforeCommandExecute(object, object[])"/> will stop a command from being invoked
        /// </summary>
        /// <remarks>True by default</remarks>
        public bool ByPopularVote { get; set; } = true;

        /// <summary>
        /// Writes any errors to this 
        /// </summary>
        public event Action<string> OnLog;

        internal void SendMessage(string msg)
        {
            OnLog?.Invoke(msg);
        }

        /// <summary>
        /// Ignore case when invoking commands
        /// </summary>
        /// <remarks>Defaults True</remarks>
        public bool IgnoreCase { get; set; } = true;

        internal StringComparison comp => IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

        /// <summary>
        /// A config for <see cref="CommandHandler"/>
        /// </summary>
        public HandlerConfig()
        {
        }
    }
}
