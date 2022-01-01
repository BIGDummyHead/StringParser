using System;
using System.Reflection;
using System.Threading.Tasks;

namespace CommandParser
{
    /// <summary>
    /// A special attribute, used for remaining text for a command, applied to the very last argument.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class RemainingTextAttribute : CommandParameterAttribute
    {

#pragma warning disable 
        public override async Task<string[]> OnCollect(ParameterInfo pInfo, string[] args, ParameterInfo[] parameters)
        {
            if (pInfo == parameters[^1]) //check if the parameter is the last
            {
                string lastArguments = args[(parameters.Length - 1)..].Join();

                string[] copy = new string[parameters.Length];

                Array.Copy(args, copy, parameters.Length - 1);

                copy[^1] = lastArguments;

                return copy;
            }

            return args;
        }
#pragma warning enable
    }
}
