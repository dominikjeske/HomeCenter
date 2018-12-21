namespace HomeCenter.Model.Messages.Queries.Services
{
    public class I2cQuery : Query
    {
        public static I2cQuery Create(int address, byte[] preRead) => new I2cQuery
        {
            Address = address,
            Initialize = preRead
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
    }
}