namespace HomeCenter.Model.Messages.Commands.Service
{
    public class TcpCommand : Command
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