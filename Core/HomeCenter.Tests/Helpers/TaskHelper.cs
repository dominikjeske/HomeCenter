using System;
using System.Threading;
using System.Threading.Tasks;

namespace HomeCenter.Tests.Helpers
{
    public static class TaskHelper
    {
        public static TaskCompletionSource<T> GenerateTimeoutTaskSource<T>(int millisecondsDelay = 1000)
        {
            var ts = new CancellationTokenSource(millisecondsDelay);
            var tcs = new TaskCompletionSource<T>();
            ts.Token.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);
            return tcs;
        }

        public static async Task<T> Execute<T>(Func<Task<T>> subscription, Action action)
        {
            var sub = subscription();
            action();
            var result = await sub;
            return result;
        }
    }
}