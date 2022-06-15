using System.Reflection;
using System.Threading.Tasks;

namespace StringParser;

/// <summary>
/// A base for all command modules
/// </summary>
public interface ICommandModule
{
    /// <summary>
    /// Called when any in the module is invoked. 
    /// </summary>
    /// <param name="method">The method invoked</param>
    /// <param name="instance">The instance used to invoke the <paramref name="method"/>.</param>
    /// <param name="invokes">The parameters used to invoke the <paramref name="method"/></param>
    /// <param name="returnInstance">Return of the command if any</param>
    ValueTask OnCommandExecute(MethodInfo method, object instance, object[] invokes, object? returnInstance);

    /// <summary>
    /// The <see cref="Handler"/> used to invoke this command!
    /// </summary>
    Handler UsedHandler { get; set; }
}

