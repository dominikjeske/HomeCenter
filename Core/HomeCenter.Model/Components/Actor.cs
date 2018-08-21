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
using System.Text.RegularExpressions;
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
        protected Dictionary<string, Func<Command, Task<object>>> _asyncQueryHandlers = new Dictionary<string, Func<Command, Task<object>>>();
        protected Dictionary<string, Func<Command, Task>> _asyncCommandHandlers = new Dictionary<string, Func<Command, Task>>();
        protected Dictionary<string, Action<Command>> _commandHandlers = new Dictionary<string, Action<Command>>();

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

        private async Task<Task<object>> QueueJob(Command command)
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
            var handlers = GetHandlers();

            RegisterAsyncQueryHandlers(handlers);
            RegisterAsyncCommandsHandlers(handlers);
            RegisterActionHandlers(handlers);
            RegisterSpecificTaskHandler(handlers);
            RegisterCustomTypeHandlers(handlers);
        }

        private Dictionary<string, MethodInfo> GetHandlers()
        {
            var handlers = new Dictionary<string, MethodInfo>();
            Regex r = new Regex(@"^(?<Name>\w*)Handler", RegexOptions.Compiled);
            foreach (var handler in GetType().GetMethodsBySignature(typeof(Command)))
            {
                var commandName = GetCommandName(r, handler);
                if (commandName.HasValue)
                {
                    handlers.Add(commandName.Value, handler);
                }
            }
            return handlers;
        }

        private void RegisterAsyncQueryHandlers(Dictionary<string, MethodInfo> handlers)
        {
            var filtered = handlers.Where(h => h.Value.ReturnType == typeof(Task<object>)).ToList();
            foreach (var handler in filtered)
            {
                _asyncQueryHandlers.Add(handler.Key, (Func<Command, Task<object>>)Delegate.CreateDelegate(typeof(Func<Command, Task<object>>), this, handler.Value, false));
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

        private void RegisterActionHandlers(Dictionary<string, MethodInfo> handlers)
        {
            var filtered = handlers.Where(h => h.Value.ReturnType == typeof(void)).ToList();
            foreach (var handler in filtered)
            {
                _commandHandlers.Add(handler.Key, (Action<Command>)Delegate.CreateDelegate(typeof(Action<Command>), this, handler.Value, false));
            }
            handlers.RemoveRange(filtered.Select(f => f.Key));
        }

        private void RegisterSpecificTaskHandler(Dictionary<string, MethodInfo> handlers)
        {
            var filtered = handlers.Where(h => h.Value.ReturnType.BaseType == typeof(Task)).ToList();
            filtered.ForEach(handler => _asyncQueryHandlers.Add(handler.Key, handler.Value.WrapTaskToGenericTask(this)));
            handlers.RemoveRange(filtered.Select(f => f.Key));
        }

        private void RegisterCustomTypeHandlers(Dictionary<string, MethodInfo> handlers)
        {
            handlers.ForEach(handler => _asyncQueryHandlers.Add(handler.Key, handler.Value.WrapSimpleTypeToGenericTask(this)));
        }

        private Maybe<string> GetCommandName(Regex r, MethodInfo handler)
        {
            var match = r.Match(handler.Name);
            if (match?.Success ?? false)
            {
                return match.Groups["Name"].Value;
            }
            return Maybe<string>.None;
        }

        private async Task HandleCommands()
        {
            while (await _commandQueue.OutputAvailableAsync(_disposables.Token).ConfigureAwait(false))
            {
                var command = await _commandQueue.ReceiveAsync(_disposables.Token).ConfigureAwait(false);
                try
                {
                    var result = await ProcessCommand(command.Command).ConfigureAwait(false);
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

        private Task<object> ProcessCommand(Command command)
        {
            if (_asyncQueryHandlers.ContainsKey(command.Type))
            {
                return _asyncQueryHandlers[command.Type].Invoke(command);
            }
            else if (_asyncCommandHandlers.ContainsKey(command.Type))
            {
                return _asyncCommandHandlers[command.Type].Invoke(command).Cast<object>(VoidResult.Void);
            }
            else if (_commandHandlers.ContainsKey(command.Type))
            {
                _commandHandlers[command.Type].Invoke(command);
                return Task.FromResult<object>(VoidResult.Void);
            }
            else
            {
                return UnhandledCommand(command);
            }
        }
    }
}