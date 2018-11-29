using Microsoft.Extensions.Logging;
using System;
using Windows.Foundation.Diagnostics;

namespace HomeCenter.HomeController
{
    internal class EtwLogProvider : ILoggerProvider
    {
        private EtwLogger _logger;

        public void Dispose()
        {
            _logger.Dispose();
        }

        public ILogger CreateLogger(string categoryName)
        {
            _logger = new EtwLogger(categoryName);
            return _logger;
        }

        public class EtwLogger : ILogger
        {
            private const string LOGGER_GUID = "4bd2826e-54a1-4ba9-bf63-92b73ea1ac4a";
            private readonly string _categoryName;
            private readonly LoggingChannel _loggingChannel;


            public EtwLogger(string categoryName)
            {
                _categoryName = categoryName;
                _loggingChannel = new LoggingChannel("HomeCenter", null, new Guid(LOGGER_GUID));
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (!IsEnabled(logLevel))
                {
                    return;
                }

                var fields = new LoggingFields();

                //fields.AddString("Source", logEntry.Source ?? "");
                //fields.AddString("Exception", exception?.ToString() ?? "");

                var level = MapSeverity(logLevel);
                var message = formatter(state, exception);
                _loggingChannel.LogEvent(message, fields, level);
            }

            private LoggingLevel MapSeverity(LogLevel severity)
            {
                if (severity == LogLevel.Error)
                {
                    return LoggingLevel.Error;
                }
                else if (severity == LogLevel.Critical)
                {
                    return LoggingLevel.Critical;
                }
                else if (severity == LogLevel.Warning)
                {
                    return LoggingLevel.Warning;
                }
                else if (severity == LogLevel.Debug)
                {
                    return LoggingLevel.Verbose;
                }

                return LoggingLevel.Information;
            }

            public void Dispose()
            {
                _loggingChannel.Dispose();
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