using System;

namespace HomeCenter.Services.MotionService.Model
{
    public class AreaDescriptor
    {
        public static AreaDescriptor Default => new AreaDescriptor();

        public string WorkingTime { get; set; }

        /// <summary>
        /// How many persons can be at once in single room
        /// </summary>
        public int MaxPersonCapacity { get; set; }

        public string AreaType { get; set; }

        /// <summary>
        /// How long it takes alarm to be able to detect move
        /// </summary>
        public TimeSpan MotionDetectorAlarmTime { get; set; }

        /// <summary>
        /// When using dimmers we would like to make intensity different at night
        /// </summary>
        public double? LightIntensityAtNight { get; set; }

        public TimeSpan TurnOffTimeout { get; set; }

        /// <summary>
        /// When we want only turn on light but not turn off by automation
        /// </summary>
        public bool TurnOffAutomationDisabled { get; set; }

        public AreaDescriptor Clone() => (AreaDescriptor)MemberwiseClone();
    }
}