using HomeCenter.Abstractions;
using Quartz;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HomeCenter.Quartz
{
    public class ActorScheduler : IActorScheduler
    {
        private readonly IScheduler _scheduler;

        public ActorScheduler(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public Task Start(CancellationToken cancellationToken)
        {
            return _scheduler.Start(cancellationToken);
        }

        public Task ShutDown()
        {
            return _scheduler.Shutdown();
        }

        public Task SendWithSimpleRepeat(ActorMessageContext message, TimeSpan interval, CancellationToken token = default)
        {
            return _scheduler.ScheduleInterval<ActorMessageJob, ActorMessageContext>(interval, message, message.GetMessageUid(), token);
        }

        public Task SendWithCronRepeat(ActorMessageContext message, string cronExpression, CancellationToken token = default, string calendar = default)
        {
            var uid = message.GetMessageUid();

            return _scheduler.ScheduleCron<ActorMessageJob, ActorMessageContext>(cronExpression, message, uid, token, calendar);
        }

        public async Task SendAfterDelay(ActorMessageContext message, TimeSpan delay, bool cancelExisting = true, CancellationToken token = default)
        {
            var uid = message.GetMessageUid();

            if (cancelExisting) await _scheduler.CancelJob(uid);

            await _scheduler.DelayExecution<ActorMessageJob, ActorMessageContext>(delay, message, uid, token);
        }

        public async Task SendAtTime(ActorMessageContext message, DateTimeOffset time, CancellationToken token = default)
        {
            var uid = message.GetMessageUid();

            await _scheduler.DelayExecution<ActorMessageJob, ActorMessageContext>(time, message, uid, token);
        }

        public async Task SendDailyAt(ActorMessageContext message, TimeSpan time, CancellationToken token = default, string calendar = default)
        {
            var uid = message.GetMessageUid();

            await _scheduler.ScheduleDailyTimeInterval<ActorMessageJob, ActorMessageContext>(time, message, uid, token);
        }
    }
}