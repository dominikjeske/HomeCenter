namespace HomeCenter.Model.Messages.Queries.Services
{
    public class HttpQuery : Query
    {
        public string Address { get; set; }
        public string RequestType { get; set; } = "GET";
    }
}