using System;
using System.Linq;
using System.Text;

namespace HomeCenter.Utils.ConsoleExtentions
{
    public static class ConsoleEx
    {
        public static void WriteOKLine(string text) => Write(text, ConsoleColor.Green);

        public static void WriteErrorLine(string text, bool wait = false)
        {
            Write(text, ConsoleColor.Red);
            if (wait) Console.ReadLine();
        }

        public static void WriteWarningLine(string text) => Write(text, ConsoleColor.Yellow);

        public static void WriteDebugLine(string text) => Write(text, ConsoleColor.DarkYellow);

        public static void WriteMenuLine(string text) => Write(text, ConsoleColor.Cyan);

        public static void WriteTitleLine(string text) => Write(text, ConsoleColor.DarkMagenta);


        public static void WriteOK(string text) => Write(text, ConsoleColor.Green, false);

        public static void WriteError(string text) => Write(text, ConsoleColor.Red, false);

        public static void WriteWarning(string text) => Write(text, ConsoleColor.Yellow, false);

        public static void WriteDebug(string text) => Write(text, ConsoleColor.DarkYellow, false);

        public static void WriteTrace(string text) => Write(text, ConsoleColor.Gray, false);

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

        public static int ReadNumber(bool emptyAsZero = true)
        {
            while (true)
            {
                if (ReadLine(out string selected))
                {
                    if (string.IsNullOrWhiteSpace(selected) && emptyAsZero)
                    {
                        selected = "0";
                    }

                    if (int.TryParse(selected, out int number))
                    {
                        return number;
                    }
                    WriteError($"{selected} is not a number");
                }
                return -2;
            }
        }

        public static bool ReadLine(out string value)
        {
            value = string.Empty;
            var buffer = new StringBuilder();
            var key = Console.ReadKey(true);
            while (key.Key != ConsoleKey.Enter && key.Key != ConsoleKey.Escape)
            {
                if (key.Key == ConsoleKey.Backspace && Console.CursorLeft > 0)
                {
                    var cli = --Console.CursorLeft;
                    buffer.Remove(cli, 1);
                    Console.CursorLeft = 0;
                    Console.Write(new String(Enumerable.Range(0, buffer.Length + 1).Select(o => ' ').ToArray()));
                    Console.CursorLeft = 0;
                    Console.Write(buffer.ToString());
                    Console.CursorLeft = cli;
                    key = Console.ReadKey(true);
                }
                else if (Char.IsLetterOrDigit(key.KeyChar) || Char.IsWhiteSpace(key.KeyChar))
                {
                    var cli = Console.CursorLeft;
                    buffer.Insert(cli, key.KeyChar);
                    Console.CursorLeft = 0;
                    Console.Write(buffer.ToString());
                    Console.CursorLeft = cli + 1;
                    key = Console.ReadKey(true);
                }
                else if (key.Key == ConsoleKey.LeftArrow && Console.CursorLeft > 0)
                {
                    Console.CursorLeft--;
                    key = Console.ReadKey(true);
                }
                else if (key.Key == ConsoleKey.RightArrow && Console.CursorLeft < buffer.Length)
                {
                    Console.CursorLeft++;
                    key = Console.ReadKey(true);
                }
                else
                {
                    key = Console.ReadKey(true);
                }
            }

            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                value = buffer.ToString();
                return true;
            }
            return false;
        }
    }
}