using HomeCenter.Abstractions;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace HomeCenter.Messages.Events.Device
{
    public class PropertyChangedEvent : Event
    {
        public static PropertyChangedEvent Create(string messageSource, string changedPropertyName, string? oldValue, string newValue, [AllowNull] IDictionary<string, string> additionalProperties = null)
        {
            return BuildEvent(messageSource, changedPropertyName, oldValue, newValue, additionalProperties ?? ImmutableDictionary<string, string>.Empty);
        }

        public static PropertyChangedEvent Create(string messageSource, string changedPropertyName, bool? oldValue, bool newValue, [AllowNull] IDictionary<string, string> additionalProperties = null)
        {
            return BuildEvent(messageSource, changedPropertyName, oldValue.ToString(), newValue.ToString(), additionalProperties ?? ImmutableDictionary<string, string>.Empty);
        }

        private static PropertyChangedEvent BuildEvent(string messageSource, string changedPropertyName, string? oldValue, string newValue, IDictionary<string, string> additionalProperties)
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

            foreach (var val in additionalProperties)
            {
                propertyChangedEvent.SetProperty(val.Key, val.Value);
            }

            return propertyChangedEvent;
        }

        public string PropertyChangedName
        {
            get => this.AsString(MessageProperties.StateName);
            set => this.SetProperty(MessageProperties.StateName, value);
        }

        public string NewValue
        {
            get => this.AsString(MessageProperties.NewValue);
            set => this.SetProperty(MessageProperties.NewValue, value);
        }

        public string OldValue
        {
            get => this.AsString(MessageProperties.OldValue);
            set => this.SetProperty(MessageProperties.OldValue, value);
        }
    }
}