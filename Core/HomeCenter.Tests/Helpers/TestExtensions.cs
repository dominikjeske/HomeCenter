using HomeCenter.Model.Core;
using HomeCenter.Model.Messages.Events;
using HomeCenter.Tests.Helpers;
using HomeCenter.Utils.Extensions;
using System;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace HomeCenter.Tests.ComponentModel
{
    public static class TestExtensions
    {
        public static async Task<T> WaitForEvent<T>(this IMessageBroker broker, Action messageGenerator = null, int timeout = 500) where T : Event
        {
            T result = default;
            var tcs = TaskHelper.GenerateTimeoutTaskSource<bool>(timeout);
            var subscription = broker.SubscribeForEvent<T>(ev =>
            {
                result = ev.Message;
                tcs.SetResult(true);
            }, "*");

            _ = Task.Run(() => messageGenerator());

            await tcs.Task;

            subscription.Dispose();

            return result;
        }

        public static async Task<T> WaitForMessage<T>(this Task<T> task, Action messageGenerator = null, int timeout = 500) where T : class
        {
            _ = Task.Run(() => messageGenerator());
            var result = await task.WhenDone(TimeSpan.FromMilliseconds(timeout));

            return result;
        }

        public static Task<T> ToFirstTask<T>(this IObservable<T> observable) => observable.FirstAsync().ToTask();
    }
}