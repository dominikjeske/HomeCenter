using System;
using System.Threading.Tasks;

namespace Wirehome.Core.EventAggregator
{
    public sealed class AsyncWithResultCommandHandler : BaseCommandHandler, IAsyncCommandHandler
    {
        public AsyncWithResultCommandHandler(Type type, Guid token, object handler, RoutingFilter filter) : base(type, token, handler, filter)
        {
        }

        public async Task<R> HandleAsync<T, R>(IMessageEnvelope<T> message) where R : class
        {
            var handler = Handler as Func<IMessageEnvelope<T>, Task<object>>;
            if(handler == null) throw new InvalidCastException($"Invalid cast from {Handler.GetType()} to Func<IMessageEnvelope<{typeof(T).Name}>, Task<object>>");
            var result = await handler(message).ConfigureAwait(false);
            var typedResult = result as R;
            if (result != null && typedResult == null) throw new InvalidCastException($"Excepted type {typeof(R)} is diffrent that actual {result.GetType()}");

            return typedResult;
        }
    }
}
