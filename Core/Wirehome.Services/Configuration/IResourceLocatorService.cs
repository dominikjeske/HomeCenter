namespace Wirehome.ComponentModel.Configuration
{
    public interface IResourceLocatorService
    {
        string GetConfigurationPath();
        string GetRepositoyLocation();
    }
}