using HomeCenter.Abstractions;
using Quartz;
using System.Linq;
using System.Threading.Tasks;

namespace HomeCenter.Quartz
{
    public class ActorMessageJob : IJob
    {
        private readonly IMessageBroker _actorMessageBroker;

        public ActorMessageJob(IMessageBroker actorMessageBroker)
        {
            _actorMessageBroker = actorMessageBroker;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var actorMessageContext = context.GetDataContext<ActorMessageContext>();

            if (actorMessageContext.Condition != null)
            {
                var validationResult = await actorMessageContext.Condition.Validate();
                if (!validationResult) return;
            }

            foreach (var cmd in actorMessageContext.Commands)
            {
                _actorMessageBroker.Send(cmd, actorMessageContext.Actor);
            }

            if (actorMessageContext.FinishCommands.Any())
            {
                var time = context.FireTimeUtc.Add(actorMessageContext.FinishCommandTime.GetValueOrDefault());
                var finishContext = ActorMessageContext.Create(actorMessageContext.Actor, actorMessageContext.Condition, actorMessageContext.FinishCommands.ToArray());

                await _actorMessageBroker.SendAtTime(finishContext, time, actorMessageContext.Token);
            }
        }
    }
}