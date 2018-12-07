using HomeCenter.Model.Core;
using Microsoft.Extensions.Logging;

namespace HomeCenter.Model.Messages
{
    public abstract class ActorMessage : BaseObject
    {
        public Proto.IContext Context { get; set; }
        public LogLevel DefaultLogLevel { get; set; } = LogLevel.Debug;

        protected ActorMessage()
        {
            Type = GetType().Name;
        }
    }
}