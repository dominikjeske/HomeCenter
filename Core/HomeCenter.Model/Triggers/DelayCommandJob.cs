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
        private readonly IMessageBroker _actorMessageBroker;

        public DelayCommandJob(IMessageBroker actorMessageBroker)
        {
            _actorMessageBroker = actorMessageBroker;
        }

        public Task Execute(IJobExecutionContext context)
        {
            var command = context.GetDataContext<Command>();
            var destination = command.AsString(MessageProperties.MessageDestination);

            _actorMessageBroker.Send(command, destination);

            return Task.CompletedTask;
        }
    }
}