using System;

namespace Wirehome.Core.EventAggregator
{
    public static class BehaviorExtensions
    {
        public static BehaviorChain WithRetry(this BehaviorChain chain, int retryNumber = 3)
        {
            if (retryNumber < 1) return chain;

            return chain.WithPolicy(new RetryBehavior(retryNumber));
        }

        public static BehaviorChain WithTimeout(this BehaviorChain chain, TimeSpan? timeout)
        {
            if (!timeout.HasValue) return chain;

            return chain.WithPolicy(new TimeoutBehavior(timeout.Value));
        }

        public static BehaviorChain WithAsync(this BehaviorChain chain)
        {
            return chain.WithPolicy(new AsyncBehavior());
        }

        public static BehaviorChain WithAsync(this BehaviorChain chain, bool async)
        {
            if (!async) return chain;
            return chain.WithPolicy(new AsyncBehavior());
        }
    }
}
