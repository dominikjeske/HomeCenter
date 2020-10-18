using HomeCenter.Services.MotionService.Model;
using System;

namespace HomeCenter.Services.MotionService
{
    internal class RoomStatistic
    {
        public DateTimeOffset? AutomationEnableOn { get; set; }
        public DateTimeOffset? LastAutoIncrement { get; set; }
        public DateTimeOffset? LastAutoTurnOff { get; set; }
        public DateTimeOffset? FirstEnterTime { get; set; }

        public MotionVector? LastLeaveVector { get; set; }
    }
}