using System;

namespace HomeCenter.Services.MotionService.Model
{
    public record AreaDescriptor
    {
        public static AreaDescriptor Default(MotionConfiguration config, string areaName) => new AreaDescriptor(MotionDefaults.MaxPersonCapacity, MotionDefaults.MotionDetectionAlarmTime, null, MotionDefaults.TurnOffTimeOut, false, 
            config, Model.WorkingTime.AllDay, Model.AreaType.Room, areaName);
        
        public AreaDescriptor(int maxPersonCapacity, TimeSpan motionDetectorAlarmTime, double? lightIntensityAtNight, TimeSpan turnOffTimeout, bool turnOffAutomationDisabled, 
            MotionConfiguration motion, string workingTime, string areaType, string areaName)
        {
            MaxPersonCapacity = maxPersonCapacity;
            MotionDetectorAlarmTime = motionDetectorAlarmTime;
            LightIntensityAtNight = lightIntensityAtNight;
            TurnOffTimeout = turnOffTimeout;
            TurnOffAutomationDisabled = turnOffAutomationDisabled;
            Motion = motion;
            WorkingTime = workingTime;
            AreaType = areaType;
            AreaName = areaName;
        }

        /// <summary>
        /// Automation working profile
        /// </summary>
        public string WorkingTime { get; }

        /// <summary>
        /// How many persons can be at once in single room
        /// </summary>
        public int MaxPersonCapacity { get;  }

        /// <summary>
        /// Type of area - standard, room, passage..
        /// </summary>
        public string AreaType { get; } 

        /// <summary>
        /// How long it takes alarm to be able to detect move after previous alarm state
        /// </summary>
        public TimeSpan MotionDetectorAlarmTime { get;  }

        /// <summary>
        /// When using dimmers we would like to make intensity different at night
        /// </summary>
        public double? LightIntensityAtNight { get;  }

        /// <summary>
        /// Time after which we turn off light without move
        /// </summary>
        public TimeSpan TurnOffTimeout { get;  }

        /// <summary>
        /// When we want only turn on light but not turn off by automation
        /// </summary>
        public bool TurnOffAutomationDisabled { get;  }

        public MotionConfiguration Motion { get;  }

        public string AreaName { get; }

        public AreaDescriptor Copy() => (AreaDescriptor)MemberwiseClone();
    }
}