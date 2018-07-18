using Quartz.Impl.Calendar;
using System;
using System.Collections.Generic;
using Wirehome.Core.Services.DependencyInjection;

namespace Wirehome.ComponentModel.Components
{
    public class Schedule
    {
        [Map] public string CronExpression { get; private set; }
        [Map] public string Calendar { get; private set; }
        [Map] public TimeSpan? WorkingTime { get; private set; }
        [Map] public IList<ManualSchedule> ManualSchedules { get; private set; }
    }
}