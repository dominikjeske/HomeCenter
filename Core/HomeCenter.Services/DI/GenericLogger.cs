using Microsoft.Extensions.Logging;
using System;

namespace HomeCenter.Services.DI
{
    public abstract partial class Bootstrapper
    {
        private class GenericLogger<T> : ILogger<T>
        {
            private readonly ILogger<T> _underlying;

            public GenericLogger(ILoggerFactory factory)
            {
                _underlying = factory.CreateLogger<T>();
            }

            public IDisposable BeginScope<TState>(TState state) => _underlying.BeginScope(state);

            public bool IsEnabled(LogLevel logLevel) => _underlying.IsEnabled(logLevel);

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
                    => _underlying.Log(logLevel, eventId, state, exception, formatter);
        }
    }
}