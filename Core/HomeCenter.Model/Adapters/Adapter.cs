using HomeCenter.Model.Actors;
using HomeCenter.Model.Exceptions;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.Messages.Events.Device;
using Proto;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeCenter.Model.Adapters
{
    public abstract class Adapter : DeviceActor
    {
        protected readonly List<string> _requierdProperties = new List<string>();

        protected IList<string> RequierdProperties() => _requierdProperties;

        protected async Task<T> UpdateState<T>(string stateName, T oldValue, T newValue, IDictionary<string, string> additionalProperties = null)
        {
            if (newValue == null || EqualityComparer<T>.Default.Equals(oldValue, newValue)) return oldValue;

            if(_requierdProperties.Count > 0)
            {
                if( additionalProperties == null || additionalProperties.Count != _requierdProperties.Count || !_requierdProperties.SequenceEqual(additionalProperties.Keys))
                {
                    throw new MissingPropertyException($"Update state on component {Uid} should be invoked with required properties: {string.Join(",", _requierdProperties)}");
                }
            }

            await MessageBroker.PublisEvent(new PropertyChangedEvent(Uid, stateName, oldValue?.ToString(), newValue.ToString(), additionalProperties), _requierdProperties).ConfigureAwait(false);
            return newValue;
        }

        protected Task ScheduleDeviceRefresh<T>(TimeSpan interval) where T : IJob => Scheduler.ScheduleInterval<T, PID>(interval, Self, Uid, _disposables.Token);

        protected Task DelayDeviceRefresh<T>(TimeSpan interval) where T : IJob => Scheduler.DelayExecution<T, PID>(interval, Self, Uid, _disposables.Token);
    }
}