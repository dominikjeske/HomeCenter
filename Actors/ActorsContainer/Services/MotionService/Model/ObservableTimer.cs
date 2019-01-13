using System;
using System.Linq;
using System.Reactive.Linq;

namespace HomeCenter.Services.MotionService.Model
{
    public class ObservableTimer : IObservableTimer
    {
        public IObservable<DateTimeOffset> GenerateTime(TimeSpan period)
        {
            return Observable.Timer(period).Timestamp().Select(time => time.Timestamp);
        }
    }
}