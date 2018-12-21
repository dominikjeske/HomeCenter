namespace HomeCenter.Model.Messages.Queries.Services
{
    public class TcpQuery : Query
    {
        public string Address
        {
            get => AsString(MessageProperties.Address);
            set => SetProperty(MessageProperties.Address, value);
        }

        public byte[] Body
        {
            get => AsByteArray(MessageProperties.Body);
            set => SetProperty(MessageProperties.Body, value);
        }
    }
}