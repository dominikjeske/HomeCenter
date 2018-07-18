
namespace Wirehome.WindowsService.Interop
{
    public class PropertyStoreProperty
    {
        private PropertyKey _key;
        private PropVariant _value;

        public PropertyStoreProperty(PropertyKey key, PropVariant value)
        {
            _key = key;
            _value = value;
        }

        public bool IsEmpty
        {
            get { return _value.IsEmpty; }
        }

        public PropertyKey Key
        {
            get { return _key; }
        }

        public object Value
        {
            get { return _value.Value; }
        }
    }
}

