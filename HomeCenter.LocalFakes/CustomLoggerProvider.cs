using HomeCenter.Abstractions;
using HomeCenter.Runner.ConsoleExtentions;
using Microsoft.Extensions.Logging;
using System;
using System.Reactive.Disposables;

namespace HomeCenter.Utils.LogProviders
{
    public class ConsoleLogProvider : ILoggerProvider
    {
        private readonly object _locki = new object();

        public void Dispose()
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new CustomConsoleLogger(categoryName, this);
        }

        public void Log(LogLevel logLevel, string message)
        {
            lock (_locki)
            {
                var time = SystemTime.Now.ToString("HH:mm:ss.fff");

                if (logLevel == LogLevel.Error)
                {
                    ConsoleEx.WriteError($"[E] {time}: ");
                }
                else if (logLevel == LogLevel.Warning)
                {
                    ConsoleEx.WriteWarning($"[W] {time}: ");
                }
                else if (logLevel == LogLevel.Information)
                {
                    ConsoleEx.WriteOK($"[I] {time}: ");
                }
                else if (logLevel == LogLevel.Debug)
                {
                    ConsoleEx.WriteDebug($"[D] {time}: ");
                }
                else if (logLevel == LogLevel.Trace)
                {
                    ConsoleEx.WriteTrace($"[T] {time}: ");
                }
                else
                {
                    Console.Write($"{logLevel}: ");
                }
                Console.WriteLine(message);
            }
        }

        public class CustomConsoleLogger : ILogger
        {
            private readonly string _categoryName;

            private readonly ConsoleLogProvider _consoleLogProvider;

            public CustomConsoleLogger(string categoryName, ConsoleLogProvider consoleLogProvider)
            {
                _categoryName = categoryName;
                _consoleLogProvider = consoleLogProvider;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (!IsEnabled(logLevel))
                {
                    return;
                }

                // _consoleLogProvider.Log(logLevel, $"{formatter(state, exception)}");
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return Disposable.Empty;
            }
        }
    }
}