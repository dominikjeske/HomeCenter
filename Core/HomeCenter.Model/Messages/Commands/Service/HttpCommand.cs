namespace HomeCenter.Model.Messages.Commands.Service
{
    public class HttpCommand : Command
    {
        public HttpCommand()
        {
            RequestType = "POST";
        }

        public string Address
        {
            get => AsString(MessageProperties.Address);
            set => SetProperty(MessageProperties.Address, value);
        }

        public string Body
        {
            get => AsString(MessageProperties.Body);
            set => SetProperty(MessageProperties.Body, value);
        }

        public string RequestType
        {
            get => AsString(MessageProperties.RequestType);
            set => SetProperty(MessageProperties.RequestType, value);
        }

        public string ContentType
        {
            get => AsString(MessageProperties.ContentType);
            set => SetProperty(MessageProperties.ContentType, value);
        }
    }
}