using HomeCenter.Extensions;
using HomeCenter.Services.MotionService.Model;
using Microsoft.Extensions.Logging;
using System;
using System.Reactive.Subjects;

namespace HomeCenter.Services.MotionService
{
    internal class RoomStatistic
    {
        private readonly ILogger _logger;
        private readonly AreaDescriptor _areaDescriptor;
        private readonly Subject<Probability> _probabilitySubject = new();

        private DateTimeOffset? _lastAutoIncrement;
        private DateTimeOffset? _lastAutoTurnOff;
        private TimeSpan _baseTime;
        private DateTimeOffset? _motionStart;
        private Probability _probability = Probability.Zero;

        public DateTimeOffset? FirstEnterTime { get; private set; }
        public int NumberOfPersons { get; private set; }
        public TimeSpan Timeout { get; private set; }
        public VisitType VisitType { get; private set; } = VisitType.None;
        public MotionStamp LastMotion { get; } = new MotionStamp();
        public MotionVector? LastLeaveVector { get; private set; }
        public IObservable<Probability> ProbabilityChange => _probabilitySubject;

        public RoomStatistic(ILogger logger, AreaDescriptor areaDescriptor)
        {
            _logger = logger;
            _areaDescriptor = areaDescriptor;
            _baseTime = _areaDescriptor.TurnOffTimeout;

            Reset();
        }

        public void MarkMotion(DateTimeOffset motionTime)
        {
            _logger.LogDebug(MoveEventId.Motion, "Motion");

            if (_probability == Probability.Zero)
            {
                FirstEnterTime = motionTime;
            }

            SetProbability(Probability.Full);

            LastMotion.SetTime(motionTime);

            IncrementTimeout(motionTime);

            TrySetAtLeastOnePerson(motionTime);

            TryTuneTurnOffTimeOut(motionTime, _probability);
        }

        public void MarkLeave(MotionVector vector)
        {
            DecrementNumberOfPersons();
            LastLeaveVector = vector;

            _logger.LogDebug(MoveEventId.MarkLeave, "Leave");

            // Only when we have one person room we can be sure that we can turn of light immediately
            if (_areaDescriptor.MaxPersonCapacity == 1)
            {
                SetProbability(Probability.Zero);
            }
            else
            {
                var decreasePercent = GetLeaveDeltaProbability();

                _logger.LogDebug(MoveEventId.Probability, "Probability => {percent}%", decreasePercent * 100);

                SetProbability(_probability.DecreaseByPercent(decreasePercent));
            }
        }

        /// <summary>
        /// Decrease probability of person in room after each time interval
        /// </summary>
        public void RecalculateProbability(DateTimeOffset motionTime)
        {
            // When we just have a move in room there is no need for recalculation
            if (motionTime == LastMotion.Time || _probability.IsNoProbability) return;

            var probabilityDelta = GetDeltaProbability();

            _logger.LogDebug(MoveEventId.Probability, "Recalculate with {delta} from {probability:00.00}%", probabilityDelta, _probability.Value * 100);

            SetProbability(_probability.Decrease(probabilityDelta));
        }

        public void SetAutoTurnOffTime(DateTimeOffset time) => _lastAutoTurnOff = time;

        public void TryIncrementNumberOfPersons(DateTimeOffset moveTime)
        {
            if (!_lastAutoIncrement.HasValue || moveTime.Between(_lastAutoIncrement.Value).LastedLongerThen(TimeSpan.FromMilliseconds(100)))
            {
                NumberOfPersons++;
            }
        }

        public void Reset()
        {
            _motionStart = null;
            Timeout = _baseTime;
            VisitType = VisitType.None;
            NumberOfPersons = 0;
        }

        /// <summary>
        /// Increment can move timeout between VisitTypes time zones
        /// </summary>
        private void IncrementTimeout(DateTimeOffset motionTime)
        {
            if (!_motionStart.HasValue)
            {
                _motionStart = motionTime;
            }

            if (motionTime - _motionStart < _areaDescriptor.Motion.MotionTypePassThru)
            {
                VisitType = VisitType.PassThru;
            }
            else if (motionTime - _motionStart < _areaDescriptor.Motion.MotionTypeShortVisit)
            {
                VisitType = VisitType.ShortVisit;
            }
            else
            {
                VisitType = VisitType.LongerVisit;
            }

            // egz. 10s * 1 | 2 | 3
            Timeout = TimeSpan.FromTicks((int)(_baseTime.Ticks * VisitType.Value));
        }

        private double GetDeltaProbability()
        {
            // For TurnOffTimeOut = 30s and PeriodicCheckTime = 1s => 0.03 (30 seconds to full probability)
            return 1.0 / (Timeout.Ticks / _areaDescriptor.Motion.PeriodicCheckTime.Ticks);
        }

        private bool TryIncreaseBaseTime(DateTimeOffset moveTime, DateTimeOffset? lastTurnOffTime)
        {
            if (!lastTurnOffTime.HasValue || !moveTime.Between(lastTurnOffTime.Value).LastedLessThen(_areaDescriptor.Motion.MotionTimeWindow)) return false;

            var before = _baseTime;
            _baseTime = _baseTime.Increase(_areaDescriptor.Motion.TurnOffTimeoutExtenderFactor);

            _logger.LogDebug(MoveEventId.Tuning, "Turn-off time out updated {before} -> {after}", before, _baseTime);

            return true;
        }

        private double GetLeaveDeltaProbability()
        {
            var numberOfPeopleFactor = NumberOfPersons == 0 ? _areaDescriptor.Motion.DecreaseLeavingFactor : _areaDescriptor.Motion.DecreaseLeavingFactor / NumberOfPersons;
            var visitTypeFactor = VisitType.Value;
            var decreasePercent = numberOfPeopleFactor / visitTypeFactor;

            return decreasePercent;
        }

        /// <summary>
        /// Set probability of occurrence of the person in the room
        /// </summary>
        private void SetProbability(Probability probability)
        {
            if (probability == _probability) return;

            _logger.LogDebug(MoveEventId.Probability, "{probability:00.00}% [{timeout}]", probability.Value * 100, Timeout);

            _probability = probability;

            _probabilitySubject.OnNext(probability);
        }

        private void DecrementNumberOfPersons()
        {
            if (NumberOfPersons > 0)
            {
                NumberOfPersons--;
            }
        }

        /// <summary>
        /// If light was turned off too early area TurnOffTimeout is too low and we have to update it
        /// </summary>
        private void TryTuneTurnOffTimeOut(DateTimeOffset moveTime, Probability current)
        {
            if (current.IsNoProbability)
            {
                TryIncreaseBaseTime(moveTime, _lastAutoTurnOff);
            }
        }

        /// <summary>
        /// When we don't detect motion vector previously but there is move in room and currently we have 0 person so we know that there is a least one
        /// </summary>
        private void TrySetAtLeastOnePerson(DateTimeOffset time)
        {
            if (NumberOfPersons == 0)
            {
                _lastAutoIncrement = time;
                NumberOfPersons++;
            }
        }
    }
}