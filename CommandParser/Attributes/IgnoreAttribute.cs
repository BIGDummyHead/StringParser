using System;

namespace CommandParser
{
    /// <summary>
    /// Ignore this command from being registered.
    /// </summary>
    /// <remarks>This command is special and cannot be replicated.</remarks>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class IgnoreAttribute : Attribute
    {

    }
}
