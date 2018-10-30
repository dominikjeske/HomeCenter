namespace HomeCenter.Model.Messages.Queries.Services
{
    public abstract class HttpQuery : Query
    {
        public string Address { get; set; }
        public string RequestType { get; set; } = "GET";

        public abstract object Parse(string rawHttpResult);
    }
}