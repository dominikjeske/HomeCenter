using HomeCenter.Assemblies;
using HomeCenter.Model.Calendars;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System.Threading.Tasks;

namespace HomeCenter.Quartz
{
    public static class IServiceCollectionExtensions
    {
        //TODO
        public static void RegisterQuartz(IServiceCollection services)
        {
            //var jobFactory = new SimpleInjectorJobFactory(_container);
            //var jobSchedulerFactory = new SimpleInjectorSchedulerFactory(jobFactory);
            //var scheduler = await jobSchedulerFactory.GetScheduler();

            //_container.RegisterInstance<IJobFactory>(jobFactory);
            //_container.RegisterInstance<ISchedulerFactory>(jobSchedulerFactory);
            //_container.RegisterInstance(scheduler);
        }

        private static async Task LoadCalendars(IScheduler scheduler)
        {
            foreach (var calendarType in AssemblyHelper.GetAllTypes<IDayOffProvider>())
            {
                var dayOffProvider = calendarType.CreateInstance<IDayOffProvider>();
                var calendar = new QuartzCalendar(dayOffProvider);

                await scheduler.AddCalendar(dayOffProvider.Name, calendar, false, false);
            }
        }
    }
}