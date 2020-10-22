using Quartz.Impl.Calendar;
using System;
using HomeCenter.Abstractions;

namespace HomeCenter.Quartz
{
    public class QuartzCalendar : HolidayCalendar
    {
        public QuartzCalendar(IDayOffProvider dayOffProvider)
        {
            Description = dayOffProvider.Name;

            for (int year = DateTime.Now.Year; year < DateTime.Now.Year + 100; year++)
            {
                for (int month = 1; month < 13; month++)
                {
                    for (int day = 1; day <= DateTime.DaysInMonth(year, month); day++)
                    {
                        var date = new DateTime(year, month, day);
                        if (dayOffProvider.IsDayOff(date))
                        {
                            AddExcludedDate(date);
                        }
                    }
                }
            }
        }
    }
}