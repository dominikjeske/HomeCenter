using System.Runtime.InteropServices;
using Wirehome.WindowsService.Interop;

namespace Wirehome.WindowsService.Audio
{
    public class AudioDevice
    {
        private readonly IMMDevice _underlyingDevice;
        private PropertyStore _propertyStore;

        internal AudioDevice(IMMDevice underlyingDevice)
        {
            _underlyingDevice = underlyingDevice;
        }

        public bool IsActive
        {
            get { return State == AudioDeviceState.Active; }
        }

        public PropertyStore Properties
        {
            get
            {
                if (_propertyStore == null)
                    _propertyStore = OpenPropertyStore();

                return _propertyStore;
            }
        }

        public bool TryGetDeviceDescription(out string result)
        {
            object value;
            if (Properties.TryGetValue(PropertyKeys.PKEY_Device_DeviceDesc, out value))
            {
                result = (string)value;
                return true;
            }

            result = string.Empty;
            return false;
        }

        public bool TryGetFriendlyName(out string result)
        {
            object value;
            if (Properties.TryGetValue(PropertyKeys.PKEY_Device_FriendlyName, out value))
            {
                result = (string)value;
                return true;
            }

            result = string.Empty;
            return false;
        }

        public bool TryDeviceFriendlyName(out string result)
        {
            object value;
            if (Properties.TryGetValue(PropertyKeys.PKEY_DeviceInterface_FriendlyName, out value))
            {
                result = (string)value;
                return true;
            }

            result = string.Empty;
            return false;
        }

        public bool TryGetDeviceClassIconPath(out string result)
        {
            object value;
            if (Properties.TryGetValue(PropertyKeys.PKEY_DeviceClass_IconPath, out value))
            {
                result = (string)value;
                return true;
            }

            result = string.Empty;
            return false;
        }

        public string Id
        {
            get
            {
                string result;
                Marshal.ThrowExceptionForHR(_underlyingDevice.GetId(out result));
                return result;
            }
        }

        public AudioDeviceKind Kind
        {
            get
            {
                AudioDeviceKind result;
                var ep = (IMMEndpoint)_underlyingDevice;
                ep.GetDataFlow(out result);
                return result;
            }
        }

        public AudioDeviceState State
        {
            get
            {
                AudioDeviceState result;
                Marshal.ThrowExceptionForHR(_underlyingDevice.GetState(out result));
                return result;
            }
        }

        public override string ToString()
        {
            string result;
            TryGetFriendlyName(out result);

            return result;
        }

        private PropertyStore OpenPropertyStore()
        {
            IPropertyStore underlyingPropertyStore;
            Marshal.ThrowExceptionForHR(_underlyingDevice.OpenPropertyStore(StorageAccessMode.Read, out underlyingPropertyStore));
            return new PropertyStore(underlyingPropertyStore);
        }
    }
}
