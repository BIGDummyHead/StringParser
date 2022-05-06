using System;
using System.Reflection;
using System.Threading.Tasks;

namespace StringParser;

/// <summary>
/// Allows you to set a range of minimum and maximum arguments that can be passed into the last string parameter.
/// </summary>
/// <remarks>Can only be applied to the last parameter</remarks>
public class RangeAttribute : CommandParameterAttribute
{
    /// <summary>
    /// Number range
    /// </summary>
    public readonly int min, max;

    const int m_min = 0;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <exception cref="Exception">Thrown when min > max or min/max lower than 1</exception>
    public RangeAttribute(int min, int max) : base()
    {
        //check if min is lower than max
        if (min > max)
            throw new Exception("Min cannot be higher than max.");

        //check if min is lower than 1
        else if (min < m_min || max < m_min)
            throw new Exception($"Max/Min cannot be lower than {m_min}.");

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

    /// <summary>
    /// Sets the min to 0 and max to <see cref="int.MaxValue"/>
    /// </summary>
    public RangeAttribute() : this(0, int.MaxValue)
    {

    }


#pragma warning disable
    public override async Task<string[]> OnCollect(ParameterInfo pInfo, object[] bef, string[] args, object[] aft, ParameterInfo[] parameters)
    {
        int stringArgLen = parameters.Length - bef.Length - aft.Length;

        //check if the last parameter is the one we are looking for
        if (parameters[stringArgLen - 1] != pInfo)
        {
            //Invalid use on {pInfo.Name}. {typeof(Range).Name} must be used on the last parameter ({pparameters[^1].Name}
            Handler.UserConfig.Logger?.Log($"Invalid use on {pInfo.Name}. {typeof(Range).Name} attribute must be used on the last parameter ({parameters[^1].Name})", LogLevel.Warning);
            return args;
        }

        string[] collectedArgs = args[(stringArgLen - 1)..];

        //check if the amount of arguments is lower than the min

        if (args.Length < stringArgLen && min == 0)
        {
            string[] empCopy = new string[args.Length + 1];
            Array.Copy(args, empCopy, args.Length);
            empCopy[^1] = "";
            return empCopy;
        }
        else if (collectedArgs.Length < min || collectedArgs.Length > max)
        {
            string _base = $"Invocation may not be possible because the arguments are ";
            string _add = collectedArgs.Length < min ? $"lower than {min}" : $"greater than {max}";
            Handler.UserConfig.Logger?.Log(_base + _add, LogLevel.Information);
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