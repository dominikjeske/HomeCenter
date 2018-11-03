namespace HomeCenter.Model.Messages.Queries.Services
{
    public class TcpQuery : Query
    {
        public string Address { get; set; }
        public byte[] Body { get; set; }
    }
}