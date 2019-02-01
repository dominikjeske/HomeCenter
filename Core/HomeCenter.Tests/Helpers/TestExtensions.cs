using HomeCenter.Model.Core;
using HomeCenter.Model.Messages.Events;
using HomeCenter.Tests.Helpers;
using System.Threading.Tasks;

namespace HomeCenter.Tests.ComponentModel
{
    public static class TestExtensions
    {
        public static async Task<T> SubscribeAsync<T>(this IMessageBroker broker, int timeout = 15000) where T : Event
        {
            T result = default;
            var tcs = TaskHelper.GenerateTimeoutTaskSource<bool>(timeout);
            var subscription = broker.SubscribeForEvent<T>(ev =>
            {
                result = ev.Message;
                tcs.SetResult(true);
            }, "*");

            await tcs.Task;

            subscription.Dispose();

            return result;
        }
    }
}