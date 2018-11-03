namespace HomeCenter.Model.Messages.Commands.Service
{
    //TODO move command properties to ValueType properties
    public class I2cCommand : Command
    {
        public int Address { get; set; }
        public byte[] Body { get; set; }
        public bool UseCache { get; set; }

        public static I2cCommand Create(int address, byte[] data, bool useCache = true) => new I2cCommand { Address = address, Body = data, UseCache = useCache };
    }
}