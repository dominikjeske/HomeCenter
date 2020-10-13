using HomeCenter.Model.Core;
using System;
using System.Collections.Generic;

namespace HomeCenter.Model.Triggers
{
    public class Schedule
    {
        public string CronExpression { get; private set; }
        public string Calendar { get; private set; }
        public TimeSpan? WorkingTime { get; private set; }
        public IList<ManualSchedule> ManualSchedules { get; private set; }
    }
}