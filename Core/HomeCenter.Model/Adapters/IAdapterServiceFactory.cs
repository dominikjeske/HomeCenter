using HomeCenter.Broker;
using HomeCenter.Model.Native;
using Microsoft.Extensions.Logging;
using Quartz;

namespace HomeCenter.Model.Adapters
{
    public interface IAdapterServiceFactory
    {
        IEventAggregator GetEventAggregator();

        II2CBusService GetI2CService();

        IScheduler GetScheduler();

        ILogger<T> GetLogger<T>() where T : class;
    }
}