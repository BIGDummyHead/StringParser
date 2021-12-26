using System.Reflection;

namespace CommandParser
{
    /// <summary>
    /// An attribute that is collected to provide information to the CommandHandler about chaning the arguments and such
    /// </summary>
    public class AdvancedCommandAttribute : BaseCommandAttribute
    {
        /// <summary>
        /// When this attribute is collected in the <see cref="CommandHandler"/>
        /// </summary>
        /// <param name="pInfo">Parameters passed into the Command</param>
        /// <param name="args">Arguments</param>
        public virtual void OnCollect(ParameterInfo[] pInfo, ref string[] args)
        {

        }
    }
}
