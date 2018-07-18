using Quartz.Impl.Calendar;
using System;
using System.Collections.Generic;
using HomeCenter.Core.Services.DependencyInjection;

namespace HomeCenter.ComponentModel.Components
{
    public class Schedule
    {
        [Map] public string CronExpression { get; private set; }
        [Map] public string Calendar { get; private set; }
        [Map] public TimeSpan? WorkingTime { get; private set; }
        [Map] public IList<ManualSchedule> ManualSchedules { get; private set; }
    }
}