using HomeCenter.Core.EventAggregator;
using HomeCenter.Core.Services;
using HomeCenter.Core.Services.I2C;
using Microsoft.Extensions.Logging;
using Quartz;

namespace HomeCenter.ComponentModel.Adapters
{
    public interface IAdapterServiceFactory
    {
        IEventAggregator GetEventAggregator();

        II2CBusService GetI2CService();

        IScheduler GetScheduler();

        ISerialMessagingService GetUartService();

        ILogger<T> GetLogger<T>() where T : class;
    }
}