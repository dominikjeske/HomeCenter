using Windows.Storage;
using HomeCenter.Core.Interface.Native;

namespace HomeCenter.Raspberry
{
    internal class RaspberryStorage : INativeStorage
    {
        public string LocalFolderPath() => ApplicationData.Current.LocalFolder.Path;
    }
}
