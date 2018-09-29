namespace HomeCenter.Model.Messages.Queries.Services
{
    public class HttpQuery : Query
    {
        public virtual string Address { get; set; }
        public string RequestType { get; set; } = "GET";
    }
}