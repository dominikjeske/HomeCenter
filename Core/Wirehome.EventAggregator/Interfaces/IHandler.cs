namespace HomeCenter.Core.EventAggregator
{
    public interface IHandler<T>
    {
        void Handle(IMessageEnvelope<T> message);
    }
}