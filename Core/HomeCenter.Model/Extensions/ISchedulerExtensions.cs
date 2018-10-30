using HomeCenter.Model.Messages.Commands;
using Quartz;
using Quartz.Impl.Matchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HomeCenter.Model.Extensions
{
    public static class QuartzExtensions
    {
        private const string CONTEXT = "context";

        public static async Task<JobKey> ScheduleInterval<T, D>(this IScheduler scheduler, TimeSpan interval, D data, string uid, CancellationToken token = default) where T : IJob
        {
            IJobDetail job = JobBuilder.Create<T>()
                                       .WithIdentity(GetUniqueName(uid))
                                       .SetJobData(WrapJobData(data))
                                       .Build();

            ITrigger trigger = TriggerBuilder.Create()
                                             .WithIdentity(GetUniqueName(uid))
                                             .WithSimpleSchedule(x => x.WithInterval(interval).RepeatForever())
                                             .Build();

            await scheduler.ScheduleJob(job, trigger, token).ConfigureAwait(false);

            return job.Key;
        }

        public static async Task<JobKey> ScheduleCron<T, D>(this IScheduler scheduler, D data, string cronExpression, string uid, CancellationToken token = default, string calendar = null) where T : IJob
        {
            var job = JobBuilder.Create<T>()
                                .WithIdentity(GetUniqueName(uid))
                                .SetJobData(WrapJobData(data))
                                .Build();

            var tb = TriggerBuilder.Create()
                                   .WithIdentity(GetUniqueName(uid))
                                   .WithCronSchedule(cronExpression);

            if (calendar != null)
            {
                tb.ModifiedByCalendar(calendar);
            }

            await scheduler.ScheduleJob(job, tb.Build(), token).ConfigureAwait(false);

            return job.Key;
        }

        public static async Task<List<IJobDetail>> GetJobs(this IScheduler scheduler)
        {
            var jobs = new List<IJobDetail>();

            foreach (JobKey jobKey in await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup()).ConfigureAwait(false))
            {
                jobs.Add(await scheduler.GetJobDetail(jobKey).ConfigureAwait(false));
            }

            return jobs;
        }

        public static async Task<List<JobKey>> GetJobKeys(this IScheduler scheduler, string searchPattern)
        {
            var jobs = new List<JobKey>();

            foreach (JobKey jobKey in await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup()).ConfigureAwait(false))
            {
                if (jobKey.Name.IndexOf(searchPattern) > -1)
                {
                    jobs.Add(jobKey);
                }
            }

            return jobs;
        }

        public static async Task CancelJob(this IScheduler scheduler, string searchPattern)
        {
            var current = (await scheduler.GetJobKeys(searchPattern).ConfigureAwait(false)).FirstOrDefault();
            if (current != null)
            {
                await scheduler.DeleteJob(current).ConfigureAwait(false);
            }
        }

        public static async Task<JobKey> DelayExecution<T, D>(this IScheduler scheduler, TimeSpan delay, D data, string uid, CancellationToken token = default) where T : IJob
        {
            
            var job = JobBuilder.Create<T>()
                                .WithIdentity(GetUniqueName(uid))
                                .SetJobData(WrapJobData(data))
                                .Build();

            var tb = TriggerBuilder.Create()
                                   .WithIdentity(GetUniqueName(uid))
                                   .WithSimpleSchedule(s => s.WithInterval(delay));

            await scheduler.ScheduleJob(job, tb.Build(), token).ConfigureAwait(false);

            return job.Key;
        }

        public static async Task<JobKey> DelayExecution<T>(this IScheduler scheduler, TimeSpan delay, Command command, string uid, bool cancelExisting = true, CancellationToken token = default) where T : IJob
        {
            if (cancelExisting) await scheduler.CancelJob(uid).ConfigureAwait(false);

            var job = JobBuilder.Create<T>()
                                .WithIdentity(GetUniqueName(uid))
                                .SetJobData(WrapJobData(command))
                                .Build();

            var tb = TriggerBuilder.Create()
                                   .WithIdentity(GetUniqueName(uid))
                                   .WithSimpleSchedule(s => s.WithInterval(delay));

            await scheduler.ScheduleJob(job, tb.Build(), token).ConfigureAwait(false);

            return job.Key;
        }

        public static async Task<JobKey> ScheduleDailyTimeInterval<T, D>(this IScheduler scheduler, D data, TimeSpan time, string uid, CancellationToken token = default, string calendar = null) where T : IJob
        {
            var job = JobBuilder.Create<T>()
                                .WithIdentity(GetUniqueName(uid))
                                .SetJobData(WrapJobData(data))
                                .Build();

            var tb = TriggerBuilder.Create()
                                   .WithIdentity(GetUniqueName(uid))
                                   .WithDailyTimeIntervalSchedule(d => d.StartingDailyAt(time.ToTimeOfDay())
                                                                        .EndingDailyAfterCount(1));

            if (calendar != null)
            {
                tb.ModifiedByCalendar(calendar);
            }

            await scheduler.ScheduleJob(job, tb.Build(), token).ConfigureAwait(false);

            return job.Key;
        }

        public static void AddListner(this IScheduler scheduler, IJobListener listner, JobKey key)
        {
            scheduler.ListenerManager.AddJobListener(listner, KeyMatcher<JobKey>.KeyEquals(key));
        }

        public static IReadOnlyList<DateTimeOffset> GetFireTimes(this ITrigger trigger, int numTimes = 10)
        {
            return TriggerUtils.ComputeFireTimes(trigger as Quartz.Spi.IOperableTrigger, null, numTimes);
        }

        private static string GetUniqueName(string uid) => $"{uid}_{Guid.NewGuid()}";

        private static JobDataMap WrapJobData<D>(D data)
        {
            return new JobDataMap
            {
                { CONTEXT, data }
            };
        }

        public static T GetDataContext<T>(this IJobExecutionContext context) where T : class
        {
            if (context.JobDetail.JobDataMap.TryGetValue(CONTEXT, out object value))
            {
                return value as T;
            }
            return default;
        }
    }
}