using System.Collections.Generic;

namespace HomeCenter.Model.Messages.Events.Device
{
    public class PropertyChangedEvent : Event
    {
        public static PropertyChangedEvent Create(string deviceUID, string changedPropertyName, string oldValue, string newValue, IDictionary<string, string> additionalProperties = null)
        {
            return BuildEvent(deviceUID, changedPropertyName, oldValue, newValue, additionalProperties);
        }

        public static PropertyChangedEvent Create(string deviceUID, string changedPropertyName, bool oldValue, bool newValue, IDictionary<string, string> additionalProperties = null)
        {
            return BuildEvent(deviceUID, changedPropertyName, oldValue.ToString(), newValue.ToString(), additionalProperties);
        }

        private static PropertyChangedEvent BuildEvent(string deviceUID, string changedPropertyName, string oldValue, string newValue, IDictionary<string, string> additionalProperties)
        {
            var propertyChangeEvent = new PropertyChangedEvent
            {
                PropertyChangedName = changedPropertyName,
                MessageSource = deviceUID,
                NewValue = newValue
            };

            if (oldValue != null)
            {
                propertyChangeEvent.OldValue = oldValue;
            }

            if (additionalProperties != null)
            {
                foreach (var val in additionalProperties)
                {
                    propertyChangeEvent.SetProperty(val.Key, val.Value);
                }
            }

            return propertyChangeEvent;
        }

        public string PropertyChangedName
        {
            get => AsString(MessageProperties.StateName);
            set => SetProperty(MessageProperties.StateName, value);
        }

        public string NewValue
        {
            get => AsString(MessageProperties.NewValue);
            set => SetProperty(MessageProperties.NewValue, value);
        }

        public string OldValue
        {
            get => AsString(MessageProperties.OldValue);
            set => SetProperty(MessageProperties.OldValue, value);
        }
    }
}