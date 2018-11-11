using HomeCenter.Model.Core;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.Messages.Events.Device;
using Proto;
using Quartz;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomeCenter.Model.Adapters
{
    public abstract class Adapter : DeviceActor
    {
        protected readonly List<string> _requierdProperties = new List<string>();

        protected IList<string> RequierdProperties() => _requierdProperties;

        protected async Task<T> UpdateState<T>(string stateName, T oldValue, T newValue)
        {
            if (newValue == null || EqualityComparer<T>.Default.Equals(oldValue, newValue)) return oldValue;

            await MessageBroker.PublisEvent(new PropertyChangedEvent(Uid, stateName, oldValue?.ToString(), newValue.ToString()), _requierdProperties).ConfigureAwait(false);
            return newValue;
        }

        protected Task ScheduleDeviceRefresh<T>(TimeSpan interval) where T : IJob => Scheduler.ScheduleInterval<T, PID>(interval, Self, Uid, _disposables.Token);

        protected Task DelayDeviceRefresh<T>(TimeSpan interval) where T : IJob => Scheduler.DelayExecution<T, PID>(interval, Self, Uid, _disposables.Token);
    }
}