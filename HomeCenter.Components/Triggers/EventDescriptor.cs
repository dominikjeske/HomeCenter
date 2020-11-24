using System;
using HomeCenter.Abstractions;

namespace HomeCenter.Services.MotionService
{
    public class EventDescriptor
    {
        public EventDescriptor(Event @event)
        {
            Event = @event;
        }

        public Event Event { get; }
        //public TimeSpan? MinTimeToNext { get; set; }

        //public TimeSpan? MaxTimeToNext { get; set; }
    }
}