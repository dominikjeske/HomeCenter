using Quartz;
using Quartz.Spi;
using System;

namespace HomeCenter.Quartz
{
    internal class SimpleInjectorJobFactory : IJobFactory
    {
        private readonly IServiceProvider _serviceProvider;

        //TODO do we need IServiceProvider??
        public SimpleInjectorJobFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            var jobDetail = bundle.JobDetail;
            var jobType = jobDetail.JobType;

            try
            {
                return new JobWrapper(bundle, _serviceProvider);
            }
            catch (Exception ex)
            {
                throw new SchedulerException($"Problem instantiating class '{jobDetail.JobType.FullName}'", ex);
            }
        }

        public void ReturnJob(IJob job)
        {
            (job as IDisposable)?.Dispose();
        }
    }
}