using HomeCenter.Model.Messages.Commands.Service;
using Proto;
using System.Linq;
using System.Runtime.InteropServices;

namespace HomeCenter.Model.Messages.Queries.Service
{
    public class SerialRegistrationQuery : Query
    {
        public Format[] ResultFormat { get; }
        public byte MessageType { get; }
        public int MessageSize { get; }
        public PID Actor { get; }

        public SerialRegistrationQuery(PID actor, byte messageType, Format[] resultFormat)
        {
            Actor = actor;
            MessageType = messageType;
            MessageSize = resultFormat.Sum(format => Marshal.SizeOf(format.ValueType));
            ResultFormat = resultFormat;
        }
    }
}