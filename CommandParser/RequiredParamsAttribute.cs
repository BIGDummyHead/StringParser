using System;

namespace CommandParser
{
    /// <summary>
    /// An attribute to specify how many arguments need to be passed in
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    public sealed class RequiredParamsAttribute : Attribute
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

            ParamCount = pCount ;
        }
    }
}
