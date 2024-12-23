using System.Threading.Tasks;
using System.Threading;

namespace HomeCenter.Extensions
{
    public static class CancellationTokenExtensions 
    { 
        public static Task AsTask(this CancellationToken cancellationToken) 
        { 
            var tcs = new TaskCompletionSource<object>(); 
            cancellationToken.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false); 
            return tcs.Task; 
        } 
    }
}