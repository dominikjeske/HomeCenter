using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;

namespace Wirehome.Motion.Model
{
    //TODO Add thread safe
    public class TimeList<T> : IEnumerable<TimePair<T>>
    {
        private List<TimePair<T>> _innerList { get; } = new List<TimePair<T>>();
        private readonly IScheduler _scheduler;

        public TimeList(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }
        public void Add(DateTimeOffset time, T value) => _innerList.Add(new TimePair<T>(time, value));
        public void Add(T value) => Add(_scheduler.Now, value);
        

        public IEnumerable<T> GetLastElements(DateTimeOffset endTime, TimeSpan period)
        {
            return _innerList.Where(el => el.Time < endTime && endTime - el.Time < period).Select(v => v.Value);
        }
        
        public bool HasElement(TimeSpan period) => _innerList.Any(el => _scheduler.Now - el.Time < period);
        public void ClearOldData(TimeSpan period) =>  _innerList.RemoveAll(el => _scheduler.Now - el.Time > period);
        

        public IEnumerator<TimePair<T>> GetEnumerator() => _innerList.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class TimePair<T>
    {
        public static TimePair<T> Empty = new TimePair<T>(new DateTimeOffset(0, TimeSpan.FromTicks(0)), default);

        public DateTimeOffset Time { get; }
        public T Value { get; }

        public TimePair(DateTimeOffset time, T value)
        {
            Time = time;
            Value = value;
        }
    }
}
