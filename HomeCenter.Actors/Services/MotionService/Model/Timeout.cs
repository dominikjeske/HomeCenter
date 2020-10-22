using System;
using HomeCenter.Extensions;

namespace HomeCenter.Services.MotionService.Model
{
    /// <summary>
    /// Timeout after person inactivity in room
    /// </summary>
    public class Timeout
    {
        private readonly MotionConfiguration _motionConfiguration;

        private TimeSpan _baseTime;
        private DateTimeOffset? _motionStart;

        /// <summary>
        /// Value of timeout
        /// </summary>
        public TimeSpan Value { get; private set; }

        /// <summary>
        /// Type of the visit determinate by time of movement in room
        /// </summary>
        public VisitType VisitType { get; private set; } = VisitType.None;

        /// <summary>
        /// Timeout constructor
        /// </summary>
        /// <param name="baseTime">Base time used to calculate final timeout</param>
        /// <param name="incrementFactor">Factor used to increment <paramref name="baseTime"/></param>
        public Timeout(TimeSpan baseTime, MotionConfiguration motionConfiguration)
        {
            _baseTime = baseTime;
            _motionConfiguration = motionConfiguration;

            Reset();
        }

        public (bool result, TimeSpan before, TimeSpan after) TryIncreaseBaseTime(DateTimeOffset moveTime, DateTimeOffset? lastTurnOffTime)
        {
            if (!lastTurnOffTime.HasValue || !moveTime.Between(lastTurnOffTime.Value).LastedLessThen(_motionConfiguration.MotionTimeWindow)) return (false, _baseTime, _baseTime);

            var before = _baseTime;
            _baseTime = _baseTime.Increase(_motionConfiguration.TurnOffTimeoutExtenderFactor);
            return (true, before, _baseTime);
        }

        /// <summary>
        /// Increment timeout on each move in the room
        /// </summary>
        public void Increment(DateTimeOffset motionTime)
        {
            if (!_motionStart.HasValue)
            {
                _motionStart = motionTime;
            }

            if (motionTime - _motionStart < TimeSpan.FromSeconds(10))
            {
                VisitType = VisitType.PassThru;
            }
            else if (motionTime - _motionStart < TimeSpan.FromSeconds(60))
            {
                VisitType = VisitType.ShortVisit;
            }
            else
            {
                VisitType = VisitType.LongerVisit;
            }

            Value = TimeSpan.FromTicks((int)(_baseTime.Ticks * VisitType.Value));
        }

        public void Reset()
        {
            _motionStart = null;
            Value = _baseTime;
            VisitType = VisitType.None;
        }
    }
}