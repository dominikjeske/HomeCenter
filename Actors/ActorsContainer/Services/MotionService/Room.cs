using HomeCenter.Model.Conditions;
using HomeCenter.Model.Core;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Messages.Events.Device;
using HomeCenter.Services.MotionService.Model;
using HomeCenter.Utils.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace HomeCenter.Services.MotionService
{
    internal class Room : IDisposable
    {
        //TODO validate conditions
        private readonly Condition _turnOnConditionsValidator;

        private readonly Condition _turnOffConditionsValidator;

        private readonly MotionConfiguration _motionConfiguration;
        private readonly DisposeContainer _disposeContainer = new DisposeContainer();
        private string Lamp { get; }

        // Configuration parameters
        public string Uid { get; }

        internal IEnumerable<string> Neighbors { get; }
        internal IReadOnlyCollection<Room> NeighborsCache { get; private set; }

        // Dynamic parameters
        internal bool AutomationDisabled { get; private set; }

        internal int NumberOfPersonsInArea { get; private set; }
        internal MotionStamp LastMotion { get; } = new MotionStamp();
        internal AreaDescriptor AreaDescriptor { get; }
        internal MotionVector LastVectorEnter { get; private set; }

        private Probability _PresenceProbability { get; set; } = Probability.Zero;
        private DateTimeOffset _AutomationEnableOn { get; set; }
        private DateTimeOffset? _LastAutoIncrement;
        private readonly IConcurrencyProvider _concurrencyProvider;
        private readonly IEnumerable<IEventDecoder> _eventsDecoders;
        private readonly ILogger _logger;
        private DateTimeOffset? _LastAutoTurnOff { get; set; }
        private Timeout _TurnOffTimeOut;
        private readonly IMessageBroker _messageBroker;
        private readonly string _lamp;

        public override string ToString()
        {
            return $"{Uid} [Last move: {LastMotion}] [Persons: {NumberOfPersonsInArea}]";
        }

        public Room(string uid, IEnumerable<string> neighbors, string lamp, IConcurrencyProvider concurrencyProvider, ILogger logger, IMessageBroker messageBroker,
                    AreaDescriptor areaDescriptor, MotionConfiguration motionConfiguration, IEnumerable<IEventDecoder> eventsDecoders)
        {
            Uid = uid ?? throw new ArgumentNullException(nameof(uid));
            Neighbors = neighbors ?? throw new ArgumentNullException(nameof(neighbors));
            Lamp = lamp ?? throw new ArgumentNullException(nameof(lamp));

            //TODO configure conditions
            if (areaDescriptor.WorkingTime == WorkingTime.DayLight)
            {
                //    _turnOnConditionsValidator.WithCondition(ConditionRelation.And, new IsDayCondition(daylightService, dateTimeService));
            }
            else if (areaDescriptor.WorkingTime == WorkingTime.AfterDusk)
            {
                //    _turnOnConditionsValidator.WithCondition(ConditionRelation.And, new IsNightCondition(daylightService, dateTimeService));
            }

            //_turnOnConditionsValidator.WithCondition(ConditionRelation.And, new IsEnabledAutomationCondition(this));
            //_turnOffConditionsValidator.WithCondition(ConditionRelation.And, new IsEnabledAutomationCondition(this));
            //_turnOffConditionsValidator.WithCondition(ConditionRelation.And, new IsTurnOffAutomaionCondition(this));

            _logger = logger;
            _motionConfiguration = motionConfiguration;
            _concurrencyProvider = concurrencyProvider;
            _eventsDecoders = eventsDecoders;
            AreaDescriptor = areaDescriptor;
            _TurnOffTimeOut = new Timeout(AreaDescriptor.TurnOffTimeout, _motionConfiguration.TurnOffPresenceFactor);

            _eventsDecoders?.ForEach(decoder => decoder.Init(this));
            _messageBroker = messageBroker;
            _lamp = lamp;
        }

        internal void RegisterForLampChangeState()
        {
            RegisterManualChangeDecodersSource();
            RegisterChangeStateSource();
        }

        private void RegisterManualChangeDecodersSource()
        {
            //TODO
            //var manualEventSource = Lamp.PowerStateChange.Where(ev => ev.EventSource == PowerStateChangeEvent.ManualSource);
            //var subscription = manualEventSource.Timestamp()
            //                                    .Buffer(manualEventSource, _ => Observable.Timer(_motionConfiguration.ManualCodeWindow, _concurrencyProvider.Scheduler))
            //                                    .Subscribe(DecodeMessage);

            //_disposeContainer.Add(subscription);
        }

        private void DecodeMessage(IList<Timestamped<PowerStateChangeEvent>> powerStateEvents) => _eventsDecoders?.ForEach(decoder => decoder.DecodeMessage(powerStateEvents));

        private void RegisterChangeStateSource()
        {
            //TODO
            //_disposeContainer.Add(Lamp.PowerStateChange.Subscribe(PowerStateChangeHandler));
        }

        private void PowerStateChangeHandler(PowerStateChangeEvent powerChangeEvent)
        {
            if (powerChangeEvent.Value == false)
            {
                ResetStatistics();
                RegisterTurnOffTime();
            }

            _logger.LogInformation($"[{Uid} Light] = {powerChangeEvent.Value} | Source: {powerChangeEvent.EventTriggerSource}");
        }

        public async Task MarkMotion(DateTimeOffset time)
        {
            CheckTurnOffTimeOut(time);
            LastMotion.SetTime(time);
            await SetProbability(Probability.Full).ConfigureAwait(false);
            CheckAutoIncrementForOnePerson(time);

            _TurnOffTimeOut.IncrementCounter();
        }

        private void CheckTurnOffTimeOut(DateTimeOffset time)
        {
            // if light is turned off to early area TurnOffTimeout is too low and we have to update it
            if (_PresenceProbability == Probability.Zero && time.HappendInPrecedingTimeWindow(_LastAutoTurnOff, _motionConfiguration.MotionTimeWindow))
            {
                UpdateAreaTurnoffTimeOut();
                _TurnOffTimeOut.UpdateBaseTime(AreaDescriptor.TurnOffTimeout);
            }
        }

        private void UpdateAreaTurnoffTimeOut()
        {
            var newTimeOut = AreaDescriptor.TurnOffTimeout.IncreaseByPercentage(_motionConfiguration.TurnOffTimeoutIncrementPercentage);
            _logger.LogInformation($"[{Uid} turn-off time out] {AreaDescriptor.TurnOffTimeout} -> {newTimeOut}");
            AreaDescriptor.TurnOffTimeout = newTimeOut;
        }

        public async Task Update()
        {
            CheckForTurnOnAutomationAgain();
            await RecalculateProbability().ConfigureAwait(false);
        }

        public void MarkEnter(MotionVector vector)
        {
            LastVectorEnter = vector;
            IncrementNumberOfPersons(vector.End.TimeStamp);
        }

        public async Task MarkLeave(MotionVector vector)
        {
            DecrementNumberOfPersons();

            if (AreaDescriptor.MaxPersonCapacity == 1)
            {
                await SetProbability(Probability.Zero).ConfigureAwait(false);
            }
            else
            {
                //TODO change this value
                await SetProbability(Probability.FromValue(0.1)).ConfigureAwait(false);
            }
        }

        public void Dispose() => _disposeContainer.Dispose();

        internal void BuildNeighborsCache(IEnumerable<Room> neighbors) => NeighborsCache = new ReadOnlyCollection<Room>(neighbors.ToList());

        internal void DisableAutomation()
        {
            AutomationDisabled = true;
            _logger.LogInformation($"Room {Uid} automation disabled");
        }

        internal void EnableAutomation()
        {
            AutomationDisabled = false;
            _logger.LogInformation($"Room {Uid} automation enabled");
        }

        internal void DisableAutomation(TimeSpan time)
        {
            DisableAutomation();
            _AutomationEnableOn = _concurrencyProvider.Scheduler.Now + time;
        }

        internal IList<MotionPoint> GetConfusingPoints(MotionVector vector) => NeighborsCache.ToList()
                                                                                             .AddChained(this)
                                                                                             .Where(room => room.Uid != vector.Start.Uid)
                                                                                             .Select(room => room.GetConfusion(vector.End.TimeStamp))
                                                                                             .Where(y => y != null)
                                                                                             .ToList();

        /// <summary>
        /// When we don't detect motion vector previously but there is move in room and currently we have 0 person so we know that there is a least one
        /// </summary>
        private void CheckAutoIncrementForOnePerson(DateTimeOffset time)
        {
            if (NumberOfPersonsInArea == 0)
            {
                _LastAutoIncrement = time;
                NumberOfPersonsInArea++;
            }
        }

        private void IncrementNumberOfPersons(DateTimeOffset time)
        {
            if (!_LastAutoIncrement.HasValue || time.HappendBeforePrecedingTimeWindow(_LastAutoIncrement, TimeSpan.FromMilliseconds(100)))
            {
                NumberOfPersonsInArea++;
            }
        }

        private void DecrementNumberOfPersons()
        {
            if (NumberOfPersonsInArea > 0)
            {
                NumberOfPersonsInArea--;

                if (NumberOfPersonsInArea == 0)
                {
                    LastMotion.UnConfuze();
                }
            }
        }

        private MotionPoint GetConfusion(DateTimeOffset timeOfMotion)
        {
            var lastMotion = LastMotion;

            // If last motion time has same value we have to go back in time for previous value to check real previous
            if (timeOfMotion == lastMotion.Time)
            {
                lastMotion = lastMotion.Previous;
            }

            if
            (
                  lastMotion?.Time != null
               && lastMotion.CanConfuze
               && timeOfMotion.IsMovePhisicallyPosible(lastMotion.Time.Value, _motionConfiguration.MotionMinDiff)
               && timeOfMotion.HappendInPrecedingTimeWindow(lastMotion.Time, AreaDescriptor.MotionDetectorAlarmTime)
            )
            {
                return new MotionPoint(Uid, lastMotion.Time.Value);
            }

            return null;
        }

        private async Task RecalculateProbability()
        {
            var probabilityDelta = 1.0 / (_TurnOffTimeOut.Value.Ticks / _motionConfiguration.PeriodicCheckTime.Ticks);

            await SetProbability(_PresenceProbability.Decrease(probabilityDelta)).ConfigureAwait(false);
        }

        private void CheckForTurnOnAutomationAgain()
        {
            if (AutomationDisabled && _concurrencyProvider.Scheduler.Now > _AutomationEnableOn)
            {
                EnableAutomation();
            }
        }

        private async Task SetProbability(Probability probability)
        {
            _PresenceProbability = probability;

            if (_PresenceProbability.IsFullProbability)
            {
                await TryTurnOnLamp().ConfigureAwait(false);
            }
            else if (_PresenceProbability.IsNoProbability)
            {
                await TryTurnOffLamp().ConfigureAwait(false);
            }
        }

        private async Task TryTurnOnLamp()
        {
            if (await CanTurnOnLamp().ConfigureAwait(false))
            {
                _messageBroker.Send(new TurnOnCommand(), _lamp);
            }
        }

        private async Task TryTurnOffLamp()
        {
            if (await CanTurnOffLamp().ConfigureAwait(false))
            {
                _messageBroker.Send(new TurnOffCommand(), _lamp);
            }
        }

        private void ResetStatistics()
        {
            NumberOfPersonsInArea = 0;
            _TurnOffTimeOut.Reset();
        }

        private void RegisterTurnOffTime() => _LastAutoTurnOff = _concurrencyProvider.Scheduler.Now;

        private Task<bool> CanTurnOnLamp() => _turnOnConditionsValidator.Validate();

        private Task<bool> CanTurnOffLamp() => _turnOffConditionsValidator.Validate();
    }
}