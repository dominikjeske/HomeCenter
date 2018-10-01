using HomeCenter.Core.Services.DependencyInjection;
using HomeCenter.Messaging;
using HomeCenter.Messaging.Exceptions;
using HomeCenter.Model.Exceptions;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Queries;
using HomeCenter.Model.Messages.Queries.Services;
using Proto;
using Proto.Mailbox;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeCenter.Model.Core
{
    public abstract class Actor : BaseObject, IDisposable, IActor
    {
        public PID Self { get; private set; }
        protected readonly DisposeContainer _disposables = new DisposeContainer();
        protected readonly IEventAggregator _eventAggregator;

        [Map] protected bool IsEnabled { get; private set; } = true;

        public virtual Task ReceiveAsync(IContext context) => Task.CompletedTask;

        public Actor(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        protected virtual Task UnhandledCommand(Proto.IContext command)
        {
            if (command.Message is ActorMessage actorMessage)
            {
                throw new MissingHandlerException($"Component [{Uid}] cannot process message because there is no registered handler for [{actorMessage.Type}]");
            }
            else
            {
                throw new UnsupportedMessageException($"Component [{Uid}] cannot process message because type {command.Message.GetType().Name} is not ActorMessage");
            }
        }

        public void Dispose() => _disposables.Dispose();

        protected virtual async Task<bool> HandleSystemMessages(IContext context)
        {
            var msg = context.Message;
            if (msg is Started)
            {
                if (!IsEnabled) return false;

                //TODO kill actor and clean subscriptions

                await OnStarted(context).ConfigureAwait(false);
                return true;
            }
            else if (msg is Restarting)
            {
                await OnRestarting(context).ConfigureAwait(false);
                return true;
            }
            else if (msg is Restart)
            {
                await OnRestart(context).ConfigureAwait(false);
                return true;
            }
            else if (msg is Stop)
            {
                await OnStop(context).ConfigureAwait(false);
                return true;
            }
            else if (msg is Stopped)
            {
                await OnStopped(context).ConfigureAwait(false);
                return true;
            }
            else if (msg is Stopping)
            {
                await Stopping(context).ConfigureAwait(false);
                return true;
            }
            else if (msg is SystemMessage)
            {
                await OtherSystemMessage(context).ConfigureAwait(false);
                return true;
            }

            return false;
        }

        protected virtual Task OnStarted(IContext context)
        {
            Self = context.Self;
            return Task.CompletedTask;
        }

        protected virtual Task OnRestarting(IContext context)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnRestart(IContext context)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnStop(IContext context)
        {
            //TODO check if this is eecuted only once
            _disposables.Dispose();
            return Task.CompletedTask;
        }

        protected virtual Task OnStopped(IContext context)
        {
            return Task.CompletedTask;
        }

        protected virtual Task Stopping(IContext context)
        {
            return Task.CompletedTask;
        }

        protected virtual Task OtherSystemMessage(IContext context)
        {
            return Task.CompletedTask;
        }

        private void SubscribeForCommand<T>(RoutingFilter filter = null, IContext parent = null) where T : Command
        {
            _disposables.Add
            (
                _eventAggregator.Subscribe<T>(message => (parent ?? (IContext)RootContext.Empty).Send(Self, message.Message), filter)
            );
        }

        private void SubscribeForQuery<T, R>(RoutingFilter filter = null, IContext parent = null) where T : Query
        {
            _disposables.Add
            (
                _eventAggregator.SubscribeForAsyncResult<T>(async message =>
                {
                    var result = await (parent ?? (IContext)RootContext.Empty).RequestAsync<R>(Self, message.Message, message.CancellationToken).ConfigureAwait(false);
                    return result;
                }, filter)
            );
        }

        protected Task<R> QueryService<T, R>(T query, RoutingFilter filter = null) where T : Query
                                                                                   where R : class
        {
            if (query is IFormatableMessage<T> formatableMessage)
            {
                query = formatableMessage.FormatMessage();
            }

            return _eventAggregator.QueryAsync<T, R>(query, filter);
        }

        protected async Task<bool> QueryServiceWithVerify<T, Q, R>(T query, R expectedResult, RoutingFilter filter = null) where T : Query, IMessageResult<Q, R>
                                                                                                                           where Q : class
                                                                                                                           where R : class
        {
            var result = await QueryService<T, Q>(query, filter).ConfigureAwait(false);
            return query.Verify(result, expectedResult);
        }

        //TODO invoke in proxy
        //private void AssertActorState()
        //{
        //    if (!IsEnabled) throw new UnsupportedStateException($"Component {Uid} is disabled");
        //}
    }
}