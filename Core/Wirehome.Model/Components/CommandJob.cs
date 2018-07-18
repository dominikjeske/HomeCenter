using System;
using System.Threading.Tasks;
using Wirehome.ComponentModel.Commands;

namespace Wirehome.ComponentModel.Adapters
{
    public class CommandJob<T>
    {
        private TaskCompletionSource<T> _result { get; } = new TaskCompletionSource<T>();
        public Command Command { get; }
        public Task<T> Result => _result.Task;

        public CommandJob(Command command) => Command = command;

        public void SetResult(T result) => _result.SetResult(result);
        public void SetException(Exception error) => _result.SetException(error);
    }
}