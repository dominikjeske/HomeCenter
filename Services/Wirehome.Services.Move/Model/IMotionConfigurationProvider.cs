using System;

namespace Wirehome.Motion.Model
{
    public interface IMotionConfigurationProvider
    {
        MotionConfiguration GetConfiguration();
    }
}