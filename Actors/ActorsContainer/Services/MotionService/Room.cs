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
        private readonly MotionStamp _lastMotion = new MotionStamp();
        private readonly RoomStatistic _roomStatistic = new RoomStatistic();
        private readonly ConcurrentHashSet<MotionVector> _confusingVectors = new ConcurrentHashSet<MotionVector>();
        private Probability _presenceProbability = Probability.Zero;

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
        public int NumberOfPersons { get; private set; }
        public AreaDescriptor AreaDescriptor { get; }

        public override string ToString() => $"{Uid} [Last move: {_lastMotion}] [Persons: {NumberOfPersons}]";

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

            RegisterChangeStateSource();
        }

        public bool IsNeighbor(string uid) => NeighborsCache.ContainsKey(uid);

        /// <summary>
        /// Take action when there is a move in the room
        /// </summary>
        /// <param name="motionTime"></param>
        /// <returns></returns>
        public async Task MarkMotion(DateTimeOffset motionTime)
        {
            _logger.LogInformation($"[M] [{Uid}] {motionTime.TimeOfDay.TotalMilliseconds}");

            TryTuneTurnOffTimeOut(motionTime);

            _lastMotion.SetTime(motionTime);

            await SetProbability(Probability.Full, motionTime);

            CheckAutoIncrementForOnePerson(motionTime);

            _turnOffTimeOut.Increment(motionTime);
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
        /// Try to resolve confusion in previously marked vectors
        /// </summary>
        /// <param name="currentTime"></param>
        /// <returns></returns>
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
        /// <param name="motionVector"></param>
        /// <returns></returns>
        private Task TryResolveAfterCancel(MotionVector motionVector)
        {
            _logger.LogInformation($"{motionVector} [Cancel]");

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
        /// <param name="currentTime"></param>
        private IEnumerable<MotionVector> GetConfusedVecotrsCanceledByOthers(DateTimeOffset currentTime)
        {
            //!!!!!!TODO
            //&& currentTime.Between(v.EndTime).LastedLongerThen(_motionConfiguration.ConfusionResolutionTime / 2)
            return _confusingVectors.Where(v => GetSourceRoom(v)._roomStatistic.LastLeaveVector?.Start == v.Start );
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
            _roomStatistic.AutomationEnableOn = _concurrencyProvider.Scheduler.Now + time;
        }

        public void Dispose() => _disposeContainer.Dispose();

        /// <summary>
        /// Check if <paramref name="motionVector"/> is vector that turned on the light
        /// </summary>
        /// <param name="motionVector"></param>
        /// <returns></returns>
        private bool IsTurnOnVector(MotionVector motionVector)
        {
            if (!_roomStatistic.FirstEnterTime.HasValue || _roomStatistic.FirstEnterTime == motionVector.EndTime)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Marks enter to target room and leave from source room
        /// </summary>
        /// <param name="motionVector"></param>
        /// <returns></returns>
        private async Task MarkVector(MotionVector motionVector, bool resolved)
        {
            _logger.LogInformation($"{motionVector} {(resolved ? " [Resolved]" : " [OK]")}");
            
            await GetSourceRoom(motionVector).MarkLeave(motionVector);
            MarkEnter(motionVector);
        }

        /// <summary>
        /// Marks entrance of last motion vector
        /// </summary>
        /// <param name="vector"></param>
        private void MarkEnter(MotionVector vector)
        {
            IncrementNumberOfPersons(vector.EndTime);
        }

        /// <summary>
        /// Mark some motion that can be enter vector but we are not sure
        /// </summary>
        /// <param name="vector"></param>
        private void MarkConfusion(MotionVector vector)
        {
            _logger.LogInformation($"{vector} [Confused]");

            _confusingVectors.Add(vector);
        }

        /// <summary>
        /// Get list of all confused vectors that should be resolved
        /// </summary>
        /// <param name="dateTimeOffset">We get all vectors before this time</param>
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
        /// <param name="vector"></param>
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
        /// <param name="vector"></param>
        /// <returns></returns>
        private bool NoMoveInStartNeighbors(MotionVector vector)
        {
            var sourceRoom = GetSourceRoom(vector);
            var moveInStartNeighbors = sourceRoom.MoveInNeighborhood(this, vector.StartTime);
            return !moveInStartNeighbors;
        }

        /// <summary>
        /// Checks if there was any move in current room and all neighbors excluding <paramref name="roomToExclude"/> after <paramref name="referenceTime"/>
        /// </summary>
        /// <param name="roomToExclude"></param>
        /// <param name="referenceTime"></param>
        /// <returns></returns>
        private bool MoveInNeighborhood(Room roomToExclude, DateTimeOffset referenceTime)
        {
            return NeighborsCache.Values.Where(r => r.Uid != roomToExclude.Uid).Any(n => n._lastMotion.Time > referenceTime) || _lastMotion.Time > referenceTime;
        }

        /// <summary>
        /// Get room pointed by beginning of the <paramref name="motionVector"/>
        /// </summary>
        /// <param name="motionVector"></param>
        private Room GetSourceRoom(MotionVector motionVector) => NeighborsCache[motionVector.StartPoint];

        private async Task MarkLeave(MotionVector vector)
        {
            DecrementNumberOfPersons();

            _roomStatistic.LastLeaveVector = vector;

            // Only when we have one person room we can be sure that we can turn of light immediately
            if (AreaDescriptor.MaxPersonCapacity == 1)
            {
                await SetProbability(Probability.Zero, vector.EndTime);
            }
            else
            {
                var numberOfPeopleFactor = NumberOfPersons == 0 ? _motionConfiguration.DecreaseLeavingFactor : _motionConfiguration.DecreaseLeavingFactor / NumberOfPersons;
                var visitTypeFactor = _turnOffTimeOut.VisitType.Value;
                var decreasePercent = numberOfPeopleFactor / visitTypeFactor;

                _logger.LogInformation($"[PD] {Uid} => {(decreasePercent * 100)}%");

                await SetProbability(_presenceProbability.DecreaseByPercent(decreasePercent), vector.EndTime);
            }
        }

        /// <summary>
        /// If light is turned off too early area TurnOffTimeout is too low and we have to update it
        /// </summary>
        /// <param name="moveTime"></param>
        private void TryTuneTurnOffTimeOut(DateTimeOffset moveTime)
        {
            if (_presenceProbability == Probability.Zero)
            {
                (bool result, TimeSpan before, TimeSpan after) = _turnOffTimeOut.TryIncreaseBaseTime(moveTime, _roomStatistic.LastAutoTurnOff);
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
            if (motionTime == _lastMotion.Time) return;

            var probabilityDelta = 1.0 / (_turnOffTimeOut.Value.Ticks / _motionConfiguration.PeriodicCheckTime.Ticks);

            await SetProbability(_presenceProbability.Decrease(probabilityDelta), motionTime);
        }

        /// <summary>
        /// Set probability of occurrence of the person in the room
        /// </summary>
        /// <param name="probability"></param>
        /// <returns></returns>
        private async Task SetProbability(Probability probability, DateTimeOffset time)
        {
            if (probability == _presenceProbability) return;

            // When we change from zero to full
            if (_presenceProbability == Probability.Zero && probability == Probability.Full)
            {
                _roomStatistic.FirstEnterTime = time;
            }

            _logger.LogInformation($"[P] {Uid} {(probability.Value*100):00.00}% [{_turnOffTimeOut.Value}]");

            _presenceProbability = probability;

            if (_presenceProbability.IsFullProbability)
            {
                await TryTurnOnLamp();
            }
            else if (_presenceProbability.IsNoProbability)
            {
                await TryTurnOffLamp();

               // await NeighborsCache.Select(n => n.Value.EvaluateConfusions()).WhenAll();
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
                _roomStatistic.LastAutoIncrement = time;
                NumberOfPersons++;
            }
        }

        //TODO maybe do it differently
        private void CheckForTurnOnAutomationAgain()
        {
            if (AutomationDisabled && _concurrencyProvider.Scheduler.Now > _roomStatistic.AutomationEnableOn)
            {
                EnableAutomation();
            }
        }

        private void IncrementNumberOfPersons(DateTimeOffset moveTime)
        {
            if (!_roomStatistic.LastAutoIncrement.HasValue || moveTime.Between(_roomStatistic.LastAutoIncrement.Value).LastedLongerThen(TimeSpan.FromMilliseconds(100)))
            {
                NumberOfPersons++;
            }
        }

        private void DecrementNumberOfPersons()
        {
            if (NumberOfPersons > 0)
            {
                NumberOfPersons--;
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
                _logger.LogInformation($"[OFF] [{Uid}]");

                _messageBroker.Send(new TurnOffCommand(), _lamp);

                _roomStatistic.LastAutoTurnOff = _concurrencyProvider.Scheduler.Now;
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