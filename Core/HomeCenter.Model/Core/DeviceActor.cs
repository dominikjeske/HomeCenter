using HomeCenter.Broker;
using HomeCenter.Model.Exceptions;
using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Events;
using Microsoft.Extensions.Logging;
using Proto;
using Proto.Mailbox;
using Quartz;
using System;
using System.Threading.Tasks;

namespace HomeCenter.Model.Core
{
    public abstract class DeviceActor : BaseObject, IDisposable, IActor
    {
        [Map] protected bool IsEnabled { get; private set; } = true;
        [DI] protected IActorMessageBroker MessageBroker { get; set; }
        [DI] protected IScheduler Scheduler { get; set; }
        [DI] protected ILogger Logger { get; set; }

        protected readonly DisposeContainer _disposables = new DisposeContainer();
        protected PID Self { get; private set; }

        public virtual Task ReceiveAsync(IContext context) => Task.CompletedTask;

        protected virtual Task UnhandledMessage(IContext context)
        {
            if (context.Message is ActorMessage actorMessage)
            {
                throw new MissingHandlerException($"Component [{Uid}] cannot process message because there is no registered handler for [{actorMessage.Type ?? actorMessage.GetType().Name}]");
            }
            else
            {
                throw new UnsupportedMessageException($"Component [{Uid}] cannot process message because type {context.Message.GetType().Name} is not ActorMessage");
            }
        }

        //TODO kill actor and clean subscriptions ?
        public void Dispose() => _disposables.Dispose();

        protected virtual async Task<bool> HandleSystemMessages(IContext context)
        {
            // If actor is disabled we are ignoring all messages
            if (!IsEnabled)
            {
                Logger.LogInformation($"Device '{Uid}' is disabled and message type '{context.Message.GetType().Name}' will be ignored");
                return true;
            }

            var msg = context.Message;
            if (msg is Started)
            {
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
            //TODO check if this is executed only once
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

        protected void Subscribe<T>(RoutingFilter filter = null) where T : ActorMessage
        {
            _disposables.Add(MessageBroker.SubscribeForMessage<T>(Self, filter));
        }

        //TODO invoke in proxy
        //private void AssertActorState()
        //{
        //    if (!IsEnabled) throw new UnsupportedStateException($"Component {Uid} is disabled");
        //}
    }
}