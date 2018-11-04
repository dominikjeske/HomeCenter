using HomeCenter.Model.Conditions;
using HomeCenter.Model.Core;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Events;
using Proto;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace HomeCenter.Model.Triggers
{
    public class Trigger
    {
        [Map] public Event Event { get; private set; }
        [Map] public IList<Command> Commands { get; set; }
        [Map] public Schedule Schedule { get; private set; }
        [Map] public ConditionContainer Condition { get; private set; }

        public Task<bool> ValidateCondition() => Condition?.Validate() ?? Task.FromResult(true);

        public TriggerJobDataDTO ToJobData(PID actor) => new TriggerJobDataDTO
        {
            Condition = Condition,
            Actor = actor,
            Commands = Commands.Where(x => !x.ContainsProperty(CommandProperties.IsFinishComand)).ToList()
        };

        public TriggerJobDataDTO ToJobDataWithFinish(PID actor) => new TriggerJobDataDTO
        {
            Condition = Condition,
            Actor = actor,
            Commands = Commands.Where(x => !x.ContainsProperty(CommandProperties.IsFinishComand)).ToList(),
            FinishCommands = Commands.Where(x => x.ContainsProperty(CommandProperties.IsFinishComand)).ToList(),
            FinishCommandTime = Schedule.WorkingTime
        };
    }
}