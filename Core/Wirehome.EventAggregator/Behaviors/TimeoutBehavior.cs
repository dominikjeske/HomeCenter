using System;
using System.Threading.Tasks;
using Wirehome.Core.Extensions;

namespace Wirehome.Core.EventAggregator
{
    public class TimeoutBehavior : Behavior
    {
        private readonly TimeSpan _timeout;

        public TimeoutBehavior(TimeSpan timeout)
        {
            _timeout = timeout;
            Priority = 30;
        }

        public override Task<R> HandleAsync<T, R>(IMessageEnvelope<T> message)
        {
            return _asyncCommandHandler.HandleAsync<T, R>(message).WhenDone(_timeout, message.CancellationToken);
        }
    }
}
