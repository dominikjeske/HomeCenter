using HomeCenter.Abstractions;
using Nito.AsyncEx;
using Quartz;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HomeCenter.Quartz
{
    internal class ActorScheduler : IActorScheduler
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly AsyncLazy<IScheduler> _scheduler;

        public ActorScheduler(ISchedulerFactory schedulerFactory)
        {
            _schedulerFactory = schedulerFactory;
            _scheduler = new AsyncLazy<IScheduler>(() => _schedulerFactory.GetScheduler());
        }

        public async Task Start(CancellationToken cancellationToken)
        {
            var scheduler = await _scheduler;
            await scheduler.Start(cancellationToken);
        }

        public async Task ShutDown()
        {
            var scheduler = await _scheduler;
            await scheduler.Shutdown();
        }

        public async Task SendWithSimpleRepeat(ActorMessageContext message, TimeSpan interval, CancellationToken token = default)
        {
            var scheduler = await _scheduler;
            await scheduler.ScheduleInterval<ActorMessageJob, ActorMessageContext>(interval, message, message.GetMessageUid(), token);
        }

        public async Task SendWithCronRepeat(ActorMessageContext message, string cronExpression, CancellationToken token = default, string? calendar = default)
        {
            var scheduler = await _scheduler;
            var uid = message.GetMessageUid();

            await scheduler.ScheduleCron<ActorMessageJob, ActorMessageContext>(cronExpression, message, uid, token, calendar);
        }

        public async Task SendAfterDelay(ActorMessageContext message, TimeSpan delay, bool cancelExisting = true, CancellationToken token = default)
        {
            var scheduler = await _scheduler;
            var uid = message.GetMessageUid();

            if (cancelExisting) await scheduler.CancelJob(uid);

            await scheduler.DelayExecution<ActorMessageJob, ActorMessageContext>(delay, message, uid, token);
        }

        public async Task SendAtTime(ActorMessageContext message, DateTimeOffset time, CancellationToken token = default)
        {
            var scheduler = await _scheduler;
            var uid = message.GetMessageUid();

            await scheduler.DelayExecution<ActorMessageJob, ActorMessageContext>(time, message, uid, token);
        }

        public async Task SendDailyAt(ActorMessageContext message, TimeSpan time, CancellationToken token = default, string? calendar = default)
        {
            var scheduler = await _scheduler;
            var uid = message.GetMessageUid();

            await scheduler.ScheduleDailyTimeInterval<ActorMessageJob, ActorMessageContext>(time, message, uid, token);
        }
    }
}