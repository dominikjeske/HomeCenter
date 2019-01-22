using System.Threading.Tasks;

namespace HomeCenter.Broker
{
    public interface IAsyncCommandHandler
    {
        Task<R> HandleAsync<T, R>(IMessageEnvelope<T> message);
    }
}