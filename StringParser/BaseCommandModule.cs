using System.Reflection;
using System.Threading.Tasks;

namespace StringParser
{
    /// <summary>
    /// An abstract for <see cref="ICommandModule"/> that rids of the need to implement the <see cref="ICommandModule"/> interface.
    /// </summary>
    public abstract class BaseCommandModule : ICommandModule
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

        public Handler UsedHandler { get; set; }



        public virtual ValueTask OnCommandExecute(MethodInfo method, object instance, object[] invokes, object returnInstance)
        {
            return ValueTask.CompletedTask;
        }
        //restore warning
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
