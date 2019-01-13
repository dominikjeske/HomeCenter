namespace HomeCenter.Model.Messages.Events.Device
{
    public class PowerStateChangeEvent : Event
    {
        public static string ManualSource = "Manual";
        public static string AutoSource = "Auto";

        public static PowerStateChangeEvent Create(bool value, string messageSource, string eventTriggerSource)
        {
            return (PowerStateChangeEvent)new PowerStateChangeEvent().SetProperty(MessageProperties.EventTriggerSource, eventTriggerSource)
                                                                     .SetProperty(MessageProperties.Value, value)
                                                                     .SetProperty(MessageProperties.MessageSource, messageSource);
        }

        public string EventTriggerSource => AsString(MessageProperties.EventTriggerSource);

        public bool Value => AsBool(MessageProperties.Value);
    }
}