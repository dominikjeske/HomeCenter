using Quartz;
using System;
using Wirehome.Core.EventAggregator;
using Wirehome.Core.Services;
using Wirehome.Core.Services.I2C;
using Wirehome.Core.Services.Logging;

namespace Wirehome.ComponentModel.Adapters
{
    public class AdapterServiceFactory : IAdapterServiceFactory
    {
        protected readonly II2CBusService _i2CBusService;
        protected readonly IEventAggregator _eventAggregator;
        protected readonly IScheduler _scheduler;
        private readonly ILogService _logService;
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly ISerialMessagingService _serialMessagingService;

        public AdapterServiceFactory(IEventAggregator eventAggregator, ISchedulerFactory schedulerFactory, II2CBusService i2CBusService, 
            ILogService logService, ISerialMessagingService serialMessagingService)
        {
            _i2CBusService = i2CBusService ?? throw new ArgumentNullException(nameof(i2CBusService));
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
            _schedulerFactory = schedulerFactory ?? throw new ArgumentNullException(nameof(schedulerFactory));
            _serialMessagingService = serialMessagingService ?? throw new ArgumentNullException(nameof(serialMessagingService));
        }

        public ILogService GetLogger() => _logService;
        public II2CBusService GetI2CService() => _i2CBusService;
        public IEventAggregator GetEventAggregator() => _eventAggregator;
        public ISchedulerFactory GetSchedulerFactory() => _schedulerFactory;
        public ISerialMessagingService GetUartService() => _serialMessagingService;
    }
}