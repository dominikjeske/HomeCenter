using System.Reactive.Concurrency;
using HomeCenter.Abstractions;
using Microsoft.Reactive.Testing;

namespace HomeCenter.Actors.Tests.Helpers
{
    public class TestConcurrencyProvider : IConcurrencyProvider
    {
        public TestConcurrencyProvider(TestScheduler scheduler)
        {
            Scheduler = scheduler;
            Task = scheduler;
            Thread = scheduler;
        }

        public IScheduler Scheduler { get; }
        public IScheduler Task { get; }
        public IScheduler Thread { get; }
    }
}