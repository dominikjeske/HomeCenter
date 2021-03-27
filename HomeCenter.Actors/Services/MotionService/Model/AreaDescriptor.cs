using System;

namespace HomeCenter.Services.MotionService.Model
{
    public record AreaDescriptor
    {
        public static AreaDescriptor Default => new AreaDescriptor();

        /// <summary>
        /// Automation working profile
        /// </summary>
        public string WorkingTime { get; init; } = Model.WorkingTime.AllDay;

        /// <summary>
        /// How many persons can be at once in single room
        /// </summary>
        public int MaxPersonCapacity { get; init; }

        /// <summary>
        /// Type of area - standard, room, passage..
        /// </summary>
        public string AreaType { get; init; } = Model.AreaType.Room;

        /// <summary>
        /// How long it takes alarm to be able to detect move after previous alarm state
        /// </summary>
        public TimeSpan MotionDetectorAlarmTime { get; init; }

        /// <summary>
        /// When using dimmers we would like to make intensity different at night
        /// </summary>
        public double? LightIntensityAtNight { get; init; }

        /// <summary>
        /// Time after which we turn off light without move
        /// </summary>
        public TimeSpan TurnOffTimeout { get; init; }

        /// <summary>
        /// When we want only turn on light but not turn off by automation
        /// </summary>
        public bool TurnOffAutomationDisabled { get; init; }

        public AreaDescriptor Copy() => (AreaDescriptor)MemberwiseClone();

        public MotionConfiguration Motion { get; init; }
    }
}