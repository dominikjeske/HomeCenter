using System;
using HomeCenter.Abstractions;

namespace HomeCenter.Services.MotionService
{
    public class EventDescriptor
    {
        public Event Event { get; set; }
        public TimeSpan? MinTimeToNext { get; set; }

        public TimeSpan? MaxTimeToNext { get; set; }
    }
}