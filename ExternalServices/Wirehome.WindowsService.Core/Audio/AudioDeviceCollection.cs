using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Wirehome.WindowsService.Interop;

namespace Wirehome.WindowsService.Audio
{
    public class AudioDeviceCollection : IEnumerable<AudioDevice>
    {
        private readonly IMMDeviceCollection _underlyingCollection;

        internal AudioDeviceCollection(IMMDeviceCollection parent)
        {
            _underlyingCollection = parent;
        }

        public int Count
        {
            get
            {
                int result;
                Marshal.ThrowExceptionForHR(_underlyingCollection.GetCount(out result));
                return result;
            }
        }

        public AudioDevice this[int index]
        {
            get
            {
                IMMDevice underlyingDevice;
                Marshal.ThrowExceptionForHR(_underlyingCollection.Item(index, out underlyingDevice));
                
                return new AudioDevice(underlyingDevice);
            }
        }

        public IEnumerator<AudioDevice> GetEnumerator()
        {
            int count = Count;
            for (int index = 0; index < count; index++)
            {
                yield return this[index];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
