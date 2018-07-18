using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Threading.Tasks;
using Wirehome.Core.EventAggregator;
using Wirehome.Model.Extensions;

namespace Wirehome.Extensions.Tests
{
    [TestClass]
    public class EventAggregatorTests : ReactiveTest
    {
        private static IEventAggregator InitAggregator()
        {
            return new EventAggregator();
        }

        [TestMethod]
        public void GetSubscriptors_WhenSubscribeForType_ShouldReturnProperSubscriptions()
        {
            var aggregator = InitAggregator();

            aggregator.Subscribe<TestMessage>(handler => { });
            aggregator.Subscribe<OtherMessage>(handler => { });

            var result = aggregator.GetSubscriptors<TestMessage>(null);

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void GetSubscriptors_WhenSubscribeForType_ShouldReturnAlsoDerivedTypesSubscriptions()
        {
            var aggregator = InitAggregator();

            aggregator.Subscribe<TestMessage>(handler => { });
            aggregator.Subscribe<OtherMessage>(handler => { });

            var result = aggregator.GetSubscriptors<DerivedTestMessage>();

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void GetSubscriptors_WhenSubscribeWithSimpleFilter_ShouldReturnOnlySubscriptionsWithThatType()
        {
            var aggregator = InitAggregator();

            aggregator.Subscribe<TestMessage>(handler => { });
            aggregator.Subscribe<TestMessage>(handler => { }, "x");

            var result = aggregator.GetSubscriptors<TestMessage>("x");

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void GetSubscriptors_WhenSubscribeWithSimpleFilterAndSubscriblesHaveNoFilter_ShouldReturnNone()
        {
            var aggregator = InitAggregator();

            aggregator.Subscribe<TestMessage>(handler => { });
            aggregator.Subscribe<TestMessage>(handler => { });

            var result = aggregator.GetSubscriptors<TestMessage>("x");

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetSubscriptors_WhenSubscribeWithNoFilter_ShouldReturnOnlyNotFilteredSubscribles()
        {
            var aggregator = InitAggregator();

            aggregator.Subscribe<TestMessage>(handler => { });
            aggregator.Subscribe<TestMessage>(handler => { }, "x");

            var result = aggregator.GetSubscriptors<TestMessage>();

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void GetSubscriptors_WhenSubscribeWithStarFilter_ShouldResultAllNotfilteredAndFilteredSubscriblesByType()
        {
            var aggregator = InitAggregator();

            aggregator.Subscribe<TestMessage>(handler => { });
            aggregator.Subscribe<TestMessage>(handler => { }, "x");

            var result = aggregator.GetSubscriptors<TestMessage>("*");

            Assert.AreEqual(2, result.Count);
        }

        [TestMethod]
        public async Task QueryAsync_WhenSubscribed_ShouldReturnProperResult()
        {
            var aggregator = InitAggregator();

            aggregator.SubscribeForAsyncResult<TestMessage>(async handler =>
            {
                await Task.Delay(10).ConfigureAwait(false);
                return "Test";
            });

            var result = await aggregator.QueryAsync<TestMessage, string>(new TestMessage()).ConfigureAwait(false);

            Assert.AreEqual("Test", result);
        }

        [TestMethod]
        public async Task QueryAsync_WhenSubscribedWithProperSimpleFilter_ShouldReturnProperResult()
        {
            var aggregator = InitAggregator();

            aggregator.SubscribeForAsyncResult<TestMessage>(async handler =>
            {
                await Task.Delay(10).ConfigureAwait(false);
                return "Test";
            }, "DNF");

            var result = await aggregator.QueryAsync<TestMessage, string>(new TestMessage(), "DNF").ConfigureAwait(false);

            Assert.AreEqual("Test", result);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task QueryAsync_WhenTwoSubscribed_ShouldThrow()
        {
            var aggregator = InitAggregator();

            aggregator.SubscribeForAsyncResult<TestMessage>(async handler =>
            {
                await Task.Delay(50).ConfigureAwait(false);
                return "Slower";
            });

            aggregator.SubscribeForAsyncResult<TestMessage>(async handler =>
            {
                await Task.Delay(10).ConfigureAwait(false);
                return "Faster";
            });

            var result = await aggregator.QueryAsync<TestMessage, string>(new TestMessage()).ConfigureAwait(false);
        }

        [TestMethod]
        public void QueryAsync_WhenSubscribedForWrongReturnType_ShouldThrowInvalidCastException()
        {
            var aggregator = InitAggregator();

            aggregator.SubscribeForAsyncResult<TestMessage>(async handler =>
            {
                await Task.Delay(10).ConfigureAwait(false);
                return "Test";
            });

            AggregateExceptionHelper.AssertInnerException<InvalidCastException>(aggregator.QueryAsync<TestMessage, List<string>>(new TestMessage()));
        }

        [TestMethod]
        [ExpectedException(typeof(TimeoutException))]
        public async Task QueryAsync_WhenLongRun_ShouldThrowTimeoutException()
        {
            var aggregator = InitAggregator();

            aggregator.SubscribeForAsyncResult<TestMessage>(async handler =>
            {
                await Task.Delay(100).ConfigureAwait(false);
                return "Test";
            });

            await aggregator.QueryAsync<TestMessage, string>(new TestMessage(), timeout: TimeSpan.FromMilliseconds(50)).ConfigureAwait(false);
        }

        [TestMethod]
        public void QueryAsync_WhenExceptionInHandler_ShouldCatchIt()
        {
            var aggregator = InitAggregator();

            aggregator.SubscribeForAsyncResult<TestMessage>(async handler =>
            {
                await Task.Delay(10).ConfigureAwait(false);
                throw new TestException();
            });

            AggregateExceptionHelper.AssertInnerException<TestException>(aggregator.QueryAsync<TestMessage, string>(new TestMessage()));
        }

        [TestMethod]
        public async Task QueryAsync_WhenRetry_ShouldRunAgainAndSucceed()
        {
            var aggregator = InitAggregator();
            int i = 1;

            aggregator.SubscribeForAsyncResult<TestMessage>(async handler =>
            {
                await Task.Delay(10).ConfigureAwait(false);
                if (i-- > 0) throw new Exception("Test");
                return "OK";
            });

            var result = await aggregator.QueryAsync<TestMessage, string>(new TestMessage(), retryCount: 1).ConfigureAwait(false);
            Assert.AreEqual("OK", result);
        }

        [TestMethod]
        [ExpectedException(typeof(OperationCanceledException))]
        public async Task QueryAsync_WhenCanceled_ShouldThrowOperationCancel()
        {
            var aggregator = InitAggregator();

            aggregator.SubscribeForAsyncResult<TestMessage>(async handler =>
            {
                await Task.Delay(50).ConfigureAwait(false);
                return "OK";
            });

            var ts = new CancellationTokenSource();
            var result = aggregator.QueryAsync<TestMessage, string>(new TestMessage(), cancellationToken: ts.Token).ConfigureAwait(false);
            ts.Cancel();
            await result;
        }

        [TestMethod]
        public void IsSubscribed_WhenCheckForActiveSubscription_ShouldReturnTrue()
        {
            var aggregator = InitAggregator();

            var subscription = aggregator.SubscribeForAsyncResult<TestMessage>(async handler =>
            {
                await Task.Delay(50).ConfigureAwait(false);
                return "OK";
            });

            var result = aggregator.IsSubscribed(subscription.Token);
            Assert.AreEqual(result, true);
        }

        [TestMethod]
        public void UnSubscribe_WhenInvokedForActiveSubscription_ShouldRemoveIt()
        {
            var aggregator = InitAggregator();

            var subscription = aggregator.SubscribeForAsyncResult<TestMessage>(async handler =>
            {
                await Task.Delay(50).ConfigureAwait(false);
                return "OK";
            });

            aggregator.UnSubscribe(subscription.Token);

            var result = aggregator.IsSubscribed(subscription.Token);
            Assert.AreEqual(result, false);
        }

        [TestMethod]
        public void ClearSubscriptions_WhenInvoked_ShouldClearAllSubscriptions()
        {
            var aggregator = InitAggregator();

            aggregator.SubscribeForAsyncResult<TestMessage>(async handler =>
            {
                await Task.Delay(50).ConfigureAwait(false);
                return "OK";
            });

            aggregator.SubscribeForAsyncResult<TestMessage>(async handler =>
            {
                await Task.Delay(50).ConfigureAwait(false);
                return "OK";
            });

            aggregator.ClearSubscriptions();

            var result = aggregator.GetSubscriptors<TestMessage>(null);
            Assert.AreEqual(result.Count, 0);
        }

        [TestMethod]
        public void QueryWithResults_WhenSubscribed_ShouldReturnProperResult()
        {
            var aggregator = InitAggregator();
            var expected = new List<string> { "Test", "Test2" };

            aggregator.SubscribeForAsyncResult<TestMessage>(async handler =>
            {
                await Task.Delay(10).ConfigureAwait(false);
                return expected[0];
            });

            aggregator.SubscribeForAsyncResult<TestMessage>(async handler =>
            {
                await Task.Delay(30).ConfigureAwait(false);
                return expected[1];
            });

            var subscription = aggregator.QueryWithResults<TestMessage, string>(new TestMessage());

            subscription.AssertEqual(expected.ToObservable());
        }

        [TestMethod]
        public void QueryWithResults_WhenLongRun_ShouldTimeOut()
        {
            var aggregator = InitAggregator();
            var expected = new List<string> { "Test", "Test2" };

            aggregator.SubscribeForAsyncResult<TestMessage>(async handler =>
            {
                await Task.Delay(100).ConfigureAwait(false);
                return expected[1];
            });

            aggregator.SubscribeForAsyncResult<TestMessage>(async handler =>
            {
                await Task.Delay(100).ConfigureAwait(false);
                return expected[0];
            });

            var subscription = aggregator.QueryWithResults<TestMessage, string>(new TestMessage(), behaviors: new BehaviorChain().WithTimeout(TimeSpan.FromMilliseconds(10)));

            AggregateExceptionHelper.AssertInnerException<TimeoutException>(subscription);
        }

        [TestMethod]
        public void QueryWithResults_WhenCanceled_ShouldThrowOperationCanceledException()
        {
            var aggregator = InitAggregator();
            var expected = new List<string> { "Test", "Test2" };

            aggregator.SubscribeForAsyncResult<TestMessage>(async message =>
            {
                message.CancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(100).ConfigureAwait(false);
                return expected[1];
            });

            var ts = new CancellationTokenSource();
            ts.Cancel();

            var subscription = aggregator.QueryWithResults<TestMessage, string>(new TestMessage(), cancellationToken: ts.Token);

            AggregateExceptionHelper.AssertInnerException<TaskCanceledException>(subscription);
        }

        [TestMethod]
        public async Task Publish_WhenSubscribed_ShouldInvokeSubscriber()
        {
            var aggregator = InitAggregator();
            bool isWorking = false;

            aggregator.Subscribe<TestMessage>(handler =>
            {
                isWorking = true;
            });

            await aggregator.Publish(new TestMessage()).ConfigureAwait(false);

            Assert.AreEqual(true, isWorking);
        }

        [TestMethod]
        [ExpectedException(typeof(OperationCanceledException))]
        public async Task Publish_WhenCanceled_ShouldThrowOperationCanceledException()
        {
            var aggregator = InitAggregator();

            aggregator.Subscribe<TestMessage>(message =>
            {
                message.CancellationToken.ThrowIfCancellationRequested();
            });

            var ts = new CancellationTokenSource();
            ts.Cancel();

            await aggregator.Publish(new TestMessage(), cancellationToken: ts.Token).ConfigureAwait(false);
        }

        [TestMethod]
        public async Task QueryWithRepublishResult_WhenPublishWithResend_ShouldGetResultInSeparateSubscription()
        {
            var aggregator = InitAggregator();
            bool isWorking = false;

            aggregator.SubscribeForAsyncResult<TestMessage>(async handler =>
            {
                await Task.Delay(10).ConfigureAwait(false);
                return new OtherMessage();
            });

            aggregator.Subscribe<OtherMessage>((x) =>
            {
                isWorking = true;
            });

            await aggregator.QueryWithRepublishResult<TestMessage, OtherMessage>(new TestMessage()).ConfigureAwait(false);

            Assert.AreEqual(true, isWorking);
        }

        [TestMethod]
        public async Task Observe_ShouldWorkUntilDispose()
        {
            var aggregator = InitAggregator();
            int counter = 0;

            var messages = aggregator.Observe<TestMessage>();

            var subscription = messages.Subscribe(x =>
            {
                counter++;
            });

            await aggregator.Publish(new TestMessage()).ConfigureAwait(false);

            subscription.Dispose();

            await aggregator.Publish(new TestMessage()).ConfigureAwait(false);

            Assert.AreEqual(1, counter);
        }

        //[TestMethod]
        //public async Task RegisterHandlers_ShouldReristerHandlersFromContainer()
        //{
        //    var aggregator = InitAggregator();
        //    var handler = new TestHandler();
        //    var container = new Container(new ControllerOptions());
        //    container.RegisterSingleton(typeof(TestHandler), handler);
        //    aggregator.RegisterHandlers(container);

        //    await aggregator.Publish(new MotionEvent("test"));

        //    Assert.AreEqual(true, handler.IsHandled);
        //}
    }
}