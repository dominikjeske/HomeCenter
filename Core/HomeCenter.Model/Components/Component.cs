using CSharpFunctionalExtensions;
using HomeCenter.ComponentModel.Adapters;
using HomeCenter.ComponentModel.Capabilities;
using HomeCenter.ComponentModel.Capabilities.Constants;
using HomeCenter.ComponentModel.Commands;
using HomeCenter.ComponentModel.Commands.Responses;
using HomeCenter.ComponentModel.Events;
using HomeCenter.ComponentModel.ValueTypes;
using HomeCenter.Contracts.Exceptions;
using HomeCenter.Core.Extensions;
using HomeCenter.Core.Services.DependencyInjection;
using HomeCenter.Messaging;
using HomeCenter.Model.Exceptions;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.Queries.Specialized;
using Quartz;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeCenter.ComponentModel.Components
{
    public class Component : Actor
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IScheduler _scheduler;
        private List<string> _tagCache = new List<string>();

        private Dictionary<string, State> _capabilities { get; } = new Dictionary<string, State>();
        private Dictionary<string, AdapterReference> _adapterStateMap { get; } = new Dictionary<string, AdapterReference>();
        private Dictionary<string, AdapterReference> _eventSources { get; } = new Dictionary<string, AdapterReference>();
        [Map] private IList<AdapterReference> _adapters { get; set; } = new List<AdapterReference>();
        [Map] private IList<Trigger> _triggers { get; set; } = new List<Trigger>();
        [Map] private Dictionary<string, IValueConverter> _converters { get; set; } = new Dictionary<string, IValueConverter>();

        public Component(IEventAggregator eventAggregator, IScheduler scheduler)
        {
            _eventAggregator = eventAggregator;
            _scheduler = scheduler;
        }

        public IReadOnlyList<string> AdapterReferences => _adapters.Select(a => a.Uid).ToList().AsReadOnly();

        public override async Task Initialize()
        {
            if (!IsEnabled) return;

            await InitializeAdapters().ConfigureAwait(false);
            await InitializeTriggers().ConfigureAwait(false);
            SubscribeForRemoteCommands();

            await base.Initialize().ConfigureAwait(false);
        }

        public void InitializeAdapter(Adapter adapter)
        {
            var reference = _adapters.FirstOrDefault(a => a.Uid == adapter.Uid);
            if (reference == null) throw new KeyNotFoundException($"Adapter {adapter.Uid} was not found in component {Uid}");
            reference.InitializeAdapter(adapter);
        }

        private Task InitializeTriggers()
        {
            InitEventTriggers();
            return InitScheduledTriggers();
        }

        private async Task InitScheduledTriggers()
        {
            foreach (var trigger in _triggers.Where(x => x.Schedule != null))
            {
                trigger.Commands.ForEach(c => c[CommandProperties.CommandSource] = (StringValue)Uid);

                if (!string.IsNullOrWhiteSpace(trigger.Schedule.CronExpression))
                {
                    //TODO DNF
                    //await scheduler.ScheduleCron<TriggerJob, TriggerJobDataDTO>(trigger.ToJobDataWithFinish(this), trigger.Schedule.CronExpression, Uid, _disposables.Token, trigger.Schedule.Calendar).ConfigureAwait(false);
                }
                else if (trigger.Schedule.ManualSchedules.Count > 0)
                {
                    foreach (var manualTrigger in trigger.Schedule.ManualSchedules)
                    {
                        //TODO DNF
                        //await scheduler.ScheduleDailyTimeInterval<TriggerJob, TriggerJobDataDTO>(trigger.ToJobData(this), manualTrigger.Start, Uid, _disposables.Token, trigger.Schedule.Calendar).ConfigureAwait(false);
                        //await scheduler.ScheduleDailyTimeInterval<TriggerJob, TriggerJobDataDTO>(trigger.ToJobData(this), manualTrigger.Finish, Uid, _disposables.Token, trigger.Schedule.Calendar).ConfigureAwait(false);
                    }
                }
            }
        }

        private void InitEventTriggers()
        {
            foreach (var trigger in _triggers.Where(x => x.Schedule == null))
            {
                trigger.Commands.ForEach(c => c[CommandProperties.CommandSource] = (StringValue)Uid);
                var subscription = _eventAggregator.SubscribeForDeviceEvent(DeviceTriggerHandler, trigger.Event.GetPropertiesStrings(), trigger.Event.Type);
                _disposables.Add(subscription);
            }
        }

        private async Task DeviceTriggerHandler(IMessageEnvelope<Event> deviceEvent)
        {
            var trigger = _triggers.FirstOrDefault(t => t.Event.Equals(deviceEvent.Message));
            if (await (trigger.ValidateCondition()).ConfigureAwait(false))
            {
                foreach (var command in trigger.Commands)
                {
                    if (command.ContainsProperty(CommandProperties.ExecutionDelay))
                    {
                        var cancelPrevious = command.GetPropertyValue(CommandProperties.CancelPrevious).AsBool(false);
                        await _scheduler.DelayExecution<DelayCommandJob>(command[CommandProperties.ExecutionDelay].AsTimeSpan(), command, $"{Uid}_{command.Type}", cancelPrevious).ConfigureAwait(false);
                        continue;
                    }

                    await ExecuteCommand(command).ConfigureAwait(false);
                }
            }
        }

        private async Task InitializeAdapters()
        {
            foreach (var adapterRef in _adapters)
            {
                var capabilities = await adapterRef.Adapter.ExecuteQuery<DiscoveryResponse>(DiscoverQuery.Default).ConfigureAwait(false);
                if (capabilities == null) throw new DiscoveryException($"Failed to initialize adapter {adapterRef.Uid} in component {Uid}. There is no response from DiscoveryResponse command");

                MapCapabilitiesToAdapters(adapterRef, capabilities.SupportedStates);
                BuildCapabilityStates(capabilities);
                MapEventSourcesToAdapters(adapterRef, capabilities.EventSources);
                SubscribeToAdapterEvents(adapterRef, capabilities.RequierdProperties);
            }
        }

        private void BuildCapabilityStates(DiscoveryResponse capabilities)
        {
            _capabilities.AddRangeNewOnly(capabilities.SupportedStates.ToDictionary(key => ((StringValue)key[StateProperties.StateName]).ToString(), val => val));
        }

        private void MapCapabilitiesToAdapters(AdapterReference adapter, State[] states)
        {
            states.ForEach(state => _adapterStateMap[state[StateProperties.StateName].AsString()] = adapter);
        }

        private void MapEventSourcesToAdapters(AdapterReference adapter, IList<EventSource> eventSources)
        {
            eventSources.ForEach(es => _eventSources[es[EventProperties.EventType].AsString()] = adapter);
        }

        private void SubscribeToAdapterEvents(AdapterReference adapter, IList<string> requierdProperties)
        {
            _disposables.Add(_eventAggregator.SubscribeForDeviceEvent(DeviceEventHandler, GetAdapterRouterAttributes(adapter, requierdProperties)));
        }

        private void SubscribeForRemoteCommands()
        {
            _disposables.Add(_eventAggregator.SubscribeForDeviceCommnd((IMessageEnvelope<Command> deviceCommand) => ExecuteCommand(deviceCommand.Message), Uid));
        }

        private Dictionary<string, string> GetAdapterRouterAttributes(AdapterReference adapter, IList<string> requierdProperties)
        {
            var routerAttributes = new Dictionary<string, string>();
            foreach (var adapterProperty in requierdProperties)
            {
                if (!adapter.ContainsProperty(adapterProperty)) throw new ConfigurationException($"Adapter {adapter.Uid} in component {Uid} missing configuration property {adapterProperty}");
                routerAttributes.Add(adapterProperty, adapter[adapterProperty].ToString());
            }
            routerAttributes.Add(EventProperties.SourceDeviceUid, adapter.Uid);

            return routerAttributes;
        }

        /// <summary>
        /// All command not handled by the component directly are routed to adapters
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        protected override async Task<object> UnhandledCommand(Command command)
        {
            bool handled = false;
            // TODO use value converter before publish and maybe queue?
            foreach (var state in _capabilities.Values.Where(capability => capability.IsCommandSupported(command)))
            {
                var adapter = _adapterStateMap[state[StateProperties.StateName].ToString()];
                await _eventAggregator.PublishDeviceCommnd(adapter.GetDeviceCommand(command)).ConfigureAwait(false);

                handled = true;
            }

            if (!handled)
            {
                return base.UnhandledCommand(command);
            }
            else
            {
                return VoidResult.Void;
            }
        }

        private async Task DeviceEventHandler(IMessageEnvelope<Event> deviceEvent)
        {
            var propertyName = (StringValue)deviceEvent.Message[StateProperties.StateName];
            if (!_capabilities.ContainsKey(propertyName)) return;

            var state = _capabilities[propertyName];
            var oldValue = state[StateProperties.Value];
            var newValue = deviceEvent.Message[EventProperties.NewValue];

            if (newValue.Equals(oldValue)) return;

            state[StateProperties.Value] = newValue;

            await _eventAggregator.PublishDeviceEvent(new PropertyChangedEvent(Uid, propertyName, oldValue, newValue)).ConfigureAwait(false);
        }

        protected IReadOnlyCollection<string> Capabilities(CapabilitiesQuery command) => _capabilities.Values
                                                                                                .Select(cap => cap.GetPropertyValue(StateProperties.StateName))
                                                                                                .Where(cap => cap.HasValue)
                                                                                                .Cast<StringValue>()
                                                                                                .Select(cap => cap.Value)
                                                                                                .Distinct()
                                                                                                .ToList()
                                                                                                .AsReadOnly();

        protected IReadOnlyCollection<string> SupportedStates(SupportedStatesQuery command) => _capabilities.Values
                                                                                   .Select(cap => cap.GetPropertyValue(StateProperties.StateName))
                                                                                   .Where(cap => cap.HasValue)
                                                                                   .Cast<StringValue>()
                                                                                   .Select(cap => cap.Value)
                                                                                   .Distinct()
                                                                                   .ToList()
                                                                                   .AsReadOnly();

        protected IReadOnlyCollection<string> SupportedTags(TagsQuery command)
        {
            if (_tagCache == null)
            {
                _tagCache = new List<string>(Tags);
                _tagCache.AddRange(_capabilities.Values.SelectMany(x => x.Tags));
            }
            return _tagCache.AsReadOnly();
        }

        protected Maybe<IValue> GetStateCommandHandler(StateQuery command)
        {
            var stateName = command[CommandProperties.StateName].AsString();

            if (!_capabilities.ContainsKey(stateName)) return Maybe<IValue>.None;
            var value = _capabilities[stateName][StateProperties.Value];
            if (_converters.ContainsKey(stateName))
            {
                value = _converters[stateName].Convert(value);
            }

            return Maybe<IValue>.From(value);
        }
    }
}