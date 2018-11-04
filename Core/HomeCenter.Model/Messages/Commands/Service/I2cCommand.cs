using HomeCenter.Model.Extensions;
using HomeCenter.Model.ValueTypes;

namespace HomeCenter.Model.Messages.Commands.Service
{
    public class I2cCommand : Command
    {
        public int Address
        {
            get => this[nameof(Address)].AsInt();
            set => this[nameof(Address)] = (IntValue)value;
        }

        public byte[] Body
        {
            get => this[nameof(Body)].AsByteArray();
            set => this[nameof(Body)] = (ByteArrayValue)value;
        }

        public bool UseCache
        {
            get => GetPropertyValue(nameof(UseCache), new BooleanValue(true)).AsBool();
            set => this[nameof(UseCache)] = (BooleanValue)value;
        }

        public static I2cCommand Create(int address, byte[] data, bool useCache = true) => new I2cCommand { Address = address, Body = data, UseCache = useCache };
    }
}