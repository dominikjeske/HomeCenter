using System;

namespace HomeCenter.Services.MotionService.Model
{
    public interface IObservableTimer
    {
        IObservable<DateTimeOffset> GenerateTime(TimeSpan period);
    }
}