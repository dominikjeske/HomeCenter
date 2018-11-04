using HomeCenter.Model.Core;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands;
using Quartz;
using System.Threading.Tasks;

namespace HomeCenter.Model.Triggers
{
    public class DelayCommandJob : IJob
    {
        private readonly IActorMessageBroker _actorMessageBroker;

        public DelayCommandJob(IActorMessageBroker actorMessageBroker)
        {
            _actorMessageBroker = actorMessageBroker;
        }

        public Task Execute(IJobExecutionContext context)
        {
            var command = context.GetDataContext<Command>();
            var uid = command[MessageProperties.MessageSource].AsString();

            _actorMessageBroker.Send(command, uid);

            return Task.CompletedTask;
        }
    }
}