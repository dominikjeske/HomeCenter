using Quartz;
using Wirehome.Core.EventAggregator;
using Wirehome.Core.Services.I2C;
using Wirehome.Core.Services.Logging;
using Wirehome.Core.Services;

namespace Wirehome.ComponentModel.Adapters
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