using System.Reactive.Concurrency;

namespace HomeCenter.Model.Core
{
    public interface IConcurrencyProvider
    {
        IScheduler Scheduler { get; }
        IScheduler Task { get; }
        IScheduler Thread { get; }
    }
}