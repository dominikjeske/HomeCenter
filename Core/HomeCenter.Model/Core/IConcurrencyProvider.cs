using System.Reactive.Concurrency;

namespace HomeCenter.Motion.Model
{
    public interface IConcurrencyProvider
    {
        IScheduler Scheduler { get; }
        IScheduler Task { get; }
        IScheduler Thread { get; }
    }
}
