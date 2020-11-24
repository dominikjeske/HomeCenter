using HomeCenter.Abstractions;
using Proto;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeCenter.Model.Triggers
{
    public class Trigger
    {
        public Event Event { get; }
        public IList<Command> Commands { get; }
        public Schedule Schedule { get; }
        public IValidable Condition { get; }

        public Trigger(Event @event, IList<Command> commands, Schedule schedule, IValidable condition)
        {
            Event = @event;
            Commands = commands;
            Schedule = schedule;
            Condition = condition;
        }

        public Task<bool> ValidateCondition() => Condition.Validate();

        public ActorMessageContext ToActorContext(PID actor) => ActorMessageContext.Create(actor, Condition, Commands.Where(x => !x.ContainsProperty(MessageProperties.IsFinishComand)).ToArray());

        //TODO default fo Schedule.WorkingTime
        public ActorMessageContext ToActorContextWithFinish(PID actor)
            => ActorMessageContext.Create(actor, Condition, Commands.Where(x => x.ContainsProperty(MessageProperties.IsFinishComand)), Schedule.WorkingTime.GetValueOrDefault(), Commands.Where(x => !x.ContainsProperty(MessageProperties.IsFinishComand)));
    }
}