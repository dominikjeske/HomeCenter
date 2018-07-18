using System;
using System.Linq;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using Wirehome.Motion.Model;
using Wirehome.Conditions;
using Wirehome.ComponentModel.Components;
using Wirehome.Core.Services.Logging;
using Wirehome.Contracts.Environment;
using Wirehome.Conditions.Specialized;
using Wirehome.Core.Extensions;
using Wirehome.Model.Extensions;
using Wirehome.ComponentModel.Commands;
using Wirehome.Contracts.Conditions;
using Wirehome.Core;
using Wirehome.ComponentModel.Events;
using Wirehome.Model.Events;
using Wirehome.ComponentModel.Capabilities.Constants;

namespace Wirehome.Motion
{
    //TODO Thread safe
    public class Room : IDisposable
    {
        private readonly ConditionsValidator _turnOnConditionsValidator = new ConditionsValidator();
        private readonly ConditionsValidator _turnOffConditionsValidator = new ConditionsValidator();
        private readonly MotionConfiguration _motionConfiguration;
        private readonly DisposeContainer _disposeContainer = new DisposeContainer();
        private Component Lamp { get; }

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

        public override string ToString()
        {
            return $"{Uid} [Last move: {LastMotion}] [Persons: {NumberOfPersonsInArea}]";
            //TODO DEBUG
            // return $"{Uid} [Last move: {LastMotion}] [Persons: {NumberOfPersonsInArea}] [Lamp: {(Lamp as MotionLamp)?.GetIsTurnedOn()}]";
        }

        public Room(string uid, IEnumerable<string> neighbors, Component lamp, IDaylightService daylightService,
                    IConcurrencyProvider concurrencyProvider, ILogger logger, 
                    AreaDescriptor areaDescriptor, MotionConfiguration motionConfiguration, IEnumerable<IEventDecoder> eventsDecoders)
        {
            Uid = uid ?? throw new ArgumentNullException(nameof(uid));
            Neighbors = neighbors ?? throw new ArgumentNullException(nameof(neighbors));
            Lamp = lamp ?? throw new ArgumentNullException(nameof(lamp));

            if (areaDescriptor.WorkingTime == WorkingTime.DayLight)
            {
                _turnOnConditionsValidator.WithCondition(ConditionRelation.And, new IsDayCondition(daylightService));
            }
            else if (areaDescriptor.WorkingTime == WorkingTime.AfterDusk)
            {
                _turnOnConditionsValidator.WithCondition(ConditionRelation.And, new IsNightCondition(daylightService));
            }

            _turnOnConditionsValidator.WithCondition(ConditionRelation.And, new IsEnabledAutomationCondition(this));
            _turnOffConditionsValidator.WithCondition(ConditionRelation.And, new IsEnabledAutomationCondition(this));
            _turnOffConditionsValidator.WithCondition(ConditionRelation.And, new IsTurnOffAutomaionCondition(this));
            
            _logger = logger;
            _motionConfiguration = motionConfiguration;
            _concurrencyProvider = concurrencyProvider;
            _eventsDecoders = eventsDecoders;
            AreaDescriptor = areaDescriptor;
            _TurnOffTimeOut = new Timeout(AreaDescriptor.TurnOffTimeout, _motionConfiguration.TurnOffPresenceFactor);

            _eventsDecoders?.ForEach(decoder => decoder.Init(this));
        }

        internal void RegisterForLampChangeState()
        {
            RegisterManualChangeDecodersSource();
            RegisterChangeStateSource();
        }

        //TODO check if event is power change
        private void RegisterManualChangeDecodersSource()
        {
            var manualEventSource = Lamp.Events.Where(ev => ev[EventProperties.EventSource].ToStringValue() == EventSources.Manual);
            var subscription = manualEventSource.Timestamp()
                                                .Buffer(manualEventSource, _ => Observable.Timer(_motionConfiguration.ManualCodeWindow, _concurrencyProvider.Scheduler))
                                                .Subscribe(DecodeMessage);

            _disposeContainer.Add(subscription);
        }

        private void DecodeMessage(IList<Timestamped<Event>> powerStateEvents) => _eventsDecoders?.ForEach(decoder => decoder.DecodeMessage(powerStateEvents));

        private void RegisterChangeStateSource() =>  _disposeContainer.Add(Lamp.Events.Subscribe(PowerStateChangeHandler));
        
        private void PowerStateChangeHandler(Event powerChangeEvent)
        {
            if(powerChangeEvent[EventProperties.NewValue].ToStringValue() == PowerStateValue.OFF)
            {
                ResetStatistics();
                RegisterTurnOffTime();
            }

            _logger.Info($"[{Uid} Light] = {powerChangeEvent[EventProperties.NewValue]} | Source: {powerChangeEvent[EventProperties.EventSource]}");
        }

        public void MarkMotion(DateTimeOffset time)
        {
            CheckTurnOffTimeOut(time);
            LastMotion.SetTime(time);
            SetProbability(Probability.Full);
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
            _logger.Info($"[{Uid} turn-off time out] {AreaDescriptor.TurnOffTimeout} -> {newTimeOut}");
            AreaDescriptor.TurnOffTimeout = newTimeOut;
        }

        public void Update()
        {
            CheckForTurnOnAutomationAgain();
            RecalculateProbability();
        }

        public void MarkEnter(MotionVector vector)
        {
            LastVectorEnter = vector;
            IncrementNumberOfPersons(vector.End.TimeStamp);
        }

        public void MarkLeave(MotionVector vector)
        {
            DecrementNumberOfPersons();

            if (AreaDescriptor.MaxPersonCapacity == 1)
            {
                SetProbability(Probability.Zero);
            }
            else
            {
                //TODO change this value                                                                                                                                                        
                SetProbability(Probability.FromValue(0.1));
            }
        }

        public void Dispose() => _disposeContainer.Dispose();

        internal void BuildNeighborsCache(IEnumerable<Room> neighbors) => NeighborsCache = new ReadOnlyCollection<Room>(neighbors.ToList());
        internal void DisableAutomation()
        {
            AutomationDisabled = true;
            _logger.Info($"Room {Uid} automation disabled");
        }

        internal void EnableAutomation()
        {
            AutomationDisabled = false;
            _logger.Info($"Room {Uid} automation enabled");
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

                if(NumberOfPersonsInArea == 0)
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

        private void RecalculateProbability()
        {
            var probabilityDelta = 1.0 / (_TurnOffTimeOut.Value.Ticks / _motionConfiguration.PeriodicCheckTime.Ticks);

            SetProbability(_PresenceProbability.Decrease(probabilityDelta));
        }

        private void CheckForTurnOnAutomationAgain()
        {
            if (AutomationDisabled && _concurrencyProvider.Scheduler.Now > _AutomationEnableOn)
            {
                EnableAutomation();
            }
        }

        private void SetProbability(Probability probability)
        {
            _PresenceProbability = probability;

            if(_PresenceProbability.IsFullProbability)
            {
                TryTurnOnLamp();
            }
            else if(_PresenceProbability.IsNoProbability)
            {
                TryTurnOffLamp();
            }
        }

        private void TryTurnOnLamp()
        {
            if (CanTurnOnLamp())
            {
                Lamp.ExecuteCommand(CommandFatory.TurnOnCommand);
            }
        }

        private void TryTurnOffLamp()
        {
            if (CanTurnOffLamp())
            {
                Lamp.ExecuteCommand(CommandFatory.TurnOffCommand);
            }
        }

        private void ResetStatistics()
        {
            NumberOfPersonsInArea = 0;
            _TurnOffTimeOut.Reset();
        }

        private void RegisterTurnOffTime() => _LastAutoTurnOff = _concurrencyProvider.Scheduler.Now;
        private bool CanTurnOnLamp() => _turnOnConditionsValidator.Validate() != ConditionState.NotFulfilled;
        private bool CanTurnOffLamp() => _turnOffConditionsValidator.Validate() != ConditionState.NotFulfilled;
    }
}