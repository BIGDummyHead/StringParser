using System.Reflection;
using System.Threading.Tasks;

namespace CommandParser
{
    /// <summary>
    /// An attribute that is collected to provide information to the CommandHandler about chaning the arguments and such
    /// </summary>
    public class AdvancedCommandAttribute : BaseCommandAttribute
    {
#pragma warning disable CS1998 //async method does not contain await.
        /// <summary>
        /// When this attribute is collected in the <see cref="CommandHandler"/>
        /// </summary>
        /// <param name="pInfo">Parameters passed into the Command</param>
        /// <param name="args">Arguments</param>
        public virtual async Task<string[]> OnCollect(ParameterInfo[] pInfo, string[] args)
        {
            return args;
        }
#pragma warning restore CS1998
    }
}
