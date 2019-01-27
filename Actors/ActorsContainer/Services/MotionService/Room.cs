using HomeCenter.Model.Conditions;
using HomeCenter.Model.Conditions.Specific;
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
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace HomeCenter.Services.MotionService
{
    internal class Room : IDisposable
    {
        private readonly ConditionContainer _turnOnConditionsValidator = new ConditionContainer();
        private readonly ConditionContainer _turnOffConditionsValidator = new ConditionContainer();

        private readonly MotionConfiguration _motionConfiguration;
        private readonly DisposeContainer _disposeContainer = new DisposeContainer();

        // Configuration parameters
        public string Uid { get; }

        internal IEnumerable<string> _neighbors { get; }
        internal IReadOnlyCollection<Room> NeighborsCache { get; private set; }

        // Dynamic parameters
        internal bool AutomationDisabled { get; private set; }

        internal int NumberOfPersonsInArea { get; private set; }
        internal MotionStamp LastMotion { get; } = new MotionStamp();
        internal AreaDescriptor _areaDescriptor { get; }
        internal MotionVector LastVectorEnter { get; private set; }

        private Probability _PresenceProbability { get; set; } = Probability.Zero;
        private DateTimeOffset _AutomationEnableOn { get; set; }
        private DateTimeOffset? _LastAutoIncrement;
        private readonly IConcurrencyProvider _concurrencyProvider;
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
                    AreaDescriptor areaDescriptor, MotionConfiguration motionConfiguration)
        {
            Uid = uid ?? throw new ArgumentNullException(nameof(uid));
            _neighbors = neighbors ?? throw new ArgumentNullException(nameof(neighbors));
            _lamp = lamp ?? throw new ArgumentNullException(nameof(lamp));
            _logger = logger;
            _motionConfiguration = motionConfiguration;
            _concurrencyProvider = concurrencyProvider;
            _messageBroker = messageBroker;
            _areaDescriptor = areaDescriptor;

            if (areaDescriptor.WorkingTime == WorkingTime.DayLight)
            {
                _turnOnConditionsValidator.WithCondition(new IsDayCondition(_messageBroker));
            }
            else if (areaDescriptor.WorkingTime == WorkingTime.AfterDusk)
            {
                _turnOnConditionsValidator.WithCondition(new IsNightCondition(_messageBroker));
            }

            _turnOnConditionsValidator.WithCondition(new IsEnabledAutomationCondition(this));
            _turnOffConditionsValidator.WithCondition(new IsEnabledAutomationCondition(this));
            _turnOffConditionsValidator.WithCondition(new IsTurnOffAutomaionCondition(this));

            _TurnOffTimeOut = new Timeout(_areaDescriptor.TurnOffTimeout, _motionConfiguration.TurnOffPresenceFactor);
        }

        internal void RegisterForLampChangeState()
        {
            RegisterChangeStateSource();
        }

        private void RegisterChangeStateSource()
        {
            //TODO
            //_disposeContainer.Add(Lamp.PowerStateChange.Subscribe(PowerStateChangeHandler));
        }

        private void PowerStateChangeHandler(PowerStateChangeEvent powerChangeEvent)
        {
            if (!powerChangeEvent.Value)
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
                _TurnOffTimeOut.UpdateBaseTime(_areaDescriptor.TurnOffTimeout);
            }
        }

        private void UpdateAreaTurnoffTimeOut()
        {
            var newTimeOut = _areaDescriptor.TurnOffTimeout.IncreaseByPercentage(_motionConfiguration.TurnOffTimeoutIncrementPercentage);
            _logger.LogInformation($"[{Uid} turn-off time out] {_areaDescriptor.TurnOffTimeout} -> {newTimeOut}");
            _areaDescriptor.TurnOffTimeout = newTimeOut;
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

            if (_areaDescriptor.MaxPersonCapacity == 1)
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
               && timeOfMotion.HappendInPrecedingTimeWindow(lastMotion.Time, _areaDescriptor.MotionDetectorAlarmTime)
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