﻿using StringParser.Interfaces;
using System;

namespace StringParser
{
    /// <summary>
    /// Basic logger that is an interface for <see cref="ILogger"/>
    /// </summary>

    public sealed class Logger : ILogger
    {
#pragma warning disable CS1591
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

        ConsoleColor Color(LogLevel lvl)
        {
            switch (lvl)
            {
                case LogLevel.Debug:
                    return ConsoleColor.Cyan;
                case LogLevel.Warning:
                    return ConsoleColor.Yellow;
                case LogLevel.Error:
                    return ConsoleColor.Red;
                default:
                    return ConsoleColor.White;
            }
        }

        public void LogDebug(string message)
        {
        }

        public void LogError(string message)
        {
        }

        public void LogInfo(string message)
        {
        }

        public void LogWarning(string message)
        {
        }
#pragma warning restore CS1591
    }
}