using HomeCenter.Model.Core;
using System;

namespace HomeCenter.Model.Triggers
{
    public class ManualSchedule
    {
        private TimeSpan? _finish;

        public TimeSpan Start { get; private set; }

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

        public TimeSpan? WorkingTime { get; private set; }
    }
}