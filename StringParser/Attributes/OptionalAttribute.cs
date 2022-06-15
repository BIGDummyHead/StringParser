namespace StringParser;
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="max">Max amount of parameters.</param>
    public OptionalAttribute(int max) : base(0, max)
    {

    }
}