using HomeCenter.CodeGeneration;
using HomeCenter.Model.Triggers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeCenter.Abstractions;
using HomeCenter.Abstractions.Defaults;
using HomeCenter.Actors.Core;
using HomeCenter.EventAggregator;
using HomeCenter.Extensions;
using HomeCenter.Messages.Events.Device;
using HomeCenter.Messages.Events.Service;
using HomeCenter.Messages.Queries.Device;

namespace HomeCenter.Model.Components
{
    [ProxyCodeGenerator]
    public class Component : DeviceActor
    {
        public Component(IList<AdapterReference> adapterReferences, IList<Translator> translators, IList<Trigger> triggers)
        {
            AdapterReferences = adapterReferences;
            Translators = translators;
            Triggers = triggers;
        }

        private ComponentState _componentState;
        public IList<AdapterReference> AdapterReferences { get; }
        public IList<Translator> Translators { get; }
        public IList<Trigger> Triggers { get; }

        /// <summary>
        /// Decides if we want to re send event from adapters if there is no translator attached
        /// </summary>
        protected bool RelayNotTranslatedEvents => this.AsBool(MessageProperties.RelayNotTranslatedEvents, false);

        protected async override Task OnSystemStarted(SystemStartedEvent systemStartedEvent)
        {
            //TODO Is this proper?
            if (!IsEnabled) return;

            await base.OnSystemStarted(systemStartedEvent);

            await InitializeAdapters();
            await InitializeTriggers();
            SubscribeForRemoteCommands();

            await MessageBroker.Publish(ComponentStartedEvent.Create(Uid), Uid);
        }

        private Task InitializeTriggers()
        {
            InitEventTriggers();
            return InitScheduledTriggers();
        }

        private void InitEventTriggers()
        {
            foreach (var trigger in Triggers.Where(x => x.Schedule == null))
            {
                Subscribe<Event>(false, GetRoutingFilterFromProperties(trigger.Event));
            }
        }

        private RoutingFilter GetRoutingFilterFromProperties(Event ev)
        {
            var attributes = ev.GetProperties().ToDictionary();
            AddRequiersProperties(ev, attributes);

            return new RoutingFilter(attributes);
        }

        /// <summary>
        /// When trigger is attached to event from internal adapter we add required properties to routing filter if they are missing
        /// </summary>
        /// <param name="ev"></param>
        /// <param name="attributes"></param>
        private void AddRequiersProperties(Event ev, Dictionary<string, object> attributes)
        {
            if (ev.ContainsProperty(MessageProperties.MessageSource))
            {
                var adapter = AdapterReferences.FirstOrDefault(a => a.Uid == ev.MessageSource);
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
            foreach (var trigger in Triggers.Where(x => x.Schedule != null))
            {
                if (!string.IsNullOrWhiteSpace(trigger.Schedule.CronExpression))
                {
                    await MessageBroker.SendWithCronRepeat(trigger.ToActorContextWithFinish(Self), trigger.Schedule.CronExpression, _disposables.Token, trigger.Schedule.Calendar);
                }
                else if (trigger.Schedule.ManualSchedules.Count > 0)
                {
                    foreach (var manualTrigger in trigger.Schedule.ManualSchedules)
                    {
                        await MessageBroker.SendDailyAt(trigger.ToActorContext(Self), manualTrigger.Start, _disposables.Token, trigger.Schedule.Calendar);
                        await MessageBroker.SendDailyAt(trigger.ToActorContext(Self), manualTrigger.Finish, _disposables.Token, trigger.Schedule.Calendar);
                    }
                }
            }
        }

        private async Task InitializeAdapters()
        {
            var discoveryResponses = (await AdapterReferences.WhenAll(adapter => MessageBroker.Request<DiscoverQuery, DiscoveryResponse>(DiscoverQuery.CreateQuery(adapter), adapter.Uid)));

            foreach (var response in discoveryResponses)
            {
                response.Input.AddRequierdProperties(response.Result.RequierdProperties);

                Subscribe<Event>(false, response.Input.GetRoutingFilter());
            }

            _componentState = new ComponentState(discoveryResponses.ToDictionary(k => k.Input, v => v.Result));
        }

        /// <summary>
        /// Subscribe for commands addressed to this component via event aggregate
        /// </summary>
        private void SubscribeForRemoteCommands() => Subscribe<Command>(false, new RoutingFilter(Uid));

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
                    var adapterCommand = AdapterReferences.Single(a => a.Uid == adapter).GetDeviceCommand(command);

                    var translator = Translators.FirstOrDefault(e => e.Type == MessageType.Command && e.From.Equals(adapterCommand));

                    if (translator != null)
                    {
                        MessageBroker.SendWithTranslate(adapterCommand, translator.To, adapter);
                    }
                    else
                    {
                        MessageBroker.Send(adapterCommand, adapter);
                    }

                    handled = true;
                }
            }

            if (!handled)
            {
                await base.UnhandledMessage(actorMessage);
            }
        }

        protected async Task Handle(Event ev)
        {
            var trigger = Triggers.Where(e => e.Event != null).FirstOrDefault(t => t.Event.Equals(ev));
            if (trigger != null)
            {
                await HandleEventInTrigger(trigger);
            }

            if (ev is PropertyChangedEvent propertyChanged)
            {
                await HandlePropertyChange(propertyChanged);
            }
        }

        private async Task HandlePropertyChange(PropertyChangedEvent propertyChanged)
        {
            if (_componentState.IsStateProvidingAdapter(propertyChanged.MessageSource, propertyChanged.PropertyChangedName)
                && _componentState.TryUpdateState(propertyChanged.PropertyChangedName, propertyChanged.NewValue, out var oldValue))
            {
                var translator = Translators.FirstOrDefault(e => e.Type == MessageType.Event && e.From.Equals(propertyChanged));

                if (translator != null)
                {
                    var eventPublished = await MessageBroker.PublishWithTranslate(propertyChanged, translator.To, Uid);

                    Log(ActorEventType.EventPublished, "Event published '{eventPublished}'", eventPublished);
                }
                else if (RelayNotTranslatedEvents)
                {
                    var eventPublished = PropertyChangedEvent.Create(Uid, propertyChanged.PropertyChangedName, oldValue, propertyChanged.NewValue);
                    await MessageBroker.Publish(eventPublished, Uid);

                    Log(ActorEventType.EventPublished, "Event published '{eventPublished}'", eventPublished);
                }
            }
        }

        private async Task HandleEventInTrigger(Trigger trigger)
        {
            if (await trigger.ValidateCondition())
            {
                foreach (var command in trigger.Commands)
                {
                    if (command.ContainsProperty(MessageProperties.ExecutionDelay))
                    {
                        var executionDelay = command.AsTime(MessageProperties.ExecutionDelay);
                        var cancelPrevious = command.AsBool(MessageProperties.CancelPrevious, false);

                        await MessageBroker.SendAfterDelay(ActorMessageContext.Create(Self, command), executionDelay, cancelPrevious);
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
                //TODO DNF chceck this
                return _componentState.GetStateValues(command[MessageProperties.StateName].ToString());
            }

            return _componentState.GetStateValues();
        }
    }
}