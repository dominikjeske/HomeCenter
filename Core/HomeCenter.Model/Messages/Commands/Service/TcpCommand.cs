namespace HomeCenter.Model.Messages.Commands.Service
{
    public class TcpCommand : Command
    {
        public string Address { get; set; }
        public byte[] Body { get; set; }
    }
}