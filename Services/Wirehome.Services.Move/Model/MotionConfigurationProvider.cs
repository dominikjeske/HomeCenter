using System;

namespace Wirehome.Motion.Model
{
    public class MotionConfigurationProvider : IMotionConfigurationProvider
    {
        public MotionConfiguration GetConfiguration() => new MotionConfiguration();
    }
}


