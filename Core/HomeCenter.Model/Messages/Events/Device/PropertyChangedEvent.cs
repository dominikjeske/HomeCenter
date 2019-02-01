using System.Collections.Generic;

namespace HomeCenter.Model.Messages.Events.Device
{
    public class PropertyChangedEvent : Event
    {
        public static PropertyChangedEvent Create(string messageSource, string changedPropertyName, string oldValue, string newValue, IDictionary<string, string> additionalProperties = null)
        {
            return BuildEvent(messageSource, changedPropertyName, oldValue, newValue, additionalProperties);
        }

        public static PropertyChangedEvent Create(string messageSource, string changedPropertyName, bool oldValue, bool newValue, IDictionary<string, string> additionalProperties = null)
        {
            return BuildEvent(messageSource, changedPropertyName, oldValue.ToString(), newValue.ToString(), additionalProperties);
        }

        private static PropertyChangedEvent BuildEvent(string messageSource, string changedPropertyName, string oldValue, string newValue, IDictionary<string, string> additionalProperties)
        {
            var propertyChangedEvent = new PropertyChangedEvent
            {
                PropertyChangedName = changedPropertyName,
                MessageSource = messageSource,
                NewValue = newValue
            };

            if (oldValue != null)
            {
                propertyChangedEvent.OldValue = oldValue;
            }

            if (additionalProperties != null)
            {
                foreach (var val in additionalProperties)
                {
                    propertyChangedEvent.SetProperty(val.Key, val.Value);
                }
            }

            return propertyChangedEvent;
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