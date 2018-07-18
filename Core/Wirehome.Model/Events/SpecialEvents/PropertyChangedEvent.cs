using System;
using System.Collections.Generic;
using HomeCenter.ComponentModel.Capabilities.Constants;
using HomeCenter.ComponentModel.ValueTypes;
using HomeCenter.Core;

namespace HomeCenter.ComponentModel.Events
{
    public class PropertyChangedEvent : Event
    {
        public PropertyChangedEvent(string deviceUID, string changedPropertyName, IValue oldValue, IValue newValue, IDictionary<string, IValue> additionalProperties = null)
        {
            Type = EventType.PropertyChanged;
            Uid = Guid.NewGuid().ToString();
            this[StateProperties.StateName] = (StringValue)changedPropertyName;
            this[EventProperties.SourceDeviceUid] = (StringValue)deviceUID;
            this[EventProperties.NewValue] = newValue;
            this[EventProperties.OldValue] = oldValue;
            this[EventProperties.EventTime] = (DateTimeValue)SystemTime.Now;
            
            if(additionalProperties != null)
            {
                foreach(var val in additionalProperties)
                {
                    SetPropertyValue(val.Key, val.Value);
                }
            }
        }
        public string PropertyChangedName => (StringValue)this[StateProperties.StateName];
        public IValue NewValue => this[EventProperties.NewValue];
        public IValue OldValue => this[EventProperties.OldValue];
        public DateTimeOffset EventTime => (DateTimeValue)this[EventProperties.EventTime];
        public string SourceDeviceUid => (StringValue)this[EventProperties.SourceDeviceUid];
    }
}
