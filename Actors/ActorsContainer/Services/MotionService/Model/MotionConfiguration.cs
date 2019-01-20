using System;

namespace HomeCenter.Services.MotionService
{
    internal class MotionConfiguration
    {
        public TimeSpan MotionTimeWindow { get; set; }
        public TimeSpan ConfusionResolutionTime { get; set; }
        public TimeSpan ConfusionResolutionTimeOut { get; set; }
        public TimeSpan MotionMinDiff { get; set; }  //minimal difference in movement that is possible to do physically
        public TimeSpan PeriodicCheckTime { get; set; }
        public TimeSpan ManualCodeWindow { get; set; }
        public double TurnOffTimeoutIncrementPercentage { get; set; }
        public double TurnOffPresenceFactor { get; set; }
    }
}