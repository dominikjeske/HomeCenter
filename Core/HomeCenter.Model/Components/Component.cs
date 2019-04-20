using CSharpFunctionalExtensions;
using HomeCenter.Broker;
using HomeCenter.CodeGeneration;
using HomeCenter.Model.Actors;
using HomeCenter.Model.Core;
using HomeCenter.Model.Messages;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Events;
using HomeCenter.Model.Messages.Events.Device;
using HomeCenter.Model.Messages.Events.Service;
using HomeCenter.Model.Messages.Queries.Device;
using HomeCenter.Model.Messages.Scheduler;
using HomeCenter.Model.Triggers;
using HomeCenter.Utils.Extensions;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HomeCenter.Model.Components
{
    [ProxyCodeGenerator]
    public class Component : DeviceActor
    {
        private ComponentState _componentState;

        [Map] private IList<AdapterReference> _adapterReferences { get; set; } = new List<AdapterReference>();
        [Map] private IList<Trigger> _triggers { get; set; } = new List<Trigger>();
        [Map] private IList<Translator> _translators { get; set; } = new List<Translator>();

        /// <summary>
        /// Decides if we want to re send event from adapters if there is no translator attached
        /// </summary>
        protected bool RelayNotTranslatedEvents => AsBool("RelayNotTranslatedEvents", false);

        protected async override Task OnSystemStarted(SystemStartedEvent systemStartedEvent)
        {
            if (!IsEnabled) return;

            await base.OnSystemStarted(systemStartedEvent);

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
        /// When trigger is attached to event from internal adapter we add required properties to routing filter if they are missing
        /// </summary>
        /// <param name="ev"></param>
        /// <param name="attributes"></param>
        private void AddRequiersProperties(Event ev, Dictionary<string, string> attributes)
        {
            if (ev.ContainsProperty(MessageProperties.MessageSource))
            {
                var adapter = _adapterReferences.FirstOrDefault(a => a.Uid == ev.MessageSource);
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
                    await MessageBroker.SendWithCronRepeat(trigger.ToActorContextWithFinish(Self), trigger.Schedule.CronExpression, _disposables.Token, trigger.Schedule.Calendar).ConfigureAwait(false);
                }
                else if (trigger.Schedule.ManualSchedules.Count > 0)
                {
                    foreach (var manualTrigger in trigger.Schedule.ManualSchedules)
                    {
                        await MessageBroker.SendDailyAt(trigger.ToActorContext(Self), manualTrigger.Start, _disposables.Token, trigger.Schedule.Calendar).ConfigureAwait(false);
                        await MessageBroker.SendDailyAt(trigger.ToActorContext(Self), manualTrigger.Finish, _disposables.Token, trigger.Schedule.Calendar).ConfigureAwait(false);
                    }
                }
            }
        }

        private async Task InitializeAdapters()
        {
            var discoveryResponses = (await _adapterReferences.WhenAll(adapter => MessageBroker.Request<DiscoverQuery, DiscoveryResponse>(DiscoverQuery.CreateQuery(adapter), adapter.Uid)).ConfigureAwait(false));

            foreach (var response in discoveryResponses)
            {
                response.Input.AddRequierdProperties(response.Result.RequierdProperties);

                Subscribe<Event>(response.Input.GetRoutingFilter());
            }

            _componentState = new ComponentState(discoveryResponses.ToDictionary(k => k.Input, v => v.Result));
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
                    var adapterCommand = _adapterReferences.Single(a => a.Uid == adapter).GetDeviceCommand(command);

                    var translator = _translators.FirstOrDefault(e => e.Type == MessageType.Command && e.From.Equals(adapterCommand));

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
                var translator = _translators.FirstOrDefault(e => e.Type == MessageType.Event && e.From.Equals(propertyChanged));

                if (translator != null)
                {
                    var eventPublished = await MessageBroker.PublishWithTranslate(propertyChanged, translator.To, Uid).ConfigureAwait(false);
                    Logger.Log(LogLevel.Information, $"<@{Uid}> {eventPublished}");
                }
                else if (RelayNotTranslatedEvents)
                {
                    var eventPublished = PropertyChangedEvent.Create(Uid, propertyChanged.PropertyChangedName, oldValue, propertyChanged.NewValue);
                    await MessageBroker.Publish(eventPublished, Uid).ConfigureAwait(false);
                    Logger.Log(LogLevel.Information, $"<@{Uid}> {eventPublished}");
                }
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

                        await MessageBroker.SendAfterDelay(ActorMessageContext.Create(Self, command), executionDelay, cancelPrevious).ConfigureAwait(false);
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