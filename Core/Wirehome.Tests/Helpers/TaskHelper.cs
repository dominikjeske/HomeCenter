using System.Threading;
using System.Threading.Tasks;

namespace Wirehome.Extensions.Tests.Helpers
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
    }
}
