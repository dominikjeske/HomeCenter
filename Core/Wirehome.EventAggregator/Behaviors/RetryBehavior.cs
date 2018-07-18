using System.Threading.Tasks;

namespace Wirehome.Core.EventAggregator
{
    public class RetryBehavior : Behavior
    {
        private int _retryCount;

        public RetryBehavior(int retryCount = 3)
        {
            Priority = 40;
            _retryCount = retryCount;
        }

        public override async Task<R> HandleAsync<T, R>(IMessageEnvelope<T> message)
        {
            while (true)
            {
                try
                {
                    return await _asyncCommandHandler.HandleAsync<T, R>(message).ConfigureAwait(false);
                }
                catch when (_retryCount-- > 0) { }
            }
        }
    }
}
