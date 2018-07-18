using System.Threading;

namespace Wirehome.ComponentModel.Commands
{
    public class Command : BaseObject
    {
        public CancellationToken CancellationToken { get; }

        public Command()
        {
            SupressPropertyChangeEvent = true;
        }

        public Command(string commandType) : base()
        {
            Type = commandType;
        }

        public Command(string commandType, params Property[] properties) : base(properties)
        {
            Type = commandType;
        }

        public Command(string commandType, CancellationToken cancellationToken) : base()
        {
            Type = commandType;
            CancellationToken = cancellationToken;
        }

        public static implicit operator Command(string value) => new Command(value);
    }
}