using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CommandParser
{
    /// <summary>
    /// An attribute to specify how many arguments need to be passed in.
    /// </summary>
    public sealed class RequiredParamsAttribute : CommandParameterAttribute
    {
        /// <summary>
        /// The amount of Arguments that are required for the parameter
        /// </summary>
        public int ParamCount { get; private set; }

        /// <summary>
        /// Specify how many arguments need to be passed in
        /// </summary>
        public RequiredParamsAttribute(int pCount) : base(Importance.Critical)
        {
            if (pCount <= 1)
                throw new Exception("Parameter count cannot be equal or less than 1");

            ParamCount = pCount;
        }

#pragma warning disable
        public override async Task<string[]> OnCollect(ParameterInfo pInfo, object[] bef, string[] args, object[] aft, ParameterInfo[] parameters)
        {
            int index = parameters.IndexOf(pInfo);

            string joined = args[index..(index + ParamCount)].Join();

            for (int i = index + 1; i < index + ParamCount; i++)
                args[i] = null;

            args[index] = joined;

            return args.Where(x => x != null).ToArray();
        }
#pragma warning restore
    }
}
