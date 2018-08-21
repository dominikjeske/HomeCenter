using CSharpFunctionalExtensions;
using HomeCenter.ComponentModel.Adapters;
using HomeCenter.ComponentModel.Commands;
using HomeCenter.Core;
using HomeCenter.Core.Extensions;
using HomeCenter.Core.Services.DependencyInjection;
using HomeCenter.Messaging;
using HomeCenter.Model.Exceptions;
using HomeCenter.Model.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace HomeCenter.ComponentModel.Components
{
    public abstract class Actor : BaseObject, IDisposable
    {
        [Map] protected bool IsEnabled { get; private set; } = true;
        protected bool _isInitialized;
        protected BufferBlock<CommandJob<object>> _commandQueue = new BufferBlock<CommandJob<object>>();
        protected readonly DisposeContainer _disposables = new DisposeContainer();

        protected Dictionary<string, Action<Command>> _commandHandlers = new Dictionary<string, Action<Command>>();
        protected Dictionary<string, Func<Command, Task>> _asyncCommandHandlers = new Dictionary<string, Func<Command, Task>>();
        protected Dictionary<string, Func<Query, Task<object>>> _asyncQueryHandlers = new Dictionary<string, Func<Query, Task<object>>>();

        public void Dispose() => _disposables.Dispose();

        public virtual Task Initialize()
        {
            Task.Run(HandleCommands, _disposables.Token);

            _isInitialized = true;

            return Task.CompletedTask;
        }

        public virtual Task ExecuteCommand(Command command)
        {
            AssertActorState();
            return QueueJob(command).Unwrap();
        }

        public virtual Task<T> ExecuteQuery<T>(Query command)
        {
            AssertActorState();
            var result = QueueJob(command).Unwrap();
            return result.Cast<T>();
        }

        private async Task<Task<object>> QueueJob(ActorMessage command)
        {
            var commandJob = new CommandJob<object>(command);
            var sendResult = await _commandQueue.SendAsync(commandJob).ConfigureAwait(false);
            return commandJob.Result;
        }

        protected Actor() => RegisterCommandHandlers();

        private void AssertActorState()
        {
            if (!IsEnabled) throw new UnsupportedStateException($"Component {Uid} is disabled");
            if (!_isInitialized) throw new UnsupportedStateException($"Component {Uid} is not initialized");
        }

        protected virtual Task<object> UnhandledCommand(Command command)
        {
            throw new MissingHandlerException($"Component [{Uid}] cannot process command because there is no registered handler for [{command.Type}]");
        }

        private void RegisterCommandHandlers()
        {
            var commandHandlers = GetHandlers<Command>();

            RegisterCommandHandlers(commandHandlers);
            RegisterAsyncCommandsHandlers(commandHandlers);

            var queryHandlers = GetHandlers<Query>();

            RegisterSpecificTaskHandler(queryHandlers);
            RegisterQueryHandlers(queryHandlers);
            RegisterAsyncQueryHandlers(queryHandlers);
        }

        private Dictionary<string, MethodInfo> GetHandlers<T>() where T : ActorMessage
        {
            var handlers = new Dictionary<string, MethodInfo>();

            foreach (var handler in GetType().GetMethodsBySignature(typeof(T)))
            {
                handlers.Add(handler.GetParameters().First().ParameterType.Name, handler);
            }
            return handlers;
        }

        private void RegisterCommandHandlers(Dictionary<string, MethodInfo> handlers)
        {
            var filtered = handlers.Where(h => h.Value.ReturnType == typeof(void)).ToList();
            foreach (var handler in filtered)
            {
                _commandHandlers.Add(handler.Key, (Action<Command>)Delegate.CreateDelegate(typeof(Action<Command>), this, handler.Value, false));
            }
            handlers.RemoveRange(filtered.Select(f => f.Key));
        }

        private void RegisterAsyncCommandsHandlers(Dictionary<string, MethodInfo> handlers)
        {
            var filtered = handlers.Where(h => h.Value.ReturnType == typeof(Task)).ToList();
            foreach (var handler in filtered)
            {
                _asyncCommandHandlers.Add(handler.Key, (Func<Command, Task>)Delegate.CreateDelegate(typeof(Func<Command, Task>), this, handler.Value, false));
            }
            handlers.RemoveRange(filtered.Select(f => f.Key));
        }

        private void RegisterQueryHandlers(Dictionary<string, MethodInfo> handlers)
        {
            handlers.ForEach(handler => _asyncQueryHandlers.Add(handler.Key, handler.Value.WrapSimpleTypeToGenericTask(this)));
        }

        private void RegisterAsyncQueryHandlers(Dictionary<string, MethodInfo> handlers)
        {
            var filtered = handlers.Where(h => h.Value.ReturnType == typeof(Task<object>)).ToList();
            foreach (var handler in filtered)
            {
                _asyncQueryHandlers.Add(handler.Key, (Func<Query, Task<object>>)Delegate.CreateDelegate(typeof(Func<Query, Task<object>>), this, handler.Value, false));
            }
            handlers.RemoveRange(filtered.Select(f => f.Key));
        }

        private void RegisterSpecificTaskHandler(Dictionary<string, MethodInfo> handlers)
        {
            var filtered = handlers.Where(h => h.Value.ReturnType.BaseType == typeof(Task)).ToList();
            filtered.ForEach(handler => _asyncQueryHandlers.Add(handler.Key, handler.Value.WrapTaskToGenericTask(this)));
            handlers.RemoveRange(filtered.Select(f => f.Key));
        }

        private async Task HandleCommands()
        {
            while (await _commandQueue.OutputAvailableAsync(_disposables.Token).ConfigureAwait(false))
            {
                var command = await _commandQueue.ReceiveAsync(_disposables.Token).ConfigureAwait(false);
                try
                {
                    var result = await ProcessCommand(command.Message).ConfigureAwait(false);
                    AssertForWrappedTask(result);
                    command.SetResult(result);
                }
                catch (Exception ex)
                {
                    command.SetException(ex);
                }
            }
        }

        private void AssertForWrappedTask(object result)
        {
            if (result?.GetType()?.Namespace == "System.Threading.Tasks") throw new UnwrappingResultException("Result from handler wan not unwrapped properly");
        }

        private Task<object> ProcessCommand(ActorMessage message)
        {
            if (_asyncQueryHandlers.ContainsKey(message.Type))
            {
                return _asyncQueryHandlers[message.Type].Invoke((Query)message);
            }
            else if (_asyncCommandHandlers.ContainsKey(message.Type))
            {
                return _asyncCommandHandlers[message.Type].Invoke((Command)message).Cast<object>(VoidResult.Void);
            }
            else if (_commandHandlers.ContainsKey(message.Type))
            {
                _commandHandlers[message.Type].Invoke((Command)message);
                return Task.FromResult<object>(VoidResult.Void);
            }
            else
            {
                //TODO unhandled query
                return UnhandledCommand((Command)message);
            }
        }
    }
}