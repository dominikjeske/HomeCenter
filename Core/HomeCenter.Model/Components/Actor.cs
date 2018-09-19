using HomeCenter.ComponentModel.Commands;
using HomeCenter.Core;
using HomeCenter.Core.Services.DependencyInjection;
using HomeCenter.Model.Exceptions;
using Proto;
using Proto.Mailbox;
using System;
using System.Threading.Tasks;

namespace HomeCenter.ComponentModel.Components
{
    public abstract class Actor : BaseObject, IDisposable, IActor
    {
        //TODO check for obsolate
        protected bool _isInitialized;

        //TODO check for obsolate
        protected readonly DisposeContainer _disposables = new DisposeContainer();

        [Map] protected bool IsEnabled { get; private set; } = true;

        public virtual Task ReceiveAsync(IContext context)
        {
            return Task.CompletedTask;
        }

        protected virtual Task<object> UnhandledCommand(Command command)
        {
            throw new MissingHandlerException($"Component [{Uid}] cannot process command because there is no registered handler for [{command.Type}]");
        }

        public void Dispose() => _disposables.Dispose();

        //TODO invoke in proxy
        private void AssertActorState()
        {
            if (!IsEnabled) throw new UnsupportedStateException($"Component {Uid} is disabled");
            if (!_isInitialized) throw new UnsupportedStateException($"Component {Uid} is not initialized");
        }

        protected virtual async Task<bool> HandleSystemMessages(IContext context)
        {
            var msg = context.Message;
            if (msg is Started)
            {
                if (!IsEnabled) return false;
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
            _isInitialized = true;

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
    }
}