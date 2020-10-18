namespace HomeCenter.Broker.Behaviors
{
    public interface IBehavior : IAsyncCommandHandler
    {
        void SetNextNode(IAsyncCommandHandler asyncCommandHandler);

        int Priority { get; }
    }
}