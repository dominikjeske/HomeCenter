using Microsoft.Extensions.Logging;
using System;
using System.Reactive.Concurrency;

namespace HomeCenter.Services.MotionService.Tests
{
    public class FakeLogger<T> : ILogger<T>
    {
        private readonly IScheduler _scheduler;

        public FakeLogger(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Console.WriteLine($"[{_scheduler.Now:ss:fff}] { formatter(state, exception)}");
        }
    }
}