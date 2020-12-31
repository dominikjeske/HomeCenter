using HomeCenter.Abstractions.Extensions;
using HomeCenter.Extensions;
using HomeCenter.Services.MotionService.Model;
using Microsoft.Extensions.Logging;
using System;

namespace HomeCenter.Services.MotionService
{
    internal class RoomStatistic
    {
        public DateTimeOffset? AutomationEnableOn { get; set; }
        public DateTimeOffset? LastAutoIncrement { get; set; }
        public DateTimeOffset? LastAutoTurnOff { get; set; }
        public DateTimeOffset? FirstEnterTime { get; set; }
        public MotionStamp LastMotion { get; } = new MotionStamp();

        public MotionVector? LastLeaveVector { get; set; }

        public Timeout TurnOffTimeOut { get; }

        public int NumberOfPersons { get; private set; }

        private readonly ILogger _logger;
        private readonly string _uid;

        public RoomStatistic(ILogger logger, string uid, TimeSpan baseTime, MotionConfiguration motionConfiguration)
        {
            TurnOffTimeOut = new Timeout(baseTime, motionConfiguration);
            _logger = logger;
            _uid = uid;
        }

        public void UpdateMotion(DateTimeOffset motionTime, Probability current, Probability proposed)
        {
            LastMotion.SetTime(motionTime);
            TurnOffTimeOut.Increment(motionTime);

            TryIncrementPersonsNumber(motionTime);

            TryTuneTurnOffTimeOut(motionTime, current);

            if (current == Probability.Zero && proposed == Probability.Full)
            {
                FirstEnterTime = motionTime;
            }
        }

        /// <summary>
        /// If light was turned off too early area TurnOffTimeout is too low and we have to update it
        /// </summary>
        private void TryTuneTurnOffTimeOut(DateTimeOffset moveTime, Probability current)
        {
            if (current.IsNoProbability)
            {
                (bool result, TimeSpan before, TimeSpan after) = TurnOffTimeOut.TryIncreaseBaseTime(moveTime, LastAutoTurnOff);
                if (result)
                {
                    _logger.LogDeviceEvent(_uid, MoveEventId.Tuning, "Turn-off time out updated {before} -> {after}", before, after);
                }
            }
        }

        /// <summary>
        /// When we don't detect motion vector previously but there is move in room and currently we have 0 person so we know that there is a least one
        /// </summary>
        private void TryIncrementPersonsNumber(DateTimeOffset time)
        {
            if (NumberOfPersons == 0)
            {
                LastAutoIncrement = time;
                NumberOfPersons++;
            }
        }

        public void IncrementNumberOfPersons(DateTimeOffset moveTime)
        {
            if (!LastAutoIncrement.HasValue || moveTime.Between(LastAutoIncrement.Value).LastedLongerThen(TimeSpan.FromMilliseconds(100)))
            {
                NumberOfPersons++;
            }
        }

        public void DecrementNumberOfPersons()
        {
            if (NumberOfPersons > 0)
            {
                NumberOfPersons--;
            }
        }

        public void Reset()
        {
            NumberOfPersons = 0;
            TurnOffTimeOut.Reset();
        }

    }
}