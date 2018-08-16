using HomeCenter.Messaging;
using HomeCenter.Core.Services;
using HomeCenter.Core.Services.I2C;
using Microsoft.Extensions.Logging;
using Quartz;
using System;

namespace HomeCenter.ComponentModel.Adapters
{
    public class AdapterServiceFactory : IAdapterServiceFactory
    {
        protected readonly II2CBusService _i2CBusService;
        protected readonly IEventAggregator _eventAggregator;
        protected readonly IScheduler _scheduler;
        private readonly ISerialMessagingService _serialMessagingService;
        private readonly ILoggerFactory _loggerFactory;

        public AdapterServiceFactory(IEventAggregator eventAggregator, IScheduler scheduler, II2CBusService i2CBusService,
           ILoggerFactory loggerFactory, ISerialMessagingService serialMessagingService)
        {
            _i2CBusService = i2CBusService ?? throw new ArgumentNullException(nameof(i2CBusService));
            _eventAggregator = eventAggregator ?? throw new ArgumentNullException(nameof(eventAggregator));
            _scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            _serialMessagingService = serialMessagingService ?? throw new ArgumentNullException(nameof(serialMessagingService));
            _loggerFactory = loggerFactory;
        }

        public ILogger<T> GetLogger<T>() where T : class => _loggerFactory.CreateLogger<T>();

        public II2CBusService GetI2CService() => _i2CBusService;

        public IEventAggregator GetEventAggregator() => _eventAggregator;

        public IScheduler GetScheduler() => _scheduler;

        public ISerialMessagingService GetUartService() => _serialMessagingService;
    }
}