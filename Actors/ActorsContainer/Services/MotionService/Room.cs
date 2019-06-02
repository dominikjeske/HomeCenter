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
        private readonly IConcurrencyProvider _concurrencyProvider;
        private readonly ILogger _logger;
        private readonly IMessageBroker _messageBroker;
        private readonly string _lamp;

        // Configuration parameters
        internal string Uid { get; }

        internal IEnumerable<string> _neighbors { get; }
        internal IReadOnlyCollection<Room> NeighborsCache { get; private set; }

        internal bool AutomationDisabled { get; private set; }
        internal int NumberOfPersonsInArea { get; private set; }
        internal MotionStamp LastMotion { get; } = new MotionStamp();
        internal AreaDescriptor AreaDescriptor { get; }

        private Probability _presenceProbability = Probability.Zero;
        private DateTimeOffset _AutomationEnableOn;
        private DateTimeOffset? _lastAutoIncrement;
        private DateTimeOffset? _lastAutoTurnOff;
        private readonly Timeout _turnOffTimeOut;
        private MotionVector _lastVectorEnter;

        public override string ToString() => $"{Uid} [Last move: {LastMotion}] [Persons: {NumberOfPersonsInArea}]";

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

            _turnOffTimeOut = new Timeout(AreaDescriptor.TurnOffTimeout, _motionConfiguration);
        }

        /// <summary>
        /// Take action when there is a move in the room
        /// </summary>
        /// <param name="motionTime"></param>
        /// <returns></returns>
        public async Task MarkMotion(DateTimeOffset motionTime)
        {
            TryTuneTurnOffTimeOut(motionTime);
            LastMotion.SetTime(motionTime);
            await SetProbability(Probability.Full);
            CheckAutoIncrementForOnePerson(motionTime);

            _turnOffTimeOut.Increment();
        }

        /// <summary>
        /// Update room state on time intervals
        /// </summary>
        /// <returns></returns>
        public async Task PeriodicUpdate()
        {
            CheckForTurnOnAutomationAgain();
            await RecalculateProbability();
        }

        /// <summary>
        /// Marks entrance of last motion vector
        /// </summary>
        /// <param name="vector"></param>
        public void MarkEnter(MotionVector vector)
        {
            _lastVectorEnter = vector;
            IncrementNumberOfPersons(vector.End.TimeStamp);
        }

        public async Task MarkLeave(MotionVector vector)
        {
            DecrementNumberOfPersons();

            if (AreaDescriptor.MaxPersonCapacity == 1)
            {
                await SetProbability(Probability.Zero);
            }
            else
            {
                //TODO change this value
                await SetProbability(Probability.FromValue(0.1));
            }
        }

        public void BuildNeighborsCache(IEnumerable<Room> neighbors) => NeighborsCache = new ReadOnlyCollection<Room>(neighbors.ToList());

        public void DisableAutomation()
        {
            AutomationDisabled = true;
            _logger.LogInformation($"Room {Uid} automation disabled");
        }

        public void EnableAutomation()
        {
            AutomationDisabled = false;
            _logger.LogInformation($"Room {Uid} automation enabled");
        }

        public void DisableAutomation(TimeSpan time)
        {
            DisableAutomation();
            _AutomationEnableOn = _concurrencyProvider.Scheduler.Now + time;
        }

        public bool HasSameLastTimeVector(MotionVector motionVector) => _lastVectorEnter?.EqualsWithStartTime(motionVector) ?? false;

        /// <summary>
        /// Get confusion point from all neighbors
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public IList<MotionPoint> GetConfusingPoints(MotionVector vector) => NeighborsCache.ToList()
                                                                                           .AddChained(this)
                                                                                           .Where(room => room.Uid != vector.Start.Uid)
                                                                                           .Select(room => room.GetConfusion(vector.End.TimeStamp))
                                                                                           .Where(y => y != MotionPoint.Empty)
                                                                                           .ToList();

        public void Dispose() => _disposeContainer.Dispose();

        public void RegisterForLampChangeState()
        {
            RegisterChangeStateSource();
        }

        /// <summary>
        /// If light is turned off too early area TurnOffTimeout is too low and we have to update it
        /// </summary>
        /// <param name="moveTime"></param>
        private void TryTuneTurnOffTimeOut(DateTimeOffset moveTime)
        {
            if (_presenceProbability == Probability.Zero)
            {
                (bool result, TimeSpan before, TimeSpan after) = _turnOffTimeOut.TryIncreaseBaseTime(moveTime, _lastAutoTurnOff);
                if (result) _logger.LogInformation($"[{Uid}] Turn-off time out updated {before} -> {after}");
            }
        }

        /// <summary>
        /// Decrease probability of person in room after each time interval
        /// </summary>
        /// <returns></returns>
        private async Task RecalculateProbability()
        {
            var probabilityDelta = 1.0 / (_turnOffTimeOut.Value.Ticks / _motionConfiguration.PeriodicCheckTime.Ticks);

            await SetProbability(_presenceProbability.Decrease(probabilityDelta));
        }

        /// <summary>
        /// Set probability of occurrence of the person in the room
        /// </summary>
        /// <param name="probability"></param>
        /// <returns></returns>
        private async Task SetProbability(Probability probability)
        {
            if (probability == _presenceProbability) return;

            _presenceProbability = probability;

            if (_presenceProbability.IsFullProbability)
            {
                await TryTurnOnLamp();
            }
            else if (_presenceProbability.IsNoProbability)
            {
                await TryTurnOffLamp();
            }
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
            }

            _logger.LogInformation($"[{Uid} Light] = {powerChangeEvent.Value} | Source: {powerChangeEvent.EventTriggerSource}");
        }

        /// <summary>
        /// When we don't detect motion vector previously but there is move in room and currently we have 0 person so we know that there is a least one
        /// </summary>
        private void CheckAutoIncrementForOnePerson(DateTimeOffset time)
        {
            if (NumberOfPersonsInArea == 0)
            {
                _lastAutoIncrement = time;
                NumberOfPersonsInArea++;
            }
        }

        //TODO maybe do it differently
        private void CheckForTurnOnAutomationAgain()
        {
            if (AutomationDisabled && _concurrencyProvider.Scheduler.Now > _AutomationEnableOn)
            {
                EnableAutomation();
            }
        }

        private void IncrementNumberOfPersons(DateTimeOffset moveTime)
        {
            if (!_lastAutoIncrement.HasValue || moveTime.Between(_lastAutoIncrement.Value).LastedLongerThen(TimeSpan.FromMilliseconds(100)))
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

        /// <summary>
        /// Check if last move in this room can be source of potential move that will be source of confusion for other moves
        /// </summary>
        /// <param name="motionTime"></param>
        /// <returns></returns>
        private MotionPoint GetConfusion(DateTimeOffset motionTime)
        {
            var lastMotion = GetLastMotion(motionTime);

            if (!lastMotion.CanConfuze) return MotionPoint.Empty;

            var possibleMoveTime = motionTime.Between(lastMotion.Time.Value);

            if
            (
                  possibleMoveTime.IsPossible(_motionConfiguration.MotionMinDiff)
               && possibleMoveTime.LastedLessThen(AreaDescriptor.MotionDetectorAlarmTime)   // TODO maybe increase it to 2x AreaDescriptor.MotionDetectorAlarmTime or provide distance between motion detectors
            )
            {
                return new MotionPoint(Uid, lastMotion.Time.Value);
            }

            return MotionPoint.Empty;
        }

        /// <summary>
        /// If last motion time has same value we have to go back in time for previous value to check real previous
        /// </summary>
        /// <param name="motionTime"></param>
        /// <returns></returns>
        private MotionStamp GetLastMotion(DateTimeOffset motionTime)
        {
            var lastMotion = LastMotion;

            if (motionTime == lastMotion.Time)
            {
                lastMotion = lastMotion.Previous;
            }

            return lastMotion;
        }

        private async Task TryTurnOnLamp()
        {
            if (await CanTurnOnLamp())
            {
                _messageBroker.Send(new TurnOnCommand(), _lamp);
            }
        }

        private async Task TryTurnOffLamp()
        {
            if (await CanTurnOffLamp())
            {
                _logger.LogInformation($"[{Uid}] Turn off");

                _messageBroker.Send(new TurnOffCommand(), _lamp);

                _lastAutoTurnOff = _concurrencyProvider.Scheduler.Now;
            }
        }

        private void ResetStatistics()
        {
            NumberOfPersonsInArea = 0;
            _turnOffTimeOut.Reset();
        }

        private Task<bool> CanTurnOnLamp() => _turnOnConditionsValidator.Validate();

        private Task<bool> CanTurnOffLamp() => _turnOffConditionsValidator.Validate();
    }
}