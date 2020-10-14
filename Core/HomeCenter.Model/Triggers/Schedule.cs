using HomeCenter.Model.Core;
using System;
using System.Collections.Generic;

namespace HomeCenter.Model.Triggers
{
    public class Schedule
    {
        public string CronExpression { get; init; }
        public string Calendar { get; init; }
        public TimeSpan? WorkingTime { get; init; }
        public IList<ManualSchedule> ManualSchedules { get; init; }
    }
}