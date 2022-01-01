using System;
using System.Reflection;
using System.Threading.Tasks;

namespace CommandParser
{
    /// <summary>
    /// Applied to your final Command parameter, allows you to specify how many arguments may be passed in but not required.
    /// </summary>
    public sealed class UpToAttribute : CommandParameterAttribute
    {
        private readonly int allowed;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="allowedParamCount">Parameters allowed</param>
        /// <exception cref="Exception">Thrown if <paramref name="allowedParamCount"/>is less than 1</exception>
        public UpToAttribute(int allowedParamCount) : base(Importance.Low)
        {
            if (allowedParamCount < 1)
                throw new Exception("Cannot have a less than 1 parameter.");

            allowed = allowedParamCount;
        }

#pragma warning disable
        /// <summary>
        /// </summary>
        /// <param name="pInfo"></param>
        /// <param name="args"></param>
        public override async Task<string[]> OnCollect(ParameterInfo pInfo, string[] args, ParameterInfo[] parameters)
        {
            if (parameters[^1] != pInfo)
                return args;
            else if (allowed == 1 || pInfo.GetCustomAttribute<UpToAttribute>() == null || pInfo.GetCustomAttribute<RemainingTextAttribute>() != null)
                return args;

            string[] lastPs = args[(parameters.Length - 1)..];

            if (lastPs.Length > allowed)
                return args;

            int parsAllowed = lastPs.Length;

            string joinedArgument = lastPs.Join(); //correctly set

            string[] copy = new string[args.Length - parsAllowed + 1];

            Array.Copy(args, copy, copy.Length - 1);

            copy[^1] = joinedArgument;

            return copy; //does not do something?
        }
#pragma warning restore
    }


}
