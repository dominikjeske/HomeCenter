using System;

namespace HomeCenter.Model.Calendars
{
    public interface IDayOffProvider
    {
        bool IsDayOff(DateTime date);

        string Name { get; }
    }
}