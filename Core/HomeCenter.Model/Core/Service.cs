using HomeCenter.Messaging;

namespace HomeCenter.Model.Core
{
    public abstract class Service : Actor
    {
        protected Service(IEventAggregator eventAggregator) : base(eventAggregator)
        {
        }
    }
}