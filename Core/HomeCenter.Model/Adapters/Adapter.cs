using HomeCenter.Model.Core;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.Messages.Events.Device;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeCenter.Model.Adapters
{
    public abstract class Adapter : DeviceActor
    {
        protected readonly IScheduler _scheduler;
        protected readonly ILogger<Adapter> _logger;
        protected readonly List<string> _requierdProperties = new List<string>();

        public IList<string> RequierdProperties() => _requierdProperties;

        protected Adapter(IAdapterServiceFactory adapterServiceFactory) : base(adapterServiceFactory.GetEventAggregator())
        {
            _scheduler = adapterServiceFactory.GetScheduler();
            _logger = adapterServiceFactory.GetLogger<Adapter>();
        }

        protected async Task<T> UpdateState<T>(string stateName, T oldValue, T newValue) where T : IValue
        {
            if (newValue.Equals(oldValue)) return oldValue;
            await PublisEvent(new PropertyChangedEvent(Uid, stateName, oldValue, newValue), _requierdProperties).ConfigureAwait(false);
            return newValue;
        }

        protected Task ScheduleDeviceRefresh<T>(TimeSpan interval) where T : IJob => _scheduler.ScheduleInterval<T, Proto.PID>(interval, Self, Uid, _disposables.Token);
    }
}