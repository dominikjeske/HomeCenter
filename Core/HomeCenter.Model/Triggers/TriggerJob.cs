using HomeCenter.Model.Core;
using HomeCenter.Model.Extensions;
using Quartz;
using System;
using System.Threading.Tasks;

namespace HomeCenter.Model.Triggers
{
    public class TriggerJob : IJob
    {
        private readonly IActorMessageBroker _actorMessageBroker;

        public TriggerJob(IActorMessageBroker actorMessageBroker)
        {
            _actorMessageBroker = actorMessageBroker;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var trigger = context.GetDataContext<TriggerJobDataDTO>();
            var time = context.FireTimeUtc.Add(trigger.FinishCommandTime.GetValueOrDefault());

            if (await trigger.Condition.Validate().ConfigureAwait(false))
            {
                foreach(var cmd in trigger.Commands)
                {
                    _actorMessageBroker.Send(cmd, trigger.Actor);
                }
            }

            if (trigger.FinishCommands?.Count > 0)
            {
                await ScheduleFinishJob(trigger, time, context.Scheduler).ConfigureAwait(false);
            }
        }

        private Task ScheduleFinishJob(TriggerJobDataDTO trigger, DateTimeOffset time, IScheduler scheduler)
        {
            var triggerData = new TriggerJobDataDTO
            {
                Condition = trigger.Condition,
                Actor = trigger.Actor,
                Commands = trigger.FinishCommands
            };

            var job = JobBuilder.Create<TriggerJob>()
                                       .WithIdentity($"{nameof(TriggerJob)}_{Guid.NewGuid()}")
                                       .SetJobData(triggerData.ToJobDataMap())
                                       .Build();

            var finishTrigger = TriggerBuilder.Create()
                                        .WithIdentity($"{nameof(TriggerJob)}_{Guid.NewGuid()}")
                                        .StartAt(time)
                                        .Build();
            return scheduler.ScheduleJob(job, finishTrigger, trigger.Token);
        }
    }
}