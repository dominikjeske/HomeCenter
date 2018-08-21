using System.Threading;

namespace HomeCenter.ComponentModel.Commands
{
    public class Query : ActorMessage
    {
        public CancellationToken CancellationToken { get; }

        public Query()
        {
            SupressPropertyChangeEvent = true;
        }

        public Query(string commandType) : base()
        {
            Type = commandType;
        }

        public Query(string commandType, string uid) : base()
        {
            Type = commandType;
            Uid = uid;
        }

        //public Query(string commandType, params Property[] properties) : base(properties)
        //{
        //    Type = commandType;
        //}

        public Query(string commandType, CancellationToken cancellationToken) : base()
        {
            Type = commandType;
            CancellationToken = cancellationToken;
        }

        public static implicit operator Query(string value) => new Query(value);
    }
}