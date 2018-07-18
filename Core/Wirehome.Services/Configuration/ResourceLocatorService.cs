using System.IO;
using Wirehome.Core.Interface.Native;

namespace Wirehome.ComponentModel.Configuration
{
    public class ResourceLocatorService : IResourceLocatorService
    {
        private const string AdapterRepositoryName = "Adapters";
        private const string ConfigurationName = "WirehomeConfiguration.json";
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