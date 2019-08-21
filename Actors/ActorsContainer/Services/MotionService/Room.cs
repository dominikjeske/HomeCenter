using ConcurrentCollections;
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
using System.Collections.Immutable;
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
        private readonly Lazy<IEnumerable<Room>> _neighboursFactory;
        private readonly string _lamp;
        private readonly Timeout _turnOffTimeOut;
        private readonly ConcurrentHashSet<MotionVector> _confusingVectors = new ConcurrentHashSet<MotionVector>();
        private IReadOnlyDictionary<string, Room> _neighborsCache = ImmutableDictionary<string, Room>.Empty;

        private Probability _presenceProbability = Probability.Zero;
        private DateTimeOffset _AutomationEnableOn;
        private DateTimeOffset? _lastAutoIncrement;
        private DateTimeOffset? _lastAutoTurnOff;
        private DateTimeOffset? _firstEnterTime;
        public string Uid { get; }
        public bool AutomationDisabled { get; private set; }
        public int NumberOfPersons { get; private set; }
        public MotionStamp LastMotion { get; } = new MotionStamp();
        public AreaDescriptor AreaDescriptor { get; }

        internal IReadOnlyDictionary<string, Room> NeighborsCache
        {
            get
            {
                if (_neighborsCache == ImmutableDictionary<string, Room>.Empty)
                {
                    _neighborsCache = _neighboursFactory.Value.ToDictionary(x => x.Uid, y => y).AsReadOnly();
                }

                return _neighborsCache;
            }
        }

        public override string ToString() => $"{Uid} [Last move: {LastMotion}] [Persons: {NumberOfPersons}]";

        public bool IsNeighbor(string uid) => NeighborsCache.ContainsKey(uid);

        public IEnumerable<string> Neighbors() => NeighborsCache.Keys.ToList();

        public Room(string uid, Lazy<IEnumerable<Room>> neighbours, string lamp, IConcurrencyProvider concurrencyProvider, ILogger logger,
                    IMessageBroker messageBroker, AreaDescriptor areaDescriptor, MotionConfiguration motionConfiguration)
        {
            Uid = uid ?? throw new ArgumentNullException(nameof(uid));
            _neighboursFactory = neighbours ?? throw new ArgumentNullException(nameof(lamp));
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

            if (_presenceProbability == Probability.Zero)
            {
                _firstEnterTime = motionTime;
            }

            await SetProbability(Probability.Full);
            CheckAutoIncrementForOnePerson(motionTime);

            _turnOffTimeOut.Increment(motionTime);
        }

        /// <summary>
        /// Check if <paramref name="motionVector"/> is vector that turned on the light
        /// </summary>
        /// <param name="motionVector"></param>
        /// <returns></returns>
        public bool IsTurnOnVector(MotionVector motionVector)
        {
            if (!_firstEnterTime.HasValue || _firstEnterTime == motionVector.EndTime)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Update room state on time intervals
        /// </summary>
        /// <returns></returns>
        public async Task PeriodicUpdate(DateTimeOffset motionTime)
        {
            CheckForTurnOnAutomationAgain();
            await RecalculateProbability(motionTime);
        }

        /// <summary>
        /// Marks entrance of last motion vector
        /// </summary>
        /// <param name="vector"></param>
        public void MarkEnter(MotionVector vector)
        {
            IncrementNumberOfPersons(vector.EndTime);
        }

        /// <summary>
        /// Mark some motion that can be enter vector but we are not sure
        /// </summary>
        /// <param name="vector"></param>
        public void MarkConfusion(MotionVector vector)
        {
            _confusingVectors.Add(vector);
        }

        /// <summary>
        /// Executed when after some time we can resolve confused vectors
        /// </summary>
        /// <param name="vector"></param>
        public void ResolveConfusion(MotionVector vector)
        {
            _confusingVectors.TryRemove(vector);
            MarkEnter(vector);
        }

        /// <summary>
        /// Try to resolve confusion in previously marked vectors
        /// </summary>
        /// <param name="currentTime"></param>
        /// <returns></returns>
        public async Task EvaluateConfusions(DateTimeOffset currentTime)
        {
            var confusedVectors = GetConfusionsToResolve(currentTime);

            var uncofused = new List<MotionVector>();
            foreach (var vector in confusedVectors)
            {
                if (NoMoveInStartNeighbors(vector))
                {
                    ResolveConfusion(vector);

                    await GetSourceRoom(vector).MarkLeave(vector);
                    // confused vectors from same time spot should change person probability (but not for 100%)
                }
            }
        }

        /// <summary>
        /// Get list of all confused vectors that should be resolved
        /// </summary>
        /// <param name="dateTimeOffset"></param>
        private IEnumerable<MotionVector> GetConfusionsToResolve(DateTimeOffset currentTime)
        {
            var confusedReadyToResolve = _confusingVectors.Where(t => currentTime.Between(t.EndTime).LastedLongerThen(_motionConfiguration.ConfusionResolutionTime));

            // When all vectors are older then timeout we cannot resolve confusions
            if (!confusedReadyToResolve.Any(vector => currentTime.Between(vector.EndTime).LastedLessThen(_motionConfiguration.ConfusionResolutionTimeOut))) return Enumerable.Empty<MotionVector>();

            return confusedReadyToResolve;
        }

        /// <summary>
        /// Check if there were any moves in neighbors of starting point of <paramref name="vector"/>. This indicates that <paramref name="vector"/> is not confused.
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        private bool NoMoveInStartNeighbors(MotionVector vector)
        {
            var sourceRoom = GetSourceRoom(vector);
            var moveInStartNeighbors = sourceRoom.MoveInNeighborhood(this, vector.StartTime);
            return !moveInStartNeighbors;
        }

        private bool MoveInNeighborhood(Room roomToExclude, DateTimeOffset referenceTime)
        {
            return NeighborsCache.Values.Where(r => r.Uid != roomToExclude.Uid).Any(n => n.LastMotion.Time > referenceTime);
        }

        /// <summary>
        /// Get room pointed by end of the <paramref name="motionVector"/>
        /// </summary>
        /// <param name="motionVector"></param>
        private Room GetTargetRoom(MotionVector motionVector) => NeighborsCache[motionVector.EndPoint];

        /// <summary>
        /// Get room pointed by beginning of the <paramref name="motionVector"/>
        /// </summary>
        /// <param name="motionVector"></param>
        private Room GetSourceRoom(MotionVector motionVector) => NeighborsCache[motionVector.StartPoint];

        /// <summary>
        /// Check if we have any move in room after <paramref name="dateTimeOffset"/>
        /// </summary>
        /// <param name="dateTimeOffset"></param>
        /// <returns></returns>
        public bool HasMove(DateTimeOffset dateTimeOffset) => LastMotion.Time >= dateTimeOffset;

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