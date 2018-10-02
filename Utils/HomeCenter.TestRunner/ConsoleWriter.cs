using System;

namespace HomeCenter.TestRunner
{
    public static class ConsoleWriter
    {
        public static void WriteOK(string text) => Write(text, ConsoleColor.Green);

        public static void WriteError(string text) => Write(text, ConsoleColor.Red);

        public static void WriteWarning(string text) => Write(text, ConsoleColor.Yellow);

        public static void Write(string text, ConsoleColor color = ConsoleColor.White)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
}