using HomeCenter.ComponentModel.Components;
using System.Threading;

namespace HomeCenter.ComponentModel.Commands
{
    public class Command : ActorMessage, IExecutionContext
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

        public Command(string commandType, string uid) : base()
        {
            Type = commandType;
            Uid = uid;
        }

        //public Command(string commandType, params Property[] properties) : base(properties)
        //{
        //    Type = commandType;
        //}

        public Command(string commandType, CancellationToken cancellationToken) : base()
        {
            Type = commandType;
            CancellationToken = cancellationToken;
        }

        public Proto.IContext Context { get; set; }

        public static implicit operator Command(string value) => new Command(value);
    }
}