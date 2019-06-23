using HomeCenter.Model.Conditions;
using HomeCenter.Model.Conditions.Specific;
using HomeCenter.Model.Core;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Messages.Events.Device;
using HomeCenter.Services.MotionService.Model;
using HomeCenter.Utils.Extensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace HomeCenter.Services.MotionService
{
    internal class CnfusedVectorList
    {
        private ConcurrentDictionary<DateTimeOffset, IList<MotionVector>> _dictionary = new ConcurrentDictionary<DateTimeOffset, IList<MotionVector>>();

        public void Add(IList<MotionVector> vectors)
        {
            if (vectors == null) throw new ArgumentNullException(nameof(vectors));
            if (vectors.Count == 0) throw new ArgumentException(nameof(vectors));
            if (!vectors.All(v => v.EndTime == vectors.First().EndTime)) throw new ArgumentException("All vectors should have same rnd time to be in confuion group");

            _dictionary.TryAdd(vectors.First().EndTime, vectors);
        }
    }

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
        private readonly IEnumerable<string> _neighbors;
        private readonly ConcurrentBag<MotionVector> _enterVectors = new ConcurrentBag<MotionVector>();
        private readonly ConcurrentBag<MotionVector> _leaveVectors = new ConcurrentBag<MotionVector>();
        private readonly CnfusedVectorList _confusedVectors = new CnfusedVectorList();
        private readonly Timeout _turnOffTimeOut;

        private Probability _presenceProbability = Probability.Zero;
        private DateTimeOffset _AutomationEnableOn;
        private DateTimeOffset? _lastAutoIncrement;
        private DateTimeOffset? _lastAutoTurnOff;
        
        public string Uid { get; }

        public bool AutomationDisabled { get; private set; }
        public int NumberOfPersons { get; private set; }
        public MotionStamp LastMotion { get; } = new MotionStamp();
        public AreaDescriptor AreaDescriptor { get; }

        public override string ToString() => $"{Uid} [Last move: {LastMotion}] [Persons: {NumberOfPersons}]";

        public bool IsNeighbor(string uid) => _neighbors.Contains(uid);

        public IEnumerable<string> Neighbors() => _neighbors.ToList();

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

            _turnOffTimeOut.Increment(motionTime);

            Debug.WriteLine($"New timeline: {_turnOffTimeOut.Value.Seconds}s");
        }

        /// <summary>
        /// Update room state on time intervals
        /// </summary>
        /// <returns></returns>
        public async Task PeriodicUpdate(DateTimeOffset motionTime)
        {
            _leaveVectors.Select(vector => vector.Probability < 1 && motionTime.Between(vector.StartTime).LastedLongerThen(_motionConfiguration.ConfusionResolutionTime))

            //acurrentTime.Between(confusedVector.StartTime).LastedLongerThen(_motionConfiguration.ConfusionResolutionTime)
            //                                                       && _roomService.NoMoveInStartNeighbors(confusedVector))

            CheckForTurnOnAutomationAgain();
            await RecalculateProbability(motionTime);
        }

        public void RegisterConfusions(IList<MotionVector> vectors)
        {
            _confusedVectors.Add(vectors);
        }

        /// <summary>
        /// Marks entrance of last motion vector
        /// </summary>
        /// <param name="vector"></param>
        public void MarkEnter(MotionVector vector)
        {
            _enterVectors.Add(vector);

            if (vector.Probability == 1)
            {
                IncrementNumberOfPersons(vector.EndTime);
            }
        }

        public async Task MarkLeave(MotionVector vector)
        {
            _leaveVectors.Add(vector);

            if (vector.Probability == 1)
            {
                DecrementNumberOfPersons();
            }

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
        private async Task RecalculateProbability(DateTimeOffset motionTime)
        {
            // When we just have a move in room there is no need for recalculation
            if (motionTime == LastMotion.Time) return;

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
            if (NumberOfPersons == 0)
            {
                _lastAutoIncrement = time;
                NumberOfPersons++;
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
                NumberOfPersons++;
            }
        }

        private void DecrementNumberOfPersons()
        {
            if (NumberOfPersons > 0)
            {
                NumberOfPersons--;

                //TODO is this needed?
                if (NumberOfPersons == 0)
                {
                    LastMotion.UnConfuze();
                }
            }
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
            NumberOfPersons = 0;
            _turnOffTimeOut.Reset();
        }

        private Task<bool> CanTurnOnLamp() => _turnOnConditionsValidator.Validate();

        private Task<bool> CanTurnOffLamp() => _turnOffConditionsValidator.Validate();
    }
}