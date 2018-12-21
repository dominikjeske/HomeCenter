namespace HomeCenter.Model.Messages.Queries.Services
{
    public abstract class HttpGetQuery : Query
    {
        public abstract object Parse(string rawHttpResult);

        protected HttpGetQuery()
        {
            RequestType = "GET";
        }

        public string Address
        {
            get => AsString(MessageProperties.Address);
            set => SetProperty(MessageProperties.Address, value);
        }

        public string RequestType
        {
            get => AsString(MessageProperties.RequestType);
            set => SetProperty(MessageProperties.RequestType, value);
        }
    }
}