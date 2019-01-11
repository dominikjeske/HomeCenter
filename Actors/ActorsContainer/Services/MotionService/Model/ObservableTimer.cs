using System;
using System.Linq;
using System.Reactive.Linq;

namespace Wirehome.Motion.Model
{
    public class ObservableTimer : IObservableTimer
    {
        public IObservable<DateTimeOffset> GenerateTime(TimeSpan period)
        {
            return Observable.Timer(period).Timestamp().Select(time => time.Timestamp);
        }
    }
}