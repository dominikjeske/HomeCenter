using System;
using System.Reactive.Linq;
using System.Linq;

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