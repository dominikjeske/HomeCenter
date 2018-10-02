namespace HomeCenter.Services.Configuration
{
    public interface IResourceLocatorService
    {
        string GetConfigurationPath();

        string GetRepositoyLocation();
    }
}