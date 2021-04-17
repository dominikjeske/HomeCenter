using System;

namespace HomeCenter.Services.MotionService
{
    /// <summary>
    /// Parameters that determinate how motion service is working.
    /// </summary>
    public record MotionConfiguration
    {
        /// <summary>
        /// Time in which we analyze move to find potential motion vectors.
        /// </summary>
        public TimeSpan MotionTimeWindow { get; init; }

        /// <summary>
        /// Minimal difference in movement that is possible to do physically.
        /// </summary>
        public TimeSpan MotionMinDiff { get; init; }

        /// <summary>
        /// Value by which we will increase area turn off timeout when it is turned on too quick.
        /// </summary>
        public double TurnOffTimeoutExtenderFactor { get; init; }

        /// <summary>
        /// Time interval for periodic move calculations.
        /// </summary>
        public TimeSpan PeriodicCheckTime { get; init; }

        /// <summary>
        /// Time after confused vector cannot be unconfused.
        /// </summary>
        public TimeSpan ConfusionResolutionTimeOut { get; init; }

        /// <summary>
        /// Time after which we can try to unconfuse vector that was uncertain before.
        /// </summary>
        public TimeSpan ConfusionResolutionTime { get; init; }

        /// <summary>
        /// Number by which we calculate decreasing probability when somebody leaves room.
        /// </summary>
        public double DecreaseLeavingFactor { get; init; }

        /// <summary>
        /// Time after light will be turned on.
        /// </summary>
        public TimeSpan TurnOffTimeout { get; init; }

        public TimeSpan MotionTypePassThru { get; init; }

        public TimeSpan MotionTypeShortVisit { get; init; }
    }
}