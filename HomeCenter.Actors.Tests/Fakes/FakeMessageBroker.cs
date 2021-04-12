using HomeCenter.Abstractions;
using HomeCenter.Abstractions.Defaults;
using HomeCenter.Actors.Tests.Helpers;
using HomeCenter.EventAggregator;
using HomeCenter.Messages.Commands.Device;
using HomeCenter.Messages.Events.Device;
using HomeCenter.Messages.Queries.Device;
using HomeCenter.Model.Triggers;
using Microsoft.Reactive.Testing;
using Proto;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace HomeCenter.Actors.Tests.Fakes
{
    public class FakeMessageBroker : IMessageBroker
    {
        private readonly ITestableObservable<MotionEnvelope> _motionData;
        private readonly Dictionary<string, FakeMotionLamp> _lamps;
        private readonly Subject<IMessageEnvelope<PowerStateChangeEvent>> _powerStateSubject = new Subject<IMessageEnvelope<PowerStateChangeEvent>>();


        public FakeMessageBroker(ITestableObservable<MotionEnvelope> motionData, Dictionary<string, FakeMotionLamp> lamps)
        {
            _motionData = motionData;
            _lamps = lamps;
        }

        public PID GetPID(string uid, [AllowNull] string address = null)
        {
            throw new NotImplementedException();
        }

        public IObservable<IMessageEnvelope<T>> Observe<T>([AllowNull] RoutingFilter routingFilter = null) where T : Event
        {
            if (typeof(T) == typeof(MotionEvent))
            {
                return (IObservable<IMessageEnvelope<T>>)_motionData;
            }
            else if (typeof(T) == typeof(PowerStateChangeEvent))
            {
                return (IObservable<IMessageEnvelope<T>>)_powerStateSubject.Where(x => x.Message.MessageSource == routingFilter?.RoutingKey).AsObservable();
            }
            throw new NotImplementedException();
        }

        public Task Publish<T>(T message, [AllowNull] RoutingFilter routingFilter = null) where T : ActorMessage
        {
            throw new NotImplementedException();
        }

        public Task<Event> PublishWithTranslate(ActorMessage source, ActorMessage destination, [AllowNull] RoutingFilter filter = null)
        {
            throw new NotImplementedException();
        }

        public Task<R> QueryJsonService<T, R>(T query, [AllowNull] RoutingFilter filter = null)
            where T : Query
        {
            throw new NotImplementedException();
        }

        public Task<R> QueryService<T, R>(T query, [AllowNull] RoutingFilter filter = null)
            where T : Query
        {
            if (query is SunriseQuery)
            {
                var result = Task.FromResult(new TimeSpan(6, 0, 0)) as Task<R>;
                if (result is null) throw new InvalidCastException();
                return result;
            }
            else if (query is SunsetQuery)
            {
                var result = Task.FromResult(new TimeSpan(18, 0, 0)) as Task<R>;
                if (result is null) throw new InvalidCastException();
                return result;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public Task<bool> QueryServiceWithVerify<T, Q, R>(T query, R expectedResult, [AllowNull] RoutingFilter filter = null)
            where T : Query, IMessageResult<Q, R>
            where Q : class
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

        public void Send(object message, string uid, [AllowNull] string address = null)
        {
            if (message is TurnOnCommand turnOnCommand)
            {
                _lamps[uid].SetState(true);

                _powerStateSubject.OnNext(new MessageEnvelope<PowerStateChangeEvent>(PowerStateChangeEvent.Create(true, uid, turnOnCommand.AsString(MessageProperties.EventTriggerType, EventTriggerType.Manual))));
            }
            else if (message is TurnOffCommand turnOffCommand)
            {
                _lamps[uid].SetState(false);

                _powerStateSubject.OnNext(new MessageEnvelope<PowerStateChangeEvent>(PowerStateChangeEvent.Create(true, uid, turnOffCommand.AsString(MessageProperties.EventTriggerType, EventTriggerType.Manual))));
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public Task SendAfterDelay(ActorMessageContext message, TimeSpan delay, bool cancelExisting = true, CancellationToken token = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task SendAtTime(ActorMessageContext message, DateTimeOffset time, CancellationToken token = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task SendDailyAt(ActorMessageContext message, TimeSpan time, CancellationToken token = default(CancellationToken), [AllowNull] string calendar = null)
        {
            throw new NotImplementedException();
        }

        public Task SendToService<T>(T command, [AllowNull] RoutingFilter filter = null) where T : Command
        {
            throw new NotImplementedException();
        }

        public Task SendWithCronRepeat(ActorMessageContext message, string cronExpression, CancellationToken token = default(CancellationToken), [AllowNull] string calendar = null)
        {
            throw new NotImplementedException();
        }

        public Task SendWithSimpleRepeat(ActorMessageContext message, TimeSpan interval, CancellationToken token = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public void SendWithTranslate(ActorMessage source, ActorMessage destination, string address)
        {
            throw new NotImplementedException();
        }

        public SubscriptionToken SubscribeForEvent<T>(Action<IMessageEnvelope<T>> action, [AllowNull] RoutingFilter filter = null) where T : Event
        {
            throw new NotImplementedException();
        }

        public SubscriptionToken SubscribeForEvent<T>(Func<IMessageEnvelope<T>, Task> action, [AllowNull] RoutingFilter filter = null) where T : Event
        {
            throw new NotImplementedException();
        }

        public SubscriptionToken SubscribeForMessage<T>(PID subscriber, bool subscribeOnParent, [AllowNull] RoutingFilter filter = null) where T : ActorMessage
        {
            return SubscriptionToken.Empty;
        }

        public SubscriptionToken SubscribeForQuery<T, R>(PID subscriber, bool subscribeOnParent, [AllowNull] RoutingFilter filter = null) where T : Query
        {
            throw new NotImplementedException();
        }
    }
}