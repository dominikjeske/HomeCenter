using HomeCenter.Model.Core;
using System;

namespace HomeCenter.Model.Messages.Events
{
    public class Event : ActorMessage
    {
        public Event()
        {
            Uid = Guid.NewGuid().ToString();
            EventTime = SystemTime.Now;
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