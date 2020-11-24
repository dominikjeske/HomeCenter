using System;
using System.Collections.Generic;

namespace HomeCenter.Model.Triggers
{
    public class Schedule
    {
        public Schedule(string cronExpression, string calendar, TimeSpan? workingTime, IList<ManualSchedule> manualSchedules)
        {
            CronExpression = cronExpression;
            Calendar = calendar;
            WorkingTime = workingTime;
            ManualSchedules = manualSchedules;
        }

        public string CronExpression { get; }
        public string Calendar { get; }
        public TimeSpan? WorkingTime { get;  }
        public IList<ManualSchedule> ManualSchedules { get; }
    }
}