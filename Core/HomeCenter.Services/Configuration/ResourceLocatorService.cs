using System.IO;
using HomeCenter.Core.Interface.Native;

namespace HomeCenter.ComponentModel.Configuration
{
    public class ResourceLocatorService : IResourceLocatorService
    {
        private const string AdapterRepositoryName = "Adapters";
        private const string ConfigurationName = "HomeCenterConfiguration.json";
        private readonly INativeStorage _nativeStorage;

        public ResourceLocatorService(INativeStorage nativeStorage)
        {
            _nativeStorage = nativeStorage;
        }

        public string GetRepositoyLocation()
        {
            return Path.Combine(_nativeStorage.LocalFolderPath(), AdapterRepositoryName);
        }

        public string GetConfigurationPath()
        {
            return Path.Combine(_nativeStorage.LocalFolderPath(), ConfigurationName);
        }
    }
}