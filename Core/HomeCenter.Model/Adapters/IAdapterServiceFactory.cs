using Quartz;
using HomeCenter.Core.EventAggregator;
using HomeCenter.Core.Services.I2C;
using HomeCenter.Core.Services.Logging;
using HomeCenter.Core.Services;

namespace HomeCenter.ComponentModel.Adapters
{
    public interface IAdapterServiceFactory
    {
        IEventAggregator GetEventAggregator();

        II2CBusService GetI2CService();

        ILogService GetLogger();

        ISchedulerFactory GetSchedulerFactory();

        ISerialMessagingService GetUartService();
    }
}