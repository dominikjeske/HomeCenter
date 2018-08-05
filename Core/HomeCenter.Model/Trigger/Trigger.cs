using HomeCenter.ComponentModel.Commands;
using HomeCenter.ComponentModel.Events;
using HomeCenter.Conditions;
using HomeCenter.Core.Services.DependencyInjection;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeCenter.ComponentModel.Components
{
    public class Trigger
    {
        [Map] public Event Event { get; private set; }
        [Map] public IList<Command> Commands { get; set; }
        [Map] public Schedule Schedule { get; private set; }
        [Map] public ConditionContainer Condition { get; private set; }

        public Task<bool> ValidateCondition() => Condition?.Validate() ?? Task.FromResult(true);

        //public TriggerJobDataDTO ToJobData(IActor actor) => new TriggerJobDataDTO
        //{
        //    Condition = Condition,
        //    Actor = actor,
        //    Command = Command
        //};

        //public TriggerJobDataDTO ToJobDataWithFinish(IActor actor) => new TriggerJobDataDTO
        //{
        //    Condition = Condition,
        //    Actor = actor,
        //    Command = Command,
        //    FinishCommand = FinishCommand,
        //    FinishCommandTime = Schedule.WorkingTime
        //};
    }
}