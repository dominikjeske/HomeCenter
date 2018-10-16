using HomeCenter.Model.Core;

namespace HomeCenter.Model.Messages
{
    public abstract class ActorMessage : BaseObject
    {
        public Proto.IContext Context { get; set; }

        protected ActorMessage()
        {
            Type = GetType().Name;
        }
    }
}