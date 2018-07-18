using System;

namespace Wirehome.Motion.Model
{
    public class AreaDescriptor
    {
        public WorkingTime WorkingTime { get; set; }
        public int MaxPersonCapacity { get; set; } = 10;                                                // How many persons can be at once in single room
        public AreaType AreaType { get; set; }
        public TimeSpan MotionDetectorAlarmTime { get; set; } = TimeSpan.FromMilliseconds(2500);        // How long it takes alarm to be able to detect move
        public float? LightIntensityAtNight { get; set; }                                               // When using dimmers we would like to make intensity different at night
        public TimeSpan TurnOffTimeout { get; set; } = TimeSpan.FromMilliseconds(10000);
        public bool TurnOffAutomationDisabled { get; set; }                                             // When we want only turn on light but not turn off by automation
    }
}


