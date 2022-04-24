namespace CommandParser;
/// <summary>
/// Creates an optional parameter.
/// </summary>
/// <remarks>Can only be applied to the last string parameter.</remarks>
public sealed class OptionalAttribute : RangeAttribute
{
    /// <summary>
    /// 
    /// </summary>
    public OptionalAttribute() : base(0, 1)
    {

    }
}