using StringParser.Interfaces;
using System;

namespace StringParser;

/// <summary>
/// Basic logger that is an interface for <see cref="ILogger"/>
/// </summary>

public sealed class Logger : ILogger
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public void Log(string message, LogLevel lvl)
    {
        Console.ForegroundColor = Color(lvl);
        Console.Write($"{lvl.ToString().ToUpper()}");
        Console.ResetColor();
        Console.Write(" [ ");
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write(DateTime.Now.TimeOfDay.ToString());
        Console.ResetColor();
        Console.Write(" ] : ");

        Console.ForegroundColor = Color(lvl);

        Console.Write($"{message}\r\n");

        Console.ResetColor();
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

    static ConsoleColor Color(LogLevel lvl)
    {
        return lvl switch
        {
            LogLevel.Debug => ConsoleColor.Cyan,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Error => ConsoleColor.Red,
            _ => ConsoleColor.White,
        };
    }
}
