using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reactive.Subjects;

namespace HomeCenter.Tests.ComponentModel
{
    public class LogMock : ILoggerProvider
    {
        public List<LogEntry> Messages = new List<LogEntry>();
        public Subject<LogEntry> MessageSink = new Subject<LogEntry>();

        public void Dispose()
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new CustomConsoleLogger(categoryName, this);
        }

        public void Log(LogLevel logLevel, string message)
        {
            var entry = new LogEntry { LogLevel = logLevel, Message = message };
            Messages.Add(entry);
            MessageSink.OnNext(entry);
        }

        public void Clear()
        {
            Messages.Clear();
        }


        public class CustomConsoleLogger : ILogger
        {
            private readonly string _categoryName;

            private readonly LogMock _consoleLogProvider;

            public CustomConsoleLogger(string categoryName, LogMock consoleLogProvider)
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

                _consoleLogProvider.Log(logLevel, $"{formatter(state, exception)}");
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }
        }
    }
}