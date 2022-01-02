using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CommandParser;

/// <summary>
/// Allows you to set a range of minimum and maximum arguments that can be passed into the last parameter.
/// </summary>
/// <remarks>Can only be applied to the last parameter</remarks>
public sealed class RangeAttribute : CommandParameterAttribute
{
    /// <summary>
    /// Number range
    /// </summary>
    public readonly int min, max;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <exception cref="Exception">Thrown when min > max or min/max lower than 1</exception>
    public RangeAttribute(int min, int max) : base(Importance.Medium)
    {
        //check if min is lower than max
        if (min > max)
            throw new Exception("Min cannot be higher than max.");

        //check if min is lower than 1
        else if (min < 1 || max < 1)
            throw new Exception("Max/Min cannot be lower than 1.");

        this.min = min;
        this.max = max;
    }

    /// <summary>
    /// Sets min to 1
    /// </summary>
    /// <param name="max">The max amount of arguments</param>
    public RangeAttribute(int max) : this(1, max)
    {

    }


#pragma warning disable
    public override async Task<string[]> OnCollect(ParameterInfo pInfo, string[] args, ParameterInfo[] parameters)
    {
        //check if the last parameter is the one we are looking for
        if (parameters[^1] != pInfo)
            return args;

        string[] collectedArgs = args[(parameters.Length - 1)..];
        //check if the amount of arguments is lower than the min
        if (collectedArgs.Length < min || collectedArgs.Length > max)
        {
            Handler.Options.ToLog("Cannot invoke because the arguments may be lower than the minimum or greater than maximum.", LogLevel.Information);
            return Array.Empty<string>(); //we want to do this because it is invalid so we want to just skip.
        }

        string joinedArgument = collectedArgs.Join();

        string[] copy = new string[args.Length - collectedArgs.Length + 1];

        Array.Copy(args, copy, copy.Length - 1);

        copy[^1] = joinedArgument;

        return copy;
    }
#pragma warning restore
}