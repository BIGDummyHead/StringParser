using StringParser.Interfaces;
using System;

namespace StringParser
{
    /// <summary>
    /// Configuration for <see cref="Handler"/>
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
        public bool HasPrefix => !string.IsNullOrWhiteSpace(Prefix);

        /// <summary>
        /// Determines if one false from <see cref="BaseCommandAttribute.BeforeCommandExecute(object, object[])"/> will stop a command from being invoked
        /// </summary>
        /// <remarks>Set to true by default</remarks>
        public bool ByPopularVote { get; set; } = true;

        /// <summary>
        /// The logger used for the config and the <see cref="Handler"/>
        /// </summary>
        public ILogger Logger { get; set; } = new Logger();

        /// <summary>
        /// Char that splits the commands!
        /// </summary>
        /// <remarks>Set to ' ' by default</remarks>
        public char[] Separator { get; set; } = new char[] { ' ' };

        /// <summary>
        /// Uses the <see cref="string.Trim()"/> when invoking <seealso cref="Handler.Invoke(string)"/>
        /// </summary>
        /// <remarks>Set to true by default</remarks>
        public bool AlwaysTrim { get; set; } = true;

        /// <summary>
        /// Allow null conversions to invoke a method.
        /// </summary>
        /// <remarks>Set to false by default</remarks>
        public bool AllowNulls { get; set; } = false;
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
        /// A config for <see cref="Handler"/>
        /// </summary>
        public HandlerConfig()
        {
        }

        /// <summary>
        /// Copies values from another config
        /// </summary>
        /// <param name="other">The other config to copy from!</param>
        public HandlerConfig(HandlerConfig other)
        {
            Prefix = other.Prefix;
            ByPopularVote = other.ByPopularVote;
            Logger = other.Logger;
            Separator = other.Separator;
            AlwaysTrim = other.AlwaysTrim;
            AllowNulls = other.AllowNulls;
            IgnoreCase = other.IgnoreCase;
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
