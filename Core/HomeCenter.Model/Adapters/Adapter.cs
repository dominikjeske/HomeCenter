using HomeCenter.ComponentModel.Components;
using HomeCenter.ComponentModel.Events;
using HomeCenter.ComponentModel.ValueTypes;
using HomeCenter.Messaging;
using HomeCenter.Model.Extensions;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeCenter.ComponentModel.Adapters
{
    public abstract class Adapter : Actor
    {
        protected readonly IEventAggregator _eventAggregator;
        protected readonly IScheduler _scheduler;
        protected readonly ILogger<Adapter> _logger;
        protected readonly List<string> _requierdProperties = new List<string>();

        public IList<string> RequierdProperties() => _requierdProperties;

        protected Adapter(IAdapterServiceFactory adapterServiceFactory)
        {
            _eventAggregator = adapterServiceFactory.GetEventAggregator();
            _scheduler = adapterServiceFactory.GetScheduler();
            _logger = adapterServiceFactory.GetLogger<Adapter>();
        }

        protected async Task<T> UpdateState<T>(string stateName, T oldValue, T newValue) where T : IValue
        {
            if (newValue.Equals(oldValue)) return oldValue;
            await _eventAggregator.PublishDeviceEvent(new PropertyChangedEvent(Uid, stateName, oldValue, newValue), _requierdProperties).ConfigureAwait(false);
            return newValue;
        }

        protected Task ScheduleDeviceRefresh<T>(TimeSpan interval) where T : IJob => _scheduler.ScheduleInterval<T, Adapter>(interval, this, Uid, _disposables.Token);
    }
}