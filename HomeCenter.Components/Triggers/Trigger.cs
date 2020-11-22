using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeCenter.Abstractions;
using Proto;

namespace HomeCenter.Model.Triggers
{
    public class Trigger
    {
        public Event Event { get; init; }
        public IList<Command> Commands { get; init; }
        public Schedule Schedule { get; init; }
        public IValidable Condition { get; init; }

        public Task<bool> ValidateCondition() => Condition.Validate();

        public ActorMessageContext ToActorContext(PID actor) => ActorMessageContext.Create(actor, Condition, Commands.Where(x => !x.ContainsProperty(MessageProperties.IsFinishComand)).ToArray());


        //TODO default fo Schedule.WorkingTime
        public ActorMessageContext ToActorContextWithFinish(PID actor)
            => ActorMessageContext.Create(actor, Condition, Commands.Where(x => x.ContainsProperty(MessageProperties.IsFinishComand)), Schedule.WorkingTime.GetValueOrDefault(), Commands.Where(x => !x.ContainsProperty(MessageProperties.IsFinishComand)));

    }
}