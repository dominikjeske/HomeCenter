using System;

namespace HomeCenter.Motion.Model
{
    public interface IMotionConfigurationProvider
    {
        MotionConfiguration GetConfiguration();
    }
}