namespace HomeCenter.Messaging
{
    public interface IHandler<T>
    {
        void Handle(IMessageEnvelope<T> message);
    }
}