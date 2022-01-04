namespace CommandParser;

/// <summary>
/// A special attribute, used for remaining text for a command, applied to the very last argument.
/// </summary>
public sealed class RemainingTextAttribute : RangeAttribute
{
    /// <summary>
    /// <see cref="RangeAttribute(int, int)"/> 1, <seealso cref="int.MaxValue"/>
    /// </summary>
    /// <param name="requiredParam">Whether or not you need to provide a parameter to invoke</param>
    public RemainingTextAttribute(bool requiredParam = true) : base(requiredParam ? 1 : 0, int.MaxValue)
    {

    }
}
