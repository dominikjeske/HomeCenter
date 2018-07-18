using System;

namespace HomeCenter.Motion.Model
{
    public interface IObservableTimer
    {
        IObservable<DateTimeOffset> GenerateTime(TimeSpan period);
    }
}