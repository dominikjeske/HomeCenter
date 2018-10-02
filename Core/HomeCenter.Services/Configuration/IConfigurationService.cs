namespace HomeCenter.Services.Configuration
{
    public interface IConfigurationService
    {
        HomeCenterConfiguration ReadConfiguration(AdapterMode adapterMode);
    }
}