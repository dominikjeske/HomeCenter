using System;

namespace HomeCenter.Motion.Model
{
    public class MotionConfigurationProvider : IMotionConfigurationProvider
    {
        public MotionConfiguration GetConfiguration() => new MotionConfiguration();
    }
}


