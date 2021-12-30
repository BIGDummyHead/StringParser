using System;

namespace CommandParser
{
    /// <summary>
    /// Configuration for <see cref="CommandHandler"/>
    /// </summary>
    public sealed class HandlerConfig
    {
        /// <summary>
        /// The global default Config that can be set and gotten.
        /// </summary>
        public static HandlerConfig Default 
        {
            get
            {
                return new HandlerConfig(_global);
            }

            set
            {
                if(value == null)
                    return;

                    _global = value;
            }
        }

        private static HandlerConfig _global = new HandlerConfig();

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
    
        /// <summary>
        ///Copies the values from another config
        /// </summary>
        public HandlerConfig(HandlerConfig config)
        {
            Prefix = config.Prefix;
            ByPopularVote = config.ByPopularVote;
            IgnoreCase = config.IgnoreCase;
        }

    }
}
