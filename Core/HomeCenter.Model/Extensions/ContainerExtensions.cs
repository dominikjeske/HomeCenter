using HomeCenter.Core;
using HomeCenter.Core.Quartz;
using Quartz;
using Quartz.Spi;
using SimpleInjector;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeCenter.Model.Extensions
{
    public static class ContainerExtensions
    {
        public static IEnumerable<IService> GetSerives(this Container container) => container.GetAllInstances<IService>();

        public static async Task<Container> RegisterQuartz(this Container container)
        {
            var jobFactory = new SimpleInjectorJobFactory(container);
            var jobSchedulerFactory = new SimpleInjectorSchedulerFactory(jobFactory);
            var scheduler = await jobSchedulerFactory.GetScheduler().ConfigureAwait(false);

            container.RegisterInstance<IJobFactory>(jobFactory);
            container.RegisterInstance<ISchedulerFactory>(jobSchedulerFactory);
            container.RegisterInstance(scheduler);

            return container;
        }
    }
}