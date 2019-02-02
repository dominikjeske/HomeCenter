using System;
using System.Threading;
using System.Threading.Tasks;
using HomeCenter.Utils.Extensions;

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

        public static async Task<T> Execute<T>(Func<Task<T>> subscription, Action action, int millisecondsDelay = 100)
        {
            var sub = subscription();
            action();
            var result = await sub.WhenDone(TimeSpan.FromMilliseconds(millisecondsDelay));
            return result;
        }
    }
}