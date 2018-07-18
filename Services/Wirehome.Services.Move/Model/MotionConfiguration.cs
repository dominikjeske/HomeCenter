using System;

namespace Wirehome.Motion.Model
{
    public class MotionConfiguration
    {
        public TimeSpan MotionTimeWindow { get; set; } = TimeSpan.FromMilliseconds(3000);
        public TimeSpan ConfusionResolutionTime { get; set; } = TimeSpan.FromMilliseconds(5000);
        public TimeSpan ConfusionResolutionTimeOut { get; set; } = TimeSpan.FromMilliseconds(10000);
        public TimeSpan MotionMinDiff { get; set; } = TimeSpan.FromMilliseconds(500);  //minimal difference in movement that is possible to do physically
        public TimeSpan PeriodicCheckTime { get; set; } = TimeSpan.FromMilliseconds(1000);
        public TimeSpan ManualCodeWindow { get; set; } = TimeSpan.FromMilliseconds(3000);
        public float TurnOffTimeoutIncrementPercentage { get; set; } = 50;
        public float TurnOffPresenceFactor { get; set; } = 0.05f;
    }
}


