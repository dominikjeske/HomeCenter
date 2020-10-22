using HomeCenter.Assemblies;
using HomeCenter.Quartz;
using Microsoft.AspNetCore.Hosting;
using Quartz;
using System.Threading.Tasks;

[assembly: HostingStartup(typeof(QuartzHosting))]

namespace HomeCenter.Quartz
{
    public class QuartzInitializer
    {
        private readonly ISchedulerFactory _schedulerFactory;

        public QuartzInitializer(ISchedulerFactory schedulerFactory)
        {
            _schedulerFactory = schedulerFactory;
        }

        public async Task Initialize()
        {
            var scheduler = await _schedulerFactory.GetScheduler();

            foreach (var calendarType in AssemblyHelper.GetAllTypes<IDayOffProvider>())
            {
                var dayOffProvider = calendarType.CreateInstance<IDayOffProvider>();
                var calendar = new QuartzCalendar(dayOffProvider);

                await scheduler.AddCalendar(dayOffProvider.Name, calendar, false, false);
            }
        }
    }
}