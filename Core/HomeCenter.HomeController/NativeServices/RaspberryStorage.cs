using HomeCenter.Model.Native;
using Windows.Storage;

namespace HomeCenter.HomeController.NativeServices
{
    internal class RaspberryStorage : IStorage
    {
        public string LocalFolderPath() => ApplicationData.Current.LocalFolder.Path;
    }
}