using System;

namespace HomeCenter.Services.MotionService.Model
{
    public static class MotionDefaults
    {
        public static readonly TimeSpan MotionTimeWindow = TimeSpan.FromMilliseconds(3000);
        public static readonly TimeSpan ConfusionResolutionTime = TimeSpan.FromMilliseconds(5000);
        public static readonly TimeSpan ConfusionResolutionTimeOut = TimeSpan.FromMilliseconds(10000);
        public static readonly TimeSpan MotionMinDiff = TimeSpan.FromMilliseconds(500);
        public static readonly TimeSpan PeriodicCheckTime = TimeSpan.FromMilliseconds(1000);
        public static readonly double TurnOffTimeoutExtenderFactor = 50;
        public static readonly double DecreaseLeavingFactor = 0.9;
        public static readonly TimeSpan TurnOffTimeOut = TimeSpan.FromMilliseconds(10000);
    }
}