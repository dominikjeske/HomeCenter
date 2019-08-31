using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace HomeCenter.Model.Extensions
{
    public static class LoggingExtensions
    {
        private static readonly Dictionary<string, LogLevel> _logLevels = new Dictionary<string, LogLevel>
        {
            [nameof(LogLevel.Information)] = LogLevel.Information,
            [nameof(LogLevel.Trace)] = LogLevel.Trace,
            [nameof(LogLevel.Error)] = LogLevel.Error,
            [nameof(LogLevel.Critical)] = LogLevel.Critical,
            [nameof(LogLevel.Debug)] = LogLevel.Debug,
            [nameof(LogLevel.None)] = LogLevel.None,
            [nameof(LogLevel.Warning)] = LogLevel.Warning,
        };

        public static void Log(this ILogger logger, string loglevel, string message)
        {
            logger.Log(_logLevels[loglevel], message);
        }

        public static void Log(this ILogger logger, string loglevel, string message, params object[] args)
        {
            logger.Log(_logLevels[loglevel], message, args);
        }
    }
}