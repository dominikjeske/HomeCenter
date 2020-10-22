using Quartz;
using Quartz.Impl.Matchers;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HomeCenter.Quartz
{
    public static class QuartzExtensions
    {
        private const string CONTEXT = "context";

        /// <summary>
        /// Schedule job <typeparamref name="T"/>  with interval <paramref name="interval"/> and data context <paramref name="dataContext"/>
        /// </summary>
        /// <typeparam name="T">Type of job to execute</typeparam>
        /// <typeparam name="D">Type of data context</typeparam>
        /// <param name="scheduler">Scheduler</param>
        /// <param name="interval">Interval on which we repeat job</param>
        /// <param name="dataContext">Data context</param>
        /// <param name="uid">UID for job registration</param>
        /// <param name="token">Cancellation token</param>
        public static async Task<JobKey> ScheduleInterval<T, D>(this IScheduler scheduler, TimeSpan interval, D dataContext, string uid, CancellationToken token = default) where T : IJob
        {
            IJobDetail job = JobBuilder.Create<T>()
                                       .WithIdentity(GetUniqueName(uid))
                                       .SetJobData(WrapJobData(dataContext))
                                       .Build();

            ITrigger trigger = TriggerBuilder.Create()
                                             .WithIdentity(GetUniqueName(uid))
                                             .WithSimpleSchedule(x => x.WithInterval(interval).RepeatForever())
                                             .Build();

            await scheduler.ScheduleJob(job, trigger, token);

            return job.Key;
        }

        /// <summary>
        /// Schedule job <typeparamref name="T"/>  with delay <paramref name="interval"/> and data context <paramref name="dataContext"/>
        /// </summary>
        /// <typeparam name="T">Type of job to execute</typeparam>
        /// <typeparam name="D">Type of data context</typeparam>
        /// <param name="scheduler">Scheduler</param>
        /// <param name="delay">Delay after which jobs executes</param>
        /// <param name="dataContext">Data context</param>
        /// <param name="uid">UID for job registration</param>
        /// <param name="token">Cancellation token</param>
        public static async Task<JobKey> DelayExecution<T, D>(this IScheduler scheduler, TimeSpan delay, D dataContext, string uid, CancellationToken token = default) where T : IJob
        {
            var job = JobBuilder.Create<T>()
                                .WithIdentity(GetUniqueName(uid))
                                .SetJobData(WrapJobData(dataContext))
                                .Build();

            var tb = TriggerBuilder.Create()
                                   .WithIdentity(GetUniqueName(uid))
                                   .StartAt(DateBuilder.FutureDate((int)delay.TotalMilliseconds, IntervalUnit.Millisecond));

            await scheduler.ScheduleJob(job, tb.Build(), token);

            return job.Key;
        }

        /// <summary>
        /// Schedule job <typeparamref name="T"/> at <paramref name="date"/> and data context <paramref name="dataContext"/>
        /// </summary>
        /// <typeparam name="T">Type of job to execute</typeparam>
        /// <typeparam name="D">Type of data context</typeparam>
        /// <param name="scheduler">Scheduler</param>
        /// <param name="date">Time when action should be executed</param>
        /// <param name="dataContext">Data context</param>
        /// <param name="uid">UID for job registration</param>
        /// <param name="token">Cancellation token</param>
        public static async Task<JobKey> DelayExecution<T, D>(this IScheduler scheduler, DateTimeOffset date, D dataContext, string uid, CancellationToken token = default) where T : IJob
        {
            var job = JobBuilder.Create<T>()
                                .WithIdentity(GetUniqueName(uid))
                                .SetJobData(WrapJobData(dataContext))
                                .Build();

            var tb = TriggerBuilder.Create()
                                   .WithIdentity(GetUniqueName(uid))
                                   .StartAt(date);

            await scheduler.ScheduleJob(job, tb.Build(), token);

            return job.Key;
        }

        /// <summary>
        /// Schedule job <typeparamref name="T"/>  with CRON pattern <paramref name="cronExpression"/> and data context <paramref name="dataContext"/>
        /// </summary>
        /// <typeparam name="T">Type of job to execute</typeparam>
        /// <typeparam name="D">Type of data context</typeparam>
        /// <param name="scheduler">Scheduler</param>
        /// <param name="cronExpression">Expression that defines schedule</param>
        /// <param name="dataContext">Data context</param>
        /// <param name="uid">UID for job registration</param>
        /// <param name="token">Cancellation token</param>
        /// <param name="calendar">Calendar that amends working days</param>
        public static async Task<JobKey> ScheduleCron<T, D>(this IScheduler scheduler, string cronExpression, D dataContext, string uid, CancellationToken token = default, string calendar = null) where T : IJob
        {
            var job = JobBuilder.Create<T>()
                                .WithIdentity(GetUniqueName(uid))
                                .SetJobData(WrapJobData(dataContext))
                                .Build();

            var tb = TriggerBuilder.Create()
                                   .WithIdentity(GetUniqueName(uid))
                                   .WithCronSchedule(cronExpression);

            if (calendar != null)
            {
                tb.ModifiedByCalendar(calendar);
            }

            await scheduler.ScheduleJob(job, tb.Build(), token);

            return job.Key;
        }

        /// <summary>
        /// Schedule job <typeparamref name="T"/> one time execution each day at <paramref name="dayTimeExecution"/> with data context <paramref name="dataContext"/>
        /// </summary>
        /// <typeparam name="T">Type of job to execute</typeparam>
        /// <typeparam name="D">Type of data context</typeparam>
        /// <param name="scheduler">Scheduler</param>
        /// <param name="dayTimeExecution">Time of day when execution starts</param>
        /// <param name="dataContext">Data context</param>
        /// <param name="uid">UID for job registration</param>
        /// <param name="token">Cancellation token</param>
        /// <param name="calendar">Calendar that amends working days</param>
        public static async Task<JobKey> ScheduleDailyTimeInterval<T, D>(this IScheduler scheduler, TimeSpan dayTimeExecution, D dataContext, string uid, CancellationToken token = default, string calendar = null) where T : IJob
        {
            var job = JobBuilder.Create<T>()
                                .WithIdentity(GetUniqueName(uid))
                                .SetJobData(WrapJobData(dataContext))
                                .Build();

            var tb = TriggerBuilder.Create()
                                   .WithIdentity(GetUniqueName(uid))
                                   .WithDailyTimeIntervalSchedule(d => d.StartingDailyAt(dayTimeExecution.ToTimeOfDay())
                                                                        .EndingDailyAfterCount(1));

            if (calendar != null)
            {
                tb.ModifiedByCalendar(calendar);
            }

            await scheduler.ScheduleJob(job, tb.Build(), token);

            return job.Key;
        }

        /// <summary>
        /// Cancels job schedule
        /// </summary>
        /// <param name="scheduler">Scheduler</param>
        /// <param name="searchPattern">search pattern for job</param>
        public static async Task CancelJob(this IScheduler scheduler, string searchPattern)
        {
            var current = (await scheduler.GetJobKeys(searchPattern)).FirstOrDefault();
            if (current != null)
            {
                await scheduler.DeleteJob(current);
            }
        }

        /// <summary>
        /// Get list of all registered jobs
        /// </summary>
        /// <param name="scheduler"></param>
        public static async Task<List<IJobDetail>> GetJobs(this IScheduler scheduler)
        {
            var jobs = new List<IJobDetail>();

            foreach (JobKey jobKey in await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup()))
            {
                jobs.Add(await scheduler.GetJobDetail(jobKey));
            }

            return jobs;
        }

        /// <summary>
        /// Add <paramref name="listner"/> for a given job <paramref name="key"/>
        /// </summary>
        /// <param name="scheduler"></param>
        /// <param name="listner"></param>
        /// <param name="key"></param>
        public static void AddListner(this IScheduler scheduler, IJobListener listner, JobKey key)
        {
            scheduler.ListenerManager.AddJobListener(listner, KeyMatcher<JobKey>.KeyEquals(key));
        }

        /// <summary>
        /// Get fire times for a given trigger
        /// </summary>
        /// <param name="trigger">Trigger we are operating on</param>
        /// <param name="numTimes">Number of scheduled operations we want to get</param>
        /// <returns></returns>
        public static IReadOnlyList<DateTimeOffset> GetFireTimes(this ITrigger trigger, int numTimes = 10)
        {
            return TriggerUtils.ComputeFireTimes(trigger as IOperableTrigger, null, numTimes);
        }

        /// <summary>
        /// Gets default data context for <paramref name="context"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public static T GetDataContext<T>(this IJobExecutionContext context) where T : class
        {
            if (context.JobDetail.JobDataMap.TryGetValue(CONTEXT, out object value))
            {
                return value as T;
            }
            return default;
        }

        private static JobDataMap WrapJobData<D>(D data)
        {
            return new JobDataMap
            {
                { CONTEXT, data }
            };
        }

        private static async Task<List<JobKey>> GetJobKeys(this IScheduler scheduler, string searchPattern)
        {
            var jobs = new List<JobKey>();

            foreach (JobKey jobKey in await scheduler.GetJobKeys(GroupMatcher<JobKey>.AnyGroup()))
            {
                if (jobKey.Name.IndexOf(searchPattern) > -1)
                {
                    jobs.Add(jobKey);
                }
            }

            return jobs;
        }

        private static string GetUniqueName(string uid) => $"{uid}_{Guid.NewGuid()}";
    }
}