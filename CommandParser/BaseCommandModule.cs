using System.Reflection;

namespace CommandParser
{
    /// <summary>
    /// A base for all command modules
    /// </summary>
    public abstract class BaseCommandModule
    {
        /// <summary>
        /// Called when any in the module is invoked. 
        /// </summary>
        /// <param name="method">The method invoked</param>
        /// <param name="instance">The instance used to invoke the <paramref name="method"/></param>
        /// <param name="invokes">The parameters used to invoke the <paramref name="method"/></param>
        public virtual void OnCommandExecute(MethodInfo method, object instance, object[] invokes)
        {

        }
    }
}