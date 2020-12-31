using ConcurrentCollections;
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

        private readonly RoomStatistic _roomStatistic;
        private readonly ConcurrentHashSet<MotionVector> _confusingVectors = new ConcurrentHashSet<MotionVector>();
        private Probability _currentProbability = Probability.Zero;

        private IReadOnlyDictionary<string, Room> _neighborsCache = ImmutableDictionary<string, Room>.Empty;

        private IReadOnlyDictionary<string, Room> NeighborsCache
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

        public string Uid { get; }
        public bool AutomationDisabled { get; private set; }
        public int NumberOfPersons => _roomStatistic.NumberOfPersons;
        public AreaDescriptor AreaDescriptor { get; }

        public override string ToString() => $"{Uid} [Last move: {_roomStatistic.LastMotion}] [Persons: {NumberOfPersons}]";

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

            _roomStatistic = new RoomStatistic(_logger, Uid, AreaDescriptor.TurnOffTimeout, _motionConfiguration);

            RegisterChangeStateSource();
        }

        private void RegisterChangeStateSource()
        {
            //TODO
            //_disposeContainer.Add(Lamp.PowerStateChange.Subscribe(PowerStateChangeHandler));
        }

        public bool IsNeighbor(string uid) => NeighborsCache.ContainsKey(uid);

        /// <summary>
        /// Take action when there is a move in the room
        /// </summary>
        public async Task MarkMotion(DateTimeOffset motionTime)
        {
            _logger.LogDeviceEvent(Uid, MoveEventId.Motion, "Motion at {motionTime}", motionTime);

            _roomStatistic.UpdateMotion(motionTime, _currentProbability, Probability.Full);

            await SetProbability(Probability.Full);
        }

        /// <summary>
        /// Update room state on time intervals
        /// </summary>
        public async Task PeriodicUpdate(DateTimeOffset motionTime)
        {
            CheckForTurnOnAutomationAgain();
            await RecalculateProbability(motionTime);
        }

        /// <summary>
        /// Try to resolve confusion in previously marked vectors
        /// </summary>
        public async Task EvaluateConfusions(DateTimeOffset currentTime)
        {
            await GetConfusedVectorsAfterTimeout(currentTime).Where(vector => NoMoveInStartNeighbors(vector))
                                                             .Select(v => ResolveConfusion(v))
                                                             .WhenAll();

            await GetConfusedVecotrsCanceledByOthers(currentTime).Select(v => TryResolveAfterCancel(v))
                                                                 .WhenAll();
        }

        /// <summary>
        /// After we remove canceled vector we check if there is other vector in same time that was in confusion. When there is only one we can resolve it because there is no confusion anymore
        /// </summary>
        private Task TryResolveAfterCancel(MotionVector motionVector)
        {
            _logger.LogDeviceEvent(Uid, MoveEventId.VectorCancel, "{motionVector} [Cancel]", motionVector);

            RemoveConfusedVector(motionVector);

            var confused = _confusingVectors.Where(x => x.End == motionVector.End);
            if (confused.Count() == 1)
            {
                return ResolveConfusion(confused.Single());
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// When there is approved leave vector from one of the source rooms we have confused vectors in same time we can assume that our vector is not real and we can remove it in shorter time
        /// </summary>
        private IEnumerable<MotionVector> GetConfusedVecotrsCanceledByOthers(DateTimeOffset currentTime)
        {
            //!!!!!!TODO
            //&& currentTime.Between(v.EndTime).LastedLongerThen(_motionConfiguration.ConfusionResolutionTime / 2)
            return _confusingVectors.Where(v => GetSourceRoom(v)._roomStatistic.LastLeaveVector?.Start == v.Start);
        }

        public async Task HandleVectors(IList<MotionVector> motionVectors)
        {
            if (motionVectors.Count == 0) return;

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
                    MarkConfusion(vector);
                }
            }
            // When we have at least two vectors we know that this vector is confused
            else
            {
                motionVectors.ForEach(vector => MarkConfusion(vector));
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
                _roomStatistic.AutomationEnableOn = _concurrencyProvider.Scheduler.Now + time;
            }
        }

        public void Dispose() => _disposeContainer.Dispose();

        /// <summary>
        /// Check if <paramref name="motionVector"/> is vector that turned on the light
        /// </summary>
        private bool IsTurnOnVector(MotionVector motionVector)
        {
            return !_roomStatistic.FirstEnterTime.HasValue || _roomStatistic.FirstEnterTime == motionVector.EndTime;
        }

        /// <summary>
        /// Marks enter to target room and leave from source room
        /// </summary>
        private async Task MarkVector(MotionVector motionVector, bool resolved)
        {
            _logger.LogDeviceEvent(Uid, MoveEventId.MarkVector, "Vector {motionVector} ({resolved})", motionVector, resolved ? " [Resolved]" : "[OK]");

            await GetSourceRoom(motionVector).MarkLeave(motionVector);
            MarkEnter(motionVector);
        }

        /// <summary>
        /// Marks entrance of last motion vector
        /// </summary>
        private void MarkEnter(MotionVector vector) => _roomStatistic.IncrementNumberOfPersons(vector.EndTime);

        /// <summary>
        /// Mark some motion that can be enter vector but we are not sure
        /// </summary>
        private void MarkConfusion(MotionVector vector)
        {
            _logger.LogDeviceEvent(Uid, MoveEventId.ConfusedVector, "Confused vector {vector}", vector);

            _confusingVectors.Add(vector);
        }

        /// <summary>
        /// Get list of all confused vectors that should be resolved
        /// </summary>
        private IEnumerable<MotionVector> GetConfusedVectorsAfterTimeout(DateTimeOffset currentTime)
        {
            var confusedReadyToResolve = _confusingVectors.Where(t => currentTime.Between(t.EndTime).LastedLongerThen(_motionConfiguration.ConfusionResolutionTime));

            // When all vectors are older then timeout we cannot resolve confusions
            if (!confusedReadyToResolve.Any(vector => currentTime.Between(vector.EndTime).LastedLessThen(_motionConfiguration.ConfusionResolutionTimeOut))) return Enumerable.Empty<MotionVector>();

            return confusedReadyToResolve;
        }

        /// <summary>
        /// Executed when after some time we can resolve confused vectors
        /// </summary>
        private async Task ResolveConfusion(MotionVector vector)
        {
            RemoveConfusedVector(vector);

            await MarkVector(vector, true);
        }

        private void RemoveConfusedVector(MotionVector vector)
        {
            _confusingVectors.TryRemove(vector);
        }

        /// <summary>
        /// Check if there were any moves in neighbors of starting point of <paramref name="vector"/>. This indicates that <paramref name="vector"/> is not confused.
        /// </summary>
        private bool NoMoveInStartNeighbors(MotionVector vector)
        {
            var sourceRoom = GetSourceRoom(vector);
            var moveInStartNeighbors = sourceRoom.MoveInNeighborhood(this, vector.StartTime);
            return !moveInStartNeighbors;
        }

        /// <summary>
        /// Checks if there was any move in current room and all neighbors excluding <paramref name="roomToExclude"/> after <paramref name="referenceTime"/>
        /// </summary>
        private bool MoveInNeighborhood(Room roomToExclude, DateTimeOffset referenceTime)
        {
            return NeighborsCache.Values.Where(r => r.Uid != roomToExclude.Uid).Any(n => n._roomStatistic.LastMotion.Time > referenceTime) || _roomStatistic.LastMotion.Time > referenceTime;
        }

        /// <summary>
        /// Get room pointed by beginning of the <paramref name="motionVector"/>
        /// </summary>
        private Room GetSourceRoom(MotionVector motionVector) => NeighborsCache[motionVector.StartPoint];

        private async Task MarkLeave(MotionVector vector)
        {
            _roomStatistic.DecrementNumberOfPersons();

            _roomStatistic.LastLeaveVector = vector;

            // Only when we have one person room we can be sure that we can turn of light immediately
            if (AreaDescriptor.MaxPersonCapacity == 1)
            {
                await SetProbability(Probability.Zero);
            }
            else
            {
                var numberOfPeopleFactor = NumberOfPersons == 0 ? _motionConfiguration.DecreaseLeavingFactor : _motionConfiguration.DecreaseLeavingFactor / NumberOfPersons;
                var visitTypeFactor = _roomStatistic.TurnOffTimeOut.VisitType.Value;
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
            if (motionTime == _roomStatistic.LastMotion.Time) return;

            var probabilityDelta = 1.0 / (_roomStatistic.TurnOffTimeOut.Value.Ticks / _motionConfiguration.PeriodicCheckTime.Ticks);

            await SetProbability(_currentProbability.Decrease(probabilityDelta));
        }

        /// <summary>
        /// Set probability of occurrence of the person in the room
        /// </summary>
        private async Task SetProbability(Probability probability)
        {
            if (probability == _currentProbability) return;

            _logger.LogDeviceEvent(Uid, MoveEventId.Probability, "{probability:00.00}% [{timeout}]", probability.Value * 100, _roomStatistic.TurnOffTimeOut.Value);

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
                _roomStatistic.Reset();
            }

            _logger.LogDeviceEvent(Uid, MoveEventId.PowerState, "{newState} | Source: {source}", powerChangeEvent.Value, powerChangeEvent.EventTriggerSource);
        }

        

        //TODO maybe do it differently
        private void CheckForTurnOnAutomationAgain()
        {
            if (AutomationDisabled && _concurrencyProvider.Scheduler.Now > _roomStatistic.AutomationEnableOn)
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

                _roomStatistic.LastAutoTurnOff = _concurrencyProvider.Scheduler.Now;
            }
        }
    }
}