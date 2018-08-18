using HomeCenter.ComponentModel.Commands;
using HomeCenter.ComponentModel.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HomeCenter.Model.Components
{
    public class ActorLoggingDecorator : IActor
    {
        private readonly IActor _decoratee;
        private readonly ILogger<IActor> _logger;

        public ActorLoggingDecorator(IActor decoratee, ILogger<IActor> logger)
        {
            _decoratee = decoratee;
            _logger = logger;
        }

        public string Uid => _decoratee.Uid;

        public void Dispose()
        {
            try
            {
                _decoratee.Dispose();
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Exception disposing component {_decoratee.Uid}");
                throw;
            }
        }

        public Task ExecuteCommand(Command command)
        {
            var task = _decoratee.ExecuteCommand(command);
            return WrapExecution<bool>((Task<bool>)task, $"Exception while executing command {command.Type} on component {_decoratee.Uid}");
        }

        public Task<T> ExecuteQuery<T>(Command command)
        {
            var task = _decoratee.ExecuteQuery<T>(command);
            return WrapExecution<T>(task, $"Exception while executing command {command.Type} on component {_decoratee.Uid}");
        }

        public Task Initialize()
        {
            var task = _decoratee.Initialize();
            return WrapExecution<bool>((Task<bool>)task, $"Exception initializing component {_decoratee.Uid}");
        }

        private Task<T> WrapExecution<T>(Task<T> task, string message)
        {
            var tcs = new TaskCompletionSource<T>();
            task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    _logger.LogError(t.Exception, message);
                    tcs.TrySetException(t.Exception.InnerExceptions);
                }
                else if (t.IsCanceled)
                {
                    tcs.TrySetCanceled();
                }
                else
                {
                    tcs.TrySetResult(t.Result);
                }
            }, TaskContinuationOptions.ExecuteSynchronously);
            return tcs.Task;
        }
    }
}