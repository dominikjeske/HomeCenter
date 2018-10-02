using Windows.Storage;

namespace HomeCenter.Controller.NativeServices
{
    internal class RaspberryStorage : IStorage
    {
        public string LocalFolderPath() => ApplicationData.Current.LocalFolder.Path;
    }
}