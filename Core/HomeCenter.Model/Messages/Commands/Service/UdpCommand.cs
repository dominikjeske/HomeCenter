using HomeCenter.Model.Messages.Commands;

namespace HomeCenter.Model.Messages.Commands.Service
{
    public class UdpCommand : Command
    {
        public string Address { get; set; }
        public byte[] Body { get; set; }
    }
}