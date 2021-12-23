using System;

namespace CommandParser
{
    /// <summary>
    /// A special attribute, used for remaining text for a command, applied to the very last argument.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple =false)]
    public sealed class RemainingTextAttribute : Attribute
    {

    }
}
