using Quartz;
using Quartz.Core;
using Quartz.Impl;
using Quartz.Spi;

namespace HomeCenter.Services.DI
{
    public class SimpleInjectorSchedulerFactory : StdSchedulerFactory
    {
        private readonly IJobFactory _jobFactory;

        public SimpleInjectorSchedulerFactory(IJobFactory jobFactory)
        {
            _jobFactory = jobFactory;
        }

        protected override IScheduler Instantiate(QuartzSchedulerResources rsrcs, QuartzScheduler qs)
        {
            qs.JobFactory = _jobFactory;
            return base.Instantiate(rsrcs, qs);
        }
    }
}