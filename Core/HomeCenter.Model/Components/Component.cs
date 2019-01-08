using CSharpFunctionalExtensions;
using HomeCenter.Broker;
using HomeCenter.CodeGeneration;
using HomeCenter.Model.Actors;
using HomeCenter.Model.Core;
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
        private ComponentState _componentState;

        [Map] private IList<AdapterReference> _adapters { get; set; } = new List<AdapterReference>();
        [Map] private IList<Trigger> _triggers { get; set; } = new List<Trigger>();
        [Map] private Dictionary<string, IValueConverter> _converters { get; set; } = new Dictionary<string, IValueConverter>();

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            if (!IsEnabled) return;

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
                Subscribe<Event>(GetRoutingFilterFromProperties(trigger.Event));
            }
        }

        private RoutingFilter GetRoutingFilterFromProperties(Event ev)
        {
            var attributes = ev.GetProperties().ToDictionary();
            AddRequiersProperties(ev, attributes);

            return new RoutingFilter(attributes);
        }

        /// <summary>
        /// When trigger is attached to event from internal adapter we add requierd properties to routing filter if they are missing
        /// </summary>
        /// <param name="ev"></param>
        /// <param name="attributes"></param>
        private void AddRequiersProperties(Event ev, Dictionary<string, string> attributes)
        {
            if (ev.ContainsProperty(MessageProperties.MessageSource))
            {
                var adapter = _adapters.FirstOrDefault(a => a.Uid == ev.MessageSource);
                if (adapter != null)
                {
                    foreach (var property in adapter.RequierdProperties)
                    {
                        if (!attributes.ContainsKey(property))
                        {
                            attributes.Add(property, adapter.AsString(property));
                            ev.SetProperty(property, adapter.AsString(property));
                        }
                    }
                }
            }
        }

        private async Task InitScheduledTriggers()
        {
            foreach (var trigger in _triggers.Where(x => x.Schedule != null))
            {
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
            var discoveryRequests = (await _adapters.WhenAll(adapter => MessageBroker.Request<DiscoverQuery, DiscoveryResponse>(DiscoverQuery.CreateQuery(adapter), adapter.Uid)).ConfigureAwait(false));

            foreach (var discovery in discoveryRequests)
            {
                discovery.Input.AddRequierdProperties(discovery.Result.RequierdProperties);

                Subscribe<Event>(discovery.Input.GetRoutingFilter());
            }

            _componentState = new ComponentState(discoveryRequests.ToDictionary(k => k.Input, v => v.Result));
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
                foreach (var adapter in _componentState.GetCommandAdapter(command))
                {
                    var adapterCommand = _adapters.Single(a => a.Uid == adapter).GetDeviceCommand(command);
                    MessageBroker.Send(adapterCommand, adapter);
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
            if (_componentState.IsStateProvidingAdapter(propertyChanged.MessageSource, propertyChanged.PropertyChangedName)
                && _componentState.TryUpdateState(propertyChanged.PropertyChangedName, propertyChanged.NewValue, out var oldValue))
            {
                await MessageBroker.PublishEvent(PropertyChangedEvent.Create(Uid, propertyChanged.PropertyChangedName, oldValue, propertyChanged.NewValue), Uid).ConfigureAwait(false);
            }
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

        protected IReadOnlyCollection<string> Handle(CapabilitiesQuery command) => _componentState.SupportedCapabilities();

        protected IReadOnlyCollection<string> Handle(SupportedStatesQuery command) => _componentState.SupportedStates();

        protected IReadOnlyDictionary<string, string> Handle(StateQuery command)
        {
            if (command.ContainsProperty(MessageProperties.StateName))
            {
                return _componentState.GetStateValues(command[MessageProperties.StateName]);
            }

            return _componentState.GetStateValues();
        }
    }
}