using Microsoft.Reactive.Testing;
using System.Reactive.Concurrency;
using Wirehome.Motion.Model;

namespace Wirehome.Extensions.Tests
{
    public class TestConcurrencyProvider : IConcurrencyProvider
    {
        public TestConcurrencyProvider(TestScheduler scheduler)
        {
            Scheduler = scheduler;
            Task = scheduler;//TaskPoolScheduler.Default;
            Thread = NewThreadScheduler.Default;
        }

        public IScheduler Scheduler { get; }
        public IScheduler Task { get; }
        public IScheduler Thread { get; }
        public IScheduler Dispatcher { get; }
    }
}