using System;

namespace Wirehome.Motion.Model
{
    public interface IObservableTimer
    {
        IObservable<DateTimeOffset> GenerateTime(TimeSpan period);
    }
}