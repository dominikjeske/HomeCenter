using System;
using System.Threading.Tasks;

namespace Wirehome.Core.EventAggregator
{
    public sealed class AsyncCommandHandler : BaseCommandHandler, IAsyncCommandHandler
    {
        public AsyncCommandHandler(Type type, Guid token, object handler, RoutingFilter filter) : base(type, token, handler, filter)
        {
        }

        public async Task<R> HandleAsync<T, R>(IMessageEnvelope<T> message) where R : class
        {
            var handler = Handler as Func<IMessageEnvelope<T>, Task>;
            if (handler == null) throw new InvalidCastException($"Invalid cast from {Handler.GetType()} to Func<IMessageEnvelope<{typeof(T).Name}>, Task<object>>");
            await handler(message).ConfigureAwait(false);

            return default;
        }
    }
}
