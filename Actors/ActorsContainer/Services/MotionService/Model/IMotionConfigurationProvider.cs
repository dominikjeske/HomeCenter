using System;

namespace HomeCenter.Services.MotionService.Model
{
    public interface IMotionConfigurationProvider
    {
        MotionConfiguration GetConfiguration();
    }
}