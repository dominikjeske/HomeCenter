using System;
using System.Threading;

namespace Wirehome.Core.EventAggregator
{
    public class MessageEnvelope<T> : IMessageEnvelope<T>
    {
        public MessageEnvelope(T message, CancellationToken token = default, Type responseType = null)
        {
            Message = message;
            CancellationToken = token;
            ResponseType = responseType;
        }

        public T Message { get;  }
        public CancellationToken CancellationToken { get; }
        public Type ResponseType { get; }
    }
}
