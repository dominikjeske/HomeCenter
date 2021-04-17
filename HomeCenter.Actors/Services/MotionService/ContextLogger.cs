using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace HomeCenter.Services.MotionService
{
    internal class ContextLogger : ILogger
    {
        private readonly ILogger _logger;
        private readonly Func<IDictionary<string, object>> _context;

        public ContextLogger(ILogger logger, Func<IDictionary<string, object>> context)
        {
            _logger = logger;
            _context = context;
        }

        public IDisposable BeginScope<TState>(TState state) => _logger.BeginScope(state);

        public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            using (_logger.BeginScope(_context()))
            {
                _logger.Log(logLevel, eventId, state, exception, formatter);
            }
        }
    }
}