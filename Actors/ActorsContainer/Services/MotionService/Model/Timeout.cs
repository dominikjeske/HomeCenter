using HomeCenter.Utils.Extensions;
using System;

namespace HomeCenter.Services.MotionService.Model
{
    /// <summary>
    /// Timeout after person inactivity in room
    /// </summary>
    public class Timeout
    {
        private TimeSpan _baseTime;
        private TimeSpan _currentExtension = TimeSpan.FromTicks(0);
        private int _counter;
        private readonly MotionConfiguration _motionConfiguration;

        /// <summary>
        /// Value of timeout
        /// </summary>
        public TimeSpan Value => _baseTime + _currentExtension;

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
        public void Increment()
        {
            _counter++;
            var factor = _counter * _motionConfiguration.TurnOffTimeoutIncrementFactor;
            var value = TimeSpan.FromTicks((long)(Value.Ticks * factor));
            _currentExtension = _currentExtension.Add(value);
        }

        public void Reset()
        {
            _currentExtension = TimeSpan.FromTicks(0);
            _counter = 0;
        }
    }
}