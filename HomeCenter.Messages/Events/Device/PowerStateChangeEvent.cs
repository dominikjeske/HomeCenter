using HomeCenter.Abstractions;

namespace HomeCenter.Messages.Events.Device
{
    public class PowerStateChangeEvent : Event
    {
        public static PowerStateChangeEvent Create(bool value, string messageSource, string eventTriggerSource)
        {
            return (PowerStateChangeEvent)new PowerStateChangeEvent().SetProperty(MessageProperties.EventTriggerType, eventTriggerSource)
                                                                     .SetProperty(MessageProperties.Value, value)
                                                                     .SetProperty(MessageProperties.MessageSource, messageSource);
        }

        public string EventTrigger => this.AsString(MessageProperties.EventTriggerType);

        public bool Value => this.AsBool(MessageProperties.Value);
    }
}