using CommandParser.Interfaces;
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
                if (value == null)
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
        /// The logger used for the config and the <see cref="CommandHandler"/>
        /// </summary>
        public ILog Logger { get; set; }

        /// <summary>
        /// Char that splits the commands!
        /// </summary>
        public char[] Separator { get; set; } = new char[] { ' ' };

        /// <summary>
        /// Uses the <see cref="string.Trim()"/> when invoking <seealso cref="CommandHandler.Invoke(string)"/>
        /// </summary>
        public bool AlwaysTrim { get; set; } = true;

        /// <summary>
        /// Sends a message to all Loggers in the HandlerConfig
        /// </summary>
        /// <param name="msg">The message to send</param>
        /// <param name="level">The type of log</param>
        internal void ToLog(string msg, LogLevel level)
        {
            Logger.Log(msg, level);

            switch (level)
            {
                case LogLevel.Debug:
                    Logger.LogDebug(msg);
                    break;
                case LogLevel.Information:
                    Logger.LogInfo(msg);
                    break;
                case LogLevel.Warning:
                    Logger.LogWarning(msg);
                    break;
                case LogLevel.Error:
                    Logger.LogError(msg);
                    break;
            }
        }

        /// <summary>
        /// Ignore case when invoking commands
        /// </summary>
        /// <remarks>Defaults True</remarks>
        public bool IgnoreCase { get; set; } = true;

        internal StringComparison Comp => IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

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
            Separator = config.Separator;
            AlwaysTrim = config.AlwaysTrim;
        }

    }

    /// <summary>
    /// Logging levels
    /// </summary>
    public enum LogLevel
    {
#pragma warning disable
        Debug,
        Information,
        Warning,
        Error
#pragma warning restore
    }
}
