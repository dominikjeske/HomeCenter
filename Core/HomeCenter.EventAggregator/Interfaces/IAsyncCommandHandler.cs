using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeCenter.Core.EventAggregator
{
    public interface IAsyncCommandHandler
    {
        Task<R> HandleAsync<T, R>(IMessageEnvelope<T> message) where R : class;
    }
}