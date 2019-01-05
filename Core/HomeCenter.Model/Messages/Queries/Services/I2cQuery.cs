namespace HomeCenter.Model.Messages.Queries.Services
{
    public class I2cQuery : Query
    {
        public static I2cQuery Create(int address, byte[] InitializeWrite, int bufferSize) => new I2cQuery
        {
            Address = address,
            Initialize = InitializeWrite,
            BufferSize = bufferSize,
            LogLevel = nameof(Microsoft.Extensions.Logging.LogLevel.Trace)
        };

        public int Address
        {
            get => AsInt(MessageProperties.Address);
            set => SetProperty(MessageProperties.Address, value);
        }

        public byte[] Initialize
        {
            get => AsByteArray(MessageProperties.Initialize);
            set => SetProperty(MessageProperties.Initialize, value);
        }

        public int BufferSize
        {
            get => AsInt(MessageProperties.Size);
            set => SetProperty(MessageProperties.Size, value);
        }
    }
}