using HomeCenter.Model.Core;
using Microsoft.Reactive.Testing;
using System.Reactive.Concurrency;

namespace HomeCenter.Services.MotionService.Tests
{
    public class TestConcurrencyProvider : IConcurrencyProvider
    {
        public TestConcurrencyProvider(TestScheduler scheduler)
        {
            Scheduler = scheduler;
            Task = scheduler;
            Thread = NewThreadScheduler.Default;
        }

        public IScheduler Scheduler { get; }
        public IScheduler Task { get; }
        public IScheduler Thread { get; }
        public IScheduler Dispatcher { get; }
    }
}