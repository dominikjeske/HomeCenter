using Destructurama.Attributed;
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

        public TimeSpan BaseTimeOut { get; private set; }
        [LogAsScalar]
        public VisitType VisitType { get; private set; } = VisitType.None;
        [LogAsScalar]
        public Probability Probability { get; private set; } = Probability.Zero;
        public DateTimeOffset? FirstEnterTime { get; private set; }
        public int NumberOfPersons { get; private set; }
        [LogAsScalar]
        public MotionStamp LastMotion { get; } = new MotionStamp();
        [LogAsScalar]
        public MotionVector? LastLeaveVector { get; private set; }
        [NotLogged]
        public IObservable<Probability> ProbabilityChange => _probabilitySubject;

        public RoomStatistic(ILogger logger, AreaDescriptor areaDescriptor)
        {
            _logger = logger;
            _areaDescriptor = areaDescriptor;
            BaseTimeOut = _areaDescriptor.TurnOffTimeout;

            Reset();
        }

        public void MarkMotion(DateTimeOffset motionTime)
        {
            _logger.LogInformation(MoveEventId.Motion, "Motion");

            if (Probability == Probability.Zero)
            {
                FirstEnterTime = motionTime;
            }

            TryTuneTurnOffTimeOut(motionTime, Probability);

            SetProbability(Probability.Full);

            LastMotion.SetTime(motionTime);

            UpdateVisitType(motionTime);

            TrySetAtLeastOnePerson(motionTime);
        }

        public void MarkLeave(MotionVector vector)
        {
            DecrementNumberOfPersons();
            LastLeaveVector = vector;

            _logger.LogInformation(MoveEventId.MarkLeave, "Leave");

            // Only when we have one person room we can be sure that we can turn of light immediately
            if (_areaDescriptor.MaxPersonCapacity == 1)
            {
                SetProbability(Probability.Zero);
            }
            else
            {
                var decreasePercent = GetLeaveDeltaProbability();

                _logger.LogInformation(MoveEventId.Probability, "Probability => {percent}%", decreasePercent * 100);

                SetProbability(Probability.DecreaseByPercent(decreasePercent));
            }
        }

        /// <summary>
        /// Decrease probability of person in room after each time interval
        /// </summary>
        public void RecalculateProbability(DateTimeOffset motionTime)
        {
            // When we just have a move in room there is no need for recalculation
            if (motionTime == LastMotion.Value || Probability.IsNoProbability) return;

            var probabilityDelta = GetDeltaProbability();

            _logger.LogDebug(MoveEventId.Probability, "Recalculate with {delta}", probabilityDelta);

            SetProbability(Probability.Decrease(probabilityDelta));
        }

        public void SetAutoTurnOffTime(DateTimeOffset time) => _lastAutoTurnOff = time;

        public void MarkEnter(DateTimeOffset moveTime)
        {
            // When we made auto increment we don't wont to do it twice
            if (!_lastAutoIncrement.HasValue || moveTime.Between(_lastAutoIncrement.Value).LastedLongerThen(TimeSpan.FromMilliseconds(100)))
            {
                NumberOfPersons++;
            }
        }

        public void Reset()
        {
            VisitType = VisitType.None;
            NumberOfPersons = 0;
            FirstEnterTime = null;
        }

        /// <summary>
        /// Increment can move timeout between VisitTypes time zones
        /// </summary>
        private void UpdateVisitType(DateTimeOffset motionTime)
        {
            if (motionTime - FirstEnterTime < _areaDescriptor.Motion.MotionTypePassThru)
            {
                VisitType = VisitType.PassThru;
            }
            else if (motionTime - FirstEnterTime < _areaDescriptor.Motion.MotionTypeShortVisit)
            {
                VisitType = VisitType.ShortVisit;
            }
            else
            {
                VisitType = VisitType.LongerVisit;
            }
        }

        private double GetDeltaProbability()
        {
            // egz. 10s * 1 | 2 | 3 => 10-30s
            var timeout = TimeSpan.FromTicks((int)(BaseTimeOut.Ticks * VisitType.Id));
            // egz. 0.1 - 0.033 (10% - 3%)
            return 1.0 / (timeout.Ticks / _areaDescriptor.Motion.PeriodicCheckTime.Ticks);
        }

        private double GetLeaveDeltaProbability()
        {
            var numberOfPeopleFactor = NumberOfPersons == 0 ? _areaDescriptor.Motion.DecreaseLeavingFactor : _areaDescriptor.Motion.DecreaseLeavingFactor / NumberOfPersons;
            var visitTypeFactor = VisitType.Id;
            var decreasePercent = numberOfPeopleFactor / visitTypeFactor;

            return decreasePercent;
        }

        /// <summary>
        /// Set probability of occurrence of the person in the room
        /// </summary>
        private void SetProbability(Probability probability)
        {
            if (probability == Probability) return;

            var previous = Probability;
            
            Probability = probability;

            _logger.LogDebug(MoveEventId.Probability, "{Previous}", previous);

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
            if (current.IsNoProbability && _lastAutoTurnOff.HasValue && moveTime.Between(_lastAutoTurnOff.Value).LastedLessThen(_areaDescriptor.Motion.MotionTimeWindow))
            {
                var before = BaseTimeOut;
                BaseTimeOut = BaseTimeOut.Increase(_areaDescriptor.Motion.TurnOffTimeoutExtenderFactor);

                _logger.LogInformation(MoveEventId.Tuning, "Turn-off time out updated {before} -> {after}", before, BaseTimeOut);
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