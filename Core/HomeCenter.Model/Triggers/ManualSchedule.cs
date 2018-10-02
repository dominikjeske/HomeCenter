using HomeCenter.Model.Core;
using System;

namespace HomeCenter.Model.Triggers
{
    public class ManualSchedule
    {
        private TimeSpan? _finish;

        [Map] public TimeSpan Start { get; private set; }

        [Map]
        public TimeSpan Finish
        {
            get
            {
                if (!_finish.HasValue && WorkingTime.HasValue)
                {
                    _finish = Start.Add(WorkingTime.Value);
                }
                return _finish.Value;
            }
            private set { _finish = value; }
        }

        [Map] public TimeSpan? WorkingTime { get; private set; }
    }
}