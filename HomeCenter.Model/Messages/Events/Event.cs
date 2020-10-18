using HomeCenter.Model.Core;
using System;

namespace HomeCenter.Model.Messages.Events
{
    //TODO - Why it has to be property based
    public class Event : ActorMessage
    {
        public Event()
        {
            Uid = Guid.NewGuid().ToString();
            EventTime = SystemTime.Now;
        }

        public DateTimeOffset EventTime
        {
            get => this.AsDate(MessageProperties.EventTime);
            set => this.SetProperty(MessageProperties.EventTime, value);
        }

        public string MessageSource
        {
            get => this.AsString(MessageProperties.MessageSource);
            set => this.SetProperty(MessageProperties.MessageSource, value);
        }
    }
}