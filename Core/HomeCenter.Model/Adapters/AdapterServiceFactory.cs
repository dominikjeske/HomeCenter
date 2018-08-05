using Quartz;
using System;
using HomeCenter.Core.EventAggregator;
using HomeCenter.Core.Services;
using HomeCenter.Core.Services.I2C;
using HomeCenter.Core.Services.Logging;

namespace HomeCenter.ComponentModel.Adapters
{
    public class AdapterServiceFactory : IAdapterServiceFactory
    {
        protected readonly II2CBusService _i2CBusService;
        protected readonly IEventAggregator _eventAggregator;
        protected readonly IScheduler _scheduler;
        private readonly ILogService _logService;
        private readonly ISerialMessagingService _serialMessagingService;

        public AdapterServiceFactory(IEventAggregator eventAggregator, IScheduler scheduler, II2CBusService i2CBusService, 
            ILogService logService, ISerialMessagingService serialMessagingService)
        {
            _i2CBusService = i2CBusService ?? throw new ArgumentNullException(nameof(i2CBusService));
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            _serialMessagingService = serialMessagingService ?? throw new ArgumentNullException(nameof(serialMessagingService));
        }

        public ILogService GetLogger() => _logService;
        public II2CBusService GetI2CService() => _i2CBusService;
        public IEventAggregator GetEventAggregator() => _eventAggregator;
        public IScheduler GetScheduler() => _scheduler;
        public ISerialMessagingService GetUartService() => _serialMessagingService;
    }
}