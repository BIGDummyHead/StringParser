using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandParser
{
    internal class Program
    {
        static void Main()
        {
            CommandHandler handler  = new CommandHandler();
            handler.RegisterModule<Basic>();

            handler.RegisterConverter(delegate(string parse)
            {
                return new Person
                {
                    name = "Shawn",
                    age = 17
                };
            });

            handler.Invoke("say Hello world");
        }
    }

    class Person 
    {
        public string name;
        public int age;
    }

    class Basic : BaseCommandModule
    {
        [Command("say")]
        public void Say([UpTo(3)] Person say)
        {
            Console.WriteLine(say.name);
        }
    }
}
