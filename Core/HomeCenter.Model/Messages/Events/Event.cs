using HomeCenter.Model.Core;
using HomeCenter.Utils.Extensions;
using System;

namespace HomeCenter.Model.Messages.Events
{
    public class Event : ActorMessage, IEquatable<Event>
    {
        public Event() 
        {
            Uid = Guid.NewGuid().ToString();
            EventTime = SystemTime.Now;
        }

        public bool Equals(Event other)
        {
           return other != null && GetProperties().LeftEqual(other.GetProperties());
        }

        public DateTimeOffset EventTime
        {
            get => AsDate(MessageProperties.EventTime);
            set => SetProperty(MessageProperties.EventTime, value);
        }

        public string MessageSource
        {
            get => AsString(MessageProperties.MessageSource);
            set => SetProperty(MessageProperties.MessageSource, value);
        }
    }
}