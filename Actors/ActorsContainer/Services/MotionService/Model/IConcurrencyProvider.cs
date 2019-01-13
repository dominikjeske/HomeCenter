using System.Reactive.Concurrency;

namespace HomeCenter.Services.MotionService.Model
{
    public interface IConcurrencyProvider
    {
        IScheduler Scheduler { get; }
        IScheduler Task { get; }
        IScheduler Thread { get; }
    }
}
