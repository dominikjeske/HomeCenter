using System;

namespace HomeCenter.Model.Triggers
{
    public class ManualSchedule
    {
        private TimeSpan? _finish;
        public TimeSpan Start { get; }
        public TimeSpan? WorkingTime { get;  }

        public ManualSchedule(TimeSpan start, TimeSpan? finish = null, TimeSpan? workingTime = null)
        {
            if (finish is null && workingTime is null) throw new ArgumentException("FinishTime or Working time should be defined");

            Start = start;
            _finish = finish;
            WorkingTime = workingTime;
        }

        public TimeSpan Finish
        {
            get
            {
                return _finish ??= Start.Add(WorkingTime.GetValueOrDefault());
            }
            init { _finish = value; }
        }
    }
}