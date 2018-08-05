using Quartz;
using Quartz.Spi;
using SimpleInjector;
using System;

namespace HomeCenter.Core.Quartz
{
    public class SimpleInjectorJobFactory : IJobFactory
    {
        private readonly Container _container;

        public SimpleInjectorJobFactory(Container container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            var jobDetail = bundle.JobDetail;
            var jobType = jobDetail.JobType;

            try
            {
                return new JobWrapper(bundle, _container);
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