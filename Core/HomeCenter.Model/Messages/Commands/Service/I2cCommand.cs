namespace HomeCenter.Model.Messages.Commands.Service
{
    public class I2cCommand : Command
    {
        public int Address
        {
            get => AsInt(nameof(Address));
            set => SetProperty(nameof(Address), value);
        }

        public byte[] Body
        {
            get => AsByteArray(nameof(Body));
            set => SetProperty(nameof(Body), value);
        }

        public static I2cCommand Create(int address, byte[] data, bool useCache = true) => new I2cCommand { Address = address, Body = data};
    }
}