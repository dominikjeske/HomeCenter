namespace HomeCenter.Broker
{
    public interface IHandler<T>
    {
        void Handle(IMessageEnvelope<T> message);
    }
}