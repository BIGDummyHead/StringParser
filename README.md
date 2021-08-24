# Command Parser
Parse string to methods and arguments and invoke.

## Examples:

```csharp

CommandHandler cmds = new();

//must inherit BaseCommandModule
//cmds.Register(typeof(CmdModule));
cmds.Register<CmdModule>();

string cmdLine = "say Joe";

cmds.Invoke(cmdLine);

//an output of: "Hello Joe, my name is Joe as well!"

class CmdModule : BaseCommandModule 
{
    //optionally we can use the method name by just using the command attr
    //[Command]
    [Command("say")]
    public void SayHello(string name)
    {
        Console.WriteLine($"Hello {name}, my name is {name} as well!");
    }
    
    //we can ignore this command, by adding the ignore attribute.
    [Ignore]
    [Command("not_in_use")]
    public void NextUpdateCommand(){}
    
    //uses the remaining text attribute, this will use the remaining text either create an instance of a class or keep it is as string
    [Command]
    public void UseRemainText([RemainingText]string words){}
     
    //we can provide classes that have constructors of strings.
    [Command]
    public void ConvertToPerson(Person person){}
    
    [Command]
    public void ConvertToDif(Difficulty dif)
    {
       //converts string to difficulty
    }
    
    public class Person
    {
        public Person(string name){}
    }
    
    public enum Difficulty
    {
       easy, med, hard
    }
}
```
