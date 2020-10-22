using System;

namespace HomeCenter.Model.Triggers
{
    public class ManualSchedule
    {
        private TimeSpan? _finish;

        public TimeSpan Start { get; init; }

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
            init { _finish = value; }
        }

        public TimeSpan? WorkingTime { get; init; }
    }
}