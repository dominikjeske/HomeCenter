using HomeCenter.Extensions;
using System;
using System.Threading.Tasks;

namespace HomeCenter.EventAggregator.Behaviors
{
    internal class TimeoutBehavior : Behavior
    {
        private readonly TimeSpan _timeout;

        public TimeoutBehavior(TimeSpan timeout)
        {
            _timeout = timeout;
            Priority = 30;
        }

        public override Task<R> HandleAsync<T, R>(IMessageEnvelope<T> message)
        {
            if (_asyncCommandHandler == null) throw new InvalidOperationException();

            return _asyncCommandHandler.HandleAsync<T, R>(message).WhenDone(_timeout, message.CancellationToken);
        }
    }
}