using Proto;
using System.Linq;
using System.Runtime.InteropServices;

namespace HomeCenter.Model.Messages.Commands.Service
{
    public class SerialRegistrationCommand : Command
    {
        public Format[] ResultFormat { get; }
        public byte MessageType { get; }
        public int MessageSize { get; }
        public PID Actor { get; }

        public SerialRegistrationCommand(PID actor, byte messageType, Format[] resultFormat)
        {
            Actor = actor;
            MessageType = messageType;
            MessageSize = resultFormat.Sum(format => Marshal.SizeOf(format.ValueType));
            ResultFormat = resultFormat;
        }
    }
}