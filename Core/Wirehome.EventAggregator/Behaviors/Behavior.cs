using System;
using System.Threading.Tasks;

namespace Wirehome.Core.EventAggregator
{
    public class Behavior : IBehavior
    {
        protected IAsyncCommandHandler _asyncCommandHandler;

        public int Priority { get; protected set; }

        public void SetNextNode(IAsyncCommandHandler asyncCommandHandler)
        {
            _asyncCommandHandler = asyncCommandHandler ?? throw new ArgumentNullException(nameof(asyncCommandHandler));
        }

        public virtual Task<R> HandleAsync<T, R>(IMessageEnvelope<T> message) where R : class
        {
            return _asyncCommandHandler.HandleAsync<T, R>(message);
        }
    }
}
