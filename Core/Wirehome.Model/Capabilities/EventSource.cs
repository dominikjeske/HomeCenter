﻿using HomeCenter.ComponentModel.Capabilities.Constants;
using HomeCenter.ComponentModel.Events;
using HomeCenter.ComponentModel.ValueTypes;

namespace HomeCenter.ComponentModel.Capabilities
{
    public class EventSource : BaseObject
    {
        public EventSource(string eventType, string direction)
        {
            this[EventProperties.EventTime] = new DateTimeValue();
            this[EventProperties.EventType] = new StringValue(eventType);
            this[EventProperties.EventDirection] = new StringValue(direction);
        }
    }
}
