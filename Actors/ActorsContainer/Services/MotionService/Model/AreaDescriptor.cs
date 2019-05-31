using HomeCenter.Utils.Extensions;
using System;

namespace HomeCenter.Services.MotionService.Model
{
    public class AreaDescriptor
    {
        public static AreaDescriptor Default => new AreaDescriptor();

        public string WorkingTime { get; set; }
        public int MaxPersonCapacity { get; set; }                   // How many persons can be at once in single room
        public string AreaType { get; set; }
        public TimeSpan MotionDetectorAlarmTime { get; set; }        // How long it takes alarm to be able to detect move
        public double? LightIntensityAtNight { get; set; }           // When using dimmers we would like to make intensity different at night
        public TimeSpan TurnOffTimeout { get; set; }
        public bool TurnOffAutomationDisabled { get; set; }          // When we want only turn on light but not turn off by automation

        public AreaDescriptor Clone() => (AreaDescriptor)MemberwiseClone();

    }
}