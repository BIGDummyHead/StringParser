using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CommandParser
{
    /// <summary>
    /// An attribute to specify how many arguments need to be passed in
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    public sealed class RequiredParamsAttribute : CommandParameterAttribute
    {
        /// <summary>
        /// The amount of Arguments that are required for the parameter
        /// </summary>
        public int ParamCount { get; private set; }

        /// <summary>
        /// Specify how many arguments need to be passed in
        /// </summary>
        public RequiredParamsAttribute(int pCount)
        {
            if (pCount <= 1)
                throw new Exception("Parameter count cannot be equal or less than 1");

            ParamCount = pCount;
        }

#pragma warning disable
        public override async Task<string[]> OnCollect(ParameterInfo pInfo, string[] args, ParameterInfo[] parameters)
        {
            if (args.Length > parameters.Length)
            {
                for (int p = 0; p < parameters.Length; p++)
                {
                    int to = ParamCount + p;

                    if (to > args.Length)
                    {
                        return args; //special exception instead of throwing!
                    }

                    string joinedArgument = args[p..to].Join();

                    //we must replace the item at a specific position
                    int newArgCount = args.Length - ParamCount + 1;

                    args[p] = joinedArgument.Trim();

                    //the rest of the arguments
                    for (int i = p + 1; i < p + ParamCount; i++)
                    {
                        args[i] = null;
                    }

                    return args.Where(x => x != null).ToArray();
                }
            }

            return args;
        }
#pragma warning restore
    }
}
