using HomeCenter.Model.Core;
using System;
using System.Collections.Generic;

namespace HomeCenter.Model.Triggers
{
    public class Schedule
    {
        [Map] public string CronExpression { get; private set; }
        [Map] public string Calendar { get; private set; }
        [Map] public TimeSpan? WorkingTime { get; private set; }
        [Map] public IList<ManualSchedule> ManualSchedules { get; private set; }
    }
}