using CSharpFunctionalExtensions;
using HomeCenter.Broker;
using HomeCenter.CodeGeneration;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Capabilities.Constants;
using HomeCenter.Model.Core;
using HomeCenter.Model.Exceptions;
using HomeCenter.Model.Extensions;
using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Events;
using HomeCenter.Model.Messages.Events.Device;
using HomeCenter.Model.Messages.Queries.Device;
using HomeCenter.Model.Triggers;
using HomeCenter.Model.ValueTypes;
using HomeCenter.Utils.Extensions;
using Proto;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeCenter.Model.Components
{
    [ProxyCodeGenerator]
    public class Component : DeviceActor
    {
        private List<string> _tagCache = new List<string>();

        private Dictionary<string, State> _capabilities { get; } = new Dictionary<string, State>();
        private Dictionary<string, AdapterReference> _adapterStateMap { get; } = new Dictionary<string, AdapterReference>();
        private Dictionary<string, AdapterReference> _eventSources { get; } = new Dictionary<string, AdapterReference>();
        [Map] private IList<AdapterReference> _adapters { get; set; } = new List<AdapterReference>();
        [Map] private IList<Trigger> _triggers { get; set; } = new List<Trigger>();
        [Map] private Dictionary<string, IValueConverter> _converters { get; set; } = new Dictionary<string, IValueConverter>();

        protected override async Task OnStarted(IContext context)
        {
            if (!IsEnabled) return;

            await base.OnStarted(context).ConfigureAwait(false);

            await InitializeAdapters().ConfigureAwait(false);
            await InitializeTriggers().ConfigureAwait(false);
            SubscribeForRemoteCommands();
        }

        private Task InitializeTriggers()
        {
            InitEventTriggers();
            return InitScheduledTriggers();
        }

        private void InitEventTriggers()
        {
            foreach (var trigger in _triggers.Where(x => x.Schedule == null))
            {
                trigger.Commands.ForEach(c => c[MessageProperties.MessageSource] = (StringValue)Uid);
                
                //TODO
                //Subscribe<Event>(trigger.Event.GetRoutingFilter());
            }
        }

        private async Task InitScheduledTriggers()
        {
            foreach (var trigger in _triggers.Where(x => x.Schedule != null))
            {
                trigger.Commands.ForEach(c => c[MessageProperties.MessageSource] = (StringValue)Uid);

                if (!string.IsNullOrWhiteSpace(trigger.Schedule.CronExpression))
                {
                    await Scheduler.ScheduleCron<TriggerJob, TriggerJobDataDTO>(trigger.ToJobDataWithFinish(Self), trigger.Schedule.CronExpression, Uid, _disposables.Token, trigger.Schedule.Calendar).ConfigureAwait(false);
                }
                else if (trigger.Schedule.ManualSchedules.Count > 0)
                {
                    foreach (var manualTrigger in trigger.Schedule.ManualSchedules)
                    {
                        await Scheduler.ScheduleDailyTimeInterval<TriggerJob, TriggerJobDataDTO>(trigger.ToJobData(Self), manualTrigger.Start, Uid, _disposables.Token, trigger.Schedule.Calendar).ConfigureAwait(false);
                        await Scheduler.ScheduleDailyTimeInterval<TriggerJob, TriggerJobDataDTO>(trigger.ToJobData(Self), manualTrigger.Finish, Uid, _disposables.Token, trigger.Schedule.Calendar).ConfigureAwait(false);
                    }
                }
            }
        }

        private async Task InitializeAdapters()
        {
            foreach (var adapterRef in _adapters)
            {
                var capabilities = await MessageBroker.Request<DiscoverQuery, DiscoveryResponse>(DiscoverQuery.Default, adapterRef.Uid).ConfigureAwait(false);
                if (capabilities == null) throw new DiscoveryException($"Failed to initialize adapter {adapterRef.Uid} in component {Uid}. There is no response from DiscoveryResponse command");

                MapCapabilitiesToAdapters(adapterRef, capabilities.SupportedStates);
                BuildCapabilityStates(capabilities);
                MapEventSourcesToAdapters(adapterRef, capabilities.EventSources);
                SubscribeToAdapterEvents(adapterRef, capabilities.RequierdProperties);
            }
        }

        private void MapCapabilitiesToAdapters(AdapterReference adapter, State[] states)
        {
            states.ForEach(state => _adapterStateMap[state.Name] = adapter);
        }

        private void BuildCapabilityStates(DiscoveryResponse capabilities)
        {
            _capabilities.AddRangeNewOnly(capabilities.SupportedStates.ToDictionary(key => key.Name, val => val));
        }

        private void MapEventSourcesToAdapters(AdapterReference adapter, IList<EventSource> eventSources)
        {
            eventSources.ForEach(es => _eventSources[es[EventProperties.EventType].AsString()] = adapter);
        }

        /// <summary>
        /// Subscribe for event generated by child adapters
        /// </summary>
        private void SubscribeToAdapterEvents(AdapterReference adapter, IList<string> requierdProperties)
        {
            var routingFilter = adapter.GetRoutingFilter(requierdProperties);

            Subscribe<Event>(routingFilter);
        }

        /// <summary>
        /// Subscribe for commands addressed to this component via event aggregator
        /// </summary>
        private void SubscribeForRemoteCommands() => Subscribe<Command>(new RoutingFilter(Uid));

        
        /// <summary>
        /// Every message that is not directly should be check for compatibility with connected adapters
        /// </summary>
        protected override async Task UnhandledMessage(IContext context)
        {
            var message = context.Message as ActorMessage;

            bool handled = false;

            if (message is Command command)
            {
                // TODO use value converter before publish and maybe queue?
                var supportedCapabilities = _capabilities.Values.Where(capability => capability.IsCommandSupported(message));
                foreach (var state in supportedCapabilities)
                {
                    var adapter = _adapterStateMap[state.Name];
                    var adapterCommand = adapter.GetDeviceCommand(command);
                    MessageBroker.Send(adapterCommand, adapter.Uid);
                    handled = true;
                }
            }

            if (!handled)
            {
                await base.UnhandledMessage(context).ConfigureAwait(false);
            }
        }

        protected async Task Handle(Event ev)
        {
            var trigger = _triggers.FirstOrDefault(t => t.Event.Equals(ev));
            if (trigger != null && await trigger.ValidateCondition().ConfigureAwait(false))
            {
                foreach (var command in trigger.Commands)
                {
                    if (command.ContainsProperty(CommandProperties.ExecutionDelay))
                    {
                        var cancelPrevious = command.GetPropertyValue(CommandProperties.CancelPrevious).AsBool(false);
                        await Scheduler.DelayExecution<DelayCommandJob>(command[CommandProperties.ExecutionDelay].AsTimeSpan(), command, $"{Uid}_{command.Type}", cancelPrevious).ConfigureAwait(false);
                        continue;
                    }

                    MessageBroker.Send(command, Self);
                }
            }

            //TODO distinct trigger and property change from adapter
            if (ev is PropertyChangedEvent propertyChanged)
            {
                var propertyName = propertyChanged.PropertyChangedName;
                if (!_capabilities.ContainsKey(propertyName)) return;

                var state = _capabilities[propertyName];
                var oldValue = state.Value;
                var newValue = propertyChanged.NewValue;

                if (newValue.Equals(oldValue)) return;

                state.Value = newValue;

                await MessageBroker.PublisEvent(new PropertyChangedEvent(Uid, propertyName, oldValue, newValue)).ConfigureAwait(false);
            }
        }

        protected IReadOnlyCollection<string> Handle(CapabilitiesQuery command) => _capabilities.Values
                                                                                                .Select(cap => cap.CapabilityName)
                                                                                                .Distinct()
                                                                                                .ToList()
                                                                                                .AsReadOnly();

        protected IReadOnlyCollection<string> Handle(SupportedStatesQuery command) => _capabilities.Values
                                                                                   .Select(cap => cap.Name)
                                                                                   .Distinct()
                                                                                   .ToList()
                                                                                   .AsReadOnly();

        protected IReadOnlyCollection<string> Handle(TagsQuery command)
        {
            if (_tagCache == null)
            {
                _tagCache = new List<string>(Tags);
                _tagCache.AddRange(_capabilities.Values.SelectMany(x => x.Tags));
            }
            return _tagCache.AsReadOnly();
        }

        protected Maybe<IValue> Handle(StateQuery command)
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