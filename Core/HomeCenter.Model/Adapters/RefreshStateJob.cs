using HomeCenter.Model.Core;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.Messages.Commands.Device;
using Proto;
using Quartz;
using System.Threading.Tasks;

namespace HomeCenter.Model.Adapters
{
    public class RefreshStateJob : IJob
    {
        private readonly IMessageBroker _actorMessageBroker;

        public RefreshStateJob(IMessageBroker actorMessageBroker)
        {
            _actorMessageBroker = actorMessageBroker;
        }

        public Task Execute(IJobExecutionContext context)
        {
            var adapter = context.GetDataContext<PID>();
            _actorMessageBroker.Send(RefreshCommand.Default, adapter);
            return Task.CompletedTask;
        }
    }
}