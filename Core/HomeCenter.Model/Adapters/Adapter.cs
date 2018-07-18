using Quartz;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HomeCenter.ComponentModel.Components;
using HomeCenter.ComponentModel.Events;
using HomeCenter.ComponentModel.ValueTypes;
using HomeCenter.Core.EventAggregator;
using HomeCenter.Core.Services.Logging;
using HomeCenter.Model.Extensions;

namespace HomeCenter.ComponentModel.Adapters
{
    public abstract class Adapter : Actor
    {
        protected readonly IEventAggregator _eventAggregator;
        protected readonly ISchedulerFactory _schedulerFactory;
        protected readonly ILogger _logger;
        protected readonly List<string> _requierdProperties = new List<string>();

        public IList<string> RequierdProperties() => _requierdProperties;

        protected Adapter(IAdapterServiceFactory adapterServiceFactory)
        {
            _eventAggregator = adapterServiceFactory.GetEventAggregator();
            _schedulerFactory = adapterServiceFactory.GetSchedulerFactory();
            _logger = adapterServiceFactory.GetLogger().CreatePublisher($"Adapter_{Uid}_Logger");
        }

        protected async Task<T> UpdateState<T>(string stateName, T oldValue, T newValue) where T : IValue
        {
            if (newValue.Equals(oldValue)) return oldValue;
            await _eventAggregator.PublishDeviceEvent(new PropertyChangedEvent(Uid, stateName, oldValue, newValue), _requierdProperties).ConfigureAwait(false);
            return newValue;
        }

        protected async Task ScheduleDeviceRefresh<T>(TimeSpan interval) where T : IJob
        {
            var scheduler = await _schedulerFactory.GetScheduler().ConfigureAwait(false);
            await scheduler.ScheduleInterval<T, Adapter>(interval, this, Uid, _disposables.Token).ConfigureAwait(false);
        }

        protected override void LogException(Exception ex) => _logger.Error(ex, $"Unhanded adapter {Uid} exception");
    }
}