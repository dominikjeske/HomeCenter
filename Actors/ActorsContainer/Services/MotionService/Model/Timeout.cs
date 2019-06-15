using HomeCenter.Utils.Extensions;
using System;

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
        private TimeSpan _timeout;

        /// <summary>
        /// Value of timeout
        /// </summary>
        public TimeSpan Value => _timeout;

        /// <summary>
        /// Timeout constructor
        /// </summary>
        /// <param name="baseTime">Base time used to calculate final timeout</param>
        /// <param name="incrementFactor">Factor used to increment <paramref name="baseTime"/></param>
        public Timeout(TimeSpan baseTime, MotionConfiguration motionConfiguration)
        {
            _baseTime = baseTime;
            _motionConfiguration = motionConfiguration;
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
            // Start of the motion
            if(!_motionStart.HasValue)
            {
                _motionStart = motionTime;
                _timeout = _baseTime;

                return;
            }

            // This can be accidencial or passthru so we keep base timeout
            if(motionTime - _motionStart < TimeSpan.FromSeconds(10))
            {
                _timeout = _baseTime;
                return;
            }

            // Short visit
            if(motionTime - _motionStart < TimeSpan.FromSeconds(60))
            {
                _timeout = TimeSpan.FromTicks( _baseTime.Ticks * 2);
                return;
            }

            // longer visit
            _timeout = TimeSpan.FromTicks(_baseTime.Ticks * 3);
        }

        public void Reset()
        {
            _motionStart = null;
        }
    }
}