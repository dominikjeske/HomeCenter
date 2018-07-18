using System.Runtime.InteropServices;

namespace Wirehome.WindowsService.Interop
{
    public class PropertyStore
    {
        private readonly IPropertyStore _underlyingStore;

        internal PropertyStore(IPropertyStore store)
        {
           _underlyingStore = store;
        }

        public int Count
        {
            get
            {
                int result;
                Marshal.ThrowExceptionForHR(_underlyingStore.GetCount(out result));
                return result;
            }
        }

        public PropertyStoreProperty this[int index]
        {
            get
            {
                PropVariant result;
                PropertyKey key = Get(index);
                Marshal.ThrowExceptionForHR(_underlyingStore.GetValue(ref key, out result));
                return new PropertyStoreProperty(key, result);
            }
        }

        public bool Contains(PropertyKey key)
        {
            for (int i = 0; i < Count; i++)
            {
                PropertyKey other = Get(i);
                if ((other.formatId == key.formatId) && (other.propertyId == key.propertyId))
                {
                    return true;
                }
            }
            return false;
        }

        public PropertyStoreProperty this[PropertyKey key]
        {
            get
            {
                PropVariant result;
                for (int i = 0; i < Count; i++)
                {
                    PropertyKey other = Get(i);
                    if ((other.formatId == key.formatId) && (other.propertyId == key.propertyId))
                    {
                        Marshal.ThrowExceptionForHR(_underlyingStore.GetValue(ref other, out result));
                        return new PropertyStoreProperty(other, result);
                    }
                }
                return null;
            }
        }

        public PropertyKey Get(int index)
        {
            PropertyKey key;
            Marshal.ThrowExceptionForHR(_underlyingStore.GetAt(index, out key));
            return key;
        }

        public bool TryGetValue(PropertyKey key, out object value)
        {
            value = null;

            try
            {
                var property = this[key];
                if (property == null || property.IsEmpty)
                    return false;

                value = property.Value;
                return true;
            }
            catch (COMException ex)
            {
                const int NoSuchHDevinst = unchecked((int)0xE000020B);

                // Bad installation of driver
                if (ex.HResult == NoSuchHDevinst)
                {
                    return false;
                }

                throw;
            }
        }

        public PropVariant GetValue(int index)
        {
            PropVariant result;
            PropertyKey key = Get(index);
            Marshal.ThrowExceptionForHR(_underlyingStore.GetValue(ref key, out result));
            return result;
        }
    }
}

