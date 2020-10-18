using HomeCenter.Model.Conditions;
using HomeCenter.Model.Core;
using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Events;
using HomeCenter.Model.Messages.Scheduler;
using Proto;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeCenter.Model.Triggers
{
    public class Trigger
    {
        public Event Event { get; init; }
        public IList<Command> Commands { get; init; }
        public Schedule Schedule { get; init; }
        public IValidable Condition { get; init; }

        public Task<bool> ValidateCondition() => Condition.Validate();

        public ActorMessageContext ToActorContext(PID actor) => new ActorMessageContext
        {
            Condition = Condition,
            Actor = actor,
            Commands = Commands.Where(x => !x.ContainsProperty(MessageProperties.IsFinishComand)).ToList()
        };

        public ActorMessageContext ToActorContextWithFinish(PID actor) => new ActorMessageContext
        {
            Condition = Condition,
            Actor = actor,
            Commands = Commands.Where(x => !x.ContainsProperty(MessageProperties.IsFinishComand)).ToList(),
            FinishCommands = Commands.Where(x => x.ContainsProperty(MessageProperties.IsFinishComand)).ToList(),
            FinishCommandTime = Schedule.WorkingTime
        };
    }
}