namespace HomeCenter.Model.Configuration
{
    public interface IResourceLocatorService
    {
        string GetConfigurationPath();
        string GetRepositoyLocation();
    }
}