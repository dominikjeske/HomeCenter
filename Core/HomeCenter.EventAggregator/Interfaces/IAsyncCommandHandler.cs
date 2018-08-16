using System.Threading.Tasks;

namespace HomeCenter.Messaging
{
    public interface IAsyncCommandHandler
    {
        Task<R> HandleAsync<T, R>(IMessageEnvelope<T> message) where R : class;
    }
}