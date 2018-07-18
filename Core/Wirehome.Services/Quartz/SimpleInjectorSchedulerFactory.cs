using Quartz.Spi;
using Quartz;
using Quartz.Impl;
using Quartz.Core;

namespace Wirehome.Core.Services.Quartz
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
