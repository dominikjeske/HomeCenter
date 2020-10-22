namespace HomeCenter.Models.Messages.Events.Device
{
    public class MotionEvent : Event
    {
        public static MotionEvent Create(string messageSource) => (MotionEvent)new MotionEvent().SetProperty(MessageProperties.MessageSource, messageSource);
    }
}