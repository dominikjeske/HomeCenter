using HomeCenter.Services.MotionService;
using HomeCenter.Services.MotionService.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Reactive.Disposables;

namespace HomeCenter.Actors.Tests
{

    internal class FakeLogger : ILogger
    {
        private readonly bool _isEnabled;

        public FakeLogger(bool isEnabled = true)
        {
            _isEnabled = isEnabled;
        }

        public IDisposable BeginScope<TState>(TState state) => Disposable.Empty;

        public bool IsEnabled(LogLevel logLevel) => _isEnabled;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
        }
    }
}