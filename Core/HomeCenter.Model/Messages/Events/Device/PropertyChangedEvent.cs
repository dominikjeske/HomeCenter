using HomeCenter.Model.Capabilities.Constants;
using HomeCenter.Model.Core;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.ValueTypes;
using System;
using System.Collections.Generic;

namespace HomeCenter.Model.Messages.Events.Device
{
    public class PropertyChangedEvent : Event
    {
        public PropertyChangedEvent(string deviceUID, string changedPropertyName, IValue oldValue, IValue newValue, IDictionary<string, IValue> additionalProperties = null)
        {
            Type = EventType.PropertyChanged;
            Uid = Guid.NewGuid().ToString();
            this[StateProperties.StateName] = (StringValue)changedPropertyName;
            this[MessageProperties.MessageSource] = (StringValue)deviceUID;
            this[EventProperties.NewValue] = newValue;
            this[EventProperties.OldValue] = oldValue;
            this[EventProperties.EventTime] = (DateTimeValue)SystemTime.Now;

            if (additionalProperties != null)
            {
                foreach (var val in additionalProperties)
                {
                    SetPropertyValue(val.Key, val.Value);
                }
            }
        }

        public string PropertyChangedName => this[StateProperties.StateName].AsString();
        public IValue NewValue => this[EventProperties.NewValue];
        public IValue OldValue => this[EventProperties.OldValue];
        public DateTimeOffset EventTime => this[EventProperties.EventTime].AsDate();
        public string SourceDeviceUid => this[MessageProperties.MessageSource].AsString();
    }
}