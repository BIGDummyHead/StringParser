using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommandParser
{
    internal class Program
    {
        static CommandHandler handler = new CommandHandler();

        static void Main()
        {
            handler.RegisterConverter(x =>
            {
                string[] args = x.Split(' ');

                if (args.Length != 2)
                    return default;

                if(handler.UseConverter(args[1], out int age))
                {
                    return new Person
                    {
                        age = age,
                        name = args[0]
                    };
                }

                return default;
            });

            handler.RegisterModule<BasicCommands>();

            ExpectInput();
        }

        public static void ExpectInput()
        {
            handler.Invoke(Console.ReadLine());
            ExpectInput();
        }
    }

    public class Person
    {
        public string name;
        public int age;
    }

    public class BasicCommands : BaseCommandModule
    {
        [Command("gen")]
        public void GenPerson([RequiredParams(2)] Person person)
        {
            Console.WriteLine($"Generated person: {person.name} with the age of {person.age}");
        }
    }

}
