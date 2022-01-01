using System;

namespace CommandParser
{
    /// <summary>
    /// Ignore this command from being registered.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class IgnoreAttribute : Attribute
    {

    }
}
