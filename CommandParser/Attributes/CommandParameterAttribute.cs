using System;
using System.Reflection;
using System.Threading.Tasks;

namespace CommandParser
{
    /// <summary>
    /// An attribute that is collected to provide information to the CommandHandler about chaning the arguments and such
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class CommandParameterAttribute : Attribute
    {

        /// <summary>
        /// The level of importance of the parameter
        /// </summary>
        public readonly Importance importance;

        //a ctor with the importance parameter
        /// <summary>
        /// </summary>
        public CommandParameterAttribute(Importance importance)
        {
            this.importance = importance;
        }

#pragma warning disable CS1998 //async method does not contain await.
        /// <summary>
        /// When this attribute is collected in the <see cref="CommandHandler"/>
        /// </summary>
        /// <param name="pInfo">Parameters passed into the Command</param>
        /// <param name="args">Arguments</param>
        /// <param name="parameters">Parameters of the method</param>
        public virtual async Task<string[]> OnCollect(ParameterInfo pInfo, string[] args, ParameterInfo[] parameters)
        {
            return args;
        }

                


#pragma warning restore CS1998
    }

    


}
