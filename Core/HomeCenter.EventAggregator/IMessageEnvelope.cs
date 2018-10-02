using System;
using System.Threading;

namespace HomeCenter.Broker
{
    public interface IMessageEnvelope<out T>
    {
        CancellationToken CancellationToken { get; }
        T Message { get; }
        Type ResponseType { get; }
    }
}