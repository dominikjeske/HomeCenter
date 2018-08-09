using Windows.Storage;
using HomeCenter.Core.Interface.Native;

namespace HomeCenter.Raspberry
{
    internal class RaspberryStorage : IStorage
    {
        public string LocalFolderPath() => ApplicationData.Current.LocalFolder.Path;
    }
}
