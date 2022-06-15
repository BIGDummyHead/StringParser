using System;
using System.Reflection;
using System.Threading.Tasks;

namespace StringParser
{
    /// <summary>
    /// An attribute that is collected to provide information to the CommandHandler about chaning the arguments and such
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public class CommandParameterAttribute : Attribute
    {
        /// <summary>
        /// The instance of the handler used to invoke the <see cref="OnCollect(ParameterInfo, object[], string[], object[], ParameterInfo[])"/> method
        /// </summary>
        public Handler Handler { get; internal set; }

        //a ctor with the importance parameter
        /// <summary>
        /// </summary>
        public CommandParameterAttribute()
        {
        }

#pragma warning disable CS1998 //async method does not contain await.
        /// <summary>
        /// When this attribute is collected in the <see cref="StringParser.Handler"/>
        /// </summary>
        /// <param name="applied">Parameters passed into the Command</param>
        /// <param name="args">Arguments</param>
        /// <param name="methodParameters">Parameters of the method</param>
        /// <param name="preArgs">Parameters supplied before string</param>
        /// <param name="aftArgs">Paramaters supplied after string</param>
        public virtual async Task<string[]> OnCollect(ParameterInfo applied, object[] preArgs, string[] args, object[] aftArgs, ParameterInfo[] methodParameters)
        {
            return args;
        }




#pragma warning restore CS1998
    }




}
