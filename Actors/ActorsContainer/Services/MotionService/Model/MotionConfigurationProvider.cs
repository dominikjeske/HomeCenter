namespace HomeCenter.Services.MotionService.Model
{
    public class MotionConfigurationProvider : IMotionConfigurationProvider
    {
        public MotionConfiguration GetConfiguration() => new MotionConfiguration();
    }
}