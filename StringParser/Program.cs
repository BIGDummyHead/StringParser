using System;
using System.Reflection;
using System.Threading.Tasks;

namespace StringParser;
internal class Program
{
    public static Handler handler = new Handler(new HandlerConfig()
    {
        AlwaysTrim = true,
        ByPopularVote = true,
        IgnoreCase = true,
        Prefix = ">"
    });

    static async Task Main()
    {
        handler.RegisterModule<MyModule>();
        await Input();
    }

    static async Task Input()
    {
        await handler.Invoke(">a msg");
    }

}

class Pre { }
class MyModule : ICommandModule
{
    public ValueTask OnCommandExecute(MethodInfo method, object instance, object[] invokes, object returnInstance)
    {
        return default;
    }

    [Command]
    void A(string msg)
    {
        Console.WriteLine(msg);
    }
}



