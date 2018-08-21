using HomeCenter.ComponentModel.Commands;
using HomeCenter.ComponentModel.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HomeCenter.Model.Components
{
    public class ActorLoggingDecorator : Actor
    {
        private readonly Actor _decoratee;
        private readonly ILogger<Actor> _logger;

        public ActorLoggingDecorator(Actor decoratee, ILogger<Actor> logger)
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

        public Task<T> ExecuteQuery<T>(Query query)
        {
            var task = _decoratee.ExecuteQuery<T>(query);
            return WrapExecution<T>(task, $"Exception while executing query {query.Type} on component {_decoratee.Uid}");
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