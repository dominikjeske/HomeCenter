using CSharpFunctionalExtensions;
using HomeCenter.Broker;
using HomeCenter.CodeGeneration;
using HomeCenter.Model.Actors;
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
                trigger.Commands.ForEach(c => c[MessageProperties.MessageSource] = Uid);

                Subscribe<Event>(trigger.Event.GetRoutingFilter());
            }
        }

        private async Task InitScheduledTriggers()
        {
            foreach (var trigger in _triggers.Where(x => x.Schedule != null))
            {
                trigger.Commands.ForEach(c => c[MessageProperties.MessageSource] = Uid);

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
            var discoverQuery = DiscoverQuery.CreateQuery(this);
            foreach (var adapterRef in _adapters)
            {
                var capabilities = await MessageBroker.Request<DiscoverQuery, DiscoveryResponse>(discoverQuery, adapterRef.Uid).ConfigureAwait(false);
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
            eventSources.ForEach(es => _eventSources[es.AsString(MessageProperties.EventType)] = adapter);
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
        protected override async Task UnhandledMessage(object message)
        {
            var actorMessage = message as ActorMessage;

            bool handled = false;

            if (actorMessage is Command command)
            {
                // TODO use value converter before publish
                var supportedCapabilities = _capabilities.Values.Where(capability => capability.IsCommandSupported(command));
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
                await base.UnhandledMessage(actorMessage).ConfigureAwait(false);
            }
        }

        protected async Task Handle(Event ev)
        {
            var trigger = _triggers.Where(e => e.Event != null).FirstOrDefault(t => t.Event.Equals(ev));
            if (trigger != null)
            {
                await HandleEventInTrigger(trigger).ConfigureAwait(false);
            }

            if (ev is PropertyChangedEvent propertyChanged)
            {
                await HandlePropertyChange(propertyChanged).ConfigureAwait(false);
            }
        }

        private async Task HandlePropertyChange(PropertyChangedEvent propertyChanged)
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

        private async Task HandleEventInTrigger(Trigger trigger)
        {
            if (await trigger.ValidateCondition().ConfigureAwait(false))
            {
                foreach (var command in trigger.Commands)
                {
                    if (command.ContainsProperty(MessageProperties.ExecutionDelay))
                    {
                        var executionDelay = command.AsTime(MessageProperties.ExecutionDelay);
                        var cancelPrevious = command.AsBool(MessageProperties.CancelPrevious, false);
                        await Scheduler.DelayExecution<DelayCommandJob>(executionDelay, command, $"{Uid}_{command.Type}", cancelPrevious).ConfigureAwait(false);
                        continue;
                    }

                    MessageBroker.Send(command, Self);
                }
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

        protected string Handle(StateQuery command)
        {
            var stateName = command.AsString(MessageProperties.StateName);

            if (!_capabilities.ContainsKey(stateName)) return null;
            var value = _capabilities[stateName][StateProperties.Value];
            if (_converters.ContainsKey(stateName))
            {
                value = _converters[stateName].Convert(value);
            }

            return value;
        }

        protected IReadOnlyCollection<State> Handle(StatusQuery command) => _capabilities.Values.ToList().AsReadOnly();
    }
}