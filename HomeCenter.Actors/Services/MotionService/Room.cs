using HomeCenter.Abstractions;
using HomeCenter.Abstractions.Extensions;
using HomeCenter.Conditions;
using HomeCenter.Conditions.Specific;
using HomeCenter.Extensions;
using HomeCenter.Messages.Commands.Device;
using HomeCenter.Messages.Events.Device;
using HomeCenter.Services.MotionService.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
        private readonly IConcurrencyProvider _concurrencyProvider;
        private readonly ILogger _logger;
        private readonly IMessageBroker _messageBroker;
        private readonly string _lamp;
        private readonly ConfusedVectors ConfusedVectors;
        private readonly Lazy<RoomDictionary> _roomDictionary;
        private Probability _currentProbability = Probability.Zero;
        private DateTimeOffset? _scheduledAutomationTime;

        internal RoomStatistic RoomStatistic { get; }
        internal string Uid { get; }
        internal bool AutomationDisabled { get; private set; }
        internal int NumberOfPersons => RoomStatistic.NumberOfPersons;
        internal AreaDescriptor AreaDescriptor { get; }

        public override string ToString() => $"{Uid} [Last move: {RoomStatistic.LastMotion}] [Persons: {NumberOfPersons}]";

        public Room(string uid, string lamp, IConcurrencyProvider concurrencyProvider, ILogger logger, Lazy<RoomDictionary> roomDictionary,
                    IMessageBroker messageBroker, AreaDescriptor areaDescriptor, MotionConfiguration motionConfiguration)
        {
            Uid = uid ?? throw new ArgumentNullException(nameof(uid));
            _lamp = lamp ?? throw new ArgumentNullException(nameof(lamp));
            _logger = logger;
            _motionConfiguration = motionConfiguration;
            _concurrencyProvider = concurrencyProvider;
            _messageBroker = messageBroker;
            _roomDictionary = roomDictionary;
            AreaDescriptor = areaDescriptor;

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

            RoomStatistic = new RoomStatistic(_logger, Uid, AreaDescriptor.TurnOffTimeout, _motionConfiguration);
            ConfusedVectors = new ConfusedVectors(logger, Uid, motionConfiguration.ConfusionResolutionTime,
               _motionConfiguration.ConfusionResolutionTimeOut, _roomDictionary, (v) => MarkVector(v, true));

            RegisterChangeStateSource();
        }

        private void RegisterChangeStateSource()
        {
            //TODO
            //_disposeContainer.Add(Lamp.PowerStateChange.Subscribe(PowerStateChangeHandler));
        }

        /// <summary>
        /// Take action when there is a move in the room
        /// </summary>
        public async Task MarkMotion(DateTimeOffset motionTime)
        {
            _logger.LogDeviceEvent(Uid, MoveEventId.Motion, "Motion at {motionTime}", motionTime);

            RoomStatistic.UpdateMotion(motionTime, _currentProbability, Probability.Full);

            await SetProbability(Probability.Full);
        }

        /// <summary>
        /// Update room state on time intervals
        /// </summary>
        public async Task PeriodicUpdate(DateTimeOffset motionTime)
        {
            CheckForScheduledAutomation(motionTime);
            await RecalculateProbability(motionTime);
        }

        public async Task HandleVectors(IList<MotionVector> motionVectors)
        {
            if (motionVectors.Count == 0)
            {
                return;
            }
            // When we have one vector we know that there is no concurrent vectors to same room
            else if (motionVectors.Count == 1)
            {
                var vector = motionVectors.Single();

                if (IsTurnOnVector(vector))
                {
                    await MarkVector(vector, false);
                }
                else
                {
                    ConfusedVectors.MarkConfusion(vector);
                }
            }
            // When we have at least two vectors we know that this vector is confused
            else
            {
                motionVectors.ForEach(vector => ConfusedVectors.MarkConfusion(vector));
            }
        }

        public void EnableAutomation()
        {
            AutomationDisabled = false;

            _logger.LogDeviceEvent(Uid, MoveEventId.AutomationEnabled);
        }

        public void DisableAutomation(TimeSpan time)
        {
            AutomationDisabled = true;

            _logger.LogDeviceEvent(Uid, MoveEventId.AutomationDisabled);

            if (time != TimeSpan.Zero)
            {
                _scheduledAutomationTime = _concurrencyProvider.Scheduler.Now + time;
            }
        }

        public void Dispose() => _disposeContainer.Dispose();

        /// <summary>
        /// Check if <paramref name="motionVector"/> is vector that turned on the light
        /// </summary>
        private bool IsTurnOnVector(MotionVector motionVector)
        {
            return !RoomStatistic.FirstEnterTime.HasValue || RoomStatistic.FirstEnterTime == motionVector.EndTime;
        }

        /// <summary>
        /// Marks enter to target room and leave from source room
        /// </summary>
        private async Task MarkVector(MotionVector motionVector, bool resolved)
        {
            _logger.LogDeviceEvent(Uid, MoveEventId.MarkVector, "Vector {motionVector} ({resolved})", motionVector, resolved ? " [Resolved]" : "[OK]");

            await _roomDictionary.Value[motionVector.StartPoint].MarkLeave(motionVector);
            MarkEnter(motionVector);
        }

        /// <summary>
        /// Marks entrance of last motion vector
        /// </summary>
        private void MarkEnter(MotionVector vector) => RoomStatistic.IncrementNumberOfPersons(vector.EndTime);

        internal Task EvaluateConfusions(DateTimeOffset dateTimeOffset) => ConfusedVectors.EvaluateConfusions(dateTimeOffset);

        private async Task MarkLeave(MotionVector vector)
        {
            RoomStatistic.DecrementNumberOfPersons();

            RoomStatistic.LastLeaveVector = vector;

            // Only when we have one person room we can be sure that we can turn of light immediately
            if (AreaDescriptor.MaxPersonCapacity == 1)
            {
                await SetProbability(Probability.Zero);
            }
            else
            {
                var numberOfPeopleFactor = NumberOfPersons == 0 ? _motionConfiguration.DecreaseLeavingFactor : _motionConfiguration.DecreaseLeavingFactor / NumberOfPersons;
                var visitTypeFactor = RoomStatistic.TurnOffTimeOut.VisitType.Value;
                var decreasePercent = numberOfPeopleFactor / visitTypeFactor;

                _logger.LogDeviceEvent(Uid, MoveEventId.Probability, "Probability => {probability}%", decreasePercent * 100);

                await SetProbability(_currentProbability.DecreaseByPercent(decreasePercent));
            }
        }

        /// <summary>
        /// Decrease probability of person in room after each time interval
        /// </summary>
        private async Task RecalculateProbability(DateTimeOffset motionTime)
        {
            // When we just have a move in room there is no need for recalculation
            if (motionTime == RoomStatistic.LastMotion.Time) return;

            var probabilityDelta = RoomStatistic.GetDeltaProbability();

            await SetProbability(_currentProbability.Decrease(probabilityDelta));
        }

        /// <summary>
        /// Set probability of occurrence of the person in the room
        /// </summary>
        private async Task SetProbability(Probability probability)
        {
            if (probability == _currentProbability) return;

            _logger.LogDeviceEvent(Uid, MoveEventId.Probability, "{probability:00.00}% [{timeout}]", probability.Value * 100, RoomStatistic.TurnOffTimeOut.Value);

            _currentProbability = probability;

            await TryChangeLampState();
        }

        private async Task TryChangeLampState()
        {
            if (_currentProbability.IsFullProbability)
            {
                await TryTurnOnLamp();
            }
            else if (_currentProbability.IsNoProbability)
            {
                await TryTurnOffLamp();

                // await NeighborsCache.Select(n => n.Value.EvaluateConfusions()).WhenAll();
            }
        }

        private void PowerStateChangeHandler(PowerStateChangeEvent powerChangeEvent)
        {
            if (!powerChangeEvent.Value)
            {
                RoomStatistic.Reset();
            }

            _logger.LogDeviceEvent(Uid, MoveEventId.PowerState, "{newState} | Source: {source}", powerChangeEvent.Value, powerChangeEvent.EventTriggerSource);
        }

        private void CheckForScheduledAutomation(DateTimeOffset motionTime)
        {
            if (AutomationDisabled && motionTime > _scheduledAutomationTime)
            {
                EnableAutomation();
            }
        }

        private async Task TryTurnOnLamp()
        {
            if (await _turnOnConditionsValidator.Validate())
            {
                _messageBroker.Send(new TurnOnCommand(), _lamp);
            }
        }

        private async Task TryTurnOffLamp()
        {
            if (await _turnOffConditionsValidator.Validate())
            {
                _logger.LogDeviceEvent(Uid, MoveEventId.PowerState, "Turning OFF");

                _messageBroker.Send(new TurnOffCommand(), _lamp);

                RoomStatistic.LastAutoTurnOff = _concurrencyProvider.Scheduler.Now;
            }
        }
    }
}