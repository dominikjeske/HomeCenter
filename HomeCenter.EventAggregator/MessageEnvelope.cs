using System;
using System.Threading;

namespace HomeCenter.EventAggregator
{
    internal class MessageEnvelope<T> : IMessageEnvelope<T>
    {
        public MessageEnvelope(T message, CancellationToken token = default, Type responseType = null)
        {
            Message = message;
            CancellationToken = token;
            ResponseType = responseType;
        }

        public T Message { get; }
        public CancellationToken CancellationToken { get; }
        public Type ResponseType { get; }
    }
}