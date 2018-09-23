namespace HomeCenter.Model.Core
{
    public abstract class ActorMessage : BaseObject
    {
        public Proto.IContext Context { get; set; }
    }
}