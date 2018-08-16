namespace HomeCenter.Messaging.Behaviors
{
    public interface IBehavior : IAsyncCommandHandler
    {
        void SetNextNode(IAsyncCommandHandler asyncCommandHandler);

        int Priority { get; }
    }
}