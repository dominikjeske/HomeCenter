using HomeCenter.Model.Core;
using HomeCenter.Model.Messages.Events;
using HomeCenter.Tests.Helpers;
using System.Threading.Tasks;

namespace HomeCenter.Tests.ComponentModel
{
    public static class TestExtensions
    {
        public static async Task<T> SubscribeAndWait<T>(this IMessageBroker broker, int timeout = 2000) where T : Event
        {
            T result = default;
            var tcs = TaskHelper.GenerateTimeoutTaskSource<bool>(timeout);
            broker.SubscribeForEvent<T>(ev =>
            {
                result = ev.Message;
                tcs.SetResult(true);
            });
            await tcs.Task;

            return result;
        }
    }
}