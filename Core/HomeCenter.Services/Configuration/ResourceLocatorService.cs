using HomeCenter.Model.Native;
using HomeCenter.Services.Controllers;
using System.IO;

namespace HomeCenter.Services.Configuration
{
    public class ResourceLocatorService : IResourceLocatorService
    {
        private readonly IStorage _nativeStorage;
        private readonly ControllerOptions _controllerOptions;

        public ResourceLocatorService(IStorage nativeStorage, ControllerOptions controllerOptions)
        {
            _nativeStorage = nativeStorage;
            _controllerOptions = controllerOptions;
        }

        public string GetRepositoyLocation()
        {
            return Path.Combine(_nativeStorage.LocalFolderPath(), _controllerOptions.AdapterRepoName);
        }

        public string GetConfigurationPath()
        {
            return Path.Combine(_nativeStorage.LocalFolderPath(), _controllerOptions.Configuration);
        }
    }
}