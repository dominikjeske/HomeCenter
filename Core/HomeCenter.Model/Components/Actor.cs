using HomeCenter.Core.Services.DependencyInjection;
using HomeCenter.Model.Core;
using HomeCenter.Model.Exceptions;
using Proto;
using Proto.Mailbox;
using System;
using System.Threading.Tasks;

namespace HomeCenter.ComponentModel.Components
{
    public abstract class Actor : BaseObject, IDisposable, IActor
    {
        internal PID Self { get; private set; }
        protected readonly DisposeContainer _disposables = new DisposeContainer();
        [Map] protected bool IsEnabled { get; private set; } = true;

        public virtual Task ReceiveAsync(IContext context) => Task.CompletedTask;
        
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

                //TODO kill actor

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


        //TODO invoke in proxy
        //private void AssertActorState()
        //{
        //    if (!IsEnabled) throw new UnsupportedStateException($"Component {Uid} is disabled");
        //}
    }
}