using System;

namespace HomeCenter.Model.Core
{
    public interface IObservableTimer
    {
        IObservable<DateTimeOffset> GenerateTime(TimeSpan period);
    }
}