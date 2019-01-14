using HomeCenter.Broker;
using HomeCenter.Model.Core;
using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Events;
using HomeCenter.Model.Messages.Events.Device;
using HomeCenter.Model.Messages.Queries;
using HomeCenter.Model.Messages.Queries.Services;
using Microsoft.Reactive.Testing;
using Proto;
using System;
using System.Threading.Tasks;

namespace HomeCenter.Services.MotionService.Tests
{
    public class FakeMessageBroker : IMessageBroker
    {
        private readonly ITestableObservable<MotionEnvelope> _motionData;

        //Mock.Get(daylightService).Setup(x => x.Sunrise).Returns(TimeSpan.FromHours(8));
        //Mock.Get(daylightService).Setup(x => x.Sunset).Returns(TimeSpan.FromHours(20));

        public FakeMessageBroker(ITestableObservable<MotionEnvelope> motionData)
        {
            _motionData = motionData;
        }

        public IObservable<IMessageEnvelope<T>> Observe<T>() where T : Event
        {
            if(typeof(T) == typeof(MotionEvent))
            {
                return (IObservable<IMessageEnvelope<T>>)_motionData;
            }
            throw new NotImplementedException();
        }

        public Task PublishEvent<T>(T message, RoutingFilter routingFilter = null) where T : Event
        {
            throw new NotImplementedException();
        }

        public Task<R> QueryJsonService<T, R>(T query, RoutingFilter filter = null)
            where T : Query
            where R : class
        {
            throw new NotImplementedException();
        }

        public Task<R> QueryService<T, R>(T query, RoutingFilter filter = null)
            where T : Query
            where R : class
        {
            throw new NotImplementedException();
        }

        public Task<bool> QueryServiceWithVerify<T, Q, R>(T query, R expectedResult, RoutingFilter filter = null)
            where T : Query, IMessageResult<Q, R>
            where Q : class
            where R : class
        {
            throw new NotImplementedException();
        }

        public Task<R> Request<T, R>(T message, PID actor) where T : ActorMessage
        {
            throw new NotImplementedException();
        }

        public Task<R> Request<T, R>(T message, string uid) where T : ActorMessage
        {
            throw new NotImplementedException();
        }

        public void Send(object message, PID destination)
        {
            throw new NotImplementedException();
        }

        public void Send(object message, string uid, string address = null)
        {
            throw new NotImplementedException();
        }

        public Task SendToService<T>(T command, RoutingFilter filter = null) where T : Command
        {
            throw new NotImplementedException();
        }

        public SubscriptionToken SubscribeForMessage<T>(PID subscriber, RoutingFilter filter = null) where T : ActorMessage
        {
            throw new NotImplementedException();
        }

        public SubscriptionToken SubscribeForQuery<T, R>(PID subscriber, RoutingFilter filter = null) where T : Query
        {
            throw new NotImplementedException();
        }
    }
}