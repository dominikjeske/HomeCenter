using System;
using System.Threading;

namespace Wirehome.Core.EventAggregator
{
    public interface IMessageEnvelope<out T>
    {
        CancellationToken CancellationToken { get;  }
        T Message { get; }
        Type ResponseType { get; }
    }
}