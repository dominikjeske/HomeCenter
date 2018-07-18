using HomeCenter.ComponentModel.Commands;
using HomeCenter.ComponentModel.Events;
using HomeCenter.Conditions;
using HomeCenter.Core.Services.DependencyInjection;
using HomeCenter.Model.Extensions;

namespace HomeCenter.ComponentModel.Components
{
    public class Trigger
    {
        [Map] public Event Event { get; private set; }
        [Map] public Command Command { get; private set; }
        [Map] public Command FinishCommand { get; private set; }
        [Map] public Schedule Schedule { get; private set; }
        [Map] public ConditionContainer Condition { get; private set; }

        public TriggerJobDataDTO ToJobData(IActor actor) => new TriggerJobDataDTO
        {
            Condition = Condition,
            Actor = actor,
            Command = Command
        };

        public TriggerJobDataDTO ToJobDataWithFinish(IActor actor) => new TriggerJobDataDTO
        {
            Condition = Condition,
            Actor = actor,
            Command = Command,
            FinishCommand = FinishCommand,
            FinishCommandTime = Schedule.WorkingTime
        };
    }
}