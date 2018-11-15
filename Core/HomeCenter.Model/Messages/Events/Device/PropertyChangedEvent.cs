using HomeCenter.Model.Capabilities.Constants;
using HomeCenter.Model.Core;
using System;
using System.Collections.Generic;

namespace HomeCenter.Model.Messages.Events.Device
{
    public class PropertyChangedEvent : Event
    {
        public PropertyChangedEvent(string deviceUID, string changedPropertyName, string oldValue, string newValue, IDictionary<string, string> additionalProperties = null)
        {
            BuildEvent(deviceUID, changedPropertyName, oldValue, newValue, additionalProperties);
        }

        public PropertyChangedEvent(string deviceUID, string changedPropertyName, bool oldValue, bool newValue, IDictionary<string, string> additionalProperties = null)
        {
            BuildEvent(deviceUID, changedPropertyName, oldValue.ToString(), newValue.ToString(), additionalProperties);
        }

        private void BuildEvent(string deviceUID, string changedPropertyName, string oldValue, string newValue, IDictionary<string, string> additionalProperties)
        {
            Type = EventType.PropertyChanged;
            Uid = Guid.NewGuid().ToString();
            this[StateProperties.StateName] = changedPropertyName;
            this[MessageProperties.MessageSource] = deviceUID;
            this[EventProperties.NewValue] = newValue;
            
            SetProperty(EventProperties.EventTime, SystemTime.Now);

            if(oldValue != null)
            {
                this[EventProperties.OldValue] = oldValue;
            }

            if (additionalProperties != null)
            {
                foreach (var val in additionalProperties)
                {
                    SetProperty(val.Key, val.Value);
                }
            }
        }

        public string PropertyChangedName => AsString(StateProperties.StateName);
        public string NewValue => this[EventProperties.NewValue];
        public string OldValue => this[EventProperties.OldValue];
        public DateTimeOffset EventTime => AsDate(EventProperties.EventTime);
        public string SourceDeviceUid => AsString(MessageProperties.MessageSource);
    }
}