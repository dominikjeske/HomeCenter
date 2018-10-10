using System;

namespace HomeCenter.TestRunner
{
    public static class ConsoleEx
    {
        public static void WriteOKLine(string text) => Write(text, ConsoleColor.Green);

        public static void WriteErrorLine(string text) => Write(text, ConsoleColor.Red);

        public static void WriteWarningLine(string text) => Write(text, ConsoleColor.Yellow);

        public static void WriteMenuLine(string text) => Write(text, ConsoleColor.Cyan);

        public static void WriteTitleLine(string text) => Write(text, ConsoleColor.DarkBlue);


        public static void WriteOK(string text) => Write(text, ConsoleColor.Green, false);

        public static void WriteError(string text) => Write(text, ConsoleColor.Red, false);

        public static void WriteWarning(string text) => Write(text, ConsoleColor.Yellow, false);


        public static void Write(string text, ConsoleColor color = ConsoleColor.White, bool withNewLine = true)
        {
            Console.ForegroundColor = color;
            if (withNewLine)
            {
                Console.WriteLine(text);
            }
            else
            {
                Console.Write(text);
            }
            Console.ResetColor();
        }

        public static int ReadNumber()
        {
            while (true)
            {
                var selected = Console.ReadLine();
                if (int.TryParse(selected, out int number))
                {
                    return number;
                }
                WriteError($"{selected} is not a number");
            }
        }
    }
}